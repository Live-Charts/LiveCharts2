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
using System.Diagnostics;
using System.Linq;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.Motion;

namespace LiveChartsCore;

/// <summary>
/// Defines a Cartesian chart.
/// </summary>
/// <seealso cref="Chart" />
/// <remarks>
/// Initializes a new instance of the <see cref="CartesianChartEngine"/> class.
/// </remarks>
/// <param name="view">The view.</param>
/// <param name="canvas">The canvas.</param>
public class CartesianChartEngine(
    ICartesianChartView view,
    CoreMotionCanvas canvas)
        : Chart(canvas, view, ChartKind.Cartesian)
{
    private readonly ICartesianChartView _chartView = view;
    private BoundedDrawnGeometry? _zoomingSection;
    private double _zoomingSpeed = 0;
    private ChartElement? _previousDrawMarginFrame;
    private HashSet<CartesianChartEngine>? _sharedEvents;
    private HashSet<ICartesianAxis> _crosshair = [];
    private ICartesianAxis[]? _virtualX;
    private ICartesianAxis[]? _virtualY;
    private readonly ActionDebouncer _zoommingDebouncer = new(TimeSpan.FromMilliseconds(300));

    /// <summary>
    /// Gets the x axes.
    /// </summary>
    /// <value>
    /// The x axes.
    /// </value>
    public ICartesianAxis[] XAxes { get; private set; } =
        [];

    /// <summary>
    /// Gets the y axes.
    /// </summary>
    /// <value>
    /// The y axes.
    /// </value>
    public ICartesianAxis[] YAxes { get; private set; } =
        [];

    /// <summary>
    /// Gets the sections.
    /// </summary>
    /// <value>
    /// The sections.
    /// </value>
    public IEnumerable<IChartElement> Sections { get; private set; } =
        [];

    ///<inheritdoc cref="Chart.Series"/>
    public override IEnumerable<ISeries> Series =>
        _chartView.Series?.Select(x => x.ChartElementSource).Cast<ISeries>() ?? [];

    ///<inheritdoc cref="Chart.VisibleSeries"/>
    public override IEnumerable<ISeries> VisibleSeries =>
        Series.Where(static x => x.IsVisible);

    /// <summary>
    /// Gets the view.
    /// </summary>
    /// <value>
    /// The view.
    /// </value>
    public override IChartView View => _chartView;

    /// <summary>
    /// Called when the draw margin is already known. The draw margin is the Cartesian grid
    /// where the series are drawn, ignoring the axes labels, legends, titles etc.
    /// </summary>
    public event Action<CartesianChartEngine>? DrawMarginDefined;

    /// <summary>
    /// Finds the points near to the specified location.
    /// </summary>
    /// <param name="pointerPosition">The pointer position.</param>
    /// <returns></returns>
    public override IEnumerable<ChartPoint> FindHoveredPointsBy(LvcPoint pointerPosition)
    {
        var actualStrategy = FindingStrategy;

        if (actualStrategy == FindingStrategy.Automatic)
            actualStrategy = VisibleSeries.GetFindingStrategy();

        return VisibleSeries
            .Where(series => series.IsHoverable)
            .SelectMany(series => series.FindHitPoints(this, pointerPosition, actualStrategy, FindPointFor.HoverEvent));
    }

    /// <summary>
    /// Scales the specified point to the UI.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="xAxisIndex">Index of the x axis.</param>
    /// <param name="yAxisIndex">Index of the y axis.</param>
    /// <returns></returns>
    public double[] ScaleUIPoint(LvcPoint point, int xAxisIndex = 0, int yAxisIndex = 0)
    {
        var xAxis = XAxes[xAxisIndex];
        var yAxis = YAxes[yAxisIndex];

        var xScaler = new Scaler(DrawMarginLocation, DrawMarginSize, xAxis);
        var yScaler = new Scaler(DrawMarginLocation, DrawMarginSize, yAxis);

        return [xScaler.ToChartValues(point.X), yScaler.ToChartValues(point.Y)];
    }

    /// <summary>
    /// Zooms at the specified pivot.
    /// </summary>
    /// <param name="flags">
    /// The flags, for example ZoomAndPanMode.ZoomX | ZoomAndPanMode.NoFit, will zoom only in the x axis
    /// and will ignore the fit to bounds feature.
    /// </param>
    /// <param name="pivot">The pivot, is the reference point, the center where the zoom operation is calculated.</param>
    /// <param name="direction">The direction in or out.</param>
    /// <param name="scaleFactor">
    /// The scale factor, requires direction param to be <see cref="ZoomDirection.DefinedByScaleFactor"/>, then
    /// we can define the scale factor, for example 0.8 means to decrease the viewport 20%, and 1.2 means to increase
    /// the viewport 20%, this is method normally used in touch devices when pinching.
    /// </param>
    public void Zoom(ZoomAndPanMode flags, LvcPoint pivot, ZoomDirection direction, double? scaleFactor = null)
    {
        if (YAxes is null || XAxes is null)
            return;

        if (scaleFactor is not null && direction != ZoomDirection.DefinedByScaleFactor)
            throw new InvalidOperationException(
                $"When the scale factor is defined, the zoom direction must be {nameof(ZoomDirection.DefinedByScaleFactor)}... " +
                $"it just makes sense.");

        if (flags.HasFlag(ZoomAndPanMode.ZoomX))
            foreach (var axis in XAxes)
                ZoomAxis(axis, flags, pivot.X, direction, scaleFactor);

        if (flags.HasFlag(ZoomAndPanMode.ZoomY))
            foreach (var axis in YAxes)
                ZoomAxis(axis, flags, pivot.Y, direction, scaleFactor);

        _zoommingDebouncer.Debounce(() => FitAllOnZoom(flags));
    }

    /// <summary>
    /// Pans with the specified delta.
    /// </summary>
    /// <param name="flags">
    /// The flags, for example ZoomAndPanMode.PanX | ZoomAndPanMode.NoFit, will pan only in the x axis
    /// and will ignore the fit to bounds feature.
    /// </param>
    /// <param name="delta">The delta.</param>
    public void Pan(ZoomAndPanMode flags, LvcPoint delta)
    {
        if (flags.HasFlag(ZoomAndPanMode.PanX))
            foreach (var axis in XAxes)
                PanAxis(axis, flags, delta.X, true);

        if (flags.HasFlag(ZoomAndPanMode.PanY))
            foreach (var axis in YAxes)
                PanAxis(axis, flags, delta.Y, true);
    }

    /// <summary>
    /// Starts a zooming section operation at the specified point.
    /// </summary>
    /// <param name="flags">
    /// The flags, for example ZoomAndPanMode.ZoomX | ZoomAndPanMode.NoFit, will zoom only in the x axis
    /// and will ignore the fit to bounds feature.
    /// </param>
    /// <param name="point">The point where the zooming section operation starts.</param>
    public void StartZoomingSection(ZoomAndPanMode flags, LvcPoint point)
    {
        var xMode = flags.HasFlag(ZoomAndPanMode.ZoomX);
        var yMode = flags.HasFlag(ZoomAndPanMode.ZoomY);

        if (flags.HasFlag(ZoomAndPanMode.NoZoomBySection) || (!xMode && !yMode))
            return;

        if (_zoomingSection is null)
            InitializeZoomingSection();

        if (_zoomingSection is null)
            throw new Exception("Something went wrong when initializing the zoomming section.");

        _sectionZoomingStart = point;

        var x = point.X;
        var y = point.Y;

        if (x < DrawMarginLocation.X || x > DrawMarginLocation.X + DrawMarginSize.Width ||
            y < DrawMarginLocation.Y || y > DrawMarginLocation.Y + DrawMarginSize.Height)
        {
            _sectionZoomingStart = null;
            return;
        }

        _zoomingSection.X = x;
        _zoomingSection.Y = y;

        if (!xMode)
        {
            _zoomingSection.X = DrawMarginLocation.X;
            _zoomingSection.Width = DrawMarginSize.Width;
        }

        if (!yMode)
        {
            _zoomingSection.Y = DrawMarginLocation.Y;
            _zoomingSection.Height = DrawMarginSize.Height;
        }
    }

    /// <summary>
    /// Grows the zooming section to the specified point, this updates the zooming section rectangle
    /// in the UI, it does not apply the zoom yet.
    /// </summary>
    /// <param name="flags">
    /// The flags, for example ZoomAndPanMode.ZoomX | ZoomAndPanMode.NoFit, will zoom only in the x axis
    /// and will ignore the fit to bounds feature.
    /// </param>
    /// <param name="point">The point.</param>
    public void GrowZoomingSection(ZoomAndPanMode flags, LvcPoint point)
    {
        if (_zoomingSection is null || _sectionZoomingStart is null) return;

        var xMode = flags.HasFlag(ZoomAndPanMode.ZoomX);
        var yMode = flags.HasFlag(ZoomAndPanMode.ZoomY);

        var x = point.X;
        var y = point.Y;

        if (x < DrawMarginLocation.X) x = DrawMarginLocation.X;
        if (x > DrawMarginLocation.X + DrawMarginSize.Width) x = DrawMarginLocation.X + DrawMarginSize.Width;
        if (y < DrawMarginLocation.Y) y = DrawMarginLocation.Y;
        if (y > DrawMarginLocation.Y + DrawMarginSize.Height) y = DrawMarginLocation.Y + DrawMarginSize.Height;

        if (xMode) _zoomingSection.Width = x - _sectionZoomingStart.Value.X;
        if (yMode) _zoomingSection.Height = y - _sectionZoomingStart.Value.Y;

        if (IsLoaded)
            Canvas.Invalidate();
    }

    /// <summary>
    /// End the zooming section operation at the specified point, and applies the zoom.
    /// </summary>
    /// <param name="flags">
    /// The flags, for example ZoomAndPanMode.ZoomX | ZoomAndPanMode.NoFit, will zoom only in the x axis
    /// and will ignore the fit to bounds feature.
    /// </param>
    /// <param name="point">The point.</param>
    public void EndZoomingSection(ZoomAndPanMode flags, LvcPoint point)
    {
        if (_zoomingSection is null || _sectionZoomingStart is null) return;

        var xy = Math.Sqrt(
            Math.Pow(point.X - _sectionZoomingStart.Value.X, 2) + Math.Pow(point.Y - _sectionZoomingStart.Value.Y, 2));

        if (xy < 15)
        {
            _zoomingSection.X = -1;
            _zoomingSection.Y = -1;
            _zoomingSection.Width = 0;
            _zoomingSection.Height = 0;
            Update();
            _sectionZoomingStart = null;
            return;
        }

        if (flags.HasFlag(ZoomAndPanMode.ZoomX))
            foreach (var axis in XAxes)
                ZoomAxisBySection(axis, point.X);

        if (flags.HasFlag(ZoomAndPanMode.ZoomY))
            foreach (var axis in YAxes)
                ZoomAxisBySection(axis, point.Y);

        _zoomingSection.X = -1;
        _zoomingSection.Y = -1;
        _zoomingSection.Width = 0;
        _zoomingSection.Height = 0;
        _sectionZoomingStart = null;
    }

    /// <summary>
    /// Measures this chart.
    /// </summary>
    /// <returns></returns>
    protected internal override void Measure()
    {
#if DEBUG
        if (LiveCharts.EnableLogging)
        {
            Trace.WriteLine(
                $"[Cartesian chart measured]".PadRight(60) +
                $"geometries: {Canvas.CountGeometries()}    " +
                $"thread: {Environment.CurrentManagedThreadId}");
        }
#endif

        if (!IsLoaded || !IsRendering())
            return;

        InvokeOnMeasuring();

        if (_preserveFirstDraw)
        {
            _isFirstDraw = true;
            _preserveFirstDraw = false;
        }

        #region shallow copy the current data in the view

        var viewDrawMargin = _chartView.DrawMargin;
        ControlSize = _chartView.ControlSize;

        var x = GetAxesCollection(_chartView.XAxes, ref _virtualX);
        var y = GetAxesCollection(_chartView.YAxes, ref _virtualY);

        XAxes = [.. x.Select(x => x.ChartElementSource).Cast<ICartesianAxis>()];
        YAxes = [.. y.Select(x => x.ChartElementSource).Cast<ICartesianAxis>()];

        _zoomingSpeed = _chartView.ZoomingSpeed;

        var theme = GetTheme();

        LegendPosition = _chartView.LegendPosition;
        Legend = _chartView.Legend;

        TooltipPosition = _chartView.TooltipPosition;
        FindingStrategy = _chartView.FindingStrategy;
        Tooltip = _chartView.Tooltip;

        Sections = _chartView.Sections?.Select(x => x.ChartElementSource).Where(static x => x.IsVisible) ?? [];
        VisualElements = _chartView.VisualElements ?? [];

        ActualAnimationsSpeed = _chartView.AnimationsSpeed == TimeSpan.MaxValue
            ? theme.AnimationsSpeed
            : _chartView.AnimationsSpeed;
        ActualEasingFunction = _chartView.EasingFunction == EasingFunctions.Unset
            ? theme.EasingFunction
            : _chartView.EasingFunction;

        #endregion

        SeriesContext = new SeriesContext(VisibleSeries, this);
        var themeId = theme.ThemeId;

        // restart axes bounds and meta data
        foreach (var axis in XAxes)
        {
            var ce = axis.ChartElementSource;
            ce._isInternalSet = true;
            axis.OnMeasureStarted(this, AxisOrientation.X);
            if (ce._theme != themeId)
            {
                theme.ApplyStyleToAxis(axis);
                ce._theme = themeId;
            }
            ce._isInternalSet = false;
            if (axis.CrosshairPaint is not null) _crosshair.Add(axis);
        }
        foreach (var axis in YAxes)
        {
            var ce = axis.ChartElementSource;
            ce._isInternalSet = true;
            axis.OnMeasureStarted(this, AxisOrientation.Y);
            if (ce._theme != themeId)
            {
                theme.ApplyStyleToAxis(axis);
                ce._theme = themeId;
            }
            ce._isInternalSet = false;
            if (axis.CrosshairPaint is not null) _crosshair.Add(axis);
        }

        // get seriesBounds
        SetDrawMargin(ControlSize, new Margin());
        var areAllColumns = true;
        var columnsFlags = SeriesProperties.Bar | SeriesProperties.PrimaryAxisVerticalOrientation;

        foreach (var series in VisibleSeries.Cast<ICartesianSeries>())
        {
            if (series.SeriesId == -1) series.SeriesId = GetNextSeriesId();

            // #1923: pre-register stack positions so each stacked series sees the
            // final Stacker.MaxSeriesId (the largest SeriesId in its stack group)
            // when computing its own actualZIndex during Invalidate. Without this,
            // earlier-iterated series would only see partial peer registration.
            if ((series.SeriesProperties & SeriesProperties.Stacked) == SeriesProperties.Stacked)
                _ = SeriesContext.GetStackPosition(series, series.GetStackGroup());

            var ce = series.ChartElementSource;
            ce._isInternalSet = true;
            if (ce._theme != themeId)
            {
                theme.ApplyStyleToSeries(series);
                ce._theme = themeId;
            }

            var xAxis = GetXAxis(series);
            var yAxis = GetYAxis(series);

            var seriesBounds = series.GetBounds(this, xAxis, yAxis).Bounds;
            if (seriesBounds.IsEmpty)
            {
                ce._isInternalSet = false;
                continue;
            }

            AppendLimits(xAxis, yAxis, seriesBounds);
            areAllColumns &= (series.SeriesProperties & columnsFlags) == columnsFlags;

            ce._isInternalSet = false;
        }

        #region empty bounds

        // prevent the bounds are not empty...

        foreach (var axis in XAxes)
        {
            var ce = axis.ChartElementSource;
            ce._isInternalSet = true;

            if (!axis.DataBounds.IsEmpty)
            {
                ce._isInternalSet = false;
                continue;
            }

            var min = 0;
            var max = 10d * axis.UnitWidth;

            axis.DataBounds.AppendValue(max);
            axis.DataBounds.AppendValue(min);
            axis.VisibleDataBounds.AppendValue(max);
            axis.VisibleDataBounds.AppendValue(min);

            if (axis.DataBounds.MinDelta < max) axis.DataBounds.MinDelta = max;
            ce._isInternalSet = false;
        }
        foreach (var axis in YAxes)
        {
            var ce = axis.ChartElementSource;
            ce._isInternalSet = true;

            if (!axis.DataBounds.IsEmpty)
            {
                ce._isInternalSet = false;
                continue;
            }

            var min = 0;
            var max = 10d * axis.UnitWidth;

            axis.DataBounds.AppendValue(max);
            axis.DataBounds.AppendValue(min);
            axis.VisibleDataBounds.AppendValue(max);
            axis.VisibleDataBounds.AppendValue(min);

            if (axis.DataBounds.MinDelta < max) axis.DataBounds.MinDelta = max;
            ce._isInternalSet = false;
        }

        #endregion

        InitializeVisualsCollector();

        // measure and draw title.
        var m = new Margin();
        float ts = 0f, bs = 0f, ls = 0f, rs = 0f;
        // X axes with InLineNamePlacement reserve horizontal space for the name on the
        // left of their row (and half the leftmost label so it doesn't bleed into the
        // name). Tracked separately so we can apply it AFTER the Y axis loop, which
        // would otherwise overwrite m.Left/m.Right with smaller values.
        float xInlineLeftReserve = 0f, xInlineRightReserve = 0f;
        if (View.Title is not null)
        {
            var titleSize = MeasureTitle();
            m.Top = titleSize.Height;
            ts = titleSize.Height;
            _titleHeight = titleSize.Height;
        }

        // measure and draw legend.
        DrawLegend(ref ts, ref bs, ref ls, ref rs);

        m.Top = ts;
        m.Bottom = bs;
        m.Left = ls;
        m.Right = rs;

        SetDrawMargin(ControlSize, m);

        foreach (var axis in XAxes)
        {
            if (!axis.IsVisible) continue;

            if (axis.DataBounds.Max == axis.DataBounds.Min)
            {
                var c = axis.UnitWidth * 0.5;
                axis.DataBounds.Min = axis.DataBounds.Min - c;
                axis.DataBounds.Max = axis.DataBounds.Max + c;
                axis.VisibleDataBounds.Min = axis.VisibleDataBounds.Min - c;
                axis.VisibleDataBounds.Max = axis.VisibleDataBounds.Max + c;
            }

            var ns = axis.GetNameLabelSize(this);
            var s = axis.GetPossibleSize(this);
            axis.Size = s;

            if (axis.Position == AxisPosition.Start)
            {
                if (axis.InLineNamePlacement)
                {
                    var h = s.Height > ns.Height ? s.Height : ns.Height;

                    // X Bottom
                    axis.NameDesiredSize = new LvcRectangle(
                        new LvcPoint(0, ControlSize.Height - bs - h), new LvcSize(ns.Width, h));
                    axis.LabelsDesiredSize = new LvcRectangle(
                        new LvcPoint(0, axis.NameDesiredSize.Y - h), new LvcSize(ControlSize.Width, s.Height));

                    axis.Yo = m.Bottom + h * 0.5f;

                    // Inline placement renders name and labels in the same row of height h,
                    // so accumulate by h (not s.Height + ns.Height as in the stacked layout).
                    bs += h;
                    m.Bottom = bs;

                    var leftReserve = ns.Width + s.Width * 0.5f;
                    if (leftReserve > xInlineLeftReserve) xInlineLeftReserve = leftReserve;
                    if (s.Width * 0.5f > xInlineRightReserve) xInlineRightReserve = s.Width * 0.5f;
                }
                else
                {
                    // X Bottom
                    axis.NameDesiredSize = new LvcRectangle(
                        new LvcPoint(0, ControlSize.Height - bs - ns.Height), new LvcSize(ControlSize.Width, ns.Height));
                    axis.LabelsDesiredSize = new LvcRectangle(
                        new LvcPoint(0, axis.NameDesiredSize.Y - s.Height), new LvcSize(ControlSize.Width, s.Height));

                    axis.Yo = m.Bottom + s.Height * 0.5f + ns.Height;
                    bs += s.Height + ns.Height;
                    m.Bottom = bs;
                    if (s.Width * 0.5f > m.Left) m.Left = s.Width * 0.5f;
                    if (s.Width * 0.5f > m.Right) m.Right = s.Width * 0.5f;
                }
            }
            else
            {
                if (axis.InLineNamePlacement)
                {
                    var h = s.Height > ns.Height ? s.Height : ns.Height;

                    // X Top
                    axis.NameDesiredSize = new LvcRectangle(
                        new LvcPoint(0, ts), new LvcSize(ns.Width, h));
                    axis.LabelsDesiredSize = new LvcRectangle(
                        new LvcPoint(0, axis.NameDesiredSize.Y - h), new LvcSize(ControlSize.Width, s.Height));

                    axis.Yo = m.Top + h * 0.5f;

                    // Inline placement renders name and labels in the same row of height h,
                    // so accumulate by h (not s.Height + ns.Height as in the stacked layout).
                    ts += h;
                    m.Top = ts;

                    var leftReserve = ns.Width + s.Width * 0.5f;
                    if (leftReserve > xInlineLeftReserve) xInlineLeftReserve = leftReserve;
                    if (s.Width * 0.5f > xInlineRightReserve) xInlineRightReserve = s.Width * 0.5f;
                }
                else
                {
                    // X Top
                    axis.NameDesiredSize = new LvcRectangle(
                       new LvcPoint(0, ts), new LvcSize(ControlSize.Width, ns.Height));
                    axis.LabelsDesiredSize = new LvcRectangle(
                        new LvcPoint(0, ts + ns.Height), new LvcSize(ControlSize.Width, s.Height));

                    axis.Yo = ts + s.Height * 0.5f + ns.Height;
                    ts += s.Height + ns.Height;
                    m.Top = ts;
                    if (ls + s.Width * 0.5f > m.Left) m.Left = ls + s.Width * 0.5f;
                    if (rs + s.Width * 0.5f > m.Right) m.Right = rs + s.Width * 0.5f;
                }
            }
        }
        foreach (var axis in YAxes)
        {
            if (!axis.IsVisible) continue;

            if (axis.DataBounds.Max == axis.DataBounds.Min)
            {
                var c = axis.UnitWidth * 0.5;
                axis.DataBounds.Min = axis.DataBounds.Min - c;
                axis.DataBounds.Max = axis.DataBounds.Max + c;
                axis.VisibleDataBounds.Min = axis.VisibleDataBounds.Min - c;
                axis.VisibleDataBounds.Max = axis.VisibleDataBounds.Max + c;
            }

            var ns = axis.GetNameLabelSize(this);
            var s = axis.GetPossibleSize(this);
            axis.Size = s;
            var w = s.Width;

            if (axis.Position == AxisPosition.Start)
            {
                if (axis.InLineNamePlacement)
                {
                    if (w < ns.Width) w = ns.Width;

                    // Y Left
                    axis.NameDesiredSize = new LvcRectangle(new LvcPoint(ls, 0), new LvcSize(ns.Width, ns.Height));
                    axis.LabelsDesiredSize = new LvcRectangle(new LvcPoint(ls, 0), new LvcSize(s.Width, ControlSize.Height));

                    axis.Xo = ls + w * 0.5f;
                    ls += w;
                    m.Top = ns.Height;
                    m.Left = ls;
                }
                else
                {
                    // Y Left
                    axis.NameDesiredSize = new LvcRectangle(
                        new LvcPoint(ls, 0), new LvcSize(ns.Width, ControlSize.Height));
                    axis.LabelsDesiredSize = new LvcRectangle(
                        new LvcPoint(ls + ns.Width, 0), new LvcSize(s.Width, ControlSize.Height));

                    axis.Xo = ls + w * 0.5f + ns.Width;
                    ls += w + ns.Width;
                    m.Left = ls;

                    if (s.Height * 0.5f > m.Top) m.Top = s.Height * 0.5f;
                    if (s.Height * 0.5f > m.Bottom) m.Bottom = s.Height * 0.5f;
                }
            }
            else
            {
                if (axis.InLineNamePlacement)
                {
                    if (w < ns.Width) w = ns.Width;

                    // Y Left
                    axis.NameDesiredSize = new LvcRectangle(
                        new LvcPoint(ControlSize.Width - rs - ns.Width, 0), new LvcSize(ns.Width, ns.Height));
                    axis.LabelsDesiredSize = new LvcRectangle(
                        new LvcPoint(axis.NameDesiredSize.X - s.Width, 0), new LvcSize(s.Width, ControlSize.Height));

                    axis.Xo = rs + w * 0.5f;
                    rs += w;
                    m.Top = ns.Height;
                    m.Right = rs;
                }
                else
                {
                    // Y Right
                    axis.NameDesiredSize = new LvcRectangle(
                        new LvcPoint(ControlSize.Width - rs - ns.Width, 0), new LvcSize(ns.Width, ControlSize.Height));
                    axis.LabelsDesiredSize = new LvcRectangle(
                        new LvcPoint(axis.NameDesiredSize.X - s.Width, 0), new LvcSize(s.Width, ControlSize.Height));

                    axis.Xo = rs + w * 0.5f + ns.Width;
                    rs += w + ns.Width;
                    m.Right = rs;

                    if (s.Height * 0.5f > m.Top) m.Top = s.Height * 0.5f;
                    if (s.Height * 0.5f > m.Bottom) m.Bottom = s.Height * 0.5f;
                }
            }
        }

        // Apply X-axis inline name reservations now so the Y axis loop's m.Left/m.Right
        // assignments can't shrink the chart back into the X axis name area.
        if (xInlineLeftReserve > m.Left) m.Left = xInlineLeftReserve;
        if (xInlineRightReserve > m.Right) m.Right = xInlineRightReserve;

        var rm = viewDrawMargin ?? new Margin(Margin.Auto);

        var actualMargin = new Margin(
            Margin.IsAuto(rm.Left) ? m.Left : rm.Left,
            Margin.IsAuto(rm.Top) ? m.Top : rm.Top,
            Margin.IsAuto(rm.Right) ? m.Right : rm.Right,
            Margin.IsAuto(rm.Bottom) ? m.Bottom : rm.Bottom);

        SetDrawMargin(ControlSize, actualMargin);

        // invalid dimensions, probably the chart is too small
        // or it is initializing in the UI and has no dimensions yet
        if (DrawMarginSize.Width <= 0 || DrawMarginSize.Height <= 0) return;

        DrawMarginDefined?.Invoke(this);

        if (View.Title is not null) AddTitleToChart();

        var totalAxes = XAxes.Concat(YAxes);

        foreach (var axis in totalAxes)
        {
            if (axis.DataBounds.Max == axis.DataBounds.Min)
            {
                var c = axis.DataBounds.Min * 0.3;
                axis.DataBounds.Min = axis.DataBounds.Min - c;
                axis.DataBounds.Max = axis.DataBounds.Max + c;
            }

            // apply padding
            if (axis.MinLimit is null)
            {
                var s = new Scaler(DrawMarginLocation, DrawMarginSize, axis);
                // correction by geometry size
                var p = Math.Abs(s.ToChartValues(axis.DataBounds.RequestedGeometrySize) - s.ToChartValues(0));
                if (axis.DataBounds.PaddingMin > p) p = axis.DataBounds.PaddingMin;
                var ce = axis.ChartElementSource;
                ce._isInternalSet = true;
                axis.DataBounds.Min = axis.DataBounds.Min - p;
                axis.VisibleDataBounds.Min = axis.VisibleDataBounds.Min - p;

                if (areAllColumns && axis.Orientation == AxisOrientation.Y &&
                    axis.DataBounds.Min + p >= 0 && axis.DataBounds.Max + p >= 0)
                {
                    // exception when all columns and positive
                    if (axis.VisibleDataBounds.Min < 0)
                        axis.VisibleDataBounds.Min = 0;
                }

                ce._isInternalSet = false;
            }

            // apply padding
            if (axis.MaxLimit is null)
            {
                var s = new Scaler(DrawMarginLocation, DrawMarginSize, axis);
                // correction by geometry size
                var p = Math.Abs(s.ToChartValues(axis.DataBounds.RequestedGeometrySize) - s.ToChartValues(0));
                if (axis.DataBounds.PaddingMax > p) p = axis.DataBounds.PaddingMax;
                var ce = axis.ChartElementSource;
                ce._isInternalSet = true;
                axis.DataBounds.Max = axis.DataBounds.Max + p;
                axis.VisibleDataBounds.Max = axis.VisibleDataBounds.Max + p;

                if (areAllColumns && axis.Orientation == AxisOrientation.Y &&
                    axis.DataBounds.Min - p <= 0 && axis.DataBounds.Max - p <= 0)
                {
                    // exception when all columns and negative
                    if (axis.VisibleDataBounds.Min > 0)
                        axis.VisibleDataBounds.Min = 0;
                }

                ce._isInternalSet = false;
            }

            if (axis.IsVisible)
            {
                AddVisual(axis.ChartElementSource);
                axis.InvalidateCrosshair(this, _pointerPosition);
            }
            axis.ChartElementSource.RemoveOldPaints(View); // <- this is probably obsolete.
            // the probable issue is the "IsVisible" property
        }

        RegisterClipZones();

        // we draw all the series even invisible because it animates the series when hidden.
        // Sections and Visuals are not animated when hidden, thus we just skip them.
        // it means that invisible series have a performance impact, it should not be a big deal
        // but ideally, do not keep invisible series in the chart, instead, add/remove them when needed.

        foreach (var section in Sections.Where(static x => x.IsVisible)) AddVisual(section);
        foreach (var visual in VisualElements.Where(static x => x.IsVisible)) AddVisual(visual);
        foreach (var series in Series)
        {
            AddVisual(series.ChartElementSource);
            _drawnSeries.Add(series.SeriesId);
        }

        var actualDrawMarginFrame = _chartView.DrawMarginFrame?.ChartElementSource
            ?? theme.DrawMarginFrameGetter?.Invoke();

        if (_previousDrawMarginFrame is not null && _chartView.DrawMarginFrame != _previousDrawMarginFrame)
        {
            // probably obsolete?
            // this should be handled by the RegisterAndInvalidateVisual() method.
            _previousDrawMarginFrame.RemoveFromUI(this);
            _previousDrawMarginFrame = null;
        }
        if (actualDrawMarginFrame is not null)
        {
            var ce = actualDrawMarginFrame;
            if (ce._theme != themeId)
            {
                ce._isInternalSet = true;
                theme.ApplyStyleToDrawMarginFrame((CoreDrawMarginFrame)ce);
                ce._theme = themeId;
                ce._isInternalSet = false;
            }

            if (actualDrawMarginFrame.IsVisible) AddVisual(actualDrawMarginFrame);
            _previousDrawMarginFrame = ce;
        }

        CollectVisuals();

        foreach (var axis in totalAxes)
        {
            if (!axis.IsVisible) continue;

            var ce = axis.ChartElementSource;
            ce._isInternalSet = true;
            ce._isInternalSet = false;
        }

        InvokeOnUpdateStarted();

        if (_isToolTipOpen) _ = DrawToolTip();

        if (IsLoaded)
            Canvas.Invalidate();

        _isFirstDraw = false;
    }

    /// <inheritdoc cref="ICartesianChartView.ScalePixelsToData(LvcPointD, int, int)"/>
    public LvcPointD ScalePixelsToData(LvcPointD point, int xAxisIndex = 0, int yAxisIndex = 0)
    {
        var xScaler = new Scaler(DrawMarginLocation, DrawMarginSize, XAxes[xAxisIndex]);
        var yScaler = new Scaler(DrawMarginLocation, DrawMarginSize, YAxes[yAxisIndex]);

        return new LvcPointD { X = xScaler.ToChartValues(point.X), Y = yScaler.ToChartValues(point.Y) };
    }

    /// <inheritdoc cref="ICartesianChartView.ScaleDataToPixels(LvcPointD, int, int)"/>
    public LvcPointD ScaleDataToPixels(LvcPointD point, int xAxisIndex = 0, int yAxisIndex = 0)
    {
        var xScaler = new Scaler(DrawMarginLocation, DrawMarginSize, XAxes[xAxisIndex]);
        var yScaler = new Scaler(DrawMarginLocation, DrawMarginSize, YAxes[yAxisIndex]);

        return new LvcPointD { X = xScaler.ToPixels(point.X), Y = yScaler.ToPixels(point.Y) };
    }

    /// <summary>
    /// Gets the x axis for the specified series.
    /// </summary>
    /// <param name="index">The index.</param>
    public ICartesianAxis GetXAxis(int index)
        // we ensure it is in the axes collection bounds, this is just to
        // prevent crashes on hot-reload scenarios.
        => XAxes[index > XAxes.Length - 1 ? 0 : index];

    /// <summary>
    /// Gets the y axis for the specified series.
    /// </summary>
    /// <param name="index">The index.</param>
    public ICartesianAxis GetYAxis(int index)
        // we ensure it is in the axes collection bounds, this is just to
        // prevent crashes on hot-reload scenarios.
        => YAxes[index > YAxes.Length - 1 ? 0 : index];

    /// <summary>
    /// Gets the x axis for the specified series.
    /// </summary>
    /// <param name="series">The series.</param>
    public ICartesianAxis GetXAxis(ICartesianSeries series) => GetXAxis(series.ScalesXAt);

    /// <summary>
    /// Gets the y axis for the specified series.
    /// </summary>
    /// <param name="series">The series.</param>
    public ICartesianAxis GetYAxis(ICartesianSeries series) => GetYAxis(series.ScalesYAt);

    /// <inheritdoc cref="Chart.Unload"/>
    public override void Unload()
    {
        base.Unload();
        _virtualX = null;
        _virtualY = null;
        _crosshair = [];
        _sharedEvents = null;
        _zoomingSection = null;
        _isFirstDraw = true;
        _zoommingDebouncer.Dispose();
    }

    private LvcPoint? _sectionZoomingStart = null;

    /// <summary>
    /// Invokes the pointer down.
    /// </summary>
    /// <param name="point">The pointer position.</param>
    /// <param name="isSecondaryAction">Flags the action as secondary (normally rigth click or double tap on mobile)</param>
    protected internal override void InvokePointerDown(LvcPoint point, bool isSecondaryAction)
    {
        var caretesianView = (ICartesianChartView)View;
        if (caretesianView.ZoomMode.HasFlag(ZoomAndPanMode.InvertPanningPointerTrigger))
            isSecondaryAction = !isSecondaryAction;

        if (isSecondaryAction)
        {
            StartZoomingSection(_chartView.ZoomMode, point);
            return;
        }

        base.InvokePointerDown(point, isSecondaryAction);
    }

    /// <summary>
    /// Invokes the pointer move.
    /// </summary>
    /// <param name="point">The pointer position.</param>
    protected internal override void InvokePointerMove(LvcPoint point)
    {
        InvalidateCrosshairs(this, point);

        if (_sectionZoomingStart is not null)
        {
            GrowZoomingSection(_chartView.ZoomMode, point);
            return;
        }

        base.InvokePointerMove(point);
    }

    /// <summary>
    /// Invokes the pointer up.
    /// </summary>
    /// <param name="point">The pointer position.</param>
    /// <param name="isSecondaryAction">Flags the action as secondary (normally rigth click or double tap on mobile)</param>
    protected internal override void InvokePointerUp(LvcPoint point, bool isSecondaryAction)
    {
        var flags = _chartView.ZoomMode;

        if (_sectionZoomingStart is not null)
        {
            EndZoomingSection(flags, point);
            return;
        }

        // bounce back on pointer up
        var fits = !flags.HasFlag(ZoomAndPanMode.NoFit);
        if (fits)
        {
            if (flags.HasFlag(ZoomAndPanMode.PanX))
                foreach (var axis in XAxes)
                    PanAxis(axis, flags, 0, false);
            if (flags.HasFlag(ZoomAndPanMode.PanY))
                foreach (var axis in YAxes)
                    PanAxis(axis, flags, 0, false);
        }

        base.InvokePointerUp(point, isSecondaryAction);
    }

    /// <inheritdoc cref="InvokePointerLeft"/>
    protected internal override void InvokePointerLeft()
    {
        OnPointerLeft();

        //propagate to shared charts
        foreach (var sharedChart in _sharedEvents ?? [])
        {
            if (sharedChart == this) continue;

            sharedChart.OnPointerLeft();
        }
    }

    internal void ClearPointerDown()
    {
        _isPanning = false;
        _isPointerDown = false;
        _sectionZoomingStart = null;
    }

    internal override bool IsPanEnabled =>
        (_chartView.ZoomMode & (ZoomAndPanMode.PanX | ZoomAndPanMode.PanY)) != 0;

    internal void SubscribeSharedEvents(HashSet<CartesianChartEngine> instance)
    {
        // An experimental feature, it allows a chart to propagate some events to other charts,
        // this feature was created to share crosshairs between multiple charts.

        _sharedEvents = instance;
        _ = _sharedEvents.Add(this);
    }

    private void FitAllOnZoom(ZoomAndPanMode flags)
    {
        // Honor the flags passed to the public Zoom(...) call rather than the view's
        // ZoomMode. Manual callers (e.g. a button-driven zoom) need NoFit to take
        // effect even when the view's ZoomMode does not include it.
        if (flags.HasFlag(ZoomAndPanMode.NoFit))
            return;

        void Fit(ICartesianAxis axis)
        {
            var scale = new Scaler(DrawMarginLocation, DrawMarginSize, axis);

            var geometryOffset = GetGeometryOffset(axis, scale);

            var limits = axis.GetLimits();

            var min = axis.MinLimit ?? limits.DataMin;
            var max = axis.MaxLimit ?? limits.DataMax;

            if (min < limits.DataMin)
                min = limits.DataMin - geometryOffset;

            if (max > limits.DataMax)
                max = limits.DataMax + geometryOffset;

            axis.SetLimits(min, max);
        }

        if (flags.HasFlag(ZoomAndPanMode.ZoomX))
            foreach (var axis in XAxes)
                Fit(axis);

        if (flags.HasFlag(ZoomAndPanMode.ZoomY))
            foreach (var axis in YAxes)
                Fit(axis);
    }

    private void ZoomAxis(
        ICartesianAxis axis, ZoomAndPanMode flags, float pivot, ZoomDirection direction, double? scaleFactor = null)
    {
        var fits = !flags.HasFlag(ZoomAndPanMode.NoFit);
        var speed = _zoomingSpeed < 0.1 ? 0.1 : (_zoomingSpeed > 0.95 ? 0.95 : _zoomingSpeed);
        speed = 1 - speed;
        var m = direction == ZoomDirection.ZoomIn ? speed : 1 / speed;
        var scale = new Scaler(DrawMarginLocation, DrawMarginSize, axis);
        var pivotPixels = scale.ToChartValues(pivot);

        var limits = axis.GetLimits();

        var max = limits.Max;
        var min = limits.Min;

        double mint, maxt;
        var l = max - min;

        if (scaleFactor is null)
        {
            var rMin = (pivotPixels - min) / l;
            var rMax = 1 - rMin;

            var target = l * m;

            mint = pivotPixels - target * rMin;
            maxt = pivotPixels + target * rMax;
        }
        else
        {
            var delta = 1 - scaleFactor.Value;
            int dir;

            if (delta < 0)
            {
                dir = -1;
                direction = ZoomDirection.ZoomIn;
            }
            else
            {
                dir = 1;
                direction = ZoomDirection.ZoomOut;
            }

            var ld = l * Math.Abs(delta);
            mint = min - ld * 0.5 * dir;
            maxt = max + ld * 0.5 * dir;
        }

        if (direction == ZoomDirection.ZoomIn)
        {
            if (maxt - mint < limits.MinDelta)
                return;
        }
        else
        {
            var threshold = GetThreshold(axis, scale);

            if (fits && mint < limits.DataMin - threshold)
                mint = limits.DataMin - threshold;

            if (fits && maxt > limits.DataMax + threshold)
                maxt = limits.DataMax + threshold;
        }

        if (maxt < mint)
            (maxt, mint) = (mint, maxt); // is this needed?

        axis.SetLimits(mint, maxt);
    }

    private void ZoomAxisBySection(ICartesianAxis axis, float delta)
    {
        if (_sectionZoomingStart is null) return;

        var scaler = new Scaler(DrawMarginLocation, DrawMarginSize, axis);

        var value = axis.Orientation == AxisOrientation.X
            ? _sectionZoomingStart.Value.X
            : _sectionZoomingStart.Value.Y;

        var i = scaler.ToChartValues(value);
        var j = scaler.ToChartValues(delta);

        double max, min;

        if (i > j)
        {
            max = i;
            min = j;
        }
        else
        {
            max = j;
            min = i;
        }

        if (max > (axis.MaxLimit ?? double.MaxValue)) max = axis.MaxLimit ?? double.MaxValue;
        if (min < (axis.MinLimit ?? double.MinValue)) min = axis.MinLimit ?? double.MinValue;

        var minDelta = axis.MinZoomDelta ?? axis.DataBounds.MinDelta * 3;

        if (max - min > minDelta)
        {
            axis.SetLimits(min, max);
        }
        else
        {
            if (axis.MaxLimit is not null && axis.MinLimit is not null)
            {
                var d = max - min;
                var ad = axis.MaxLimit.Value - axis.MinLimit.Value;
                var c = (ad - d) * 0.5;

                axis.SetLimits(min - c, max + c);
            }
        }
    }

    private void PanAxis(ICartesianAxis axis, ZoomAndPanMode flags, float delta, bool thresholded)
    {
        var scale = new Scaler(DrawMarginLocation, DrawMarginSize, axis);
        var fits = !flags.HasFlag(ZoomAndPanMode.NoFit);

        var deltapixels = scale.ToChartValues(0) - scale.ToChartValues(delta);

        var limits = axis.GetLimits();

        var length = limits.Max - limits.Min;

        var max = limits.Max + deltapixels;
        var min = limits.Min + deltapixels;

        var threshold = thresholded ? GetThreshold(axis, scale) : 0;

        if (fits && min < limits.DataMin)
        {
            var thresholdedMin = min < limits.DataMin - threshold
                ? limits.DataMin - threshold
                : min;

            var geometryOffset = GetGeometryOffset(axis, scale);

            min = thresholdedMin - geometryOffset;
            max = thresholdedMin - geometryOffset + length;
        }

        if (fits && max > limits.DataMax)
        {
            var thresholdedMax = max > limits.DataMax + threshold
                ? limits.DataMax + threshold
                : max;

            var geometryOffset = GetGeometryOffset(axis, scale);

            max = thresholdedMax + geometryOffset;
            min = thresholdedMax + geometryOffset - length;
        }

        axis.SetLimits(min, max);
    }

    private void RegisterClipZones()
    {
        var size = ControlSize;

        var x = DrawMarginLocation.X;
        var y = DrawMarginLocation.Y;
        var w = DrawMarginSize.Width;
        var h = DrawMarginSize.Height;

        Canvas.Zones[CanvasZone.NoClip].Clip = LvcRectangle.Empty;
        Canvas.Zones[CanvasZone.DrawMargin].Clip = new(new(x, y), new(w, h));
        Canvas.Zones[CanvasZone.XCrosshair].Clip = new(new(x, 0), new(w, size.Height));
        Canvas.Zones[CanvasZone.YCrosshair].Clip = new(new(0, y), new(size.Width, h));
    }

    private double GetThreshold(ICartesianAxis axis, Scaler scale)
    {
        var bouncingDistancePixels = axis.Orientation == AxisOrientation.X
            ? DrawMarginSize.Width * axis.BouncingDistance
            : DrawMarginSize.Height * axis.BouncingDistance;

        return Math.Abs(scale.ToChartValues(bouncingDistancePixels) - scale.ToChartValues(0));
    }

    private double GetGeometryOffset(ICartesianAxis axis, Scaler scale) =>
        Math.Abs(scale.ToChartValues(axis.DataBounds.RequestedGeometrySize) - scale.ToChartValues(0));

    private IEnumerable<ICartesianAxis> GetAxesCollection(
        IEnumerable<ICartesianAxis>? viewAxes,
        ref ICartesianAxis[]? virtualAxes)
    {
        if (viewAxes is not null && viewAxes.Any()) return viewAxes;

        if (virtualAxes is null)
        {
            var provider = LiveCharts.DefaultSettings.GetProvider();
            var virtualAxis = provider.GetDefaultCartesianAxis();
            virtualAxis.PropertyChanged += (s, e) => Update();
            virtualAxes = [virtualAxis];
        }

        return virtualAxes;
    }

    private void OnPointerLeft() => base.InvokePointerLeft();

    private void InvalidateCrosshairs(Chart chart, LvcPoint point)
    {
        foreach (var axis in _crosshair)
            axis.InvalidateCrosshair(chart, point);

        //propagate to shared charts
        foreach (var sharedChart in _sharedEvents ?? [])
        {
            if (sharedChart == this) continue;

            foreach (var axis in sharedChart._crosshair)
                axis.InvalidateCrosshair(sharedChart, point);
        }
    }

    private void InitializeZoomingSection()
    {
        var provider = LiveCharts.DefaultSettings.GetProvider();
        _zoomingSection = provider.InitializeZoommingSection(Canvas);

        _zoomingSection.X = -1;
        _zoomingSection.Y = -1;
        _zoomingSection.Width = 0;
        _zoomingSection.Height = 0;
    }

    private static void AppendLimits(ICartesianAxis x, ICartesianAxis y, DimensionalBounds bounds)
    {
        x.DataBounds.AppendValue(bounds.SecondaryBounds);
        x.VisibleDataBounds.AppendValue(bounds.VisibleSecondaryBounds);
        y.DataBounds.AppendValue(bounds.PrimaryBounds);
        y.VisibleDataBounds.AppendValue(bounds.VisiblePrimaryBounds);

        foreach (var sharedX in x.SharedWith ?? [])
        {
            sharedX.DataBounds.AppendValue(bounds.SecondaryBounds);
            sharedX.VisibleDataBounds.AppendValue(bounds.VisibleSecondaryBounds);
        }

        foreach (var sharedY in y.SharedWith ?? [])
        {
            sharedY.DataBounds.AppendValue(bounds.PrimaryBounds);
            sharedY.VisibleDataBounds.AppendValue(bounds.VisiblePrimaryBounds);
        }
    }
}
