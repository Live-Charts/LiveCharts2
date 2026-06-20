// The MIT License(MIT)
//
// Copyright(c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiveChartsCore.Drawing;
using LiveChartsCore.Geo;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.Motion;
using LiveChartsCore.Painting;

namespace LiveChartsCore;

/// <summary>
/// Defines a geo map chart.
/// </summary>
/// <remarks>
/// PHASE 1 SKETCH: inherits from <see cref="Chart"/> so the map participates in
/// the same lifecycle / throttler / pointer pipeline as Cartesian, Pie, Polar.
///
/// What this class no longer owns (delegated to <see cref="Chart"/> base):
///   - <c>_updateThrottler</c>, <c>_panningThrottler</c> and the
///     <c>Update</c> / <c>UpdateThrottlerUnlocked</c> methods.
///   - <c>_pointerPosition</c>, <c>_isPointerIn</c>, <c>_isPointerDown</c>,
///     <c>_isPanning</c>, <c>_pointerPanningPosition</c>,
///     <c>_pointerPreviousPanningPosition</c> (all base internals).
///   - The custom <c>Action&lt;LvcPoint&gt;</c> pointer events; <see cref="Chart"/>
///     fires its own <c>Action&lt;Chart, LvcPoint&gt;</c> events.
///   - Canvas (via base), the <c>Load</c>/<c>Unload</c> shells.
///
/// What stays geo-specific (overrides):
///   - <see cref="Measure"/>, <see cref="PanningThrottlerUnlocked"/>,
///     <see cref="IsPanEnabled"/>, <see cref="InvokePointerDown"/> family.
///   - Land-based tooltip pipeline (own <c>_tooltipThrottler</c> — different
///     semantics from <see cref="IChartTooltip"/>; unifying is Phase 2 work).
///   - Viewport state (zoom, pan, rotation) and bounce/rotation animators.
/// </remarks>
public class GeoMapChart : Chart
{
    private readonly HashSet<IGeoSeries> _everMeasuredSeries = [];
    private readonly ActionThrottler _tooltipThrottler;
    private bool _isHeatInCanvas = false;
    private Paint _heatPaint;
    private Paint? _previousStroke;
    private Paint? _previousFill;
    private IMapFactory _mapFactory;
    private DrawnMap? _activeMap;
    private bool _isUnloaded = false;
    // _isToolTipOpen is inherited from Chart base; we reuse it for the geo tooltip.
    private LandDefinition? _hoveredLand;
    private float _zoomLevel = 1f;
    private LvcPoint _panOffset = new(0, 0);
    private bool _isBouncing = false;
    private System.Threading.Timer? _bounceTimer;
    private LvcPoint _pointerDownPosition = new(-10, -10);
    private bool _pointerDownIsClick = false;
    private readonly RotationTracker _rotation = new();
    // Cache of the last hovered ChartPoint set so HoveredPointsChanged can pass
    // an accurate oldPoints arg. A geo "hover" is at most one land — but the
    // contract accepts an enumerable so we keep that shape.
    private ChartPoint[]? _hoveredChartPoints;
    // Last projector built by Measure(); reused by FindLandAt /
    // ComputeLandScreenBounds so hit-testing matches the on-screen layout
    // (including title / legend margin reservations).
    private MapProjector? _lastProjector;

    /// <summary>
    /// Initializes a new instance of the <see cref="GeoMapChart"/> class.
    /// </summary>
    /// <param name="mapView">The map view.</param>
    public GeoMapChart(IGeoMapView mapView)
        : base(mapView.CoreCanvas, mapView, ChartKind.GeoMap)
    {
        MapView = mapView;
        _heatPaint = LiveCharts.DefaultSettings.GetProvider().GetSolidColorPaint();
        _mapFactory = LiveCharts.DefaultSettings.GetProvider().GetDefaultMapFactory();
        _tooltipThrottler = new ActionThrottler(TooltipThrottlerUnlocked, TimeSpan.FromMilliseconds(50));

        // Drive orthographic rotation off the canvas paint cycle: after each
        // paint completes, if the rotation motion property is still mid-flight,
        // queue another Measure. This naturally rate-matches rotation to paint
        // capacity instead of queueing Timer ticks on a busy UI thread.
        Canvas.Validated += OnCanvasValidatedForRotation;
    }

    /// <summary>
    /// Gets the map view, typed.
    /// </summary>
    public IGeoMapView MapView { get; }

    /// <inheritdoc/>
    public override IChartView View => MapView;

    /// <inheritdoc/>
    public override IEnumerable<ISeries> Series => [];

    /// <inheritdoc cref="Chart.EnumerateHeatLegendSources" />
    public override IEnumerable<IHeatLegendSource> EnumerateHeatLegendSources() =>
        MapView.Series?.OfType<IHeatLegendSource>() ?? [];

    /// <inheritdoc/>
    public override IEnumerable<ISeries> VisibleSeries => [];

    /// <inheritdoc/>
    public override IEnumerable<ChartPoint> FindHoveredPointsBy(LvcPoint pointerPosition) => [];

    /// <summary>
    /// Pan is gated by the user's <see cref="IGeoMapView.InteractionMode"/>.
    /// Returning false here makes the base pan-engagement deadzone in
    /// <see cref="Chart.InvokePointerMove"/> a no-op so a drag stays a
    /// tooltip-only gesture instead of moving the map.
    /// </summary>
    internal override bool IsPanEnabled =>
        (MapView.InteractionMode & MapInteractionMode.Pan) == MapInteractionMode.Pan;

    /// <summary>
    /// Gets or sets the current zoom level. 1 is the default scale; values
    /// greater than 1 zoom in around the chart center. Mirrors the imperative
    /// API exposed alongside <see cref="CenterLongitude"/> /
    /// <see cref="CenterLatitude"/> — pair them to drive the viewport from
    /// code. <see cref="Zoom(LvcPoint, ZoomDirection)"/> is the gesture form
    /// (wheel zoom around a pivot).
    /// </summary>
    public float ZoomLevel
    {
        get => _zoomLevel;
        set
        {
            _zoomLevel = value;
            Update(new ChartUpdateParams { Throttling = false });
        }
    }

    /// <summary>Gets the current pan offset.</summary>
    public LvcPoint PanOffset
    {
        get => _panOffset;
        internal set => _panOffset = value;
    }

    /// <summary>
    /// Longitude of the viewport center — where the camera is looking. For
    /// <see cref="MapProjection.Orthographic"/> this rotates the globe;
    /// future flat-projection support uses it for horizontal panning.
    /// </summary>
    /// <remarks>
    /// Direct sets snap immediately (no animation). Use <see cref="RotateTo"/>
    /// for an animated transition.
    /// </remarks>
    public double CenterLongitude
    {
        get => _rotation.X;
        set
        {
            _rotation.RemoveTransition();
            _rotation.X = value;
            Update(new ChartUpdateParams { Throttling = false });
        }
    }

    /// <summary>
    /// Latitude of the viewport center. For
    /// <see cref="MapProjection.Orthographic"/> this rotates the globe;
    /// future flat-projection support uses it for vertical panning.
    /// </summary>
    /// <remarks>
    /// Direct sets snap immediately (no animation). Use <see cref="RotateTo"/>
    /// for an animated transition.
    /// </remarks>
    public double CenterLatitude
    {
        get => _rotation.Y;
        set
        {
            _rotation.RemoveTransition();
            _rotation.Y = value;
            Update(new ChartUpdateParams { Throttling = false });
        }
    }

    /// <inheritdoc cref="IMapFactory.ViewTo(GeoMapChart, object)"/>
    public virtual void ViewTo(object? command) => _mapFactory.ViewTo(this, command);

    /// <inheritdoc cref="IMapFactory.Pan(GeoMapChart, LvcPoint)"/>
    public virtual void Pan(LvcPoint delta) => _mapFactory.Pan(this, delta);

    /// <inheritdoc cref="IMapFactory.Zoom(GeoMapChart, LvcPoint, ZoomDirection)"/>
    public virtual void Zoom(LvcPoint pivot, ZoomDirection direction) =>
        _mapFactory.Zoom(this, pivot, direction);

    /// <summary>Resets the viewport to default zoom and pan.</summary>
    public void ResetViewport()
    {
        _isBouncing = false;
        _mapFactory.SetViewport(this, 1f, new LvcPoint(0, 0));
    }

    /// <summary>
    /// Animates the globe rotation. Only visual under <see cref="MapProjection.Orthographic"/>.
    /// </summary>
    public void RotateTo(double longitude, double latitude, int durationMs = 800)
    {
        // Normalize longitude diff to shortest path so the rotation doesn't
        // sweep the long way around the globe (e.g. 170° → -170° goes through
        // the antimeridian, not the full 340° around).
        var startLon = _rotation.X;
        var deltaLon = longitude - startLon;
        if (deltaLon > 180) longitude = startLon + (deltaLon - 360);
        else if (deltaLon < -180) longitude = startLon + (deltaLon + 360);

        _rotation.SetTransition(new Animation(EasingFunctions.QuadraticInOut, TimeSpan.FromMilliseconds(durationMs)));
        _rotation.X = longitude;
        _rotation.Y = latitude;

        // Kick the first frame; subsequent frames flow through
        // OnCanvasValidatedForRotation until the motion property settles.
        Update(new ChartUpdateParams { Throttling = false });
    }

    /// <summary>
    /// Projects (longitude, latitude) onto the chart's screen-space pixel
    /// coordinates using the projector built by the last <see cref="Measure"/>
    /// pass. Returns null when the coordinate isn't visible — e.g. the back
    /// hemisphere on <see cref="MapProjection.Orthographic"/>. The
    /// projection honors the current draw margin, title/legend reservations,
    /// and orthographic rotation; pair with <see cref="Unproject"/> for the
    /// inverse.
    /// </summary>
    public LvcPoint? Project(double longitude, double latitude)
    {
        var projector = GetOrBuildProjector();
        if (!projector.IsVisible(longitude, latitude)) return null;
        projector.ToMap(longitude, latitude, out var x, out var y);
        return ApplyViewportTransform(new LvcPoint(x, y));
    }

    /// <summary>
    /// Inverse of <see cref="Project"/> — converts a screen-space pixel back
    /// to (longitude, latitude). Returns null when the pixel doesn't map to
    /// a valid coordinate (e.g. a click outside the orthographic disc).
    /// </summary>
    public (double Longitude, double Latitude)? Unproject(LvcPoint screenPoint)
    {
        var projector = GetOrBuildProjector();
        var unzoomed = InverseViewportTransform(screenPoint);
        return projector.ToCoordinates(unzoomed.X, unzoomed.Y, out var lon, out var lat)
            ? (lon, lat)
            : null;
    }

    // The map applies a viewport transform on top of the projector's raw
    // output to support pan/zoom (see ComputeLandScreenCenter /
    // ComputeLandScreenBounds and MapFactory's _viewportTransform). Overlays
    // need the same transform applied / inverted or they won't track pan/zoom.
    private LvcPoint ApplyViewportTransform(LvcPoint baseCoord)
    {
        var ctrlCx = MapView.ControlSize.Width * 0.5f;
        var ctrlCy = MapView.ControlSize.Height * 0.5f;
        var tx = ctrlCx * (1 - _zoomLevel) + _panOffset.X;
        var ty = ctrlCy * (1 - _zoomLevel) + _panOffset.Y;
        return new LvcPoint(baseCoord.X * _zoomLevel + tx, baseCoord.Y * _zoomLevel + ty);
    }

    private LvcPoint InverseViewportTransform(LvcPoint screenCoord)
    {
        var ctrlCx = MapView.ControlSize.Width * 0.5f;
        var ctrlCy = MapView.ControlSize.Height * 0.5f;
        var tx = ctrlCx * (1 - _zoomLevel) + _panOffset.X;
        var ty = ctrlCy * (1 - _zoomLevel) + _panOffset.Y;
        var zoom = _zoomLevel == 0f ? 1f : _zoomLevel;
        return new LvcPoint((screenCoord.X - tx) / zoom, (screenCoord.Y - ty) / zoom);
    }

    private void OnCanvasValidatedForRotation(CoreMotionCanvas _)
    {
        if (_isUnloaded) return;
        // _rotation.IsValid is flipped to false in Measure() when GetMovement
        // sees the motion property is still mid-flight. If still false here,
        // the rotation hasn't settled — request another Measure for the next
        // interpolated value. Loop exits when GetMovement stops invalidating.
        if (_rotation.IsValid) return;
        Update(new ChartUpdateParams { Throttling = false });
    }

    /// <summary>
    /// Invokes a pointer wheel event. No-op when zoom is disabled via
    /// <see cref="IGeoMapView.InteractionMode"/>.
    /// </summary>
    protected internal void InvokePointerWheel(LvcPoint point, ZoomDirection direction)
    {
        if ((MapView.InteractionMode & MapInteractionMode.Zoom) != MapInteractionMode.Zoom) return;
        Zoom(point, direction);
    }

    /// <inheritdoc/>
    public override void Load()
    {
        // Geo-specific resource (re-)init must run before base.Load() queues the
        // first Update — Measure() will read _heatPaint / _mapFactory.
        if (_isUnloaded)
        {
            _heatPaint = LiveCharts.DefaultSettings.GetProvider().GetSolidColorPaint();
            _mapFactory = LiveCharts.DefaultSettings.GetProvider().GetDefaultMapFactory();
            _isHeatInCanvas = false;
            _isUnloaded = false;
        }
        base.Load();
    }

    /// <inheritdoc/>
    public override void Unload()
    {
        if (_isUnloaded) { base.Unload(); return; }

        if (_isToolTipOpen)
        {
            MapView.Tooltip?.Hide(this);
            _isToolTipOpen = false;
        }
        _hoveredLand = null;

        if (MapView.Stroke is not null) Canvas.RemovePaintTask(MapView.Stroke);
        if (MapView.Fill is not null) Canvas.RemovePaintTask(MapView.Fill);

        _bounceTimer?.Dispose(); _bounceTimer = null;
        _isBouncing = false;
        // Rotation has no Timer to dispose — it runs through the canvas
        // Validated event hook, which Unload() naturally stops since
        // _isUnloaded gates OnCanvasValidatedForRotation.

        _everMeasuredSeries.Clear();
        _heatPaint = null!;
        _previousStroke = null!;
        _previousFill = null!;
        _isUnloaded = true;
        _mapFactory.Dispose();

        // Do NOT dispose _activeMap: the same instance is referenced by
        // MapView.ActiveMap and disposing here would make the chart unrenderable
        // on a subsequent Load (issue #1417). The view owns the map's lifetime.
        _activeMap = null!;
        _mapFactory = null!;

        base.Unload();
    }

    /// <inheritdoc/>
    protected internal override void InvokePointerDown(LvcPoint point, bool isSecondaryAction)
    {
        // Track click intent for LandClicked. Base will set _isPointerDown / seed
        // _pointerPosition / etc; we just track the geo-specific bits.
        _pointerDownPosition = point;
        _pointerDownIsClick = true;
        base.InvokePointerDown(point, isSecondaryAction);
    }

    /// <inheritdoc/>
    protected internal override void InvokePointerMove(LvcPoint point)
    {
        // Base handles _pointerPosition, _isPointerIn, pan-engagement deadzone
        // (which uses our IsPanEnabled override) and panning throttler dispatch.
        base.InvokePointerMove(point);

        if (_isPointerDown)
        {
            var dx = point.X - _pointerDownPosition.X;
            var dy = point.Y - _pointerDownPosition.Y;
            if (dx * dx + dy * dy > 25) _pointerDownIsClick = false;
        }

        // Geo tooltip dispatch uses its own throttler with land-based semantics
        // distinct from IChartTooltip. Base's _tooltipThrottler still fires but
        // DrawToolTip exits early because FindHoveredPointsBy returns [].
        if (!_isPanning && !_isPointerDown) _tooltipThrottler.Call();
    }

    /// <inheritdoc/>
    protected internal override void InvokePointerUp(LvcPoint point, bool isSecondaryAction)
    {
        var wasClick = _pointerDownIsClick;
        _pointerDownIsClick = false;

        base.InvokePointerUp(point, isSecondaryAction);

        if (_isPanning is false) BounceBack();

        if (wasClick)
        {
            var hit = FindLandAt(point);
            if (hit is not null)
                View.OnDataPointerDown(BuildHitPoints(hit.Value), point);
        }
    }

    /// <inheritdoc/>
    protected internal override void InvokePointerLeft()
    {
        base.InvokePointerLeft();

        if (_isToolTipOpen)
        {
            _hoveredLand = null;
            _isToolTipOpen = false;
            View.InvokeOnUIThread(() =>
            {
                MapView.Tooltip?.Hide(this);
                Canvas.Invalidate();
            });
        }

        if (_hoveredChartPoints is { Length: > 0 } oldPoints)
        {
            _hoveredChartPoints = null;
            View.OnHoveredPointsChanged(null, oldPoints);
        }
    }

    /// <inheritdoc/>
    protected override Task PanningThrottlerUnlocked()
    {
        return Task.Run(() =>
            View.InvokeOnUIThread(() =>
            {
                lock (Canvas.Sync)
                {
                    Pan(
                        new LvcPoint(
                            (float)(_pointerPosition.X - _pointerDownPosition.X),
                            (float)(_pointerPosition.Y - _pointerDownPosition.Y)));
                    _pointerDownPosition = _pointerPosition;
                }
            }));
    }

    /// <inheritdoc/>
    protected internal override void Measure()
    {
        // Snapshot last frame's measured elements into _toDeleteElements; each
        // AddVisual call below pulls its element out of the set. Anything
        // still there at the bottom (CollectVisuals) gets removed from the
        // canvas. Must run BEFORE AddTitleToChart / AddVisual so the title
        // doesn't get marked for deletion every frame.
        InitializeVisualsCollector();

        // GetTheme has the side effect of wiring Canvas._virtualBackgroundColor,
        // which platform views fall back to in IChartView.BackColor when no
        // native control Background is set. Without it, GPU mode (SKGLView on
        // MAUI etc.) clears to the GL surface default (transparent/black)
        // instead of the theme's chart background.
        _ = GetTheme();

        if (_activeMap is not null && _activeMap != MapView.ActiveMap)
        {
            _previousStroke?.ClearGeometriesFromPaintTask(Canvas);
            _previousFill?.ClearGeometriesFromPaintTask(Canvas);

            _previousFill = null;
            _previousStroke = null;

            Canvas.Clear();
        }
        _activeMap = MapView.ActiveMap;

        // Map paints go in the DrawMargin zone so lands beyond the projection's
        // rendering rectangle (Mercator extrapolation past ±MercatorMaxLatitude
        // — Greenland, Antarctica, etc.) get pixel-clipped to the map's draw
        // area instead of bleeding into Title / Legend space.
        if (!_isHeatInCanvas)
        {
            Canvas.AddDrawableTask(_heatPaint, zone: CanvasZone.DrawMargin);
            _isHeatInCanvas = true;
        }

        if (_previousStroke != MapView.Stroke)
        {
            if (_previousStroke is not null) Canvas.RemovePaintTask(_previousStroke);

            if (MapView.Stroke is not null)
            {
                if (MapView.Stroke.ZIndex == 0) MapView.Stroke.ZIndex = PaintConstants.GeoMapStrokeZIndex;
                MapView.Stroke.PaintStyle = PaintStyle.Stroke;
                Canvas.AddDrawableTask(MapView.Stroke, zone: CanvasZone.DrawMargin);
            }

            _previousStroke = MapView.Stroke;
        }

        if (_previousFill != MapView.Fill)
        {
            if (_previousFill is not null) Canvas.RemovePaintTask(_previousFill);

            if (MapView.Fill is not null)
            {
                MapView.Fill.PaintStyle = PaintStyle.Fill;
                Canvas.AddDrawableTask(MapView.Fill, zone: CanvasZone.DrawMargin);
            }

            _previousFill = MapView.Fill;
        }

        var i = _previousFill?.ZIndex ?? 0;
        _heatPaint.ZIndex = i + 1;

        // Mirror the cartesian engine: copy Legend/Title-adjacent properties
        // from the view onto Chart base so DrawLegend / MeasureTitle see the
        // current values. Without this DrawLegend would always early-out on
        // LegendPosition.Hidden (the protected default), and
        // ControlSize-derived positioning (title centering,
        // GetLegendPosition for Right/Bottom) would compute against (0,0).
        ControlSize = MapView.ControlSize;
        LegendPosition = MapView.LegendPosition;
        Legend = MapView.Legend;

        // Provisional draw margin so SKHeatLegend.GetLayout has a sane
        // chart.DrawMarginSize to size its gradient bar against on the first
        // measure (cartesian engine does the same trick at the same point).
        // The final draw margin is set below from the title/legend reservation.
        SetDrawMargin(ControlSize, new Margin());

        // Reserve space for Title + Legend before sizing the map render area.
        // Mirrors CartesianChartEngine's pattern: title first (Top reserve),
        // then DrawLegend mutates ts/bs/ls/rs based on LegendPosition.
        var m = new Margin();
        float ts = 0f, bs = 0f, ls = 0f, rs = 0f;
        if (View.Title is not null)
        {
            var titleSize = MeasureTitle();
            m.Top = titleSize.Height;
            ts = titleSize.Height;
        }
        DrawLegend(ref ts, ref bs, ref ls, ref rs);
        m.Top = ts;
        m.Bottom = bs;
        m.Left = ls;
        m.Right = rs;
        SetDrawMargin(MapView.ControlSize, m);

        if (View.Title is not null) AddTitleToChart();

        // Bail when the chart is too small for a map after reservations (matches cartesian behavior).
        // Hide the plot zone first so the previous, now-invalid map isn't left painted at its old
        // transform; the valid path below re-sets the draw-margin clip, so recovery is automatic.
        if (DrawMarginSize.Width <= 0 || DrawMarginSize.Height <= 0)
        {
            HidePlotZones();
            Canvas.Invalidate();
            return;
        }

        // Reset IsValid before reading the motion properties so GetMovement
        // can flip it back to false only if the rotation animation is still
        // mid-flight. The OnCanvasValidatedForRotation hook checks this flag
        // post-paint to decide whether to queue another measure.
        _rotation.IsValid = true;
        var rotX = _rotation.X;
        var rotY = _rotation.Y;
        var projector = Maps.BuildProjector(
            MapView.MapProjection,
            [DrawMarginSize.Width, DrawMarginSize.Height],
            DrawMarginLocation.X, DrawMarginLocation.Y,
            rotX, rotY,
            MapView.MinLatitude, MapView.MaxLatitude,
            MapView.MinLongitude, MapView.MaxLongitude);
        _lastProjector = projector;

        // Pixel-clip painting to the projector's actual rendering rectangle so
        // lands extrapolated beyond it (e.g. Antarctica when the default
        // Mercator MinLatitude = -65° drops the south pole) don't bleed into
        // Title/Legend space.
        Canvas.Zones[CanvasZone.NoClip].Clip = LvcRectangle.Empty;
        Canvas.Zones[CanvasZone.DrawMargin].Clip = new(
            new(projector.XOffset, projector.YOffset),
            new(projector.MapWidth, projector.MapHeight));

        var context = new MapContext(this, MapView, MapView.ActiveMap, projector);

        _mapFactory.GenerateLands(context);

        // Departed series must be deleted BEFORE measuring the new series.
        // Otherwise CoreHeatLandSeries.Delete -> ClearHeat would null the Shape.Fill
        // on lands shared with the new series AFTER the new series painted them,
        // making shared lands appear blank on series swap (issue #962).
        var currentSeries = MapView.Series?.Cast<IGeoSeries>().ToArray() ?? [];
        var currentSet = new HashSet<IGeoSeries>(currentSeries);
        foreach (var series in _everMeasuredSeries)
        {
            if (currentSet.Contains(series)) continue;
            series.Delete(context);
        }
        _everMeasuredSeries.RemoveWhere(s => !currentSet.Contains(s));

        foreach (var series in currentSeries)
        {
            series.Measure(context);
            _ = _everMeasuredSeries.Add(series);
        }

        // VisualElements on the geo map (GeoVisualElement for lat/lon-anchored
        // overlays, plain VisualElement for pixel-positioned ones). Each
        // visible one goes through AddVisual which removes the element from
        // the to-delete set (set up at the top of Measure by
        // InitializeVisualsCollector). CollectVisuals at the end drops any
        // elements still in the to-delete set — i.e. ones removed from
        // VisualElements between frames OR hidden by IsVisible=false.
        foreach (var visual in (MapView.VisualElements ?? []).Where(static x => x.IsVisible))
            AddVisual(visual);
        CollectVisuals();

        if (_hoveredLand is not null && MapView.Tooltip is not null &&
            MapView.TooltipPosition != TooltipPosition.Hidden)
        {
            var center = ComputeLandScreenCenter(_hoveredLand, context.Projector);

            MapView.Tooltip.Show(
                new GeoTooltipPoint
                {
                    Land = _hoveredLand,
                    Values = CollectLandValues(_hoveredLand),
                    LandCenter = center
                },
                this);
        }

        Canvas.Invalidate();
    }

    /// <summary>
    /// Finds the land at the specified pointer position, returning the land
    /// definition, the values contributed by every series that covers it
    /// (in <see cref="IGeoMapView.Series"/> declaration order), and the
    /// projected screen anchor for tooltip placement.
    /// </summary>
    public (LandDefinition Land, IReadOnlyList<GeoTooltipValue> Values, LvcPoint Center)? FindLandAt(LvcPoint pointerPosition)
    {
        if (_activeMap is null) return null;

        foreach (var layer in _activeMap.Layers.Values)
        {
            if (!layer.IsVisible) continue;

            foreach (var landDefinition in layer.Lands.Values)
            {
                foreach (var landData in landDefinition.Data)
                {
                    if (landData.Shape is null) continue;
                    if (!landData.Shape.ContainsPoint(pointerPosition.X, pointerPosition.Y)) continue;

                    var values = CollectLandValues(landDefinition);
                    var center = ComputeLandScreenCenter(landDefinition, GetOrBuildProjector());

                    return (landDefinition, values, center);
                }
            }
        }

        return null;
    }

    private IReadOnlyList<GeoTooltipValue> CollectLandValues(LandDefinition land)
    {
        List<GeoTooltipValue>? acc = null;
        foreach (var series in MapView.Series?.Cast<IGeoSeries>() ?? [])
        {
            if (!series.TryGetValue(land.ShortName, out var value)) continue;
            (acc ??= []).Add(new GeoTooltipValue { Series = series, Value = value });
        }
        return acc ?? (IReadOnlyList<GeoTooltipValue>)[];
    }

    private LvcPoint ComputeLandScreenCenter(LandDefinition land, MapProjector projector)
    {
        // Anchor on the largest visible contour, not the union of all contours.
        // Lands like Russia that cross the antimeridian have one contour at ~170°E
        // and another at ~-170°W; a unified bbox spans the whole map and the
        // centroid lands mid-Pacific. Per-contour selection picks the mainland.
        float bestMinX = 0f, bestMinY = 0f, bestMaxX = 0f, bestMaxY = 0f;
        var bestArea = -1f;

        foreach (var data in land.Data)
        {
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;
            var hasContourPoints = false;

            foreach (var coord in data.Coordinates)
            {
                if (!projector.IsVisible(coord.X, coord.Y)) continue;
                projector.ToMap(coord.X, coord.Y, out var px, out var py);
                if (px < minX) minX = px;
                if (py < minY) minY = py;
                if (px > maxX) maxX = px;
                if (py > maxY) maxY = py;
                hasContourPoints = true;
            }

            if (!hasContourPoints) continue;

            var area = (maxX - minX) * (maxY - minY);
            if (area > bestArea)
            {
                bestArea = area;
                bestMinX = minX; bestMinY = minY;
                bestMaxX = maxX; bestMaxY = maxY;
            }
        }

        if (bestArea < 0f) return _pointerPosition;

        var baseCx = (bestMinX + bestMaxX) / 2f;
        var baseCy = (bestMinY + bestMaxY) / 2f;
        var ctrlCx = MapView.ControlSize.Width * 0.5f;
        var ctrlCy = MapView.ControlSize.Height * 0.5f;
        var tx = ctrlCx * (1 - _zoomLevel) + _panOffset.X;
        var ty = ctrlCy * (1 - _zoomLevel) + _panOffset.Y;
        return new LvcPoint(baseCx * _zoomLevel + tx, baseCy * _zoomLevel + ty);
    }

    private Task TooltipThrottlerUnlocked()
    {
        return Task.Run(() =>
            View.InvokeOnUIThread(() =>
            {
                lock (Canvas.Sync)
                {
                    if (_isUnloaded || _isPanning || !_isPointerIn) return;

                    var result = FindLandAt(_pointerPosition);
                    var tooltipEnabled =
                        MapView.Tooltip is not null &&
                        MapView.TooltipPosition != TooltipPosition.Hidden;

                    if (result is null)
                    {
                        // Hover left every land — fire HoveredPointsChanged
                        // regardless of tooltip state so consumers tracking
                        // hover get a clean exit signal.
                        if (_hoveredChartPoints is { Length: > 0 } oldExit)
                        {
                            _hoveredChartPoints = null;
                            View.OnHoveredPointsChanged(null, oldExit);
                        }
                        if (tooltipEnabled && _isToolTipOpen)
                        {
                            _hoveredLand = null;
                            _isToolTipOpen = false;
                            MapView.Tooltip!.Hide(this);
                            Canvas.Invalidate();
                        }
                        return;
                    }

                    var hit = result.Value;
                    if (hit.Land == _hoveredLand) return;

                    var oldPoints = _hoveredChartPoints;
                    var newPoints = BuildHitPoints(hit);
                    _hoveredChartPoints = newPoints;
                    _hoveredLand = hit.Land;

                    View.OnHoveredPointsChanged(newPoints, oldPoints);

                    if (tooltipEnabled)
                    {
                        _isToolTipOpen = true;
                        MapView.Tooltip!.Show(
                            new GeoTooltipPoint
                            {
                                Land = hit.Land,
                                Values = hit.Values,
                                LandCenter = hit.Center
                            },
                            this);
                        Canvas.Invalidate();
                    }
                }
            }));
    }

    /// <summary>
    /// Builds the <see cref="ChartPoint"/> set for a land hit. One point per
    /// land — geo lands aren't owned by a single series (the same country can
    /// be in zero or many heat series), so the DataSource is the land itself.
    /// </summary>
    internal ChartPoint[] BuildHitPoints((LandDefinition Land, IReadOnlyList<GeoTooltipValue> Values, LvcPoint Center) hit)
    {
        var bbox = ComputeLandScreenBounds(hit.Land);
        var hoverArea = new RectangleHoverArea(bbox.MinX, bbox.MinY, bbox.MaxX - bbox.MinX, bbox.MaxY - bbox.MinY);
        return [new ChartPoint(View, hit.Land, hoverArea)];
    }

    // Reuse Measure()'s projector when available; fall back to a fresh
    // control-size build when hit-test APIs are called before any measure
    // (rare but covered by GetPointsAt before first paint).
    private MapProjector GetOrBuildProjector() =>
        _lastProjector ?? Maps.BuildProjector(
            MapView.MapProjection,
            [MapView.ControlSize.Width, MapView.ControlSize.Height],
            0f, 0f,
            _rotation.X, _rotation.Y,
            MapView.MinLatitude, MapView.MaxLatitude,
            MapView.MinLongitude, MapView.MaxLongitude);

    private (float MinX, float MinY, float MaxX, float MaxY) ComputeLandScreenBounds(LandDefinition land)
    {
        var projector = GetOrBuildProjector();

        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;
        var any = false;

        foreach (var data in land.Data)
        {
            foreach (var coord in data.Coordinates)
            {
                if (!projector.IsVisible(coord.X, coord.Y)) continue;
                projector.ToMap(coord.X, coord.Y, out var px, out var py);
                if (px < minX) minX = px;
                if (py < minY) minY = py;
                if (px > maxX) maxX = px;
                if (py > maxY) maxY = py;
                any = true;
            }
        }

        if (!any) return (0f, 0f, 0f, 0f);

        var ctrlCx = MapView.ControlSize.Width * 0.5f;
        var ctrlCy = MapView.ControlSize.Height * 0.5f;
        var tx = ctrlCx * (1 - _zoomLevel) + _panOffset.X;
        var ty = ctrlCy * (1 - _zoomLevel) + _panOffset.Y;

        return (
            minX * _zoomLevel + tx,
            minY * _zoomLevel + ty,
            maxX * _zoomLevel + tx,
            maxY * _zoomLevel + ty);
    }

    private void BounceBack()
    {
        if (_isBouncing) return;

        var controlW = MapView.ControlSize.Width;
        var controlH = MapView.ControlSize.Height;
        if (controlW <= 0 || controlH <= 0) return;

        var cx = controlW * 0.5f;
        var cy = controlH * 0.5f;
        var zoom = _zoomLevel;
        var minZoom = (float)MapView.MinZoomLevel;
        var targetZoom = zoom < minZoom ? minZoom : zoom;

        var mapScreenW = controlW * targetZoom;
        var mapScreenH = controlH * targetZoom;

        var targetPanX = _panOffset.X;
        var targetPanY = _panOffset.Y;

        if (Math.Abs(targetZoom - zoom) > 1e-6)
        {
            targetPanX = _panOffset.X * targetZoom / zoom;
            targetPanY = _panOffset.Y * targetZoom / zoom;
        }

        var tx = cx * (1 - targetZoom) + targetPanX;
        var ty = cy * (1 - targetZoom) + targetPanY;

        if (tx > 0) tx = 0;
        if (tx + mapScreenW < controlW) tx = controlW - mapScreenW;
        if (ty > 0) ty = 0;
        if (ty + mapScreenH < controlH) ty = controlH - mapScreenH;

        targetPanX = tx - cx * (1 - targetZoom);
        targetPanY = ty - cy * (1 - targetZoom);

        var panDiffX = Math.Abs(targetPanX - _panOffset.X);
        var panDiffY = Math.Abs(targetPanY - _panOffset.Y);
        var zoomDiff = Math.Abs(targetZoom - zoom);

        if (panDiffX < 0.5f && panDiffY < 0.5f && zoomDiff < 0.001f) return;

        _isBouncing = true;
        AnimateBounce(targetPanX, targetPanY, targetZoom);
    }

    private void AnimateBounce(float targetPanX, float targetPanY, float targetZoom)
    {
        const int steps = 8;
        const int intervalMs = 16;
        var step = 0;

        var startPanX = _panOffset.X;
        var startPanY = _panOffset.Y;
        var startZoom = _zoomLevel;

        _bounceTimer?.Dispose();
        _bounceTimer = new System.Threading.Timer(_ =>
        {
            if (_isUnloaded)
            {
                _isBouncing = false;
                _bounceTimer?.Dispose();
                _bounceTimer = null;
                return;
            }

            step++;
            var t = (float)step / steps;
            t = 1 - (1 - t) * (1 - t) * (1 - t);

            var newPanX = startPanX + (targetPanX - startPanX) * t;
            var newPanY = startPanY + (targetPanY - startPanY) * t;
            var newZoom = startZoom + (targetZoom - startZoom) * t;

            View.InvokeOnUIThread(() =>
            {
                if (_isUnloaded) return;
                _mapFactory.SetViewport(this, newZoom, new LvcPoint(newPanX, newPanY));
            });

            if (step >= steps)
            {
                _isBouncing = false;
                _bounceTimer?.Dispose();
                _bounceTimer = null;
            }
        }, null, intervalMs, intervalMs);
    }

    // AnimateRotation removed — rotation is now driven by the LiveCharts
    // motion-property engine via _rotation (RotationTracker). See RotateTo
    // and OnCanvasValidatedForRotation.
}
