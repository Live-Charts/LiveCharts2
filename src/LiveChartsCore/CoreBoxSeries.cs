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
/// Defines a box-and-whisker series.
/// </summary>
/// <typeparam name="TModel">The type of the model.</typeparam>
/// <typeparam name="TVisual">The type of the visual.</typeparam>
/// <typeparam name="TLabel">The type of the label.</typeparam>
/// <typeparam name="TMiniatureGeometry">The type of the miniature geometry, used in tool tips and legends.</typeparam>
/// <seealso cref="CartesianSeries{TModel, TVisual, TLabel}" />
/// <seealso cref="ICartesianSeries" />
public abstract class CoreBoxSeries<TModel, TVisual, TLabel, TMiniatureGeometry>
    : StrokeAndFillCartesianSeries<TModel, TVisual, TLabel>, IBoxSeries
        where TVisual : BaseBoxGeometry, new()
        where TLabel : BaseLabelGeometry, new()
        where TMiniatureGeometry : BoundedDrawnGeometry, new()
{

    /// <summary>
    /// Initializes a new instance of the <see cref="CoreBoxSeries{TModel, TVisual, TLabel, TMiniatureGeometry}"/> class.
    /// </summary>
    protected CoreBoxSeries(IReadOnlyCollection<TModel>? values)
        : base(GetProperties(), values)
    {
        YToolTipLabelFormatter = p =>
        {
            var c = p.Coordinate;
            return
                $"Max {c.PrimaryValue}, Min {c.QuinaryValue}{Environment.NewLine}" +
                $"1stQ {c.TertiaryValue}, 2dnQ {c.QuaternaryValue}{Environment.NewLine}" +
                $"Med {c.SenaryValue}";
        };

        DataLabelsFormatter = p =>
        {
            var c = p.Coordinate;
            return
                $"Max {c.PrimaryValue}, Min {c.QuinaryValue}{Environment.NewLine}" +
                $"1stQ {c.TertiaryValue}, 2dnQ {c.QuaternaryValue}{Environment.NewLine}" +
                $"Med {c.SenaryValue}"; ;
        };

        DataPadding = new LvcPoint(0, 1);
    }

    /// <inheritdoc cref="IBoxSeries.MaxBarWidth"/>
    public double MaxBarWidth { get; set => SetProperty(ref field, value); } = 25;

    /// <inheritdoc cref="IBoxSeries.Padding"/>
    public double Padding { get; set => SetProperty(ref field, value); } = 5;

    // ---- template method ----------------------------------------------------

    /// <summary>
    /// Builds a per-frame measure context from the chart.
    /// </summary>
    protected virtual BoxMeasureContext BeginMeasure(CartesianChartEngine chart)
    {
        var primaryAxis = chart.GetYAxis(this);
        var secondaryAxis = chart.GetXAxis(this);

        var drawLocation = chart.DrawMarginLocation;
        var drawMarginSize = chart.DrawMarginSize;
        var secondaryScale = secondaryAxis.GetNextScaler(chart);
        var primaryScale = primaryAxis.GetNextScaler(chart);
        var previousPrimaryScale = primaryAxis.GetActualScaler(chart);
        var previousSecondaryScale = secondaryAxis.GetActualScaler(chart);

        var uw = secondaryScale.MeasureInPixels(secondaryAxis.UnitWidth);
        var puw = previousSecondaryScale is null ? 0 : previousSecondaryScale.MeasureInPixels(secondaryAxis.UnitWidth);
        var uwm = 0.5f * uw;

        var helper = new BoxMeasureHelper(
            secondaryScale, chart, this, secondaryAxis,
            primaryScale.ToPixels(pivot),
            chart.DrawMarginLocation.Y,
            chart.DrawMarginLocation.Y + chart.DrawMarginSize.Height);

        if (uw > MaxBarWidth)
        {
            uw = (float)MaxBarWidth;
            uwm = uw * 0.5f;
            puw = uw;
        }

        var isFirstDraw = !((Chart)chart).IsDrawn(((ISeries)this).SeriesId);

        return new BoxMeasureContext(
            chart, primaryAxis, secondaryAxis,
            primaryScale, secondaryScale,
            previousPrimaryScale, previousSecondaryScale,
            helper: helper,
            categoryUnitWidth: uw,
            previousCategoryUnitWidth: puw,
            halfCategoryUnitWidth: uwm,
            tooltipPosition: chart.TooltipPosition,
            isFirstDraw: isFirstDraw,
            drawLocation: drawLocation,
            drawMarginSize: drawMarginSize,
            dataLabelsSize: (float)DataLabelsSize);
    }

    /// <summary>
    /// Computes the final-frame box geometry — body rect + five quartile/extremum
    /// pixel-Y values.
    /// </summary>
    protected virtual BoxLayout MeasureBoxLayout(ChartPoint point, in BoxMeasureContext ctx)
    {
        var coordinate = point.Coordinate;
        var helper = ctx.Helper;
        var secondary = ctx.SecondaryScale.ToPixels(coordinate.SecondaryValue);

        var high = ctx.PrimaryScale.ToPixels(coordinate.PrimaryValue);
        var open = ctx.PrimaryScale.ToPixels(coordinate.TertiaryValue);
        var close = ctx.PrimaryScale.ToPixels(coordinate.QuaternaryValue);
        var low = ctx.PrimaryScale.ToPixels(coordinate.QuinaryValue);
        var median = ctx.PrimaryScale.ToPixels(coordinate.SenaryValue);

        var x = secondary - helper.uwm + helper.cp;

        return new BoxLayout(
            x: x,
            y: high,
            width: helper.uw,
            third: open,
            first: close,
            min: low,
            median: median,
            categoryHoverX: secondary - helper.actualUw * 0.5f,
            categoryHoverWidth: helper.actualUw);
    }

    /// <summary>
    /// Ensures the visual exists. On first creation initializes it at the box's
    /// X position with all five quartile/extremum values collapsed to the median,
    /// so the box expands from a horizontal line.
    /// </summary>
    protected virtual TVisual EnsureVisualForPoint(ChartPoint point, in BoxMeasureContext ctx)
    {
        var visual = point.Context.Visual as TVisual;
        if (visual is not null) return visual;

        var coordinate = point.Coordinate;
        var helper = ctx.Helper;
        var secondary = ctx.SecondaryScale.ToPixels(coordinate.SecondaryValue);
        var median = ctx.PrimaryScale.ToPixels(coordinate.SenaryValue);

        var xi = secondary - helper.uwm + helper.cp;
        var uwi = helper.uw;

        // Mid-life entry: animate from previous frame's position.
        if (ctx.PreviousSecondaryScale is not null && ctx.PreviousPrimaryScale is not null)
        {
            xi = ctx.PreviousSecondaryScale.ToPixels(coordinate.SecondaryValue) - ctx.HalfCategoryUnitWidth;
            uwi = helper.uw;
        }

        var r = new TVisual
        {
            X = xi,
            Width = uwi,
            Y = median,
            Third = median,
            First = median,
            Min = median,
            Median = median,
        };

        point.Context.Visual = r;
        OnPointCreated(point);

        _ = everFetched.Add(point);

        return r;
    }

    /// <summary>
    /// Collapses the box to its median-Y line for empty/invisible points.
    /// </summary>
    protected virtual void CollapseEmptyVisual(ChartPoint point, in BoxMeasureContext ctx)
    {
        var coordinate = point.Coordinate;
        var helper = ctx.Helper;
        var secondary = ctx.SecondaryScale.ToPixels(coordinate.SecondaryValue);
        var median = ctx.PrimaryScale.ToPixels(coordinate.SenaryValue);

        if (point.Context.Visual is TVisual visual)
        {
            visual.X = secondary - helper.uwm + helper.cp;
            visual.Width = ctx.CategoryUnitWidth;
            visual.Y = median;
            visual.Third = median;
            visual.First = median;
            visual.Min = median;
            visual.Median = median;
            visual.RemoveOnCompleted = true;
            point.Context.Visual = null;
        }

        if (point.Context.Label is TLabel label)
        {
            label.X = secondary - helper.uwm + helper.cp;
            label.Y = median;
            label.Opacity = 0;
            label.RemoveOnCompleted = true;
            point.Context.Label = null;
        }
    }

    /// <summary>
    /// Sets per-Z-index ordering on Fill / Stroke / DataLabelsPaint and registers
    /// them as drawable tasks. Box has no error paints.
    /// </summary>
    private void InitializePaints(CartesianChartEngine chart)
    {
        var actualZIndex = ZIndex == 0 ? ((ISeries)this).SeriesId : ZIndex;

        if (Stroke is not null && Stroke != Paint.Default)
        {
            Stroke.ZIndex = actualZIndex + PaintConstants.SeriesStrokeZIndexOffset;
            chart.Canvas.AddDrawableTask(Stroke, zone: CanvasZone.DrawMargin);
        }
        if (Fill is not null && Fill != Paint.Default)
        {
            Fill.ZIndex = actualZIndex + PaintConstants.SeriesFillZIndexOffset;
            chart.Canvas.AddDrawableTask(Fill, zone: CanvasZone.DrawMargin);
        }
        if (ShowDataLabels && DataLabelsPaint is not null && DataLabelsPaint != Paint.Default)
        {
            DataLabelsPaint.ZIndex = actualZIndex + PaintConstants.SeriesGeometryFillZIndexOffset;
            chart.Canvas.AddDrawableTask(DataLabelsPaint, zone: CanvasZone.DrawMargin);
        }
    }

    /// <summary>
    /// Configures the hover-area tooltip anchor based on the chart's
    /// <see cref="TooltipPosition"/>. Box honors the full set of positions like
    /// candlesticks.
    /// </summary>
    private static void ConfigureHoverAnchor(RectangleHoverArea ha, TooltipPosition tp)
    {
        switch (tp)
        {
            case TooltipPosition.Hidden:
                break;
            case TooltipPosition.Auto:
            case TooltipPosition.Top: _ = ha.CenterXToolTip().StartYToolTip(); break;
            case TooltipPosition.Bottom: _ = ha.CenterXToolTip().EndYToolTip(); break;
            case TooltipPosition.Left: _ = ha.StartXToolTip().CenterYToolTip(); break;
            case TooltipPosition.Right: _ = ha.EndXToolTip().CenterYToolTip(); break;
            case TooltipPosition.Center: _ = ha.CenterXToolTip().CenterYToolTip(); break;
            default:
                break;
        }
    }

    /// <summary>
    /// Creates the data label visual if needed, updates its text + style, and
    /// positions it via <c>GetLabelPosition</c> with the box's body rect.
    /// </summary>
    private void MeasureDataLabel(ChartPoint point, in BoxLayout layout, in BoxMeasureContext ctx)
    {
        if (!ShowDataLabels || DataLabelsPaint is null || DataLabelsPaint == Paint.Default) return;

        var chart = ctx.Chart;
        var label = (TLabel?)point.Context.Label;

        if (label is null)
        {
            var l = new TLabel
            {
                X = layout.X,
                Y = layout.Y,
                RotateTransform = (float)DataLabelsRotation,
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
        label.Paint = DataLabelsPaint;

        var m = label.Measure();
        var labelPosition = GetLabelPosition(
            layout.X, layout.Y, layout.Width, layout.Height, m, DataLabelsPosition,
            SeriesProperties, point.Coordinate.PrimaryValue > Pivot, ctx.DrawLocation, ctx.DrawMarginSize);
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

        var ctx = BeginMeasure(cartesianChart);
        var pointsCleanup = ChartPointCleanupContext.For(everFetched);

        InitializePaints(cartesianChart);

        foreach (var point in Fetch(cartesianChart))
        {
            if (point.IsEmpty || !IsVisible)
            {
                CollapseEmptyVisual(point, in ctx);
                pointsCleanup.Clean(point);
                continue;
            }

            var visual = EnsureVisualForPoint(point, in ctx);

            if (Stroke is not null && Stroke != Paint.Default)
                Stroke.AddGeometryToPaintTask(cartesianChart.Canvas, visual);
            if (Fill is not null && Fill != Paint.Default)
                Fill.AddGeometryToPaintTask(cartesianChart.Canvas, visual);

            var layout = MeasureBoxLayout(point, in ctx);

            visual.X = layout.X;
            visual.Width = layout.Width;
            visual.Y = layout.Y;
            visual.Third = layout.Third;
            visual.First = layout.First;
            visual.Min = layout.Min;
            visual.Median = layout.Median;
            visual.RemoveOnCompleted = false;

            if (point.Context.HoverArea is not RectangleHoverArea ha)
                point.Context.HoverArea = ha = new RectangleHoverArea();

            _ = ha.SetDimensions(layout.CategoryHoverX, layout.Y, layout.CategoryHoverWidth, layout.Height);

            if (chart.FindingStrategy == FindingStrategy.ExactMatch)
                _ = ha
                    .SetDimensions(layout.X, layout.Y, layout.Width, layout.Min)
                    .CenterXToolTip();

            ConfigureHoverAnchor(ha, ctx.TooltipPosition);

            pointsCleanup.Clean(point);

            MeasureDataLabel(point, in layout, in ctx);

            OnPointMeasured(point);
        }

        pointsCleanup.CollectPoints(
            everFetched, cartesianChart.View, ctx.PrimaryScale, ctx.SecondaryScale, SoftDeleteOrDisposePoint);
    }

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
                        pointerPosition.Y > v.Y && pointerPosition.Y < v.Y + Math.Abs(v.Min - v.Y);
                })
            : base.FindPointsInPosition(chart, pointerPosition, strategy, findPointFor);
    }

    /// <inheritdoc cref="ICartesianSeries.GetBounds(Chart, ICartesianAxis, ICartesianAxis)"/>
    public override SeriesBounds GetBounds(
        Chart chart, ICartesianAxis secondaryAxis, ICartesianAxis primaryAxis)
    {
        var rawBounds = DataFactory.GetFinancialBounds(chart, this, secondaryAxis, primaryAxis);
        if (rawBounds.HasData) return rawBounds;

        var rawBaseBounds = rawBounds.Bounds;

        var tickPrimary = primaryAxis.GetTick(chart.ControlSize, rawBaseBounds.VisiblePrimaryBounds);
        var tickSecondary = secondaryAxis.GetTick(chart.ControlSize, rawBaseBounds.VisibleSecondaryBounds);

        var ts = tickSecondary.Value * DataPadding.X;
        var tp = tickPrimary.Value * DataPadding.Y;

        var rgs = GetRequestedGeometrySize();
        var rso = GetRequestedSecondaryOffset();
        var rpo = GetRequestedPrimaryOffset();

        var dimensionalBounds = new DimensionalBounds
        {
            SecondaryBounds = new Bounds
            {
                Max = rawBaseBounds.SecondaryBounds.Max + rso * secondaryAxis.UnitWidth,
                Min = rawBaseBounds.SecondaryBounds.Min - rso * secondaryAxis.UnitWidth,
                MinDelta = rawBaseBounds.SecondaryBounds.MinDelta,
                PaddingMax = ts,
                PaddingMin = ts,
                RequestedGeometrySize = rgs
            },
            PrimaryBounds = new Bounds
            {
                Max = rawBaseBounds.PrimaryBounds.Max + rpo * secondaryAxis.UnitWidth,
                Min = rawBaseBounds.PrimaryBounds.Min - rpo * secondaryAxis.UnitWidth,
                MinDelta = rawBaseBounds.PrimaryBounds.MinDelta,
                PaddingMax = tp,
                PaddingMin = tp,
                RequestedGeometrySize = rgs
            },
            VisibleSecondaryBounds = new Bounds
            {
                Max = rawBaseBounds.VisibleSecondaryBounds.Max + rso * secondaryAxis.UnitWidth,
                Min = rawBaseBounds.VisibleSecondaryBounds.Min - rso * secondaryAxis.UnitWidth,
            },
            VisiblePrimaryBounds = new Bounds
            {
                Max = rawBaseBounds.VisiblePrimaryBounds.Max + rpo * secondaryAxis.UnitWidth,
                Min = rawBaseBounds.VisiblePrimaryBounds.Min - rpo * secondaryAxis.UnitWidth
            },
            TertiaryBounds = rawBaseBounds.TertiaryBounds,
            VisibleTertiaryBounds = rawBaseBounds.VisibleTertiaryBounds
        };

        return new SeriesBounds(dimensionalBounds, false);
    }

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

    /// <inheritdoc cref="CartesianSeries{TModel, TVisual, TLabel}.GetRequestedSecondaryOffset"/>
    protected override double GetRequestedSecondaryOffset() => 0.5f;

    /// <inheritdoc cref="Series{TModel, TVisual, TLabel}.SetDefaultPointTransitions(ChartPoint)"/>
    protected override void SetDefaultPointTransitions(ChartPoint chartPoint)
    {
        var chart = chartPoint.Context.Chart;
        if (chartPoint.Context.Visual is not TVisual visual) throw new Exception("Unable to initialize the point instance.");
        visual.Animate(GetAnimation(chart.CoreChart));
    }

    /// <inheritdoc cref="CartesianSeries{TModel, TVisual, TLabel}.SoftDeleteOrDisposePoint(ChartPoint, Scaler, Scaler)"/>
    protected internal override void SoftDeleteOrDisposePoint(ChartPoint point, Scaler primaryScale, Scaler secondaryScale)
    {
        var visual = (TVisual?)point.Context.Visual;
        if (visual is null) return;
        if (DataFactory is null) throw new Exception("Data provider not found");

        var p = primaryScale.ToPixels(pivot);
        var secondary = secondaryScale.ToPixels(point.Coordinate.SecondaryValue);

        visual.X = secondary;
        visual.Y = p;
        visual.Third = p;
        visual.First = p;
        visual.Min = p;
        visual.Median = p;
        visual.RemoveOnCompleted = true;

        DataFactory.DisposePoint(point);

        var label = (TLabel?)point.Context.Label;
        if (label is null) return;

        label.TextSize = 1;
        label.RemoveOnCompleted = true;
    }

    /// <summary>
    /// Called when [paint changed].
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns></returns>
    protected override void OnPaintChanged(string? propertyName)
    {
        base.OnPaintChanged(propertyName);
        OnPropertyChanged();
    }

    private static SeriesProperties GetProperties()
    {
        return SeriesProperties.BoxSeries | SeriesProperties.PrimaryAxisVerticalOrientation |
             SeriesProperties.Solid | SeriesProperties.PrefersXStrategyTooltips;
    }
}
