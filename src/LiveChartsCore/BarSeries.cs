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
/// Defines a bar series point.
/// </summary>
/// <typeparam name="TModel">The type of the model.</typeparam>
/// <typeparam name="TVisual">The type of the visual.</typeparam>
/// <typeparam name="TLabel">The type of the label.</typeparam>
/// <seealso cref="CartesianSeries{TModel, TVisual, TLabel}" />
/// <seealso cref="IBarSeries" />
/// <remarks>
/// Initializes a new instance of the <see cref="BarSeries{TModel, TVisual, TLabel}"/> class.
/// </remarks>
/// <param name="properties">The properties.</param>
/// <param name="values">The values.</param>
public abstract class BarSeries<TModel, TVisual, TLabel>(
    SeriesProperties properties,
    IReadOnlyCollection<TModel>? values)
        : StrokeAndFillCartesianSeries<TModel, TVisual, TLabel>(properties, values), IBarSeries
            where TVisual : BoundedDrawnGeometry, new()
            where TLabel : BaseLabelGeometry, new()
{
    private bool _showError;

    /// <inheritdoc cref="IBarSeries.Padding"/>
    public double Padding { get; set => SetProperty(ref field, value); } = 2;

    /// <inheritdoc cref="IBarSeries.MaxBarWidth"/>
    public double MaxBarWidth { get; set => SetProperty(ref field, value); } = 50;

    /// <inheritdoc cref="IBarSeries.IgnoresBarPosition"/>
    public bool IgnoresBarPosition { get; set => SetProperty(ref field, value); } = false;

    /// <inheritdoc cref="IBarSeries.Rx"/>
    public double Rx { get; set => SetProperty(ref field, value); }

    /// <inheritdoc cref="IBarSeries.Ry"/>
    public double Ry { get; set => SetProperty(ref field, value); }

    /// <inheritdoc cref="IErrorSeries.ShowError"/>
    public bool ShowError
    {
        get => _showError;
        set
        {
            SetProperty(ref _showError, value);
            ErrorPaint?.IsPaused = !value;
        }
    }

    /// <inheritdoc cref="IErrorSeries.ErrorPaint"/>
    public Paint? ErrorPaint
    {
        get;
        set
        {
            SetPaintProperty(ref field, value, PaintStyle.Stroke);
            _showError = value is not null && value != Paint.Default;
        }
    } = Paint.Default;

    /// <inheritdoc cref="Series{TModel, TVisual, TLabel}.GetMiniatureGeometry"/>
    public override IDrawnElement GetMiniatureGeometry(ChartPoint? point)
    {
        var v = point?.Context.Visual;

        var m = new TVisual
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

    /// <inheritdoc cref="ChartElement.GetPaintTasks"/>
    protected internal override Paint?[] GetPaintTasks() =>
        [Stroke, Fill, DataLabelsPaint, ErrorPaint];

    /// <inheritdoc cref="Series{TModel, TVisual, TLabel}.FindPointsInPosition(Chart, LvcPoint, FindingStrategy, FindPointFor)"/>
    protected override IEnumerable<ChartPoint> FindPointsInPosition(
        Chart chart, LvcPoint pointerPosition, FindingStrategy strategy, FindPointFor findPointFor)
    {
        return strategy == FindingStrategy.ExactMatch
            ? Fetch(chart)
                .Where(point =>
                {
                    var v = (TVisual?)point.Context.Visual;

                    return
                        v is not null &&
                        pointerPosition.X > v.X && pointerPosition.X < v.X + v.Width &&
                        pointerPosition.Y > v.Y && pointerPosition.Y < v.Y + v.Height;
                })
            : base.FindPointsInPosition(chart, pointerPosition, strategy, findPointFor);
    }

    // ---- template method ----------------------------------------------------

    /// <summary>
    /// Builds a per-frame measure context from the chart. Orientation parents
    /// resolve <see cref="BarMeasureContext.PrimaryScale"/> /
    /// <see cref="BarMeasureContext.SecondaryScale"/> from the chart's X / Y axes
    /// in their orientation-correct order.
    /// </summary>
    protected abstract BarMeasureContext BeginMeasure(CartesianChartEngine chart, bool isStacked);

    /// <summary>
    /// Computes the final-frame bar geometry for a single point. The bulk of
    /// each orientation's per-point math lives here.
    /// </summary>
    protected abstract BarLayout MeasureBarLayout(ChartPoint point, in BarMeasureContext ctx);

    /// <summary>
    /// Ensures the visual + any orientation-specific additional visuals (e.g.
    /// error bars) exist for this point. On first creation initializes the
    /// visual at the orientation-correct entry position (pivot, with the
    /// growing dimension at zero). Returns the visual instance.
    /// </summary>
    protected abstract TVisual EnsureVisualForPoint(ChartPoint point, in BarMeasureContext ctx);

    /// <summary>
    /// Per-frame error-bar geometry update. No-op when the series carries no
    /// error visuals for this point.
    /// </summary>
    protected abstract void MeasureErrorBars(ChartPoint point, in BarLayout layout, in BarMeasureContext ctx);

    /// <summary>
    /// Collapses the point's visual + additional visuals to their orientation-
    /// specific zero state for empty/invisible points.
    /// </summary>
    protected abstract void CollapseEmptyVisual(ChartPoint point, in BarMeasureContext ctx);

    /// <summary>
    /// Sets the per-Z-index ordering on each non-default paint and registers it as a
    /// drawable task on the chart's canvas (within the DrawMargin zone). Run once at
    /// the top of <see cref="Invalidate"/>.
    /// </summary>
    private void InitializePaints(CartesianChartEngine chart)
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
        if (ShowError && ErrorPaint is not null && ErrorPaint != Paint.Default)
        {
            ErrorPaint.ZIndex = actualZIndex + PaintConstants.SeriesGeometryFillZIndexOffset;
            chart.Canvas.AddDrawableTask(ErrorPaint, zone: CanvasZone.DrawMargin);
        }
        if (ShowDataLabels && DataLabelsPaint is not null && DataLabelsPaint != Paint.Default)
        {
            DataLabelsPaint.ZIndex = actualZIndex + PaintConstants.SeriesGeometryStrokeZIndexOffset;
            chart.Canvas.AddDrawableTask(DataLabelsPaint, zone: CanvasZone.DrawMargin);
        }
    }

    /// <summary>
    /// Creates the data label visual if it doesn't exist yet (animation-sourced from
    /// the visual's entry position), updates its text + style, and positions it via
    /// <c>GetLabelPosition</c> with the bar's rect and the point's "above pivot" hint.
    /// No-op when the series has no data-label paint configured.
    /// </summary>
    private void MeasureDataLabel(ChartPoint point, TVisual visual, in BarLayout layout, in BarMeasureContext ctx)
    {
        if (!ShowDataLabels || DataLabelsPaint is null || DataLabelsPaint == Paint.Default) return;

        var chart = ctx.Chart;
        var label = (TLabel?)point.Context.Label;

        if (label is null)
        {
            var l = new TLabel
            {
                X = visual.X,
                Y = visual.Y,
                RotateTransform = (float)DataLabelsRotation,
                MaxWidth = (float)DataLabelsMaxWidth
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
        label.Paint = DataLabelsPaint;

        if (ctx.IsFirstDraw)
            label.CompleteTransition(
                BaseLabelGeometry.TextSizeProperty,
                BaseLabelGeometry.XProperty,
                BaseLabelGeometry.YProperty,
                BaseLabelGeometry.RotateTransformProperty);

        var m = label.Measure();
        var labelPosition = GetLabelPosition(
            layout.X, layout.Y, layout.Width, layout.Height, m,
            DataLabelsPosition, SeriesProperties,
            point.Coordinate.PrimaryValue > Pivot, ctx.DrawLocation, ctx.DrawMarginSize);
        if (DataLabelsTranslate is not null)
            label.TranslateTransform = new LvcPoint(
                m.Width * DataLabelsTranslate.Value.X, m.Height * DataLabelsTranslate.Value.Y);

        label.X = labelPosition.X;
        label.Y = labelPosition.Y;
    }

    /// <inheritdoc cref="ChartElement.Invalidate(Chart)"/>
    public sealed override void Invalidate(Chart chart)
    {
        var cartesianChart = (CartesianChartEngine)chart;
        _ = GetAnimation(cartesianChart);

        var isStacked = (SeriesProperties & SeriesProperties.Stacked) == SeriesProperties.Stacked;
        var ctx = BeginMeasure(cartesianChart, isStacked);
        var pointsCleanup = ChartPointCleanupContext.For(everFetched);

        InitializePaints(cartesianChart);

        var rx = ctx.Rx;
        var ry = ctx.Ry;

        foreach (var point in Fetch(cartesianChart))
        {
            if (point.IsEmpty || !IsVisible)
            {
                CollapseEmptyVisual(point, in ctx);
                pointsCleanup.Clean(point);
                continue;
            }

            var visual = EnsureVisualForPoint(point, in ctx);

            if (ctx.HasSvg)
            {
                var svgVisual = (IVariableSvgPath)visual;
                if (_geometrySvgChanged || svgVisual.SVGPath is null)
                    svgVisual.SVGPath = GeometrySvg ?? throw new Exception("svg path is not defined");
            }

            if (Fill is not null && Fill != Paint.Default)
                Fill.AddGeometryToPaintTask(cartesianChart.Canvas, visual);
            if (Stroke is not null && Stroke != Paint.Default)
                Stroke.AddGeometryToPaintTask(cartesianChart.Canvas, visual);

            var layout = MeasureBarLayout(point, in ctx);

            visual.X = layout.X;
            visual.Y = layout.Y;
            visual.Width = layout.Width;
            visual.Height = layout.Height;

            MeasureErrorBars(point, in layout, in ctx);

            if (visual is BaseRoundedRectangleGeometry rrg)
                rrg.BorderRadius = new LvcPoint(rx, ry);

            visual.RemoveOnCompleted = false;

            if (point.Context.HoverArea is not RectangleHoverArea ha)
                point.Context.HoverArea = ha = new RectangleHoverArea();

            _ = ha
                .SetDimensions(
                    layout.CategoryHoverX, layout.CategoryHoverY,
                    layout.CategoryHoverWidth, layout.CategoryHoverHeight)
                .CenterXToolTip();

            if (chart.FindingStrategy == FindingStrategy.ExactMatch)
                _ = ha
                    .SetDimensions(layout.X, layout.Y, layout.Width, layout.Height)
                    .CenterXToolTip();

            // Anchor the tooltip Y on the DRAWN rect, not the category-strip hover box.
            // For VerticalBar/Column these coincide (categoryHoverY == layout.Y), so this
            // is a no-op; for HorizontalBar/Row the strip is `actualUw` tall while the
            // drawn bar is `uw` tall (centered), and reading from the strip would float
            // the tooltip half-padding above the bar's top edge.
            var anchorY = point.Coordinate.PrimaryValue >= pivot
                ? layout.Y
                : layout.Y + layout.Height;
            ha.SuggestedTooltipLocation = new LvcPoint(ha.SuggestedTooltipLocation.X, anchorY);
            if (point.Coordinate.PrimaryValue < pivot) _ = ha.IsLessThanPivot();

            pointsCleanup.Clean(point);

            MeasureDataLabel(point, visual, in layout, in ctx);

            OnPointMeasured(point);
        }

        pointsCleanup.CollectPoints(
            everFetched, cartesianChart.View, ctx.PrimaryScale, ctx.SecondaryScale, SoftDeleteOrDisposePoint);
        _geometrySvgChanged = false;
    }
}
