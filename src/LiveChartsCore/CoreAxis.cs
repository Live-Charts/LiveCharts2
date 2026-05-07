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
    : ChartElement, ICartesianAxis, IPlane
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

    #endregion

    /// <inheritdoc cref="ICartesianAxis.MeasureStarted"/>
    public event Action<Chart, ICartesianAxis>? MeasureStarted;

    /// <inheritdoc cref="ChartElement.Invalidate(Chart)"/>
    public override void Invalidate(Chart chart)
    {
        var cartesianChart = (CartesianChartEngine)chart;
        var animation = GetAnimation(cartesianChart);

        var controlSize = cartesianChart.ControlSize;
        var drawLocation = cartesianChart.DrawMarginLocation;
        var drawMarginSize = cartesianChart.DrawMarginSize;

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

        var scale = this.GetNextScaler(cartesianChart);
        var actualScale = this.GetActualScaler(cartesianChart) ?? scale;
        var labeler = GetActualLabeler();

        if (NamePaint is not null && NamePaint != Paint.Default)
        {
            if (NamePaint.ZIndex == 0) NamePaint.ZIndex = PaintConstants.AxisNamePaintZIndex;
            cartesianChart.Canvas.AddDrawableTask(NamePaint, zone: CanvasZone.NoClip);
        }
        if (LabelsPaint is not null && LabelsPaint != Paint.Default)
        {
            if (LabelsPaint.ZIndex == 0) LabelsPaint.ZIndex = PaintConstants.AxisLabelsPaintZIndex;
            cartesianChart.Canvas.AddDrawableTask(LabelsPaint, zone: CanvasZone.NoClip);
        }

        var o = SeparatorsPaint?.StrokeThickness ?? 0;

        if (SubseparatorsPaint is not null && SubseparatorsPaint != Paint.Default)
        {
            if (SubseparatorsPaint.ZIndex == 0) SubseparatorsPaint.ZIndex = PaintConstants.AxisSubseparatorsPaintZIndex;
            cartesianChart.Canvas.AddDrawableTask(SubseparatorsPaint, zone: CanvasZone.DrawMargin);
        }
        if (SeparatorsPaint is not null && SeparatorsPaint != Paint.Default)
        {
            if (SeparatorsPaint.ZIndex == 0) SeparatorsPaint.ZIndex = PaintConstants.AxisSeparatorsPaintZIndex;
            cartesianChart.Canvas.AddDrawableTask(SeparatorsPaint, zone: CanvasZone.DrawMargin);
        }

        var axisZone = Orientation == AxisOrientation.X ? CanvasZone.XCrosshair : CanvasZone.YCrosshair;
        if (TicksPaint is not null && TicksPaint != Paint.Default)
        {
            if (TicksPaint.ZIndex == 0) TicksPaint.ZIndex = PaintConstants.AxisTicksPaintZIndex;
            cartesianChart.Canvas.AddDrawableTask(TicksPaint, zone: axisZone);
        }
        if (SubticksPaint is not null && SubticksPaint != Paint.Default)
        {
            if (SubticksPaint.ZIndex == 0) SubticksPaint.ZIndex = PaintConstants.AxisSubticksPaintZIndex;
            cartesianChart.Canvas.AddDrawableTask(SubticksPaint, zone: axisZone);
        }

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

        var size = (float)TextSize;
        var r = (float)LabelsRotation;
        var hasRotation = Math.Abs(r) > 0.01f;

        if (!activeSeparators.TryGetValue(cartesianChart, out var separators))
        {
            separators = [];
            activeSeparators[cartesianChart] = separators;
        }

        if (Name is not null && NamePaint is not null && NamePaint != Paint.Default)
            DrawName(cartesianChart, (float)NameTextSize, lxi, lxj, lyi, lyj);

        if (NamePaint is not null && NamePaint != Paint.Default && _nameGeometry is not null)
            NamePaint.AddGeometryToPaintTask(cartesianChart.Canvas, _nameGeometry);

        var hasActivePaint =
            (NamePaint is not null && NamePaint != Paint.Default) || (SeparatorsPaint is not null && SeparatorsPaint != Paint.Default) ||
            (LabelsPaint is not null && LabelsPaint != Paint.Default) || (TicksPaint is not null && TicksPaint != Paint.Default) ||
            (SubticksPaint is not null && SubticksPaint != Paint.Default) || (SubseparatorsPaint is not null && SubseparatorsPaint != Paint.Default);

        var measured = new HashSet<AxisVisualSeprator>();

        if (ZeroPaint is not null && ZeroPaint != Paint.Default)
        {
            float x, y;
            if (_orientation == AxisOrientation.X)
            {
                x = scale.ToPixels(0);
                y = yoo;
            }
            else
            {
                x = xoo;
                y = scale.ToPixels(0);
            }

            if (ZeroPaint.ZIndex == 0) ZeroPaint.ZIndex = PaintConstants.AxisZeroPaintZIndex;
            cartesianChart.Canvas.AddDrawableTask(ZeroPaint, zone: CanvasZone.DrawMargin);

            if (_zeroLine is null)
            {
                _zeroLine = new TLineGeometry();
                InitializeLine(_zeroLine, cartesianChart);
                UpdateSeparator(_zeroLine, x, y, lxi, lxj, lyi, lyj, UpdateMode.UpdateAndComplete);
            }
            ZeroPaint.AddGeometryToPaintTask(cartesianChart.Canvas, _zeroLine);

            UpdateSeparator(_zeroLine, x, y, lxi, lxj, lyi, lyj, UpdateMode.Update);
        }

        if (TicksPaint is not null && TicksPaint != Paint.Default && DrawTicksPath)
        {
            if (_ticksPath is null)
            {
                _ticksPath = new TLineGeometry();
                InitializeLine(_ticksPath, cartesianChart);
            }
            TicksPaint.AddGeometryToPaintTask(cartesianChart.Canvas, _ticksPath);

            if (_orientation == AxisOrientation.X)
            {
                var yp = yoo + _size.Height * 0.5f * (Position == AxisPosition.Start ? -1 : 1);
                _ticksPath.X = lxi;
                _ticksPath.X1 = lxj;
                _ticksPath.Y = yp;
                _ticksPath.Y1 = yp;
            }
            else
            {
                var xp = xoo + _size.Width * 0.5f * (Position == AxisPosition.Start ? 1 : -1);
                _ticksPath.X = xp;
                _ticksPath.X1 = xp;
                _ticksPath.Y = lyi;
                _ticksPath.Y1 = lyj;
            }

            _ticksPath.CompleteTransition(null);
        }
        if (TicksPaint is not null && TicksPaint != Paint.Default && _ticksPath is not null && !DrawTicksPath)
            TicksPaint.RemoveGeometryFromPaintTask(cartesianChart.Canvas, _ticksPath);

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

        foreach (var i in EnumerateSeparators(start, max, s))
        {
            // is outside the visible data bounds, is use full because we normally render
            // from min - s to max + s, this is usefull to correctly render subseparators and subticks
            // but labels out we use this to avoid rendering labels outside the visible data bounds.
            var isOutside = i < min || i > max;
            var separatorKey = Labelers.SixRepresentativeDigits(i - 1d + 1d);
            var labelContent = i < min || i > max ? string.Empty : TryGetLabelOrLogError(labeler, i - 1d + 1d);

            float x, y;
            if (_orientation == AxisOrientation.X)
            {
                x = scale.ToPixels(i);
                y = yoo;
            }
            else
            {
                x = xoo;
                y = scale.ToPixels(i);
            }

            float yc;
            float xc;

            if (_orientation == AxisOrientation.X)
            {
                xc = actualScale.ToPixels(i);
                yc = yoo;
            }
            else
            {
                xc = xoo;
                yc = actualScale.ToPixels(i);
            }

            if (!separators.TryGetValue(separatorKey, out var visualSeparator))
            {
                visualSeparator = new AxisVisualSeprator() { Value = i };
                separators.Add(separatorKey, visualSeparator);
            }

            #region Initialize shapes

            if (SeparatorsPaint is not null && SeparatorsPaint != Paint.Default && ShowSeparatorLines && visualSeparator.Separator is null)
            {
                InitializeSeparator(visualSeparator, cartesianChart);
                UpdateSeparator(
                    visualSeparator.Separator!, xc + sxco, yc + syco, lxi, lxj, lyi, lyj,
                    UpdateMode.UpdateAndComplete);
            }
            if (SubseparatorsPaint is not null && SubseparatorsPaint != Paint.Default && ShowSeparatorLines &&
                (visualSeparator.Subseparators is null || visualSeparator.Subseparators.Length == 0))
            {
                InitializeSubseparators(visualSeparator, cartesianChart);
                UpdateSubseparators(
                    visualSeparator.Subseparators!, actualScale, s, xc + sxco, yc + syco, lxi, lxj, lyi, lyj,
                    UpdateMode.UpdateAndComplete);
            }
            if (TicksPaint is not null && TicksPaint != Paint.Default && visualSeparator.Tick is null)
            {
                InitializeTick(visualSeparator, cartesianChart);
                UpdateTick(visualSeparator.Tick!, _tickLength, xc + txco, yc + tyco, UpdateMode.UpdateAndComplete);
            }
            if (SubticksPaint is not null && SubticksPaint != Paint.Default && SubseparatorsCount > 0 &&
                (visualSeparator.Subticks is null || visualSeparator.Subticks.Length == 0))
            {
                InitializeSubticks(visualSeparator, cartesianChart);
                UpdateSubticks(visualSeparator.Subticks!, actualScale, s, xc + txco, yc + tyco, UpdateMode.UpdateAndComplete);
            }
            if (LabelsPaint is not null && LabelsPaint != Paint.Default && visualSeparator.Label is null)
            {
                IntializeLabel(visualSeparator, cartesianChart, size, hasRotation, r);
                UpdateLabel(
                    visualSeparator.Label!, xc, yc, TryGetLabelOrLogError(labeler, i - 1d + 1d), hasRotation, r,
                    UpdateMode.UpdateAndComplete);
            }

            #endregion

            if (SeparatorsPaint is not null && SeparatorsPaint != Paint.Default && visualSeparator.Separator is not null)
            {
                if (ShowSeparatorLines)
                    SeparatorsPaint.AddGeometryToPaintTask(cartesianChart.Canvas, visualSeparator.Separator);
                else
                    SeparatorsPaint.RemoveGeometryFromPaintTask(cartesianChart.Canvas, visualSeparator.Separator);
            }

            if (SubseparatorsPaint is not null && SubseparatorsPaint != Paint.Default && visualSeparator.Subseparators is not null)
                if (ShowSeparatorLines)
                    foreach (var subtick in visualSeparator.Subseparators)
                        SubseparatorsPaint.AddGeometryToPaintTask(cartesianChart.Canvas, subtick);
                else
                    foreach (var subtick in visualSeparator.Subseparators)
                        SubseparatorsPaint.RemoveGeometryFromPaintTask(cartesianChart.Canvas, subtick);

            if (LabelsPaint is not null && LabelsPaint != Paint.Default && visualSeparator.Label is not null)
                LabelsPaint.AddGeometryToPaintTask(cartesianChart.Canvas, visualSeparator.Label);
            if (TicksPaint is not null && TicksPaint != Paint.Default && visualSeparator.Tick is not null)
                TicksPaint.AddGeometryToPaintTask(cartesianChart.Canvas, visualSeparator.Tick);
            if (SubticksPaint is not null && SubticksPaint != Paint.Default && visualSeparator.Subticks is not null)
                foreach (var subtick in visualSeparator.Subticks)
                    SubticksPaint.AddGeometryToPaintTask(cartesianChart.Canvas, subtick);

            if (visualSeparator.Separator is not null)
                UpdateSeparator(visualSeparator.Separator, x + sxco, y + syco, lxi, lxj, lyi, lyj, UpdateMode.Update);
            if (visualSeparator.Subseparators is not null)
                UpdateSubseparators(visualSeparator.Subseparators, scale, s, x + sxco, y + tyco, lxi, lxj, lyi, lyj, UpdateMode.Update);
            if (visualSeparator.Tick is not null)
                UpdateTick(visualSeparator.Tick, _tickLength, x + txco, y + tyco, UpdateMode.Update);
            if (visualSeparator.Subticks is not null)
                UpdateSubticks(visualSeparator.Subticks, scale, s, x + txco, y + tyco, UpdateMode.Update);
            if (visualSeparator.Label is not null)
            {
                UpdateLabel(visualSeparator.Label, x, y + tyco, labelContent, hasRotation, r, UpdateMode.Update);
                visualSeparator.Label.Opacity = isOutside ? 0f : 1f;
            }

            if (hasActivePaint) _ = measured.Add(visualSeparator);
        }

        foreach (var separatorValueKey in separators.ToArray())
        {
            var separator = separatorValueKey.Value;
            if (measured.Contains(separator)) continue;

            float x, y;
            if (_orientation == AxisOrientation.X)
            {
                x = scale.ToPixels(separator.Value);
                y = yoo;
            }
            else
            {
                x = xoo;
                y = scale.ToPixels(separator.Value);
            }

            if (separator.Separator is not null)
                UpdateSeparator(separator.Separator, x + sxco, y + syco, lxi, lxj, lyi, lyj, UpdateMode.UpdateAndRemove);
            if (separator.Subseparators is not null)
                UpdateSubseparators(
                    separator.Subseparators, scale, s, x + sxco, y + syco, lxi, lxj, lyi, lyj, UpdateMode.UpdateAndRemove);
            if (separator.Tick is not null)
                UpdateTick(separator.Tick, _tickLength, x + txco, y + tyco, UpdateMode.UpdateAndRemove);
            if (separator.Subticks is not null)
                UpdateSubticks(separator.Subticks, scale, s, x + txco, y + tyco, UpdateMode.UpdateAndRemove);
            if (separator.Label is not null)
                UpdateLabel(
                    separator.Label, x, y + tyco, TryGetLabelOrLogError(labeler, separator.Value - 1d + 1d), hasRotation, r,
                    UpdateMode.UpdateAndRemove);

            _ = separators.Remove(separatorValueKey.Key);
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
        if (CustomSeparators is not null)
        {
            foreach (var s in CustomSeparators)
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
        }

        if (double.IsInfinity(minZoomDelta))
        {
            // at this point the chart data is empty...
            // force the limits to the known bounds

            minZoomDelta = max - min;
            mind = min;
            maxd = max;
        }

        return new(min, max, minZoomDelta, mind, maxd);
    }

    /// <inheritdoc cref="ICartesianAxis.SetLimits(double, double, double, bool, bool)"/>
    public void SetLimits(double min, double max, double step = -1, bool propagateShared = true, bool notify = false)
    {
        var shared = propagateShared ? (SharedWith ?? []) : [];

        foreach (var axis in shared)
            axis.SetLimits(min, max, step, false, notify);

        if (notify)
        {
            MinLimit = min;
            MaxLimit = max;

            if (step > 0)
            {
                ForceStepToMin = true;
                MinStep = step;
            }
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
    }

    /// <inheritdoc cref="IChartElement.RemoveFromUI(Chart)"/>
    public override void RemoveFromUI(Chart chart)
    {
        base.RemoveFromUI(chart);
        _ = activeSeparators.Remove(chart);
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
        [SeparatorsPaint, LabelsPaint, NamePaint, ZeroPaint, TicksPaint, SubticksPaint, SubseparatorsPaint];

    private Func<double, string> GetActualLabeler()
    {
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
