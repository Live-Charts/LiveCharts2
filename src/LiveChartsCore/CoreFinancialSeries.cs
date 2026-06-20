
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
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.Motion;
using LiveChartsCore.Painting;

namespace LiveChartsCore;

/// <summary>
/// Defines a candle sticks series.
/// </summary>
/// <typeparam name="TModel">The type of the model.</typeparam>
/// <typeparam name="TVisual">The type of the visual.</typeparam>
/// <typeparam name="TLabel">The type of the label.</typeparam>
/// <typeparam name="TMiniatureGeometry">The type of the miniature geometry, used in tool tips and legends.</typeparam>
/// <seealso cref="CartesianSeries{TModel, TVisual, TLabel}" />
/// <seealso cref="ICartesianSeries" />
public abstract class CoreFinancialSeries<TModel, TVisual, TLabel, TMiniatureGeometry>
    : CartesianSeries<TModel, TVisual, TLabel>, IFinancialSeries
        where TVisual : BaseCandlestickGeometry, new()
        where TLabel : BaseLabelGeometry, new()
        where TMiniatureGeometry : BoundedDrawnGeometry, new()
{

    /// <summary>
    /// Initializes a new instance of the <see cref="CoreFinancialSeries{TModel, TVisual, TLabel, TMiniatureGeometry}"/> class.
    /// </summary>
    protected CoreFinancialSeries(IReadOnlyCollection<TModel>? values)
        : base(GetProperties(), values)
    {
        YToolTipLabelFormatter = p =>
        {
            var c = p.Coordinate;
            return
                $"H {c.PrimaryValue:C2}{Environment.NewLine}" +
                $"O {c.TertiaryValue:C2}{Environment.NewLine}" +
                $"C {c.QuaternaryValue:C2}{Environment.NewLine}" +
                $"L {c.QuinaryValue:C2}";
        };

        DataLabelsFormatter = p =>
        {
            var c = p.Coordinate;
            return $"{c.PrimaryValue:C2} - {c.QuinaryValue:C2}";
        };
    }

    /// <inheritdoc cref="IFinancialSeries.MaxBarWidth"/>
    public double MaxBarWidth { get; set => SetProperty(ref field, value); } = 25;

    /// <inheritdoc cref="IFinancialSeries.UpStroke"/>
    public Paint? UpStroke
    {
        get;
        set => SetPaintProperty(ref field, value, PaintStyle.Stroke);
    } = Paint.Default;

    /// <inheritdoc cref="IFinancialSeries.UpFill"/>
    public Paint? UpFill
    {
        get;
        set => SetPaintProperty(ref field, value);
    } = Paint.Default;

    /// <inheritdoc cref="IFinancialSeries.DownStroke"/>
    public Paint? DownStroke
    {
        get;
        set => SetPaintProperty(ref field, value, PaintStyle.Stroke);
    } = Paint.Default;

    /// <inheritdoc cref="IFinancialSeries.DownFill"/>
    public Paint? DownFill
    {
        get;
        set => SetPaintProperty(ref field, value);
    } = Paint.Default;

    // ---- template method ----------------------------------------------------

    /// <summary>
    /// Builds a per-frame measure context from the chart.
    /// </summary>
    protected virtual FinancialMeasureContext BeginMeasure(CartesianChartEngine chart)
    {
        var primaryAxis = chart.GetYAxis(this);
        var secondaryAxis = chart.GetXAxis(this);

        var drawLocation = chart.DrawMarginLocation;
        var drawMarginSize = chart.DrawMarginSize;
        var secondaryScale = secondaryAxis.GetNextScaler(chart);
        var primaryScale = primaryAxis.GetNextScaler(chart);
        var previousPrimaryScale = primaryAxis.GetActualScaler(chart);
        var previousSecondaryScale = secondaryAxis.GetActualScaler(chart);

        // categoryWidth is the full axis-unit slot in pixels; candleWidth is
        // the same value clamped by MaxBarWidth (defaults to 25 px). Keeping
        // them apart lets the hover area cover the whole category column
        // while the visual still respects MaxBarWidth — mirroring the
        // actualUw / uw split BoxMeasureHelper and BarMeasureHelper use.
        var categoryWidth = secondaryScale.MeasureInPixels(secondaryAxis.UnitWidth);
        var uw = categoryWidth;
        var puw = previousSecondaryScale is null ? 0 : previousSecondaryScale.MeasureInPixels(secondaryAxis.UnitWidth);
        var uwm = 0.5f * uw;

        if (uw > MaxBarWidth)
        {
            uw = (float)MaxBarWidth;
            uwm = uw * 0.5f;
            puw = uw;
        }

        var isFirstDraw = !((Chart)chart).IsDrawn(((ISeries)this).SeriesId);

        return new FinancialMeasureContext(
            chart, primaryAxis, secondaryAxis,
            primaryScale, secondaryScale,
            previousPrimaryScale, previousSecondaryScale,
            candleWidth: uw,
            previousCandleWidth: puw,
            halfCandleWidth: uwm,
            categoryWidth: categoryWidth,
            tooltipPosition: chart.TooltipPosition,
            isFirstDraw: isFirstDraw,
            drawLocation: drawLocation,
            drawMarginSize: drawMarginSize,
            dataLabelsSize: (float)DataLabelsSize);
    }

    /// <summary>
    /// Computes the final-frame candle geometry — body rect + OHLC pixel-Y
    /// values + bullish flag.
    /// </summary>
    protected virtual FinancialLayout MeasureFinancialLayout(ChartPoint point, in FinancialMeasureContext ctx)
    {
        var coordinate = point.Coordinate;
        var secondary = ctx.SecondaryScale.ToPixels(coordinate.SecondaryValue);

        var high = ctx.PrimaryScale.ToPixels(coordinate.PrimaryValue);
        var open = ctx.PrimaryScale.ToPixels(coordinate.TertiaryValue);
        var close = ctx.PrimaryScale.ToPixels(coordinate.QuaternaryValue);
        var low = ctx.PrimaryScale.ToPixels(coordinate.QuinaryValue);

        // open > close in pixel-Y means lower open VALUE than close VALUE — bullish.
        return new FinancialLayout(
            x: secondary - ctx.HalfCandleWidth,
            y: high,
            width: ctx.CandleWidth,
            open: open,
            close: close,
            low: low,
            isBullish: open > close,
            categoryHoverX: secondary - ctx.CategoryWidth * 0.5f,
            categoryHoverWidth: ctx.CategoryWidth);
    }

    /// <summary>
    /// Ensures the visual exists. On first creation initializes it at the candle's
    /// X position with all OHLC values collapsed to the open price, so the candle
    /// expands from a horizontal line into its final shape.
    /// </summary>
    protected virtual TVisual EnsureVisualForPoint(ChartPoint point, in FinancialMeasureContext ctx)
    {
        var visual = point.Context.Visual as TVisual;
        if (visual is not null) return visual;

        var coordinate = point.Coordinate;
        var secondary = ctx.SecondaryScale.ToPixels(coordinate.SecondaryValue);
        var open = ctx.PrimaryScale.ToPixels(coordinate.TertiaryValue);

        var xi = secondary - ctx.HalfCandleWidth;
        var uwi = ctx.CandleWidth;

        // Mid-life entry: animate from previous-frame's position so a new candle
        // joining an existing series doesn't pop in from origin.
        if (ctx.PreviousSecondaryScale is not null && ctx.PreviousPrimaryScale is not null)
        {
            xi = ctx.PreviousSecondaryScale.ToPixels(coordinate.SecondaryValue) - ctx.HalfCandleWidth;
            uwi = ctx.PreviousCandleWidth;
        }

        var middle = open;
        var r = new TVisual
        {
            X = xi,
            Width = uwi,
            Y = middle,
            Open = middle,
            Close = middle,
            Low = middle,
        };

        point.Context.Visual = r;
        OnPointCreated(point);

        _ = everFetched.Add(point);

        return r;
    }

    /// <summary>
    /// Collapses the candle to its open-Y line for empty/invisible points so the
    /// visual fades out cleanly via the RemoveOnCompleted transition.
    /// </summary>
    protected virtual void CollapseEmptyVisual(ChartPoint point, in FinancialMeasureContext ctx)
    {
        var coordinate = point.Coordinate;
        var secondary = ctx.SecondaryScale.ToPixels(coordinate.SecondaryValue);
        var middle = ctx.PrimaryScale.ToPixels(coordinate.TertiaryValue);

        if (point.Context.Visual is TVisual visual)
        {
            visual.X = secondary - ctx.HalfCandleWidth;
            visual.Width = ctx.CandleWidth;
            visual.Y = middle;
            visual.Open = middle;
            visual.Close = middle;
            visual.Low = middle;
            visual.RemoveOnCompleted = true;
            point.Context.Visual = null;
        }

        if (point.Context.Label is TLabel label)
        {
            label.X = secondary - ctx.HalfCandleWidth;
            label.Y = middle;
            label.Opacity = 0;
            label.RemoveOnCompleted = true;
            point.Context.Label = null;
        }
    }

    /// <summary>
    /// Sets per-Z-index ordering on the four candle paints + data-labels paint and
    /// registers them as drawable tasks. Per-candle add/remove cycling between
    /// Up* and Down* paints based on bullish/bearish state happens later via
    /// <see cref="AttachVisualToPaints"/>.
    /// </summary>
    private void InitializePaints(CartesianChartEngine chart)
    {
        var actualZIndex = ZIndex == 0 ? ((ISeries)this).SeriesId : ZIndex;

        if (UpFill is not null && UpFill != Paint.Default)
        {
            UpFill.ZIndex = actualZIndex + PaintConstants.SeriesFillZIndexOffset;
            chart.Canvas.AddDrawableTask(UpFill, zone: CanvasZone.DrawMargin);
        }
        if (DownFill is not null && DownFill != Paint.Default)
        {
            DownFill.ZIndex = actualZIndex + PaintConstants.SeriesFillZIndexOffset;
            chart.Canvas.AddDrawableTask(DownFill, zone: CanvasZone.DrawMargin);
        }
        if (UpStroke is not null && UpStroke != Paint.Default)
        {
            UpStroke.ZIndex = actualZIndex + PaintConstants.SeriesStrokeZIndexOffset;
            chart.Canvas.AddDrawableTask(UpStroke, zone: CanvasZone.DrawMargin);
        }
        if (DownStroke is not null && DownStroke != Paint.Default)
        {
            DownStroke.ZIndex = actualZIndex + PaintConstants.SeriesStrokeZIndexOffset;
            chart.Canvas.AddDrawableTask(DownStroke, zone: CanvasZone.DrawMargin);
        }
        if (ShowDataLabels && DataLabelsPaint is not null && DataLabelsPaint != Paint.Default)
        {
            DataLabelsPaint.ZIndex = actualZIndex + PaintConstants.SeriesGeometryFillZIndexOffset;
            chart.Canvas.AddDrawableTask(DataLabelsPaint, zone: CanvasZone.DrawMargin);
        }
    }

    /// <summary>
    /// Attaches the candle visual to the bullish or bearish paint pair, and
    /// detaches it from the opposite pair — so the same visual can switch sides
    /// as the candle flips between bullish and bearish across frames.
    /// </summary>
    private void AttachVisualToPaints(TVisual visual, bool isBullish, CartesianChartEngine chart)
    {
        if (isBullish)
        {
            if (UpFill is not null && UpFill != Paint.Default)
                UpFill.AddGeometryToPaintTask(chart.Canvas, visual);
            if (UpStroke is not null && UpStroke != Paint.Default)
                UpStroke.AddGeometryToPaintTask(chart.Canvas, visual);
            if (DownFill is not null && DownFill != Paint.Default)
                DownFill.RemoveGeometryFromPaintTask(chart.Canvas, visual);
            if (DownStroke is not null && DownStroke != Paint.Default)
                DownStroke.RemoveGeometryFromPaintTask(chart.Canvas, visual);
        }
        else
        {
            if (DownFill is not null && DownFill != Paint.Default)
                DownFill.AddGeometryToPaintTask(chart.Canvas, visual);
            if (DownStroke is not null && DownStroke != Paint.Default)
                DownStroke.AddGeometryToPaintTask(chart.Canvas, visual);
            if (UpFill is not null && UpFill != Paint.Default)
                UpFill.RemoveGeometryFromPaintTask(chart.Canvas, visual);
            if (UpStroke is not null && UpStroke != Paint.Default)
                UpStroke.RemoveGeometryFromPaintTask(chart.Canvas, visual);
        }
    }

    /// <summary>
    /// Configures the hover-area tooltip anchor based on the chart's
    /// <see cref="TooltipPosition"/>. Unlike Cartesian bars (Start/EndY by pivot),
    /// candlesticks honor the full set of positions because tooltips usually
    /// follow the OHLC summary panel.
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
    /// positions it via <c>GetLabelPosition</c> with the candle's body rect.
    /// </summary>
    private void MeasureDataLabel(ChartPoint point, in FinancialLayout layout, in FinancialMeasureContext ctx)
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

        if (ctx.IsFirstDraw)
            label.CompleteTransition(
                BaseLabelGeometry.TextSizeProperty,
                BaseLabelGeometry.XProperty,
                BaseLabelGeometry.YProperty,
                BaseLabelGeometry.RotateTransformProperty);

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

            var layout = MeasureFinancialLayout(point, in ctx);

            AttachVisualToPaints(visual, layout.IsBullish, cartesianChart);

            visual.X = layout.X;
            visual.Width = layout.Width;
            visual.Y = layout.Y;
            visual.Open = layout.Open;
            visual.Close = layout.Close;
            visual.Low = layout.Low;
            visual.RemoveOnCompleted = false;

            if (point.Context.HoverArea is not RectangleHoverArea ha)
                point.Context.HoverArea = ha = new RectangleHoverArea();

            // Hover area uses the full category width so candles spaced by
            // MaxBarWidth (default 25 px) remain hoverable across the whole
            // axis slot — matches the BoxSeries / BarSeries gap-padding UX.
            // The visual (layout.X / layout.Width) stays at the clamped
            // candle width.
            _ = ha.SetDimensions(layout.CategoryHoverX, layout.Y, layout.CategoryHoverWidth, layout.Height);

            ConfigureHoverAnchor(ha, ctx.TooltipPosition);

            pointsCleanup.Clean(point);

            MeasureDataLabel(point, in layout, in ctx);

            OnPointMeasured(point);
        }

        pointsCleanup.CollectPoints(
            everFetched, cartesianChart.View, ctx.PrimaryScale, ctx.SecondaryScale, SoftDeleteOrDisposePoint);
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
        visual.Open = p;
        visual.Close = p;
        visual.Low = p;
        visual.RemoveOnCompleted = true;

        DataFactory.DisposePoint(point);

        var label = (TLabel?)point.Context.Label;
        if (label is null) return;

        label.TextSize = 1;
        label.RemoveOnCompleted = true;
    }

    /// <inheritdoc cref="ChartElement.GetPaintTasks"/>
    protected internal override Paint?[] GetPaintTasks() =>
        [UpFill, UpStroke, DownFill, DownStroke, DataLabelsPaint];

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

    /// <inheritdoc cref="ISeries.GetMiniatureGeometry"/>
    public override IDrawnElement GetMiniatureGeometry(ChartPoint? point)
        => new TMiniatureGeometry { Width = 0, Height = 0 };

    private static SeriesProperties GetProperties()
    {
        return SeriesProperties.Financial | SeriesProperties.PrimaryAxisVerticalOrientation |
            SeriesProperties.Solid | SeriesProperties.PrefersXStrategyTooltips;
    }
}
