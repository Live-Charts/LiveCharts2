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
    public Paint? Stroke
    {
        get;
        set => SetPaintProperty(ref field, value, PaintStyle.Stroke);
    } = null;

    /// <summary>
    /// Gets or sets the fill.
    /// </summary>
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

    // ---- template method ----------------------------------------------------

    /// <summary>
    /// Builds a per-frame measure context from the chart. Resolves the available
    /// radial space (subtracting stroke / pushout / outer-label correction),
    /// rotation parameters, and applies the <see cref="RadialAlign"/> clamp when
    /// the computed radial width exceeds <see cref="MaxRadialColumnWidth"/>.
    /// </summary>
    protected virtual PieMeasureContext BeginMeasure(PieChartEngine chart, int sliceCount)
    {
        var drawLocation = chart.DrawMarginLocation;
        var drawMarginSize = chart.DrawMarginSize;
        var minDimension = drawMarginSize.Width < drawMarginSize.Height ? drawMarginSize.Width : drawMarginSize.Height;

        var maxPushout = (float)chart.PushoutBounds.Max;
        var pushout = (float)Pushout;
        var innerRadius = (float)InnerRadius;

        minDimension = minDimension - (Stroke?.StrokeThickness ?? 0) * 2 - maxPushout * 2;

        var pieLabelsCorrection = chart.SeriesContext.GetPieOuterLabelsSpace<TLabel>();
        minDimension -= pieLabelsCorrection;

        var outerRadiusOffset = (float)OuterRadiusOffset;
        minDimension -= outerRadiusOffset;

        var view = (IPieChartView)chart.View;
        var initialRotation = (float)Math.Truncate(view.InitialRotation);
        var completeAngle = (float)view.MaxAngle;

        var startValue = view.MinValue;
        double? chartTotal = double.IsNaN(view.MaxValue) ? null : view.MaxValue;

        var stackedInnerRadius = innerRadius;
        var relativeInnerRadius = (float)RelativeInnerRadius;
        var relativeOuterRadius = (float)RelativeOuterRadius;
        var maxRadialWidth = (float)MaxRadialColumnWidth;
        var cornerRadius = (float)CornerRadius;

        var mdc = minDimension;
        var wc = mdc - (mdc - 2 * innerRadius) * (sliceCount - 1) / sliceCount - relativeOuterRadius * 2;

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

        var stacker = chart.SeriesContext.GetStackPosition(this, GetStackGroup())
            ?? throw new NullReferenceException("Unexpected null stacker");

        var cx = drawLocation.X + drawMarginSize.Width * 0.5f;
        var cy = drawLocation.Y + drawMarginSize.Height * 0.5f;

        var isFirstDraw = !((Chart)chart).IsDrawn(((ISeries)this).SeriesId);

        return new PieMeasureContext(
            chart, cx, cy,
            minDimension: minDimension,
            innerRadius: innerRadius,
            pushOut: pushout,
            relativeInnerRadius: relativeInnerRadius,
            relativeOuterRadius: relativeOuterRadius,
            cornerRadius: cornerRadius,
            initialRotation: initialRotation,
            completeAngle: completeAngle,
            startValue: startValue,
            chartTotal: chartTotal,
            isClockWise: view.IsClockwise,
            stacker: stacker,
            sliceCount: sliceCount,
            isFirstDraw: isFirstDraw,
            drawLocation: drawLocation,
            drawMarginSize: drawMarginSize,
            dataLabelsSize: (float)DataLabelsSize);
    }

    /// <summary>
    /// Computes the per-slice angular and radial geometry. Takes the current
    /// running <paramref name="stackedInnerRadius"/> and <paramref name="sliceIndex"/>
    /// (1-based) so the radial math can stack outward through nested rings.
    /// </summary>
    protected virtual PieLayout MeasurePieLayout(
        ChartPoint point,
        float stackedInnerRadius,
        int sliceIndex,
        in PieMeasureContext ctx)
    {
        var coordinate = point.Coordinate;

        var stack = ctx.Stacker.GetStack(point);
        var stackedValue = stack.Start;
        var total = ctx.ChartTotal ?? stack.Total;

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
                var h = total - ctx.StartValue;
                var clampedStackedValue = Math.Min(stackedValue, h);
                var clampedTop = Math.Min(stackedValue + coordinate.PrimaryValue, h);
                var h1 = clampedTop;
                start = clampedStackedValue / h * ctx.CompleteAngle;
                sweep = h1 / h * ctx.CompleteAngle - start;
                if (!ctx.IsClockWise) start = ctx.CompleteAngle - start - sweep;
            }
            else
            {
                var clampedStackedValue = Math.Min(stackedValue, total);
                var clampedTop = Math.Min(stackedValue + coordinate.PrimaryValue, total);
                var h = total - ctx.StartValue;
                var h1 = clampedTop - ctx.StartValue;
                start = clampedStackedValue / total * ctx.CompleteAngle;
                sweep = h1 / h * ctx.CompleteAngle - start;
                if (!ctx.IsClockWise) start = ctx.CompleteAngle - start - sweep;
            }
        }

        if (IsFillSeries)
        {
            start = 0;
            sweep = ctx.CompleteAngle - 0.1f;
        }

        var md = ctx.MinDimension;
        var stackedOuterRadius = md - (md - 2 * ctx.InnerRadius) * (ctx.SliceCount - sliceIndex) / ctx.SliceCount - ctx.RelativeOuterRadius * 2;

        var sweepF = (float)sweep;
        // Issue #2131 — clamp a full-circle sweep just under 360 so the path stays
        // closed instead of degenerating into a broken arc.
        if ((float)start + ctx.InitialRotation == ctx.InitialRotation && sweep == 360)
            sweepF = 359.99f;

        return new PieLayout(
            centerX: ctx.CenterX,
            centerY: ctx.CenterY,
            x: ctx.DrawLocation.X + (ctx.DrawMarginSize.Width - stackedOuterRadius) * 0.5f,
            y: ctx.DrawLocation.Y + (ctx.DrawMarginSize.Height - stackedOuterRadius) * 0.5f,
            width: stackedOuterRadius,
            height: stackedOuterRadius,
            startAngle: (float)(start + ctx.InitialRotation),
            sweepAngle: sweepF,
            pushOut: ctx.PushOut,
            innerRadius: stackedInnerRadius,
            outerRadius: stackedOuterRadius,
            cornerRadius: ctx.CornerRadius);
    }

    /// <summary>
    /// Ensures the visual exists. On first creation initializes the slice at zero
    /// size at the chart center so the slice sweeps outward and grows.
    /// </summary>
    protected virtual TVisual EnsureVisualForPoint(ChartPoint point, double startAngle, in PieMeasureContext ctx)
    {
        var visual = point.Context.Visual as TVisual;
        if (visual is not null) return visual;

        var p = new TVisual
        {
            CenterX = ctx.CenterX,
            CenterY = ctx.CenterY,
            X = ctx.CenterX,
            Y = ctx.CenterY,
            Width = 0,
            Height = 0,
            StartAngle = (float)(ctx.IsFirstDraw ? ctx.InitialRotation : startAngle + ctx.InitialRotation),
            SweepAngle = 0,
            PushOut = 0,
            InnerRadius = 0,
            CornerRadius = 0,
        };

        point.Context.Visual = p;
        OnPointCreated(point);

        _ = everFetched.Add(point);

        return p;
    }

    /// <summary>
    /// Collapses the slice to zero size at the chart center for empty/invisible
    /// points so the slice fades / shrinks out cleanly.
    /// </summary>
    protected virtual void CollapseEmptyVisual(ChartPoint point, in PieMeasureContext ctx)
    {
        if (point.Context.Visual is TVisual visual)
        {
            visual.CenterX = ctx.CenterX;
            visual.CenterY = ctx.CenterY;
            visual.X = ctx.CenterX;
            visual.Y = ctx.CenterY;
            visual.Width = 0;
            visual.Height = 0;
            visual.SweepAngle = 0;
            visual.StartAngle = ctx.InitialRotation;
            visual.PushOut = 0;
            visual.InnerRadius = 0;
            visual.CornerRadius = 0;
            visual.RemoveOnCompleted = true;
            point.Context.Visual = null;
        }

        if (point.Context.Label is TLabel label)
        {
            label.X = ctx.CenterX;
            label.Y = ctx.CenterY;
            label.Opacity = 0;
            label.RemoveOnCompleted = true;
            point.Context.Label = null;
        }
    }

    /// <summary>
    /// Sets per-Z-index ordering on Fill / Stroke / DataLabelsPaint and registers
    /// them as drawable tasks. Data labels use a special base offset
    /// (PieSeriesDataLabelsBaseZIndex) so they always sit above slices, and don't
    /// clip to the draw margin.
    /// </summary>
    private void InitializePaints(PieChartEngine chart)
    {
        var actualZIndex = ZIndex == 0 ? ((ISeries)this).SeriesId : ZIndex;

        if (Fill is not null && Fill != Paint.Default)
        {
            Fill.ZIndex = actualZIndex + PaintConstants.SeriesFillZIndexOffset;
            chart.Canvas.AddDrawableTask(Fill, zone: CanvasZone.DrawMargin);
        }
        if (Stroke is not null && Stroke != Paint.Default)
        {
            Stroke.ZIndex = actualZIndex + PaintConstants.SeriesStrokeZIndexOffset;
            chart.Canvas.AddDrawableTask(Stroke, zone: CanvasZone.DrawMargin);
        }
        if (ShowDataLabels && DataLabelsPaint is not null && DataLabelsPaint != Paint.Default)
        {
            DataLabelsPaint.ZIndex = PaintConstants.PieSeriesDataLabelsBaseZIndex + actualZIndex + PaintConstants.SeriesGeometryFillZIndexOffset;
            chart.Canvas.AddDrawableTask(DataLabelsPaint);
        }
    }

    /// <summary>
    /// Creates the data label visual if needed, updates its text + style, and
    /// positions it via <c>GetLabelPolarPosition</c> using the slice's angular
    /// extent and inner / outer radii. Handles tangent / cotangent rotation
    /// modifiers from <see cref="LiveCharts.TangentAngle"/>.
    /// </summary>
    private void MeasureDataLabel(
        ChartPoint point,
        in PieLayout layout,
        float stackedInnerRadius,
        float stackedOuterRadius,
        float baseRotation,
        bool isTangent,
        bool isCotangent,
        in PieMeasureContext ctx)
    {
        if (!ShowDataLabels || DataLabelsPaint is null || DataLabelsPaint == Paint.Default || IsFillSeries) return;

        var chart = ctx.Chart;
        var label = (TLabel?)point.Context.Label;

        var middleAngle = layout.StartAngle + layout.SweepAngle * 0.5f;

        var actualRotation = baseRotation +
                (isTangent ? middleAngle - 90 : 0) +
                (isCotangent ? middleAngle : 0);

        if ((isTangent || isCotangent) && ((actualRotation + 90) % 360) > 180)
            actualRotation += 180;

        if (label is null)
        {
            var l = new TLabel
            {
                X = ctx.CenterX,
                Y = ctx.CenterY,
                RotateTransform = actualRotation,
                MaxWidth = (float)DataLabelsMaxWidth,
            };
            l.Animate(
                GetAnimation(chart),
                BaseLabelGeometry.XProperty,
                BaseLabelGeometry.YProperty);
            label = l;
            point.Context.Label = l;
        }

        DataLabelsPaint.AddGeometryToPaintTask(chart.Canvas, label);

        label.Text = DataLabelsFormatter(new ChartPoint<TModel, TVisual, TLabel>(point));
        label.TextSize = ctx.DataLabelsSize;
        label.Padding = DataLabelsPadding;
        label.RotateTransform = actualRotation;
        label.Paint = DataLabelsPaint;

        var start = layout.StartAngle - ctx.InitialRotation;
        AlignLabel(label, start, ctx.InitialRotation, layout.SweepAngle);

        if (ctx.IsFirstDraw)
            label.CompleteTransition(
                BaseLabelGeometry.TextSizeProperty,
                BaseLabelGeometry.XProperty,
                BaseLabelGeometry.YProperty,
                BaseLabelGeometry.RotateTransformProperty);

        var labelPosition = GetLabelPolarPosition(
            ctx.CenterX,
            ctx.CenterY,
            stackedInnerRadius,
            (stackedOuterRadius + ctx.RelativeOuterRadius * 2) * 0.5f,
            layout.StartAngle,
            layout.SweepAngle,
            label.Measure(),
            DataLabelsPosition);

        label.X = labelPosition.X;
        label.Y = labelPosition.Y;
    }

    /// <inheritdoc cref="ChartElement.Invalidate(Chart)"/>
    public sealed override void Invalidate(Chart chart)
    {
        var pieChart = (PieChartEngine)chart;
        _ = GetAnimation(pieChart);

        var fetched = Fetch(pieChart).ToArray();
        var ctx = BeginMeasure(pieChart, fetched.Length);
        var pointsCleanup = ChartPointCleanupContext.For(everFetched);

        InitializePaints(pieChart);

        // Decode TangentAngle / CotangentAngle modifiers from DataLabelsRotation
        // once per measure cycle (they're stored as flag bits on top of the
        // numeric rotation angle).
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

        var stackedInnerRadius = ctx.InnerRadius;
        var i = 1;

        foreach (var point in fetched)
        {
            if (point.IsEmpty || !IsVisible)
            {
                CollapseEmptyVisual(point, in ctx);
                pointsCleanup.Clean(point);

                // Advance the stacking radius even for empty slices so the next
                // visible slice doesn't pile up on top of an invisible one.
                var md2 = ctx.MinDimension;
                var w2 = md2 - (md2 - 2 * ctx.InnerRadius) * (fetched.Length - i) / fetched.Length - ctx.RelativeOuterRadius * 2;
                stackedInnerRadius = (w2 + ctx.RelativeOuterRadius * 2) * 0.5f;
                i++;
                continue;
            }

            // Compute the slice's pre-layout start angle so EnsureVisualForPoint
            // can seed mid-life entries (a slice appearing after isFirstDraw) with
            // a sensible animation source.
            var stack = ctx.Stacker.GetStack(point);
            var stackedValue = stack.Start;
            var total = ctx.ChartTotal ?? stack.Total;
            double rawStart;
            if (total == 0)
                rawStart = 0;
            else if (IsRelativeToMinValue)
            {
                var h = total - ctx.StartValue;
                rawStart = Math.Min(stackedValue, h) / h * ctx.CompleteAngle;
                if (!ctx.IsClockWise)
                    rawStart = ctx.CompleteAngle - rawStart - (Math.Min(stackedValue + point.Coordinate.PrimaryValue, h) / h * ctx.CompleteAngle - rawStart);
            }
            else
            {
                rawStart = Math.Min(stackedValue, total) / total * ctx.CompleteAngle;
                if (!ctx.IsClockWise)
                {
                    var h = total - ctx.StartValue;
                    var h1 = Math.Min(stackedValue + point.Coordinate.PrimaryValue, total) - ctx.StartValue;
                    rawStart = ctx.CompleteAngle - rawStart - (h1 / h * ctx.CompleteAngle - rawStart);
                }
            }

            var visual = EnsureVisualForPoint(point, rawStart, in ctx);

            if (Fill is not null && Fill != Paint.Default)
                Fill.AddGeometryToPaintTask(pieChart.Canvas, visual);
            if (Stroke is not null && Stroke != Paint.Default)
                Stroke.AddGeometryToPaintTask(pieChart.Canvas, visual);

            stackedInnerRadius += ctx.RelativeInnerRadius;

            var layout = MeasurePieLayout(point, stackedInnerRadius, i, in ctx);

            visual.CenterX = layout.CenterX;
            visual.CenterY = layout.CenterY;
            visual.X = layout.X;
            visual.Y = layout.Y;
            visual.Width = layout.Width;
            visual.Height = layout.Height;
            visual.InnerRadius = layout.InnerRadius;
            visual.PushOut = layout.PushOut;
            visual.StartAngle = layout.StartAngle;
            visual.SweepAngle = layout.SweepAngle;
            visual.CornerRadius = layout.CornerRadius;
            visual.InvertedCornerRadius = InvertedCornerRadius;
            visual.RemoveOnCompleted = false;

            if (point.Context.HoverArea is not SemicircleHoverArea ha)
                point.Context.HoverArea = ha = new SemicircleHoverArea();

            _ = ha.SetDimensions(
                ctx.CenterX, ctx.CenterY,
                layout.StartAngle,
                layout.StartAngle + layout.SweepAngle,
                stackedInnerRadius, layout.OuterRadius);

            pointsCleanup.Clean(point);

            MeasureDataLabel(point, in layout, stackedInnerRadius, layout.OuterRadius,
                baseRotation: r, isTangent: isTangent, isCotangent: isCotangent, in ctx);

            OnPointMeasured(point);

            stackedInnerRadius = (layout.OuterRadius + ctx.RelativeOuterRadius * 2) * 0.5f;
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

    /// <summary>Gets the stack group.</summary>
    public override int GetStackGroup() => 0;

    /// <inheritdoc cref="ChartElement.GetPaintTasks"/>
    protected internal override Paint?[] GetPaintTasks() =>
        [Fill, Stroke, DataLabelsPaint];

    /// <summary>Sets the default point transitions.</summary>
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

    /// <summary>Softly deletes the point.</summary>
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

    /// <summary>Gets the label polar position.</summary>
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
