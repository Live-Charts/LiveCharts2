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
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.Motion;
using LiveChartsCore.Painting;

namespace LiveChartsCore;

/// <summary>
/// Defines a pie series.
/// </summary>
/// <typeparam name="TModel">The type of the model.</typeparam>
/// <typeparam name="TVisual">The type of the visual.</typeparam>
/// <typeparam name="TLabel">The type of the label.</typeparam>
/// <typeparam name="TMiniatureGeometry">The type of the miniature geometry, used in tool tips and legends.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="CorePieSeries{TModel, TVisual, TLabel, TMiniatureGeometry}"/> class.
/// </remarks>
public abstract class CorePieSeries<TModel, TVisual, TLabel, TMiniatureGeometry>(
    IReadOnlyCollection<TModel>? values,
    bool isGauge = false,
    bool isGaugeFill = false)
        : Series<TModel, TVisual, TLabel>(GetProperties(isGauge, isGaugeFill), values), IPieSeries
            where TVisual : BaseDoughnutGeometry, new()
            where TLabel : BaseLabelGeometry, new()
            where TMiniatureGeometry : BoundedDrawnGeometry, new()
{
    private Func<ChartPoint, string>? _tooltipLabelFormatter;

    /// <summary>
    /// Gets or sets the stroke.
    /// </summary>
    /// <value>
    /// The stroke.
    /// </value>
    public Paint? Stroke
    {
        get;
        set => SetPaintProperty(ref field, value, PaintStyle.Stroke);
    } = null;

    /// <summary>
    /// Gets or sets the fill.
    /// </summary>
    /// <value>
    /// The fill.
    /// </value>
    public Paint? Fill
    {
        get;
        set => SetPaintProperty(ref field, value);
    } = null;

    /// <inheritdoc cref="IPieSeries.Pushout"/>
    public double Pushout { get; set => SetProperty(ref field, value); } = 0;

    /// <inheritdoc cref="IPieSeries.InnerRadius"/>
    public double InnerRadius { get; set => SetProperty(ref field, value); } = 0;

    /// <inheritdoc cref="IPieSeries.OuterRadiusOffset"/>
    public double OuterRadiusOffset { get; set => SetProperty(ref field, value); } = 0;

    /// <inheritdoc cref="IPieSeries.HoverPushout"/>
    public double HoverPushout { get; set => SetProperty(ref field, value); } = 20;

    /// <inheritdoc cref="IPieSeries.RelativeInnerRadius"/>
    public double RelativeInnerRadius { get; set => SetProperty(ref field, value); } = 0;

    /// <inheritdoc cref="IPieSeries.RelativeOuterRadius"/>
    public double RelativeOuterRadius { get; set => SetProperty(ref field, value); } = 0;

    /// <inheritdoc cref="IPieSeries.MaxRadialColumnWidth"/>
    public double MaxRadialColumnWidth { get; set => SetProperty(ref field, value); } = double.MaxValue;

    /// <inheritdoc cref="IPieSeries.RadialAlign"/>
    public RadialAlignment RadialAlign { get; set => SetProperty(ref field, value); } = RadialAlignment.Outer;

    /// <inheritdoc cref="IPieSeries.CornerRadius"/>
    public double CornerRadius { get; set => SetProperty(ref field, value); } = 0;

    /// <inheritdoc cref="IPieSeries.InvertedCornerRadius"/>
    public bool InvertedCornerRadius { get; set => SetProperty(ref field, value); } = false;

    /// <inheritdoc cref="IPieSeries.IsFillSeries"/>
    public bool IsFillSeries { get; set => SetProperty(ref field, value); }

    /// <inheritdoc cref="IPieSeries.IsRelativeToMinValue"/>
    public bool IsRelativeToMinValue { get; set => SetProperty(ref field, value); }

    /// <inheritdoc cref="IPieSeries.DataLabelsPosition"/>
    public PolarLabelsPosition DataLabelsPosition { get; set => SetProperty(ref field, value); } = PolarLabelsPosition.Middle;

    /// <summary>
    /// Gets or sets the tool tip label formatter for the Y axis, this function will build the label when a point in this series 
    /// is shown inside a tool tip.
    /// </summary>
    /// <value>
    /// The tool tip label formatter.
    /// </value>
    public Func<ChartPoint<TModel, TVisual, TLabel>, string>? ToolTipLabelFormatter
    {
        get => _tooltipLabelFormatter;
        set => ((IPieSeries)this).TooltipLabelFormatter = value is null ? null : p => value(ConvertToTypedChartPoint(p));
    }

    Func<ChartPoint, string>? IPieSeries.TooltipLabelFormatter
    {
        get => _tooltipLabelFormatter;
        set => SetProperty(ref _tooltipLabelFormatter, value);
    }

    /// <inheritdoc cref="ChartElement.Invalidate(Chart)"/>
    public override void Invalidate(Chart chart)
    {
        var pieChart = (PieChartEngine)chart;
        _ = GetAnimation(pieChart);

        var drawLocation = pieChart.DrawMarginLocation;
        var drawMarginSize = pieChart.DrawMarginSize;
        var minDimension = drawMarginSize.Width < drawMarginSize.Height ? drawMarginSize.Width : drawMarginSize.Height;

        var maxPushout = (float)pieChart.PushoutBounds.Max;
        var pushout = (float)Pushout;
        var innerRadius = (float)InnerRadius;

        minDimension = minDimension - (Stroke?.StrokeThickness ?? 0) * 2 - maxPushout * 2;

        var pieLabelsCorrection = chart.SeriesContext.GetPieOuterLabelsSpace<TLabel>();
        minDimension -= pieLabelsCorrection;

        var outerRadiusOffset = (float)OuterRadiusOffset;
        minDimension -= outerRadiusOffset;

        var view = (IPieChartView)pieChart.View;
        var initialRotation = (float)Math.Truncate(view.InitialRotation);
        var completeAngle = (float)view.MaxAngle;

        var startValue = view.MinValue;
        double? chartTotal = double.IsNaN(view.MaxValue) ? null : view.MaxValue;

        var actualZIndex = ZIndex == 0 ? ((ISeries)this).SeriesId : ZIndex;
        if (Fill is not null && Fill != Paint.Default)
        {
            Fill.ZIndex = actualZIndex + PaintConstants.SeriesFillZIndexOffset;
            pieChart.Canvas.AddDrawableTask(Fill, zone: CanvasZone.DrawMargin);
        }
        if (Stroke is not null && Stroke != Paint.Default)
        {
            Stroke.ZIndex = actualZIndex + PaintConstants.SeriesStrokeZIndexOffset;
            pieChart.Canvas.AddDrawableTask(Stroke, zone: CanvasZone.DrawMargin);
        }
        if (ShowDataLabels && DataLabelsPaint is not null && DataLabelsPaint != Paint.Default)
        {
            DataLabelsPaint.ZIndex = PaintConstants.PieSeriesDataLabelsBaseZIndex + actualZIndex + PaintConstants.SeriesGeometryFillZIndexOffset;
            // this does not require clipping...
            //DataLabelsPaint.SetClipRectangle(pieChart.Canvas, new LvcRectangle(drawLocation, drawMarginSize));
            pieChart.Canvas.AddDrawableTask(DataLabelsPaint);
        }

        var cx = drawLocation.X + drawMarginSize.Width * 0.5f;
        var cy = drawLocation.Y + drawMarginSize.Height * 0.5f;

        var dls = (float)DataLabelsSize;
        var stacker = pieChart.SeriesContext.GetStackPosition(this, GetStackGroup())
            ?? throw new NullReferenceException("Unexpected null stacker");
        var pointsCleanup = ChartPointCleanupContext.For(everFetched);

        var fetched = Fetch(pieChart).ToArray();

        var stackedInnerRadius = innerRadius;
        var relativeInnerRadius = (float)RelativeInnerRadius;
        var relativeOuterRadius = (float)RelativeOuterRadius;
        var maxRadialWidth = (float)MaxRadialColumnWidth;
        var cornerRadius = (float)CornerRadius;

        var mdc = minDimension;
        var wc = mdc - (mdc - 2 * innerRadius) * (fetched.Length - 1) / fetched.Length - relativeOuterRadius * 2;

        if (wc * 0.5f - stackedInnerRadius > maxRadialWidth)
        {
            var dw = wc * 0.5f - stackedInnerRadius - maxRadialWidth;

            switch (RadialAlign)
            {
                case RadialAlignment.Outer:
                    relativeOuterRadius = 0;
                    relativeInnerRadius = dw;
                    break;
                case RadialAlignment.Center:
                    relativeOuterRadius = dw * 0.5f;
                    relativeInnerRadius = dw * 0.5f;
                    break;
                case RadialAlignment.Inner:
                    relativeOuterRadius = dw;
                    relativeInnerRadius = 0;
                    break;
                default:
                    throw new NotImplementedException($"The alignment {RadialAlign} is not supported.");
            }
        }

        var r = (float)DataLabelsRotation;
        var isTangent = false;
        var isCotangent = false;

        if (((int)r & (int)LiveCharts.TangentAngle) != 0)
        {
            r -= (int)LiveCharts.TangentAngle;
            isTangent = true;
        }

        if (((int)r & (int)LiveCharts.CotangentAngle) != 0)
        {
            r -= (int)LiveCharts.CotangentAngle;
            isCotangent = true;
        }

        var i = 1f;
        var isClockWise = view.IsClockwise;

        var isFirstDraw = !chart.IsDrawn(((ISeries)this).SeriesId);

        foreach (var point in fetched)
        {
            var coordinate = point.Coordinate;
            var visual = point.Context.Visual as TVisual;

            if (point.IsEmpty || !IsVisible)
            {
                if (visual is not null)
                {
                    visual.CenterX = cx;
                    visual.CenterY = cy;
                    visual.X = cx;
                    visual.Y = cy;
                    visual.Width = 0;
                    visual.Height = 0;
                    visual.SweepAngle = 0;
                    visual.StartAngle = initialRotation;
                    visual.PushOut = 0;
                    visual.InnerRadius = 0;
                    visual.CornerRadius = 0;
                    visual.RemoveOnCompleted = true;
                    point.Context.Visual = null;
                }

                if (point.Context.Label is not null)
                {
                    var label = (TLabel)point.Context.Label;

                    label.X = cx;
                    label.Y = cy;
                    label.Opacity = 0;
                    label.RemoveOnCompleted = true;

                    point.Context.Label = null;
                }

                pointsCleanup.Clean(point);

                var md2 = minDimension;
                var w2 = md2 - (md2 - 2 * innerRadius) * (fetched.Length - i) / fetched.Length - relativeOuterRadius * 2;
                stackedInnerRadius = (w2 + relativeOuterRadius * 2) * 0.5f;
                i++;
                continue;
            }

            var stack = stacker.GetStack(point);
            var stackedValue = stack.Start;
            var total = chartTotal ?? stack.Total;

            double start, sweep;

            if (total == 0)
            {
                start = 0;
                sweep = 0;
            }
            else
            {
                // Clamp the stack so a value greater than the chart's effective range
                // cannot produce a sweep larger than the chart's complete angle, which
                // renders as a broken arc (issue #2131). The cap differs per branch
                // because each one uses a different denominator for the angle math.
                if (IsRelativeToMinValue)
                {
                    // when the series is relative to min value,
                    // the start value is always the PieChart.MinValue
                    // this is normally used on angular gauge series.
                    var h = total - startValue;
                    var clampedStackedValue = Math.Min(stackedValue, h);
                    var clampedTop = Math.Min(stackedValue + coordinate.PrimaryValue, h);
                    var h1 = clampedTop;
                    start = clampedStackedValue / h * completeAngle;
                    sweep = h1 / h * completeAngle - start;
                    if (!isClockWise) start = completeAngle - start - sweep;
                }
                else
                {
                    var clampedStackedValue = Math.Min(stackedValue, total);
                    var clampedTop = Math.Min(stackedValue + coordinate.PrimaryValue, total);
                    var h = total - startValue;
                    var h1 = clampedTop - startValue;
                    start = clampedStackedValue / total * completeAngle;
                    sweep = h1 / h * completeAngle - start;
                    if (!isClockWise) start = completeAngle - start - sweep;
                }
            }

            if (IsFillSeries)
            {
                start = 0;
                sweep = completeAngle - 0.1f;
            }

            if (visual is null)
            {
                var p = new TVisual
                {
                    CenterX = cx,
                    CenterY = cy,
                    X = cx,
                    Y = cy,
                    Width = 0,
                    Height = 0,
                    StartAngle = (float)(isFirstDraw ? initialRotation : start + initialRotation),
                    SweepAngle = 0,
                    PushOut = 0,
                    InnerRadius = 0,
                    CornerRadius = 0
                };

                visual = p;
                point.Context.Visual = visual;
                OnPointCreated(point);

                _ = everFetched.Add(point);
            }

            if (Fill is not null && Fill != Paint.Default)
                Fill.AddGeometryToPaintTask(pieChart.Canvas, visual);
            if (Stroke is not null && Stroke != Paint.Default)
                Stroke.AddGeometryToPaintTask(pieChart.Canvas, visual);

            var dougnutGeometry = visual;

            stackedInnerRadius += relativeInnerRadius;

            var md = minDimension;
            var stackedOuterRadius = md - (md - 2 * innerRadius) * (fetched.Length - i) / fetched.Length - relativeOuterRadius * 2;

            dougnutGeometry.CenterX = cx;
            dougnutGeometry.CenterY = cy;
            dougnutGeometry.X = drawLocation.X + (drawMarginSize.Width - stackedOuterRadius) * 0.5f;
            dougnutGeometry.Y = drawLocation.Y + (drawMarginSize.Height - stackedOuterRadius) * 0.5f;
            dougnutGeometry.Width = stackedOuterRadius;
            dougnutGeometry.Height = stackedOuterRadius;
            dougnutGeometry.InnerRadius = stackedInnerRadius;
            dougnutGeometry.PushOut = pushout;
            dougnutGeometry.StartAngle = (float)(start + initialRotation);
            dougnutGeometry.SweepAngle = (float)sweep;
            dougnutGeometry.CornerRadius = cornerRadius;
            dougnutGeometry.InvertedCornerRadius = InvertedCornerRadius;
            dougnutGeometry.RemoveOnCompleted = false;

            if (start + initialRotation == initialRotation && sweep == 360)
                dougnutGeometry.SweepAngle = 359.99f;

            if (point.Context.HoverArea is not SemicircleHoverArea ha)
                point.Context.HoverArea = ha = new SemicircleHoverArea();

            _ = ha.SetDimensions(
                cx, cy, (float)(start + initialRotation), (float)(start + initialRotation + sweep),
                stackedInnerRadius, stackedOuterRadius);

            pointsCleanup.Clean(point);

            if (ShowDataLabels && DataLabelsPaint is not null && DataLabelsPaint != Paint.Default && !IsFillSeries)
            {
                var label = (TLabel?)point.Context.Label;

                var middleAngle = (float)(start + initialRotation + sweep * 0.5);

                var actualRotation = r +
                        (isTangent ? middleAngle - 90 : 0) +
                        (isCotangent ? middleAngle : 0);

                if ((isTangent || isCotangent) && ((actualRotation + 90) % 360) > 180)
                    actualRotation += 180;

                if (label is null)
                {
                    var l = new TLabel { X = cx, Y = cy, RotateTransform = actualRotation, MaxWidth = (float)DataLabelsMaxWidth };
                    l.Animate(
                        GetAnimation(chart),
                        BaseLabelGeometry.XProperty,
                        BaseLabelGeometry.YProperty);
                    label = l;
                    point.Context.Label = l;
                }

                DataLabelsPaint.AddGeometryToPaintTask(pieChart.Canvas, label);

                label.Text = DataLabelsFormatter(new ChartPoint<TModel, TVisual, TLabel>(point));
                label.TextSize = dls;
                label.Padding = DataLabelsPadding;
                label.RotateTransform = actualRotation;
                label.Paint = DataLabelsPaint;

                AlignLabel(label, (float)start, initialRotation, sweep);

                if (isFirstDraw)
                    label.CompleteTransition(
                        BaseLabelGeometry.TextSizeProperty,
                        BaseLabelGeometry.XProperty,
                        BaseLabelGeometry.YProperty,
                        BaseLabelGeometry.RotateTransformProperty);

                var labelPosition = GetLabelPolarPosition(
                    cx,
                    cy,
                    stackedInnerRadius,
                    (stackedOuterRadius + relativeOuterRadius * 2) * 0.5f,
                    (float)(start + initialRotation),
                    (float)sweep,
                    label.Measure(),
                    DataLabelsPosition);

                label.X = labelPosition.X;
                label.Y = labelPosition.Y;
            }

            OnPointMeasured(point);

            stackedInnerRadius = (stackedOuterRadius + relativeOuterRadius * 2) * 0.5f;
            i++;
        }

        var u = new Scaler(); // dummy scaler, this is not used in the SoftDeleteOrDisposePoint method.
        pointsCleanup.CollectPoints(everFetched, pieChart.View, u, u, SoftDeleteOrDisposePoint);
    }

    /// <inheritdoc cref="IPieSeries.GetBounds(Chart)"/>
    public virtual DimensionalBounds GetBounds(Chart chart) =>
        DataFactory.GetPieBounds(chart, this).Bounds;

    /// <inheritdoc cref="Series{TModel, TVisual, TLabel}.GetMiniatureGeometry(ChartPoint?)"/>
    public override IDrawnElement GetMiniatureGeometry(ChartPoint? point)
    {
        var v = point?.Context.Visual;

        var m = new TMiniatureGeometry
        {
            Fill = v?.Fill ?? Fill,
            Stroke = v?.Stroke ?? Stroke,
            StrokeThickness = (float)MiniatureStrokeThickness,
            ClippingBounds = LvcRectangle.Empty,
            Width = (float)MiniatureShapeSize,
            Height = (float)MiniatureShapeSize
        };

        if (m is IVariableSvgPath svg) svg.SVGPath = GeometrySvg;

        return m;
    }

    /// <inheritdoc cref="ISeries.GetPrimaryToolTipText(ChartPoint)"/>
    public override string? GetPrimaryToolTipText(ChartPoint point)
    {
        return ToolTipLabelFormatter is not null
            ? ToolTipLabelFormatter(new ChartPoint<TModel, TVisual, TLabel>(point))
            : point.Coordinate.PrimaryValue.ToString();
    }

    /// <inheritdoc cref="ISeries.GetSecondaryToolTipText(ChartPoint)"/>
    public override string? GetSecondaryToolTipText(ChartPoint point) =>
        LiveCharts.IgnoreToolTipLabel;

    /// <summary>
    /// GEts the stack group
    /// </summary>
    /// <returns></returns>
    /// <inheritdoc />
    public override int GetStackGroup() =>
        0;

    /// <inheritdoc cref="ChartElement.GetPaintTasks"/>
    protected internal override Paint?[] GetPaintTasks() =>
        [Fill, Stroke, DataLabelsPaint];

    /// <summary>
    /// Sets the default point transitions.
    /// </summary>
    /// <param name="chartPoint">The chart point.</param>
    /// <exception cref="Exception">Unable to initialize the point instance.</exception>
    protected override void SetDefaultPointTransitions(ChartPoint chartPoint)
    {
        if (IsFillSeries) return;
        var chart = chartPoint.Context.Chart;
        if (chartPoint.Context.Visual is not TVisual visual) throw new Exception("Unable to initialize the point instance.");

        var animation = GetAnimation(chart.CoreChart);

        if ((SeriesProperties & SeriesProperties.Gauge) == 0)
            visual.Animate(animation);
        else
            visual.Animate(
                animation,
                BaseDoughnutGeometry.StartAngleProperty,
                BaseDoughnutGeometry.SweepAngleProperty);
    }

    /// <summary>
    /// Softs the delete point.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="primaryScale">The primary scale.</param>
    /// <param name="secondaryScale">The secondary scale.</param>
    protected virtual void SoftDeleteOrDisposePoint(ChartPoint point, Scaler primaryScale, Scaler secondaryScale)
    {
        var visual = (TVisual?)point.Context.Visual;
        if (visual is null) return;
        if (DataFactory is null) throw new Exception("Data provider not found");

        visual.StartAngle += visual.SweepAngle;
        visual.SweepAngle = 0;
        visual.CornerRadius = 0;
        visual.RemoveOnCompleted = true;

        DataFactory.DisposePoint(point);

        var label = (TLabel?)point.Context.Label;
        if (label is null) return;

        label.TextSize = 1;
        label.RemoveOnCompleted = true;
    }

    /// <summary>
    /// Gets the label polar position.
    /// </summary>
    /// <param name="centerX">The center x.</param>
    /// <param name="centerY">The center y.</param>
    /// <param name="innerRadius">The iner radius.</param>
    /// <param name="outerRadius">The outer radius.</param>
    /// <param name="startAngle">The start angle.</param>
    /// <param name="sweepAngle">The sweep angle.</param>
    /// <param name="labelSize">Size of the label.</param>
    /// <param name="position">The position.</param>
    /// <returns></returns>
    protected virtual LvcPoint GetLabelPolarPosition(
        float centerX,
        float centerY,
        float innerRadius,
        float outerRadius,
        float startAngle,
        float sweepAngle,
        LvcSize labelSize,
        PolarLabelsPosition position)
    {
        float angle = 0, radius = 0;

        switch (position)
        {
            case PolarLabelsPosition.End:
                angle = startAngle + sweepAngle;
                radius = innerRadius + (outerRadius - innerRadius) * 0.5f;
                break;
            case PolarLabelsPosition.Start:
                angle = startAngle;
                radius = innerRadius + (outerRadius - innerRadius) * 0.5f;
                break;
            case PolarLabelsPosition.Outer:
                angle = startAngle + sweepAngle * 0.5f;
                radius = outerRadius + 0.45f * (float)Math.Sqrt(Math.Pow(labelSize.Width, 2) + Math.Pow(labelSize.Height, 2));
                break;
            case PolarLabelsPosition.Middle:
                var f = (SeriesProperties & SeriesProperties.Gauge) != 0 ? 0.5f : 0.65f;
                angle = startAngle + sweepAngle * 0.5f;
                radius = innerRadius + (outerRadius - innerRadius) * f;
                break;
            case PolarLabelsPosition.ChartCenter:
                return new LvcPoint(centerX, centerY);
            default:
                break;
        }

        const float toRadians = (float)(Math.PI / 180);
        angle *= toRadians;

        return new LvcPoint(
             (float)(centerX + Math.Cos(angle) * radius),
             (float)(centerY + Math.Sin(angle) * radius));
    }

    /// <summary>
    /// Softly deletes the all points from the chart.
    /// </summary>
    /// <param name="chart"></param>
    /// <inheritdoc cref="ISeries.SoftDeleteOrDispose" />
    public override void SoftDeleteOrDispose(IChartView chart)
    {
        var core = ((IPieChartView)chart).Core;
        var u = new Scaler();

        var toDelete = new List<ChartPoint>();
        foreach (var point in everFetched)
        {
            if (point.Context.Chart != chart) continue;
            SoftDeleteOrDisposePoint(point, u, u);
            toDelete.Add(point);
        }

        foreach (var pt in GetPaintTasks())
        {
            if (pt is not null) core.Canvas.RemovePaintTask(pt);
        }

        foreach (var item in toDelete) _ = everFetched.Remove(item);
    }

    private void AlignLabel(TLabel label, double start, double initialRotation, double sweep)
    {
        switch (DataLabelsPosition)
        {
            case PolarLabelsPosition.Middle:
            case PolarLabelsPosition.ChartCenter:
            case PolarLabelsPosition.Outer:
                label.HorizontalAlign = Align.Middle;
                label.VerticalAlign = Align.Middle;
                break;
            case PolarLabelsPosition.End:
                var a = start + initialRotation + sweep;
                a %= 360;
                if (a < 0) a += 360;
                var c = 90;
                if (a > 180) c = -90;
                label.HorizontalAlign = a > 180 ? Align.Start : Align.End;
                label.RotateTransform = (float)(a - c);
                break;
            case PolarLabelsPosition.Start:
                var a1 = start + initialRotation;
                a1 %= 360;
                if (a1 < 0) a1 += 360;
                var c1 = 90;
                if (a1 > 180) c1 = -90;
                label.HorizontalAlign = a1 > 180 ? Align.End : Align.Start;
                label.RotateTransform = (float)(a1 - c1);
                break;
            default:
                break;
        }
    }

    private static SeriesProperties GetProperties(bool isGauge = false, bool isGaugeFill = false)
    {
        return SeriesProperties.PieSeries | SeriesProperties.Stacked | SeriesProperties.Solid |
            (isGauge ? SeriesProperties.Gauge : 0) |
            (isGaugeFill ? SeriesProperties.GaugeFill : 0);
    }
}
