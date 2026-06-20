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
using LiveChartsCore.Kernel.Helpers;
using LiveChartsCore.Kernel.Providers;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.Motion;
using LiveChartsCore.Painting;

namespace LiveChartsCore;

/// <summary>
/// Defines an Axis in a Cartesian chart.
/// </summary>
/// <typeparam name="TTextGeometry">The type of the text geometry.</typeparam>
/// <typeparam name="TLineGeometry">The type of the line geometry.</typeparam>
public abstract class CoreAxis<TTextGeometry, TLineGeometry>
    : ChartElement, ICartesianAxis, IInternalCartesianAxis, IPlane
        where TTextGeometry : BaseLabelGeometry, new()
        where TLineGeometry : BaseLineGeometry, new()
{
    #region fields

    /// <summary>
    /// The active separators
    /// </summary>
    protected internal readonly Dictionary<Chart, Dictionary<string, AxisVisualSeprator>> activeSeparators = [];

    internal float _xo = 0f, _yo = 0f;
    internal LvcSize _size;
    internal AxisOrientation _orientation;
    internal Bounds _dataBounds = new();
    internal Bounds _visibleDataBounds = new();

    private double _minStep = 0;
    private LvcRectangle _labelsDesiredSize = new(), _nameDesiredSize = new();
    private LvcSize? _possibleMaxLabelsSize = new();
    private TTextGeometry? _nameGeometry;
    private double? _minLimit = null;
    private double? _maxLimit = null;
    private double? _userSetMinLimit = null;
    private double? _userSetMaxLimit = null;
    private bool _isEngineSettingLimits;
    private BaseLineGeometry? _ticksPath;
    private BaseLineGeometry? _zeroLine;
    private BaseLineGeometry? _crosshairLine;
    private BaseLabelGeometry? _crosshairLabel;
    private LvcColor? _crosshairLabelsBackground;
    private bool _forceStepToMin;
    private readonly float _tickLength = 6f;
    internal double? _logBase;
    internal DoubleMotionProperty? _animatableMin;
    internal DoubleMotionProperty? _animatableMax;

    // Per-measure grouping state, supplied by the axis' own TryGroupSeparators (a built-in
    // grouping like DateTimeAxis.GroupTimeUnits) or by the provider's IAxisRenderOverride.
    // Computed once per visible range and cached by it, so re-measuring at the same zoom does not
    // re-consult — and so we never mutate the axis' own CustomSeparators/Labeler, which
    // would notify a property change every frame and loop the measure forever.
    private IEnumerable<double>? _groupedSeparators;
    private Func<double, string>? _groupedLabeler;
    private (double Min, double Max)? _groupedSignature;

    // The alternating-band rectangles (see AlternatingBandsPaint), keyed per chart by the band
    // start value so zoom/pan moves the same instance instead of recreating it.
    private readonly Dictionary<Chart, Dictionary<string, BoundedDrawnGeometry>> _activeBands = [];

    // Whether CreateBandGeometry supplies a geometry — probed once (constant per axis type);
    // a platform without one has the bands disabled regardless of the paint.
    private bool? _createsBandGeometries;

    // Shared by every geometry this axis animates. Mutated in-place on each Invalidate so
    // AnimationsSpeed/EasingFunction changes reach already-created visuals — every MotionProperty
    // holds a reference to this same instance, not a copy.
    private readonly Animation _animation = new(EasingFunctions.QuadraticOut, TimeSpan.Zero);

    #endregion

    #region properties

    float ICartesianAxis.Xo { get => _xo; set => _xo = value; }
    float ICartesianAxis.Yo { get => _yo; set => _yo = value; }
    LvcSize ICartesianAxis.Size { get => _size; set => _size = value; }
    LvcRectangle ICartesianAxis.LabelsDesiredSize { get => _labelsDesiredSize; set => _labelsDesiredSize = value; }
    LvcSize ICartesianAxis.PossibleMaxLabelSize => _possibleMaxLabelsSize ?? (_possibleMaxLabelsSize = GetPossibleMaxLabelSize()).Value;
    LvcRectangle ICartesianAxis.NameDesiredSize { get => _nameDesiredSize; set => _nameDesiredSize = value; }

    /// <inheritdoc cref="IPlane.DataBounds"/>
    public Bounds DataBounds => _dataBounds;

    /// <inheritdoc cref="IPlane.VisibleDataBounds"/>
    public Bounds VisibleDataBounds => _visibleDataBounds;
    double IPlane.MotionMinLimit => _animatableMin?.GetMovement(Animatable.Empty) ?? 0;
    double IPlane.MotionMaxLimit => _animatableMax?.GetMovement(Animatable.Empty) ?? 0;

    /// <inheritdoc cref="IPlane.Name"/>
    public string? Name { get; set => SetProperty(ref field, value); } = null;

    /// <inheritdoc cref="IPlane.NameTextSize"/>
    public double NameTextSize { get; set => SetProperty(ref field, value); } = 20;

    /// <inheritdoc cref="IPlane.NamePadding"/>
    public Padding NamePadding { get; set => SetProperty(ref field, value); } = new(5);

    /// <inheritdoc cref="ICartesianAxis.LabelsAlignment"/>
    public Align? LabelsAlignment { get; set => SetProperty(ref field, value); }

    /// <inheritdoc cref="ICartesianAxis.Orientation"/>
    public AxisOrientation Orientation => _orientation;

    /// <inheritdoc cref="ICartesianAxis.Padding"/>
    public Padding Padding { get; set => SetProperty(ref field, value); } = new();

    /// <inheritdoc cref="ICartesianAxis.LabelsDensity"/>
    public float LabelsDensity { get; set => SetProperty(ref field, value); } = 0.85f;

    /// <inheritdoc cref="IPlane.Labeler"/>
    public Func<double, string> Labeler { get; set => SetProperty(ref field, value); } = Labelers.Default;

    /// <inheritdoc cref="IPlane.MinStep"/>
    public double MinStep { get => _minStep; set => SetProperty(ref _minStep, value); }

    /// <inheritdoc cref="IPlane.ForceStepToMin"/>
    public bool ForceStepToMin { get => _forceStepToMin; set => SetProperty(ref _forceStepToMin, value); }

    /// <inheritdoc cref="IPlane.MinSeparators"/>
    public int MinSeparators { get; set => SetProperty(ref field, value); } = 3;

    /// <inheritdoc cref="IPlane.MinLimit"/>
    public double? MinLimit
    {
        get => _minLimit;
        set
        {
            var filtered = value;

            if (filtered is not null && double.IsNaN(filtered.Value))
                filtered = null;

            // Track the user-set pinning separately from the view min/max.
            // The chart engine's zoom/pan path also lands here (via
            // SetLimits(notify: true)) — that path raises _isEngineSettingLimits
            // so the engine's transient view is NOT mistaken for a user pin.
            // Without this guard the outer rail collapses onto the zoomed-out
            // view and the bounce-back-to-fit is lost (#2159).
            if (!_isEngineSettingLimits)
                _userSetMinLimit = filtered;

            SetProperty(ref _minLimit, filtered);
        }
    }

    /// <inheritdoc cref="IPlane.MaxLimit"/>
    public double? MaxLimit
    {
        get => _maxLimit;
        set
        {
            var filtered = value;

            if (filtered is not null && double.IsNaN(filtered.Value))
                filtered = null;

            if (!_isEngineSettingLimits)
                _userSetMaxLimit = filtered;

            SetProperty(ref _maxLimit, filtered);
        }
    }

    /// <inheritdoc cref="IPlane.UnitWidth"/>
    public double UnitWidth { get; set => SetProperty(ref field, value); } = 1;

    /// <inheritdoc cref="ICartesianAxis.Position"/>
    public AxisPosition Position { get; set => SetProperty(ref field, value); } = AxisPosition.Start;

    /// <inheritdoc cref="IPlane.LabelsRotation"/>
    public double LabelsRotation { get; set => SetProperty(ref field, value); }

    /// <inheritdoc cref="IPlane.TextSize"/>
    public double TextSize { get; set => SetProperty(ref field, value); } = 16;

    /// <inheritdoc cref="IPlane.Labels"/>
    public IList<string>? Labels { get; set; }

    /// <inheritdoc cref="IPlane.ShowSeparatorLines"/>
    public bool ShowSeparatorLines { get; set => SetProperty(ref field, value); } = true;

    /// <inheritdoc cref="IPlane.CustomSeparators"/>
    public IEnumerable<double>? CustomSeparators { get; set => SetProperty(ref field, value); }

    /// <inheritdoc cref="IPlane.IsInverted"/>
    public bool IsInverted { get; set => SetProperty(ref field, value); }

    /// <inheritdoc cref="ICartesianAxis.SeparatorsAtCenter"/>
    public bool SeparatorsAtCenter { get; set => SetProperty(ref field, value); } = true;

    /// <inheritdoc cref="ICartesianAxis.TicksAtCenter"/>
    public bool TicksAtCenter { get; set => SetProperty(ref field, value); } = true;

    /// <inheritdoc cref="IPlane.NamePaint"/>
    public Paint? NamePaint
    {
        get;
        set => SetPaintProperty(ref field, value, PaintStyle.Text);
    }

    /// <inheritdoc cref="IPlane.LabelsPaint"/>
    public Paint? LabelsPaint
    {
        get;
        set => SetPaintProperty(ref field, value, PaintStyle.Text);
    }

    /// <inheritdoc cref="IPlane.SeparatorsPaint"/>
    public Paint? SeparatorsPaint
    {
        get;
        set => SetPaintProperty(ref field, value, PaintStyle.Stroke);
    }

    /// <inheritdoc cref="ICartesianAxis.SubseparatorsPaint"/>
    public Paint? SubseparatorsPaint
    {
        get;
        set => SetPaintProperty(ref field, value, PaintStyle.Stroke);
    }

    /// <inheritdoc cref="ICartesianAxis.AlternatingBandsPaint"/>
    public Paint? AlternatingBandsPaint
    {
        get;
        set => SetPaintProperty(ref field, value);
    }

    /// <inheritdoc cref="ICartesianAxis.SubseparatorsCount"/>
    public int SubseparatorsCount { get; set => SetProperty(ref field, value); } = 3;

    /// <inheritdoc cref="ICartesianAxis.DrawTicksPath"/>
    public bool DrawTicksPath { get; set => SetProperty(ref field, value); }

    /// <inheritdoc cref="ICartesianAxis.TicksPaint"/>
    public Paint? TicksPaint
    {
        get;
        set => SetPaintProperty(ref field, value, PaintStyle.Stroke);
    }

    /// <inheritdoc cref="ICartesianAxis.SubticksPaint"/>
    public Paint? SubticksPaint
    {
        get;
        set => SetPaintProperty(ref field, value, PaintStyle.Stroke);
    }

    /// <inheritdoc cref="ICartesianAxis.ZeroPaint"/>
    public Paint? ZeroPaint
    {
        get;
        set
        {
            SetPaintProperty(ref field, value, PaintStyle.Stroke);

            // clear the reference to thre previous line.
            // so a new instance will be created for the new paint task.
            _zeroLine = null;
        }
    }

    /// <inheritdoc cref="ICartesianAxis.CrosshairPaint"/>
    public Paint? CrosshairPaint
    {
        get;
        set => SetPaintProperty(ref field, value, PaintStyle.Stroke);
    }

    /// <inheritdoc cref="ICartesianAxis.CrosshairLabelsPaint"/>
    public Paint? CrosshairLabelsPaint
    {
        get;
        set => SetPaintProperty(ref field, value, PaintStyle.Text);
    }

    /// <inheritdoc cref="ICartesianAxis.CrosshairLabelsBackground"/>
    public LvcColor? CrosshairLabelsBackground
    {
        get => _crosshairLabelsBackground;
        set => SetProperty(ref _crosshairLabelsBackground, value);
    }

    /// <inheritdoc cref="ICartesianAxis.CrosshairPadding"/>
    public Padding? CrosshairPadding { get; set; }

    /// <inheritdoc cref="ICartesianAxis.CrosshairSnapEnabled" />
    public bool CrosshairSnapEnabled { get; set => SetProperty(ref field, value); }

    /// <inheritdoc cref="IPlane.AnimationsSpeed"/>
    public TimeSpan? AnimationsSpeed { get; set; }

    /// <inheritdoc cref="IPlane.EasingFunction"/>
    public Func<float, float>? EasingFunction { get; set; }

    /// <inheritdoc cref="ICartesianAxis.MinZoomDelta"/>
    public double? MinZoomDelta { get; set; }

    /// <inheritdoc cref="ICartesianAxis.BouncingDistance"/>
    public double BouncingDistance { get; set; } = 0.25;

    /// <inheritdoc cref="ICartesianAxis.InLineNamePlacement"/>
    public bool InLineNamePlacement { get; set => SetProperty(ref field, value); }

    /// <inheritdoc cref="ICartesianAxis.SharedWith"/>
    public IEnumerable<ICartesianAxis>? SharedWith { get; set; }

    double? IInternalCartesianAxis.UserSetMinLimit => _userSetMinLimit;
    double? IInternalCartesianAxis.UserSetMaxLimit => _userSetMaxLimit;

    #endregion

    /// <inheritdoc cref="ICartesianAxis.MeasureStarted"/>
    public event Action<Chart, ICartesianAxis>? MeasureStarted;

    /// <inheritdoc cref="ChartElement.Invalidate(Chart)"/>
    // ---- template method ----------------------------------------------------

    /// <summary>
    /// Builds a per-frame measure context from the chart. Bundles all the
    /// pre-computed values that the orchestration body and its hooks would
    /// otherwise pass around as bare locals.
    /// </summary>
    protected virtual AxisMeasureContext BeginMeasure(CartesianChartEngine chart)
    {
        EnsureGrouped(chart);

        var animation = GetAnimation(chart);

        var controlSize = chart.ControlSize;
        var drawLocation = chart.DrawMarginLocation;
        var drawMarginSize = chart.DrawMarginSize;

        var max = MaxLimit is null ? _visibleDataBounds.Max : MaxLimit.Value;
        var min = MinLimit is null ? _visibleDataBounds.Min : MinLimit.Value;

        AxisLimit.ValidateLimits(ref min, ref max, MinStep);

        if (_animatableMin is null || _animatableMax is null)
        {
            _animatableMin = new DoubleMotionProperty(min) { Animation = animation };
            _animatableMax = new DoubleMotionProperty(max) { Animation = animation };
        }

        _animatableMin.SetMovement(min, Animatable.Empty);
        _animatableMax.SetMovement(max, Animatable.Empty);

        var scale = this.GetNextScaler(chart);
        var actualScale = this.GetActualScaler(chart) ?? scale;
        var labeler = GetActualLabeler();

        var lyi = drawLocation.Y;
        var lyj = drawLocation.Y + drawMarginSize.Height;
        var lxi = drawLocation.X;
        var lxj = drawLocation.X + drawMarginSize.Width;

        float xoo = 0f, yoo = 0f;
        if (_orientation == AxisOrientation.X)
            yoo = Position == AxisPosition.Start ? controlSize.Height - _yo : _yo;
        else
            xoo = Position == AxisPosition.Start ? _xo : controlSize.Width - _xo;

        var size = (float)TextSize;
        var r = (float)LabelsRotation;
        var hasRotation = Math.Abs(r) > 0.01f;

        var hasActivePaint =
            (NamePaint is not null && NamePaint != Paint.Default) || (SeparatorsPaint is not null && SeparatorsPaint != Paint.Default) ||
            (LabelsPaint is not null && LabelsPaint != Paint.Default) || (TicksPaint is not null && TicksPaint != Paint.Default) ||
            (SubticksPaint is not null && SubticksPaint != Paint.Default) || (SubseparatorsPaint is not null && SubseparatorsPaint != Paint.Default);

        float txco = 0f, tyco = 0f, sxco = 0f, syco = 0f;
        var uw = scale.MeasureInPixels(UnitWidth);
        if (!TicksAtCenter && _orientation == AxisOrientation.X) txco = uw * 0.5f;
        if (!TicksAtCenter && _orientation == AxisOrientation.Y) tyco = uw * 0.5f;
        if (!SeparatorsAtCenter && _orientation == AxisOrientation.X) sxco = uw * 0.5f;
        if (!SeparatorsAtCenter && _orientation == AxisOrientation.Y) syco = uw * 0.5f;

        var axisTick = this.GetTick(drawMarginSize, null);
        var s = axisTick.Value;
        if (s < _minStep) s = _minStep;
        if (_forceStepToMin) s = _minStep;

        var start = Math.Truncate(min / s) * s;

        return new AxisMeasureContext(
            chart, scale, actualScale, labeler,
            drawLocation, drawMarginSize, controlSize,
            lxi: lxi, lxj: lxj, lyi: lyi, lyj: lyj,
            xoo: xoo, yoo: yoo,
            labelTextSize: size,
            labelsRotation: r,
            hasRotation: hasRotation,
            hasActivePaint: hasActivePaint,
            min: min, max: max,
            step: s, start: start,
            ticksXOffset: txco, ticksYOffset: tyco,
            separatorsXOffset: sxco, separatorsYOffset: syco,
            orientation: _orientation);
    }

    /// <summary>
    /// Sets per-paint Z-index defaults (only if the user hasn't pre-assigned a Z-index)
    /// and registers each non-default paint as a drawable task in the appropriate canvas
    /// zone. Mirrors the original Invalidate paint-setup block.
    /// </summary>
    private void InitializePaints(in AxisMeasureContext ctx)
    {
        var canvas = ctx.Chart.Canvas;

        if (NamePaint is not null && NamePaint != Paint.Default)
        {
            if (NamePaint.ZIndex == 0) NamePaint.ZIndex = PaintConstants.AxisNamePaintZIndex;
            canvas.AddDrawableTask(NamePaint, zone: CanvasZone.NoClip);
        }
        if (LabelsPaint is not null && LabelsPaint != Paint.Default)
        {
            if (LabelsPaint.ZIndex == 0) LabelsPaint.ZIndex = PaintConstants.AxisLabelsPaintZIndex;
            canvas.AddDrawableTask(LabelsPaint, zone: CanvasZone.NoClip);
        }

        if (SubseparatorsPaint is not null && SubseparatorsPaint != Paint.Default)
        {
            if (SubseparatorsPaint.ZIndex == 0) SubseparatorsPaint.ZIndex = PaintConstants.AxisSubseparatorsPaintZIndex;
            canvas.AddDrawableTask(SubseparatorsPaint, zone: CanvasZone.DrawMargin);
        }
        if (SeparatorsPaint is not null && SeparatorsPaint != Paint.Default)
        {
            if (SeparatorsPaint.ZIndex == 0) SeparatorsPaint.ZIndex = PaintConstants.AxisSeparatorsPaintZIndex;
            canvas.AddDrawableTask(SeparatorsPaint, zone: CanvasZone.DrawMargin);
        }

        var axisZone = ctx.Orientation == AxisOrientation.X ? CanvasZone.XCrosshair : CanvasZone.YCrosshair;
        if (TicksPaint is not null && TicksPaint != Paint.Default)
        {
            if (TicksPaint.ZIndex == 0) TicksPaint.ZIndex = PaintConstants.AxisTicksPaintZIndex;
            canvas.AddDrawableTask(TicksPaint, zone: axisZone);
        }
        if (SubticksPaint is not null && SubticksPaint != Paint.Default)
        {
            if (SubticksPaint.ZIndex == 0) SubticksPaint.ZIndex = PaintConstants.AxisSubticksPaintZIndex;
            canvas.AddDrawableTask(SubticksPaint, zone: axisZone);
        }
    }

    /// <summary>
    /// Creates the zero-line geometry on first call (positioned at the data value 0 and
    /// stamped UpdateAndComplete so it doesn't animate in), then drives a regular
    /// Update on subsequent calls. No-op when ZeroPaint is not configured.
    /// </summary>
    private void EnsureZeroLine(in AxisMeasureContext ctx)
    {
        if (ZeroPaint is null || ZeroPaint == Paint.Default) return;

        var chart = ctx.Chart;
        float x, y;
        if (ctx.Orientation == AxisOrientation.X)
        {
            x = ctx.Scale.ToPixels(0);
            y = ctx.OffsetY;
        }
        else
        {
            x = ctx.OffsetX;
            y = ctx.Scale.ToPixels(0);
        }

        if (ZeroPaint.ZIndex == 0) ZeroPaint.ZIndex = PaintConstants.AxisZeroPaintZIndex;
        chart.Canvas.AddDrawableTask(ZeroPaint, zone: CanvasZone.DrawMargin);

        if (_zeroLine is null)
        {
            _zeroLine = new TLineGeometry();
            InitializeLine(_zeroLine, chart);
            UpdateSeparator(_zeroLine, x, y, ctx.LeftX, ctx.RightX, ctx.TopY, ctx.BottomY, UpdateMode.UpdateAndComplete);
        }
        ZeroPaint.AddGeometryToPaintTask(chart.Canvas, _zeroLine);

        UpdateSeparator(_zeroLine, x, y, ctx.LeftX, ctx.RightX, ctx.TopY, ctx.BottomY, UpdateMode.Update);
    }

    /// <summary>
    /// Drives the axis-line geometry shared by all ticks. Position is the perpendicular
    /// offset from the axis line (TextSize/2 inward) — recomputed every frame rather than
    /// animated, so it snaps to the final position. Detaches the geometry when
    /// DrawTicksPath flips back to false.
    /// </summary>
    private void EnsureTicksPath(in AxisMeasureContext ctx)
    {
        if (TicksPaint is null || TicksPaint == Paint.Default) return;

        var chart = ctx.Chart;

        if (DrawTicksPath)
        {
            if (_ticksPath is null)
            {
                _ticksPath = new TLineGeometry();
                InitializeLine(_ticksPath, chart);
            }
            TicksPaint.AddGeometryToPaintTask(chart.Canvas, _ticksPath);

            if (ctx.Orientation == AxisOrientation.X)
            {
                var yp = ctx.OffsetY + _size.Height * 0.5f * (Position == AxisPosition.Start ? -1 : 1);
                _ticksPath.X = ctx.LeftX;
                _ticksPath.X1 = ctx.RightX;
                _ticksPath.Y = yp;
                _ticksPath.Y1 = yp;
            }
            else
            {
                var xp = ctx.OffsetX + _size.Width * 0.5f * (Position == AxisPosition.Start ? 1 : -1);
                _ticksPath.X = xp;
                _ticksPath.X1 = xp;
                _ticksPath.Y = ctx.TopY;
                _ticksPath.Y1 = ctx.BottomY;
            }

            _ticksPath.CompleteTransition(null);
        }
        else if (_ticksPath is not null)
        {
            TicksPaint.RemoveGeometryFromPaintTask(chart.Canvas, _ticksPath);
        }
    }

    /// <summary>
    /// Per-separator-value body of the EnumerateSeparators loop. Looks up (or creates)
    /// the AxisVisualSeprator for the value, lazily initializes any visual shapes whose
    /// paint is configured but not yet rendered, attaches/detaches paints based on
    /// ShowSeparatorLines, drives all UpdateMode.Update transitions, and marks the
    /// separator as measured this frame so the cleanup pass knows to keep it.
    /// </summary>
    private void MeasureSeparatorAtValue(
        double i,
        Dictionary<string, AxisVisualSeprator> separators,
        HashSet<AxisVisualSeprator> measured,
        in AxisMeasureContext ctx)
    {
        // isOutside is useful because we normally render from min - s to max + s — this
        // ensures subseparators / subticks at the edges still render, while labels for
        // values outside [min, max] are blanked.
        var isOutside = i < ctx.Min || i > ctx.Max;
        var separatorKey = Labelers.SixRepresentativeDigits(i - 1d + 1d);
        var labelContent = isOutside ? string.Empty : TryGetLabelOrLogError(ctx.Labeler, i - 1d + 1d);

        float x, y, xc, yc;
        if (ctx.Orientation == AxisOrientation.X)
        {
            x = ctx.Scale.ToPixels(i);
            y = ctx.OffsetY;
            xc = ctx.ActualScale.ToPixels(i);
            yc = ctx.OffsetY;
        }
        else
        {
            x = ctx.OffsetX;
            y = ctx.Scale.ToPixels(i);
            xc = ctx.OffsetX;
            yc = ctx.ActualScale.ToPixels(i);
        }

        if (!separators.TryGetValue(separatorKey, out var visualSeparator))
        {
            visualSeparator = new AxisVisualSeprator() { Value = i };
            separators.Add(separatorKey, visualSeparator);
        }

        var chart = ctx.Chart;
        var sxco = ctx.SeparatorsXOffset;
        var syco = ctx.SeparatorsYOffset;
        var txco = ctx.TicksXOffset;
        var tyco = ctx.TicksYOffset;

        // Initialize shapes — paints can be added at runtime, so this runs per-frame.
        if (SeparatorsPaint is not null && SeparatorsPaint != Paint.Default && ShowSeparatorLines && visualSeparator.Separator is null)
        {
            InitializeSeparator(visualSeparator, chart);
            UpdateSeparator(
                visualSeparator.Separator!, xc + sxco, yc + syco, ctx.LeftX, ctx.RightX, ctx.TopY, ctx.BottomY,
                UpdateMode.UpdateAndComplete);
        }
        if (SubseparatorsPaint is not null && SubseparatorsPaint != Paint.Default && ShowSeparatorLines &&
            (visualSeparator.Subseparators is null || visualSeparator.Subseparators.Length != SubseparatorsCount))
        {
            // SubseparatorsCount can change at runtime; if the existing array length no longer
            // matches the count we must detach the old geometries from the paint task and
            // rebuild — otherwise UpdateSubseparators iterates `subseparators.Length` and just
            // repositions the stale set via (j+1)/(count+1), producing gaps (issue #2287).
            DetachSubseparators(visualSeparator, chart);
            InitializeSubseparators(visualSeparator, chart);
            UpdateSubseparators(
                visualSeparator.Subseparators!, ctx.ActualScale, ctx.Step, xc + sxco, yc + syco, ctx.LeftX, ctx.RightX, ctx.TopY, ctx.BottomY,
                UpdateMode.UpdateAndComplete);
        }
        if (TicksPaint is not null && TicksPaint != Paint.Default && visualSeparator.Tick is null)
        {
            InitializeTick(visualSeparator, chart);
            UpdateTick(visualSeparator.Tick!, _tickLength, xc + txco, yc + tyco, UpdateMode.UpdateAndComplete);
        }
        if (SubticksPaint is not null && SubticksPaint != Paint.Default && SubseparatorsCount > 0 &&
            (visualSeparator.Subticks is null || visualSeparator.Subticks.Length != SubseparatorsCount))
        {
            DetachSubticks(visualSeparator, chart);
            InitializeSubticks(visualSeparator, chart);
            UpdateSubticks(visualSeparator.Subticks!, ctx.ActualScale, ctx.Step, xc + txco, yc + tyco, UpdateMode.UpdateAndComplete);
        }
        if (LabelsPaint is not null && LabelsPaint != Paint.Default && visualSeparator.Label is null)
        {
            IntializeLabel(visualSeparator, chart, ctx.LabelTextSize, ctx.HasRotation, ctx.LabelsRotation);
            UpdateLabel(
                visualSeparator.Label!, xc, yc, TryGetLabelOrLogError(ctx.Labeler, i - 1d + 1d), ctx.HasRotation, ctx.LabelsRotation,
                UpdateMode.UpdateAndComplete);
        }

        // Attach / detach paints based on visibility toggles.
        if (SeparatorsPaint is not null && SeparatorsPaint != Paint.Default && visualSeparator.Separator is not null)
        {
            if (ShowSeparatorLines)
                SeparatorsPaint.AddGeometryToPaintTask(chart.Canvas, visualSeparator.Separator);
            else
                SeparatorsPaint.RemoveGeometryFromPaintTask(chart.Canvas, visualSeparator.Separator);
        }

        if (SubseparatorsPaint is not null && SubseparatorsPaint != Paint.Default && visualSeparator.Subseparators is not null)
            if (ShowSeparatorLines)
                foreach (var subtick in visualSeparator.Subseparators)
                    SubseparatorsPaint.AddGeometryToPaintTask(chart.Canvas, subtick);
            else
                foreach (var subtick in visualSeparator.Subseparators)
                    SubseparatorsPaint.RemoveGeometryFromPaintTask(chart.Canvas, subtick);

        if (LabelsPaint is not null && LabelsPaint != Paint.Default && visualSeparator.Label is not null)
            LabelsPaint.AddGeometryToPaintTask(chart.Canvas, visualSeparator.Label);
        if (TicksPaint is not null && TicksPaint != Paint.Default && visualSeparator.Tick is not null)
            TicksPaint.AddGeometryToPaintTask(chart.Canvas, visualSeparator.Tick);
        if (SubticksPaint is not null && SubticksPaint != Paint.Default && visualSeparator.Subticks is not null)
            foreach (var subtick in visualSeparator.Subticks)
                SubticksPaint.AddGeometryToPaintTask(chart.Canvas, subtick);

        // Drive transitions toward the next-frame position.
        if (visualSeparator.Separator is not null)
            UpdateSeparator(visualSeparator.Separator, x + sxco, y + syco, ctx.LeftX, ctx.RightX, ctx.TopY, ctx.BottomY, UpdateMode.Update);
        if (visualSeparator.Subseparators is not null)
            UpdateSubseparators(visualSeparator.Subseparators, ctx.Scale, ctx.Step, x + sxco, y + tyco, ctx.LeftX, ctx.RightX, ctx.TopY, ctx.BottomY, UpdateMode.Update);
        if (visualSeparator.Tick is not null)
            UpdateTick(visualSeparator.Tick, _tickLength, x + txco, y + tyco, UpdateMode.Update);
        if (visualSeparator.Subticks is not null)
            UpdateSubticks(visualSeparator.Subticks, ctx.Scale, ctx.Step, x + txco, y + tyco, UpdateMode.Update);
        if (visualSeparator.Label is not null)
        {
            UpdateLabel(visualSeparator.Label, x, y + tyco, labelContent, ctx.HasRotation, ctx.LabelsRotation, UpdateMode.Update);
            visualSeparator.Label.Opacity = isOutside ? 0f : 1f;
        }

        if (ctx.HasActivePaint) _ = measured.Add(visualSeparator);
    }

    /// <summary>
    /// Sweeps over the active separator dictionary and removes anything not present in
    /// the measured set. Removed separators get a final UpdateAndRemove transition so
    /// they can animate out (e.g. fade or slide) before their geometries detach.
    /// </summary>
    private void CollectOrphanSeparators(
        Dictionary<string, AxisVisualSeprator> separators,
        HashSet<AxisVisualSeprator> measured,
        in AxisMeasureContext ctx)
    {
        foreach (var separatorValueKey in separators.ToArray())
        {
            var separator = separatorValueKey.Value;
            if (measured.Contains(separator)) continue;

            float x, y;
            if (ctx.Orientation == AxisOrientation.X)
            {
                x = ctx.Scale.ToPixels(separator.Value);
                y = ctx.OffsetY;
            }
            else
            {
                x = ctx.OffsetX;
                y = ctx.Scale.ToPixels(separator.Value);
            }

            var sxco = ctx.SeparatorsXOffset;
            var syco = ctx.SeparatorsYOffset;
            var txco = ctx.TicksXOffset;
            var tyco = ctx.TicksYOffset;

            if (separator.Separator is not null)
                UpdateSeparator(separator.Separator, x + sxco, y + syco, ctx.LeftX, ctx.RightX, ctx.TopY, ctx.BottomY, UpdateMode.UpdateAndRemove);
            if (separator.Subseparators is not null)
                UpdateSubseparators(
                    separator.Subseparators, ctx.Scale, ctx.Step, x + sxco, y + syco, ctx.LeftX, ctx.RightX, ctx.TopY, ctx.BottomY, UpdateMode.UpdateAndRemove);
            if (separator.Tick is not null)
                UpdateTick(separator.Tick, _tickLength, x + txco, y + tyco, UpdateMode.UpdateAndRemove);
            if (separator.Subticks is not null)
                UpdateSubticks(separator.Subticks, ctx.Scale, ctx.Step, x + txco, y + tyco, UpdateMode.UpdateAndRemove);
            if (separator.Label is not null)
                UpdateLabel(
                    separator.Label, x, y + tyco, TryGetLabelOrLogError(ctx.Labeler, separator.Value - 1d + 1d),
                    ctx.HasRotation, ctx.LabelsRotation, UpdateMode.UpdateAndRemove);

            _ = separators.Remove(separatorValueKey.Key);
        }
    }

    /// <inheritdoc cref="ChartElement.Invalidate(Chart)"/>
    public override void Invalidate(Chart chart)
    {
        var cartesianChart = (CartesianChartEngine)chart;
        var ctx = BeginMeasure(cartesianChart);

        InitializePaints(in ctx);

        if (!activeSeparators.TryGetValue(cartesianChart, out var separators))
        {
            separators = [];
            activeSeparators[cartesianChart] = separators;
        }

        if (Name is not null && NamePaint is not null && NamePaint != Paint.Default)
            DrawName(cartesianChart, (float)NameTextSize, ctx.LeftX, ctx.RightX, ctx.TopY, ctx.BottomY);

        if (NamePaint is not null && NamePaint != Paint.Default && _nameGeometry is not null)
            NamePaint.AddGeometryToPaintTask(cartesianChart.Canvas, _nameGeometry);

        EnsureZeroLine(in ctx);
        EnsureTicksPath(in ctx);

        var measured = new HashSet<AxisVisualSeprator>();
        var separatorValues = new List<double>();

        foreach (var i in EnumerateSeparators(ctx.Start, ctx.Max, ctx.Step))
        {
            separatorValues.Add(i);
            MeasureSeparatorAtValue(i, separators, measured, in ctx);
        }

        CollectOrphanSeparators(separators, measured, in ctx);
        EnsureBands(in ctx, separatorValues);
    }

    // Draws the alternating (zebra) bands as rectangles in the draw margin, behind the series:
    // every other gap between the separators the axis is drawing (whatever produced them —
    // grouped, custom or evenly-stepped) is filled with AlternatingBandsPaint. The axis owns the
    // geometry lifecycle — bands are keyed by their start value so zoom/pan animates the same
    // instance, new bands fade in and bands leaving the range animate out.
    private void EnsureBands(in AxisMeasureContext ctx, IReadOnlyList<double> separatorValues)
    {
        var chart = ctx.Chart;
        var bandsPaint = AlternatingBandsPaint;

        var bands = bandsPaint is null
            ? null
            : ComputeAlternatingBands(ctx.Min, ctx.Max, separatorValues);

        if (bands is { Count: > 0 })
        {
            // A platform that supplies no band geometry cannot draw bands — same as having
            // none, INCLUDING sweeping out any active ones, so the disabled state is always
            // consistent. Probed once: the factory is constant for the axis type.
            _createsBandGeometries ??= CreateBandGeometry() is not null;
            if (_createsBandGeometries == false) bands = null;
        }

        var hasActive = _activeBands.TryGetValue(chart, out var active);

        if (bandsPaint is null || bands is not { Count: > 0 })
        {
            if (!hasActive) return;
            foreach (var band in active!.Values)
                SetUpdateMode(band, UpdateMode.UpdateAndRemove);
            _ = _activeBands.Remove(chart);
            return;
        }

        if (!hasActive)
        {
            active = [];
            _activeBands[chart] = active;
        }

        if (Math.Abs(bandsPaint.ZIndex) < double.Epsilon) bandsPaint.ZIndex = PaintConstants.AxisBandsPaintZIndex;
        chart.Canvas.AddDrawableTask(bandsPaint, zone: CanvasZone.DrawMargin);

        var measuredBands = new HashSet<string>();

        foreach (var band in bands)
        {
            var key = band.Start.ToString("R");
            if (!measuredBands.Add(key)) continue;

            // Stamp position with the current scaler, target with the next one — same source →
            // target convention the separators use, so bands slide along with them on zoom/pan.
            GetBandRect(in ctx, band, ctx.ActualScale, out var xc, out var yc, out var wc, out var hc);
            GetBandRect(in ctx, band, ctx.Scale, out var x, out var y, out var w, out var h);

            if (!active!.TryGetValue(key, out var rect))
            {
                // Non-null by the probe above; a pathological override that alternates null
                // just skips this band — no state was touched for it.
                if (CreateBandGeometry() is not { } newRect) continue;
                rect = newRect;
                rect.X = xc; rect.Y = yc; rect.Width = wc; rect.Height = hc;
                rect.Animate(GetAnimation(chart));
                SetUpdateMode(rect, UpdateMode.UpdateAndComplete);
                active[key] = rect;
            }

            bandsPaint.AddGeometryToPaintTask(chart.Canvas, rect);

            rect.X = x; rect.Y = y; rect.Width = w; rect.Height = h;
            rect.RemoveOnCompleted = false;
            SetUpdateMode(rect, UpdateMode.Update);
        }

        // Sweep bands whose range is gone; they animate out and detach on completion.
        foreach (var pair in active!.ToArray().Where(pair => !measuredBands.Contains(pair.Key)))
        {
            SetUpdateMode(pair.Value, UpdateMode.UpdateAndRemove);
            _ = active.Remove(pair.Key);
        }
    }

    // The zebra computation: cell edges are the separators plus the partial cells at the range
    // edges (grouped separators are exact boundaries inside [min, max], so without these the
    // first and last partial month/day/... would never be banded; the draw margin clips
    // overflow). Every other cell is banded, with parity anchored to a stable ordinal of the
    // cell START (see GetBandParityAnchor) — anchoring to the list index would swap every band
    // color each time a separator scrolls in or out on pan. Internal for the unit tests.
    internal List<AxisBand>? ComputeAlternatingBands(double min, double max, IReadOnlyList<double> separators)
    {
        if (separators.Count < 2) return null;

        var edges = new List<double>(separators.Count + 2);
        var anchorIndex = 0; // index in edges of the cell that starts at separators[0]

        if (min < separators[0])
        {
            edges.Add(min);
            anchorIndex = 1;
        }
        foreach (var s in separators)
            if (edges.Count == 0 || s > edges[edges.Count - 1])
                edges.Add(s);
        if (max > edges[edges.Count - 1]) edges.Add(max);

        if (edges.Count < 2) return null;

        var anchor = GetBandParityAnchor(min, max, separators);

        var result = new List<AxisBand>(edges.Count / 2 + 1);
        for (var i = 0; i < edges.Count - 1; i++)
        {
            var ordinal = anchor + (i - anchorIndex);
            if ((ordinal & 1L) != 0) continue;
            result.Add(new AxisBand(edges[i], edges[i + 1]));
        }

        return result;
    }

    /// <summary>
    /// Returns a stable ordinal for the cell that starts at the first separator, used to anchor
    /// the alternating-bands parity across pan. The default assumes evenly-stepped separators
    /// (the step quotient is the cell ordinal — stable while the tick step doesn't change; a
    /// re-tier legitimately reshuffles the bands). Axes that group their separators (e.g. a
    /// <c>DateTimeAxis</c> grouping time units) override this with an exact cell ordinal.
    /// </summary>
    /// <param name="min">The effective visible minimum, in axis units.</param>
    /// <param name="max">The effective visible maximum, in axis units.</param>
    /// <param name="separators">The separator values the axis is drawing, ordered ascending.</param>
    protected virtual long GetBandParityAnchor(double min, double max, IReadOnlyList<double> separators)
    {
        // User CustomSeparators with uneven gaps get a stable (per value) though approximate
        // ordinal — bands never flicker on pan, but two adjacent cells can occasionally share
        // a color.
        var step = separators[1] - separators[0];
        return step > 0 ? (long)Math.Round(separators[0] / step) : 0;
    }

    /// <summary>
    /// Creates a new rectangle geometry for one alternating band; called once per band that
    /// enters the visible range (the axis caches and animates the instances). The platform
    /// axis supplies the concrete geometry; returning null disables the bands.
    /// </summary>
    protected virtual BoundedDrawnGeometry? CreateBandGeometry() => null;

    private static void GetBandRect(
        in AxisMeasureContext ctx, AxisBand band, Scaler scaler,
        out float x, out float y, out float w, out float h)
    {
        if (ctx.Orientation == AxisOrientation.X)
        {
            var x0 = scaler.ToPixels(band.Start);
            var x1 = scaler.ToPixels(band.End);
            x = Math.Min(x0, x1);
            w = Math.Abs(x1 - x0);
            y = ctx.TopY;
            h = ctx.BottomY - ctx.TopY;
        }
        else
        {
            var y0 = scaler.ToPixels(band.Start);
            var y1 = scaler.ToPixels(band.End);
            y = Math.Min(y0, y1);
            h = Math.Abs(y1 - y0);
            x = ctx.LeftX;
            w = ctx.RightX - ctx.LeftX;
        }
    }

    /// <inheritdoc cref="ICartesianAxis.InvalidateCrosshair(Chart, LvcPoint)"/>
    public void InvalidateCrosshair(Chart chart, LvcPoint pointerPosition)
    {
        if (CrosshairPaint is null || CrosshairPaint == Paint.Default || chart is not CartesianChartEngine cartesianChart) return;

        var location = chart.DrawMarginLocation;
        var size = chart.DrawMarginSize;

        if (pointerPosition.X < location.X || pointerPosition.X > location.X + size.Width ||
            pointerPosition.Y < location.Y || pointerPosition.Y > location.Y + size.Height)
        {
            return;
        }

        var scale = this.GetNextScaler(cartesianChart);
        var controlSize = cartesianChart.ControlSize;
        var drawLocation = cartesianChart.DrawMarginLocation;
        var drawMarginSize = cartesianChart.DrawMarginSize;
        double labelValue;

        var lyi = drawLocation.Y;
        var lyj = drawLocation.Y + drawMarginSize.Height;
        var lxi = drawLocation.X;
        var lxj = drawLocation.X + drawMarginSize.Width;

        float xoo = 0f, yoo = 0f;

        if (_orientation == AxisOrientation.X)
        {
            yoo = Position == AxisPosition.Start
                 ? controlSize.Height - _yo
                 : _yo;
        }
        else
        {
            xoo = Position == AxisPosition.Start
                ? _xo
                : controlSize.Width - _xo;
        }

        float x, y;
        if (_orientation == AxisOrientation.X)
        {
            float crosshairX;
            if (CrosshairSnapEnabled)
            {
                var axisIndex = Array.IndexOf(cartesianChart.XAxes, this);
                var closestPoint = FindClosestPoint(
                    pointerPosition, cartesianChart,
                    cartesianChart.VisibleSeries
                        .Cast<ICartesianSeries>()
                        .Where(s => s.ScalesXAt == axisIndex));

                var c = closestPoint?.Coordinate;

                crosshairX = scale.ToPixels(c?.SecondaryValue ?? pointerPosition.X);
                labelValue = c?.SecondaryValue ?? scale.ToChartValues(pointerPosition.X);
            }
            else
            {
                crosshairX = pointerPosition.X;
                labelValue = scale.ToChartValues(pointerPosition.X);
            }

            x = crosshairX;
            y = yoo;
        }
        else
        {
            float crosshairY;
            if (CrosshairSnapEnabled)
            {
                var axisIndex = Array.IndexOf(cartesianChart.YAxes, this);
                var closestPoint = FindClosestPoint(
                    pointerPosition, cartesianChart,
                    cartesianChart.VisibleSeries
                        .Cast<ICartesianSeries>()
                        .Where(s => s.ScalesYAt == axisIndex));

                var c = closestPoint?.Coordinate;

                crosshairY = scale.ToPixels(c?.PrimaryValue ?? pointerPosition.Y);
                labelValue = c?.PrimaryValue ?? scale.ToChartValues(pointerPosition.Y);
            }
            else
            {
                crosshairY = pointerPosition.Y;
                labelValue = scale.ToChartValues(pointerPosition.Y);
            }

            x = xoo;
            y = crosshairY;
        }

        if (CrosshairPaint.ZIndex == 0) CrosshairPaint.ZIndex = PaintConstants.CrosshairPaintZIndex;
        cartesianChart.Canvas.AddDrawableTask(CrosshairPaint, zone: CanvasZone.DrawMargin);

        if (_crosshairLine is null)
        {
            _crosshairLine = new TLineGeometry();
            UpdateSeparator(_crosshairLine, x, y, lxi, lxj, lyi, lyj, UpdateMode.UpdateAndComplete);
        }
        CrosshairPaint.AddGeometryToPaintTask(cartesianChart.Canvas, _crosshairLine);

        if (CrosshairLabelsPaint is not null && CrosshairLabelsPaint != Paint.Default)
        {
            if (CrosshairLabelsPaint.ZIndex == 0) CrosshairLabelsPaint.ZIndex = PaintConstants.CrosshairLabelsPaintZIndex;
            var axisZone = Orientation == AxisOrientation.X ? CanvasZone.XCrosshair : CanvasZone.YCrosshair;
            cartesianChart.Canvas.AddDrawableTask(CrosshairLabelsPaint, zone: axisZone);

            _crosshairLabel ??= new TTextGeometry();
            var labeler = GetActualLabeler();

            _crosshairLabel.Text = TryGetLabelOrLogError(labeler, labelValue);
            _crosshairLabel.TextSize = (float)TextSize;
            _crosshairLabel.Background = CrosshairLabelsBackground ?? LvcColor.Empty;
            _crosshairLabel.Padding = CrosshairPadding ?? Padding;
            _crosshairLabel.X = x;
            _crosshairLabel.Y = y;
            _crosshairLabel.Paint = CrosshairLabelsPaint;

            var r = (float)LabelsRotation;
            var hasRotation = Math.Abs(r) > 0.01f;
            if (hasRotation) _crosshairLabel.RotateTransform = r;
            CrosshairLabelsPaint.AddGeometryToPaintTask(cartesianChart.Canvas, _crosshairLabel);
        }

        UpdateSeparator(_crosshairLine, x, y, lxi, lxj, lyi, lyj, UpdateMode.Update);

        chart.Canvas.Invalidate();
    }

    /// <inheritdoc cref="ICartesianAxis.ClearCrosshair(Chart)"/>
    public void ClearCrosshair(Chart chart)
    {
        if (_crosshairLine is not null)
            CrosshairPaint?.RemoveGeometryFromPaintTask(chart.Canvas, _crosshairLine);

        if (_crosshairLabel is not null)
            CrosshairLabelsPaint?.RemoveGeometryFromPaintTask(chart.Canvas, _crosshairLabel);
    }

    private IEnumerable<double> EnumerateSeparators(double start, double end, double step)
    {
        // Grouped separators (from an IAxisRenderOverride) take priority, then the user's
        // CustomSeparators, then the evenly-stepped default.
        var custom = _groupedSeparators ?? CustomSeparators;
        if (custom is not null)
        {
            foreach (var s in custom)
                yield return s;

            yield break;
        }

        var relativeEnd = end - start;

        if (relativeEnd / step > 10000)
            ThrowInfiniteSeparators();

        // start from -step to include the first separator/sub-separator
        // and end at relativeEnd + step to include the last separator/sub-separator

        for (var i = -step; i <= relativeEnd + step; i += step)
            yield return start + i;
    }

    private static ChartPoint? FindClosestPoint(
        LvcPoint pointerPosition,
        CartesianChartEngine cartesianChart,
        IEnumerable<ICartesianSeries> allSeries)
    {
        ChartPoint? closestPoint = null;
        var strategy = allSeries.GetFindingStrategy();

        foreach (var series in allSeries)
        {
            var hitpoints = series.FindHitPoints(cartesianChart, pointerPosition, strategy, FindPointFor.PointerDownEvent);
            var hitpoint = hitpoints.FirstOrDefault();
            if (hitpoint == null) continue;

            if (closestPoint is null ||
                hitpoint.DistanceTo(pointerPosition, strategy) < closestPoint.DistanceTo(pointerPosition, strategy))
            {
                closestPoint = hitpoint;
            }
        }

        return closestPoint;
    }

    /// <inheritdoc cref="IPlane.GetNameLabelSize(Chart)"/>
    public LvcSize GetNameLabelSize(Chart chart)
    {
        if (NamePaint is null || string.IsNullOrWhiteSpace(Name)) return new LvcSize(0, 0);

        var textGeometry = new TTextGeometry
        {
            Text = Name ?? string.Empty,
            TextSize = (float)NameTextSize,
            RotateTransform = Orientation == AxisOrientation.X
                ? 0
                : InLineNamePlacement ? 0 : -90,
            Padding = NamePadding,
            Paint = NamePaint
        };

        return textGeometry.Measure();
    }

    /// <inheritdoc cref="IPlane.GetPossibleSize(Chart)"/>
    public virtual LvcSize GetPossibleSize(Chart chart)
    {
        if (_dataBounds is null) throw new Exception("DataBounds not found");
        if (LabelsPaint is null) return new LvcSize(0f, 0f);

        EnsureGrouped(chart);

        var ts = (float)TextSize;
        var labeler = GetActualLabeler();

        var axisTick = this.GetTick(chart.DrawMarginSize);
        var s = axisTick.Value;

        var max = MaxLimit is null ? _visibleDataBounds.Max : MaxLimit.Value;
        var min = MinLimit is null ? _visibleDataBounds.Min : MinLimit.Value;

        AxisLimit.ValidateLimits(ref min, ref max, MinStep);

        if (s < _minStep) s = _minStep;
        if (_forceStepToMin) s = _minStep;

        var start = Math.Truncate(min / s) * s;

        var w = 0f;
        var h = 0f;
        var r = (float)LabelsRotation;

        foreach (var i in EnumerateSeparators(start, max, s))
        {
            var textGeometry = new TTextGeometry
            {
                Text = TryGetLabelOrLogError(labeler, i),
                TextSize = ts,
                RotateTransform = r,
                Padding = Padding,
                Paint = LabelsPaint
            };
            var m = textGeometry.Measure();
            if (m.Width > w) w = m.Width;
            if (m.Height > h) h = m.Height;
        }

        return new LvcSize(w, h);
    }

    /// <inheritdoc cref="ICartesianAxis.GetLimits"/>
    public AxisLimit GetLimits()
    {
        var max = MaxLimit is null ? DataBounds.Max : MaxLimit.Value;
        var min = MinLimit is null ? DataBounds.Min : MinLimit.Value;

        AxisLimit.ValidateLimits(ref min, ref max, MinStep);

        var maxd = DataBounds.Max;
        var mind = DataBounds.Min;
        var minZoomDelta = MinZoomDelta ?? DataBounds.MinDelta * 3;

        // The user pin must be aggregated across the shared group the same way
        // Min/Max/DataMin/DataMax are: a zoom started from an unpinned shared
        // axis still has to honor a sibling's pin as the outer rail, otherwise
        // SetLimits propagates a data-bounds collapse onto the pinned axis (#2159).
        var userSetMin = _userSetMinLimit;
        var userSetMax = _userSetMaxLimit;

        foreach (var axis in SharedWith ?? [])
        {
            var maxI = axis.MaxLimit is null ? axis.DataBounds.Max : axis.MaxLimit.Value;
            var minI = axis.MinLimit is null ? axis.DataBounds.Min : axis.MinLimit.Value;
            var maxDI = axis.DataBounds.Max;
            var minDI = axis.DataBounds.Min;
            var minZoomDeltaI = axis.MinZoomDelta ?? axis.DataBounds.MinDelta * 3;

            if (maxI > max) max = maxI;
            if (minI < min) min = minI;
            if (maxDI > maxd) maxd = maxDI;
            if (minDI < mind) mind = minDI;

            // widest pin wins: smallest non-null UserSetMin, largest non-null UserSetMax
            if (axis is IInternalCartesianAxis internalAxis)
            {
                if (internalAxis.UserSetMinLimit is { } usMin && (userSetMin is null || usMin < userSetMin))
                    userSetMin = usMin;
                if (internalAxis.UserSetMaxLimit is { } usMax && (userSetMax is null || usMax > userSetMax))
                    userSetMax = usMax;
            }
        }

        if (double.IsInfinity(minZoomDelta))
        {
            // at this point the chart data is empty...
            // force the limits to the known bounds

            minZoomDelta = max - min;
            mind = min;
            maxd = max;
        }

        return new(min, max, minZoomDelta, mind, maxd)
        {
            UserSetMin = userSetMin,
            UserSetMax = userSetMax,
        };
    }

    /// <inheritdoc cref="ICartesianAxis.SetLimits(double, double, double, bool, bool)"/>
    public void SetLimits(double min, double max, double step = -1, bool propagateShared = true, bool notify = false)
    {
        var shared = propagateShared ? (SharedWith ?? []) : [];

        foreach (var axis in shared)
            axis.SetLimits(min, max, step, false, notify);

        if (notify)
        {
            // notify: true still raises PropertyChanged so two-way MinLimit/
            // MaxLimit bindings track zoom/pan — but _isEngineSettingLimits
            // keeps the property setters from recording this engine-driven
            // view change as a user pin (#2159).
            _isEngineSettingLimits = true;
            MinLimit = min;
            MaxLimit = max;

            if (step > 0)
            {
                ForceStepToMin = true;
                MinStep = step;
            }
            _isEngineSettingLimits = false;
        }
        else
        {
            _maxLimit = max;
            _minLimit = min;

            if (step > 0)
            {
                _forceStepToMin = true;
                _minStep = step;
            }
        }
    }

    /// <inheritdoc cref="ICartesianAxis.OnMeasureStarted(Chart, AxisOrientation)"/>
    void ICartesianAxis.OnMeasureStarted(Chart chart, AxisOrientation orientation)
    {
        _orientation = orientation;
        _dataBounds = new Bounds();
        _visibleDataBounds = new Bounds();
        _possibleMaxLabelsSize = null;
        MeasureStarted?.Invoke(chart, this);
    }

    void ICartesianAxis.SetLogBase(double newBase)
    {
        MinStep = 1;
        Labeler = value => Math.Pow(newBase, value).ToString("N2");
        _logBase = newBase;
    }

    /// <summary>
    /// Deletes the specified chart.
    /// </summary>
    /// <param name="chart">The chart.</param>
    /// <returns></returns>
    public virtual void Delete(Chart chart)
    {
        foreach (var paint in GetPaintTasks())
        {
            if (paint is null) continue;

            chart.Canvas.RemovePaintTask(paint);
            paint.ClearGeometriesFromPaintTask(chart.Canvas);
        }

        _ = activeSeparators.Remove(chart);
        _ = _activeBands.Remove(chart);
    }

    /// <inheritdoc cref="IChartElement.RemoveFromUI(Chart)"/>
    public override void RemoveFromUI(Chart chart)
    {
        base.RemoveFromUI(chart);
        _ = activeSeparators.Remove(chart);
        _ = _activeBands.Remove(chart);
    }

    /// <summary>
    /// Called when [paint changed].
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns></returns>
    protected override void OnPaintChanged(string? propertyName)
    {
        base.OnPaintChanged(propertyName);
        OnPropertyChanged(propertyName);
    }

    /// <inheritdoc cref="ChartElement.GetPaintTasks"/>
    protected internal override Paint?[] GetPaintTasks() =>
        [SeparatorsPaint, LabelsPaint, NamePaint, ZeroPaint, TicksPaint, SubticksPaint, SubseparatorsPaint, AlternatingBandsPaint];

    /// <summary>
    /// Gets a value indicating whether this axis may group its separators (see
    /// <see cref="TryGroupSeparators"/>) — a cheap pre-check evaluated every measure, so axes
    /// without grouping skip the grouping pipeline entirely. Default is false.
    /// </summary>
    protected virtual bool GroupsSeparators => false;

    /// <summary>
    /// Supplies grouped separators + labeler for the current visible range — e.g. a
    /// <c>DateTimeAxis</c> grouping its time units into adaptive multi-level labels. Called
    /// while measuring (cached by visible range), so it must not mutate the axis: a property
    /// change would re-trigger the measure. Return false to lay the axis out as usual.
    /// </summary>
    /// <param name="chart">The chart being measured.</param>
    /// <param name="min">The effective visible minimum, in axis units.</param>
    /// <param name="max">The effective visible maximum, in axis units.</param>
    /// <param name="separators">The grouped separator values, ascending, or null.</param>
    /// <param name="labeler">The labeler for the grouped separators, or null.</param>
    protected virtual bool TryGroupSeparators(
        Chart chart, double min, double max,
        out IEnumerable<double>? separators, out Func<double, string>? labeler)
    {
        separators = null;
        labeler = null;
        return false;
    }

    // Resolves the grouped separators + labeler for the current visible range: the axis' own
    // TryGroupSeparators (a built-in grouping like DateTimeAxis.GroupTimeUnits) first, then the
    // provider's IAxisRenderOverride (an engine extension), else the axis lays itself out as
    // usual. Cached by (min, max) so the same zoom does not re-consult; the results are read
    // transiently by GetActualLabeler / EnumerateSeparators (the axis' own CustomSeparators / Labeler
    // are never written). Called at the start of both size and draw passes so two-line grouped labels
    // reserve the right margin AND draw consistently.
    private void EnsureGrouped(Chart chart)
    {
        var axisOverride = LiveCharts.DefaultSettings.GetProvider().GetAxisRenderOverride(this);
        if (!GroupsSeparators && axisOverride is null)
        {
            if (_groupedSeparators is not null || _groupedLabeler is not null)
                _possibleMaxLabelsSize = null; // labels are no longer grouped → recompute the size
            _groupedSeparators = null;
            _groupedLabeler = null;
            _groupedSignature = null;
            return;
        }

        var max = MaxLimit ?? _visibleDataBounds.Max;
        var min = MinLimit ?? _visibleDataBounds.Min;
        AxisLimit.ValidateLimits(ref min, ref max, MinStep);

        // Cache by the visible range: re-consulting (and re-allocating separators) is pointless while
        // the zoom hasn't moved. .Equals avoids the float == operator while keeping an exact cache key
        // (a miss just recomputes — no correctness impact).
        if (_groupedSignature is { } sig && sig.Min.Equals(min) && sig.Max.Equals(max)) return;
        _groupedSignature = (min, max);

        // The range changed, so the grouped labels (and their two-line height) change too: drop the
        // cached label size so it is recomputed with the new labeler.
        _possibleMaxLabelsSize = null;

        if (TryGroupSeparators(chart, min, max, out var separators, out var labeler) ||
            (axisOverride?.TryGroup(this, chart, min, max, out separators, out labeler) ?? false))
        {
            // Materialize: the separators are re-enumerated in both the size and draw passes, so a
            // single-use iterator from the grouping would yield nothing the second time.
            _groupedSeparators = separators is null
                ? null
                : separators as IReadOnlyList<double> ?? [.. separators];
            _groupedLabeler = labeler;
        }
        else
        {
            _groupedSeparators = null;
            _groupedLabeler = null;
        }
    }

    private Func<double, string> GetActualLabeler()
    {
        if (_groupedLabeler is not null) return _groupedLabeler;

        var labeler = Labeler;

        if (Labels is not null)
        {
            labeler = Labelers.BuildNamedLabeler(Labels);
            _minStep = 1;
        }

        return labeler;
    }

    private Animation GetAnimation(Chart chart)
    {
        if (AnimationsSpeed is null && EasingFunction is null)
            return chart.Animation;

        _animation.Duration = (long)(AnimationsSpeed ?? chart.ActualAnimationsSpeed).TotalMilliseconds;
        _animation.EasingFunction = EasingFunction ?? chart.ActualEasingFunction;
        return _animation;
    }

    private LvcSize GetPossibleMaxLabelSize()
    {
        if (LabelsPaint is null) return new LvcSize();

        var labeler = GetActualLabeler();

        var max = MaxLimit is null ? _visibleDataBounds.Max : MaxLimit.Value;
        var min = MinLimit is null ? _visibleDataBounds.Min : MinLimit.Value;

        AxisLimit.ValidateLimits(ref min, ref max, MinStep);

        const double testSeparators = 25;
        var s = (max - min) / testSeparators;
        if (s == 0) s = 1;
        if (s < _minStep) s = _minStep;
        if (_forceStepToMin) s = _minStep;

        var maxLabelSize = new LvcSize();

        if (max - min == 0) return maxLabelSize;

        foreach (var i in EnumerateSeparators(min, max, s))
        {
            var textGeometry = new TTextGeometry
            {
                Text = labeler(i),
                TextSize = (float)TextSize,
                RotateTransform = (float)LabelsRotation,
                Padding = Padding,
                Paint = LabelsPaint
            };

            var m = textGeometry.Measure();

            maxLabelSize = new LvcSize(
                maxLabelSize.Width > m.Width ? maxLabelSize.Width : m.Width,
                maxLabelSize.Height > m.Height ? maxLabelSize.Height : m.Height);
        }

        return maxLabelSize;
    }

    private void DrawName(
        CartesianChartEngine cartesianChart,
        float size,
        float lxi,
        float lxj,
        float lyi,
        float lyj)
    {
        var isNew = false;

        if (_nameGeometry is null)
        {
            _nameGeometry = new TTextGeometry
            {
                TextSize = size,
                HorizontalAlign = Align.Middle,
                VerticalAlign = Align.Middle
            };

            _nameGeometry.Animate(GetAnimation(cartesianChart));
            isNew = true;
        }

        _nameGeometry.Padding = NamePadding;
        _nameGeometry.Text = Name ?? string.Empty;
        _nameGeometry.TextSize = (float)NameTextSize;
        _nameGeometry.Paint = NamePaint;

        if (_orientation == AxisOrientation.X)
        {
            if (InLineNamePlacement)
            {
                _nameGeometry.X = _nameDesiredSize.X + _nameDesiredSize.Width * 0.5f;
                _nameGeometry.Y = _nameDesiredSize.Y + _nameDesiredSize.Height * 0.5f;
            }
            else
            {
                _nameGeometry.X = (lxi + lxj) * 0.5f;
                _nameGeometry.Y = _nameDesiredSize.Y + _nameDesiredSize.Height * 0.5f;
            }
        }
        else
        {
            if (InLineNamePlacement)
            {
                _nameGeometry.X = _nameDesiredSize.X + _nameDesiredSize.Width * 0.5f;
                _nameGeometry.Y = _nameDesiredSize.Height * 0.5f;
            }
            else
            {
                _nameGeometry.RotateTransform = -90;
                _nameGeometry.X = _nameDesiredSize.X + _nameDesiredSize.Width * 0.5f;
                _nameGeometry.Y = (lyi + lyj) * 0.5f;
            }
        }

        if (isNew) _nameGeometry.CompleteTransition(null);
    }

    private void InitializeSeparator(
        AxisVisualSeprator visualSeparator, CartesianChartEngine cartesianChart, TLineGeometry? separatorGeometry = null)
    {
        TLineGeometry lineGeometry;

        if (separatorGeometry is not null)
        {
            lineGeometry = separatorGeometry;
        }
        else
        {
            lineGeometry = new TLineGeometry();
            visualSeparator.Separator = lineGeometry;
        }

        visualSeparator.Separator = lineGeometry;
        InitializeLine(lineGeometry, cartesianChart);
    }

    private void InitializeSubseparators(
        AxisVisualSeprator visualSeparator, CartesianChartEngine cartesianChart)
    {
        visualSeparator.Subseparators = new TLineGeometry[SubseparatorsCount];

        for (var j = 0; j < SubseparatorsCount; j++)
        {
            var subSeparator = new TLineGeometry();
            visualSeparator.Subseparators[j] = subSeparator;
            InitializeTick(visualSeparator, cartesianChart, subSeparator);
        }
    }

    private void DetachSubseparators(AxisVisualSeprator visualSeparator, CartesianChartEngine cartesianChart)
    {
        if (visualSeparator.Subseparators is null || SubseparatorsPaint is null) return;
        foreach (var sub in visualSeparator.Subseparators)
            SubseparatorsPaint.RemoveGeometryFromPaintTask(cartesianChart.Canvas, sub);
    }

    private void DetachSubticks(AxisVisualSeprator visualSeparator, CartesianChartEngine cartesianChart)
    {
        if (visualSeparator.Subticks is null || SubticksPaint is null) return;
        foreach (var sub in visualSeparator.Subticks)
            SubticksPaint.RemoveGeometryFromPaintTask(cartesianChart.Canvas, sub);
    }

    private void InitializeLine(BaseLineGeometry lineGeometry, CartesianChartEngine cartesianChart) =>
        lineGeometry.Animate(GetAnimation(cartesianChart));

    private void InitializeTick(
        AxisVisualSeprator visualSeparator, CartesianChartEngine cartesianChart, TLineGeometry? subTickGeometry = null)
    {
        TLineGeometry tickGeometry;

        if (subTickGeometry is not null)
        {
            tickGeometry = subTickGeometry;
        }
        else
        {
            tickGeometry = new TLineGeometry();
            visualSeparator.Tick = tickGeometry;
        }

        tickGeometry.Animate(GetAnimation(cartesianChart));
    }

    private void InitializeSubticks(
        AxisVisualSeprator visualSeparator, CartesianChartEngine cartesianChart)
    {
        visualSeparator.Subticks = new TLineGeometry[SubseparatorsCount];

        for (var j = 0; j < SubseparatorsCount; j++)
        {
            var subTick = new TLineGeometry();
            visualSeparator.Subticks[j] = subTick;
            InitializeTick(visualSeparator, cartesianChart, subTick);
        }
    }

    private void IntializeLabel(
        AxisVisualSeprator visualSeparator,
        CartesianChartEngine cartesianChart,
        float size,
        bool hasRotation,
        float r)
    {
        var textGeometry = new TTextGeometry { TextSize = size };
        visualSeparator.Label = textGeometry;
        if (hasRotation) textGeometry.RotateTransform = r;

        textGeometry.Animate(
            GetAnimation(cartesianChart),
            BaseLabelGeometry.XProperty,
            BaseLabelGeometry.YProperty,
            BaseLabelGeometry.OpacityProperty);
    }

    private void UpdateSeparator(
        BaseLineGeometry line,
        float x,
        float y,
        float lxi,
        float lxj,
        float lyi,
        float lyj,
        UpdateMode mode)
    {
        if (_orientation == AxisOrientation.X)
        {
            line.X = x;
            line.X1 = x;
            line.Y = lyi;
            line.Y1 = lyj;
        }
        else
        {
            line.X = lxi;
            line.X1 = lxj;
            line.Y = y;
            line.Y1 = y;
        }

        SetUpdateMode(line, mode);
    }

    private void UpdateTick(
        BaseLineGeometry tick, float length, float x, float y, UpdateMode mode)
    {
        if (_orientation == AxisOrientation.X)
        {
            var lyi = y + _size.Height * 0.5f;
            var lyj = y - _size.Height * 0.5f;
            tick.X = x;
            tick.X1 = x;
            tick.Y = Position == AxisPosition.Start ? lyj : lyi - length;
            tick.Y1 = Position == AxisPosition.Start ? lyj + length : lyi;
        }
        else
        {
            var lxi = x + _size.Width * 0.5f;
            var lxj = x - _size.Width * 0.5f;
            tick.X = Position == AxisPosition.Start ? lxi : lxj + length;
            tick.X1 = Position == AxisPosition.Start ? lxi - length : lxj;
            tick.Y = y;
            tick.Y1 = y;
        }

        SetUpdateMode(tick, mode);
    }

    private void UpdateSubseparators(
        BaseLineGeometry[] subseparators, Scaler scale, double s, float x, float y, float lxi, float lxj, float lyi, float lyj, UpdateMode mode)
    {
        for (var j = 0; j < subseparators.Length; j++)
        {
            var subseparator = subseparators[j];
            var kl = (j + 1) / (double)(SubseparatorsCount + 1);

            if (_logBase is not null) kl = Math.Log(kl, _logBase.Value);

            float xs = 0f, ys = 0f;
            if (_orientation == AxisOrientation.X)
            {
                xs = scale.MeasureInPixels(s * kl);
            }
            else
            {
                ys = scale.MeasureInPixels(s * kl);
            }

            UpdateSeparator(subseparator, x + xs, y + ys, lxi, lxj, lyi, lyj, mode);
        }
    }

    private void UpdateSubticks(
        BaseLineGeometry[] subticks, Scaler scale, double s, float x, float y, UpdateMode mode)
    {
        for (var j = 0; j < subticks.Length; j++)
        {
            var subtick = subticks[j];

            var k = 0.5f;
            var kl = (j + 1) / (double)(SubseparatorsCount + 1);
            if (Math.Abs(kl - 0.5f) < 0.01) k += 0.25f;

            float xs = 0f, ys = 0f;
            if (_orientation == AxisOrientation.X)
            {
                xs = scale.MeasureInPixels(s * kl);
            }
            else
            {
                ys = scale.MeasureInPixels(s * kl);
            }

            UpdateTick(subtick, _tickLength * k, x + xs, y + ys, mode);
        }
    }

    private void UpdateLabel(
        BaseLabelGeometry label,
        float x,
        float y,
        string text,
        bool hasRotation,
        float r,
        UpdateMode mode)
    {
        var actualRotatation = r;
        const double toRadians = Math.PI / 180;

        // Grouped labels are multi-line blocks centered on their tick (fine unit on one line,
        // coarse context on another); left-aligned lines would shift the narrow line toward the
        // previous tick. Reset when grouping turns off so the default layout returns.
        label.LinesAlignment = _groupedLabeler is not null ? Align.Middle : Align.Start;

        if (_orientation == AxisOrientation.Y)
        {
            actualRotatation %= 180;
            if (actualRotatation < 0) actualRotatation += 360;
            if (actualRotatation is > 90 and < 180) actualRotatation += 180;
            if (actualRotatation is > 180 and < 270) actualRotatation += 180;

            var actualAlignment = LabelsAlignment == null
              ? (Position == AxisPosition.Start ? Align.End : Align.Start)
              : LabelsAlignment.Value;

            if (actualAlignment == Align.Start)
            {
                if (hasRotation && LabelsPaint is not null)
                {
                    var notRotatedSize =
                        new TTextGeometry { TextSize = (float)TextSize, Padding = Padding, Text = text, Paint = LabelsPaint }
                        .Measure();

                    var rhx = Math.Cos((90 - actualRotatation) * toRadians) * notRotatedSize.Height;
                    x += (float)Math.Abs(rhx * 0.5f);
                }

                x -= _labelsDesiredSize.Width * 0.5f;
                label.HorizontalAlign = Align.Start;
            }
            else
            {
                if (hasRotation && LabelsPaint is not null)
                {
                    var notRotatedSize =
                        new TTextGeometry { TextSize = (float)TextSize, Padding = Padding, Text = text, Paint = LabelsPaint }
                        .Measure();

                    var rhx = Math.Cos((90 - actualRotatation) * toRadians) * notRotatedSize.Height;
                    x -= (float)Math.Abs(rhx * 0.5f);
                }

                x += _labelsDesiredSize.Width * 0.5f;
                label.HorizontalAlign = Align.End;
            }
        }

        if (_orientation == AxisOrientation.X)
        {
            actualRotatation %= 180;
            if (actualRotatation < 0) actualRotatation += 180;
            if (actualRotatation >= 90) actualRotatation -= 180;

            var actualAlignment = LabelsAlignment == null
              ? (Position == AxisPosition.Start ? Align.Start : Align.End)
              : LabelsAlignment.Value;

            if (actualAlignment == Align.Start)
            {
                if (hasRotation && LabelsPaint is not null)
                {
                    var notRotatedSize =
                        new TTextGeometry { TextSize = (float)TextSize, Padding = Padding, Text = text, Paint = LabelsPaint }
                        .Measure();

                    var rhx = Math.Sin((90 - actualRotatation) * toRadians) * notRotatedSize.Height;
                    y += (float)Math.Abs(rhx * 0.5f);
                }

                if (hasRotation)
                {
                    y -= _labelsDesiredSize.Height * 0.5f;
                    label.HorizontalAlign = actualRotatation < 0
                        ? Align.End
                        : Align.Start;
                }
                else
                {
                    label.HorizontalAlign = Align.Middle;
                }
            }
            else
            {
                if (hasRotation && LabelsPaint is not null)
                {
                    var notRotatedSize =
                        new TTextGeometry { TextSize = (float)TextSize, Padding = Padding, Text = text, Paint = LabelsPaint }
                        .Measure();

                    var rhx = Math.Sin((90 - actualRotatation) * toRadians) * notRotatedSize.Height;
                    y -= (float)Math.Abs(rhx * 0.5f);
                }

                if (hasRotation)
                {
                    y += _labelsDesiredSize.Height * 0.5f;
                    label.HorizontalAlign = actualRotatation < 0
                        ? Align.Start
                        : Align.End;
                }
                else
                {
                    label.HorizontalAlign = Align.Middle;
                }
            }
        }

        label.Text = text;
        label.TextSize = (float)TextSize;
        label.Padding = Padding;
        label.X = x;
        label.Y = y;
        label.Paint = LabelsPaint;

        if (hasRotation) label.RotateTransform = actualRotatation;

        SetUpdateMode(label, mode);
    }

    private void SetUpdateMode(IDrawnElement geometry, UpdateMode mode)
    {
        switch (mode)
        {
            case UpdateMode.UpdateAndComplete:
                geometry.Opacity = 0;
                geometry.CompleteTransition(null);
                break;
            case UpdateMode.UpdateAndRemove:
                geometry.Opacity = 0;
                geometry.RemoveOnCompleted = true;
                break;
            case UpdateMode.Update:
            default:
                geometry.Opacity = 1;
                break;
        }
    }

    private string TryGetLabelOrLogError(Func<double, string> labeler, double value)
    {
        try
        {
            return labeler(value);
        }
#if DEBUG
        catch (Exception e)
        {
            Trace.WriteLine($"[Error] LiveCharts was not able to get a label from axis {_orientation} with value {value}. {e.Message}");
#else
        catch
        {
#endif
            return string.Empty;
        }
    }

    private void ThrowInfiniteSeparators()
    {
        var axisName = string.IsNullOrEmpty(Name) ? "" : $"named \"{Name}\" ";
        throw new Exception(
            $"The {_orientation} axis {axisName}has an excessive number of separators. " +
            $"If you set the step manually, ensure the number of separators is less than 10,000. " +
            $"This could also be caused because you are zooming too deep, " +
            $"try to set a limit to the current chart zoom using the Axis.{nameof(MinZoomDelta)} property. " +
            $"For more info see: https://github.com/beto-rodriguez/LiveCharts2/issues/1076.");
    }

    private enum UpdateMode
    {
        Update,
        UpdateAndComplete,
        UpdateAndRemove
    }
}
