
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

    /// <inheritdoc cref="ChartElement.Invalidate(Chart)"/>
    public override void Invalidate(Chart chart)
    {
        var cartesianChart = (CartesianChartEngine)chart;
        _ = GetAnimation(cartesianChart);

        var primaryAxis = cartesianChart.GetYAxis(this);
        var secondaryAxis = cartesianChart.GetXAxis(this);

        var drawLocation = cartesianChart.DrawMarginLocation;
        var drawMarginSize = cartesianChart.DrawMarginSize;
        var secondaryScale = secondaryAxis.GetNextScaler(cartesianChart);
        var primaryScale = primaryAxis.GetNextScaler(cartesianChart);
        var previousPrimaryScale = primaryAxis.GetActualScaler(cartesianChart);
        var previousSecondaryScale = secondaryAxis.GetActualScaler(cartesianChart);

        var uw = secondaryScale.MeasureInPixels(secondaryAxis.UnitWidth);
        var puw = previousSecondaryScale is null ? 0 : previousSecondaryScale.MeasureInPixels(secondaryAxis.UnitWidth);
        var uwm = 0.5f * uw;

        var tp = chart.TooltipPosition;

        if (uw > MaxBarWidth)
        {
            uw = (float)MaxBarWidth;
            uwm = uw * 0.5f;
            puw = uw;
        }

        var actualZIndex = ZIndex == 0 ? ((ISeries)this).SeriesId : ZIndex;

        if (UpFill is not null && UpFill != Paint.Default)
        {
            UpFill.ZIndex = actualZIndex + PaintConstants.SeriesFillZIndexOffset;
            cartesianChart.Canvas.AddDrawableTask(UpFill, zone: CanvasZone.DrawMargin);
        }
        if (DownFill is not null && DownFill != Paint.Default)
        {
            DownFill.ZIndex = actualZIndex + PaintConstants.SeriesFillZIndexOffset;
            cartesianChart.Canvas.AddDrawableTask(DownFill, zone: CanvasZone.DrawMargin);
        }
        if (UpStroke is not null && UpStroke != Paint.Default)
        {
            UpStroke.ZIndex = actualZIndex + PaintConstants.SeriesStrokeZIndexOffset;
            cartesianChart.Canvas.AddDrawableTask(UpStroke, zone: CanvasZone.DrawMargin);
        }
        if (DownStroke is not null && DownStroke != Paint.Default)
        {
            DownStroke.ZIndex = actualZIndex + PaintConstants.SeriesStrokeZIndexOffset;
            cartesianChart.Canvas.AddDrawableTask(DownStroke, zone: CanvasZone.DrawMargin);
        }
        if (ShowDataLabels && DataLabelsPaint is not null && DataLabelsPaint != Paint.Default)
        {
            DataLabelsPaint.ZIndex = actualZIndex + PaintConstants.SeriesGeometryFillZIndexOffset;
            cartesianChart.Canvas.AddDrawableTask(DataLabelsPaint, zone: CanvasZone.DrawMargin);
        }

        var dls = (float)DataLabelsSize;
        var pointsCleanup = ChartPointCleanupContext.For(everFetched);

        var isFirstDraw = !chart.IsDrawn(((ISeries)this).SeriesId);

        foreach (var point in Fetch(cartesianChart))
        {
            var coordinate = point.Coordinate;
            var visual = point.Context.Visual as TVisual;
            var secondary = secondaryScale.ToPixels(coordinate.SecondaryValue);

            var high = primaryScale.ToPixels(coordinate.PrimaryValue);
            var open = primaryScale.ToPixels(coordinate.TertiaryValue);
            var close = primaryScale.ToPixels(coordinate.QuaternaryValue);
            var low = primaryScale.ToPixels(coordinate.QuinaryValue);
            var middle = open;

            if (point.IsEmpty || !IsVisible)
            {
                if (visual is not null)
                {
                    visual.X = secondary - uwm;
                    visual.Width = uw;
                    visual.Y = middle; // y coordinate is the highest
                    visual.Open = middle;
                    visual.Close = middle;
                    visual.Low = middle;
                    visual.RemoveOnCompleted = true;
                    point.Context.Visual = null;
                }

                if (point.Context.Label is not null)
                {
                    var label = (TLabel)point.Context.Label;

                    label.X = secondary - uwm;
                    label.Y = middle;
                    label.Opacity = 0;
                    label.RemoveOnCompleted = true;

                    point.Context.Label = null;
                }

                pointsCleanup.Clean(point);

                continue;
            }

            if (visual is null)
            {
                var xi = secondary - uwm;
                var uwi = uw;

                if (previousSecondaryScale is not null && previousPrimaryScale is not null)
                {
                    var previousP = previousPrimaryScale.ToPixels(pivot);
                    var previousPrimary = previousPrimaryScale.ToPixels(coordinate.PrimaryValue);
                    var bp = Math.Abs(previousPrimary - previousP);
                    var cyp = coordinate.PrimaryValue > pivot ? previousPrimary : previousPrimary - bp;

                    xi = previousSecondaryScale.ToPixels(coordinate.SecondaryValue) - uwm;
                    uwi = puw;
                }

                var r = new TVisual
                {
                    X = xi,
                    Width = uwi,
                    Y = middle, // y == high
                    Open = middle,
                    Close = middle,
                    Low = middle
                };

                visual = r;
                point.Context.Visual = visual;
                OnPointCreated(point);

                _ = everFetched.Add(point);
            }

            if (open > close)
            {
                if (UpFill is not null && UpFill != Paint.Default)
                    UpFill.AddGeometryToPaintTask(cartesianChart.Canvas, visual);
                if (UpStroke is not null && UpStroke != Paint.Default)
                    UpStroke.AddGeometryToPaintTask(cartesianChart.Canvas, visual);
                if (DownFill is not null && DownFill != Paint.Default)
                    DownFill.RemoveGeometryFromPaintTask(cartesianChart.Canvas, visual);
                if (DownStroke is not null && DownStroke != Paint.Default)
                    DownStroke.RemoveGeometryFromPaintTask(cartesianChart.Canvas, visual);
            }
            else
            {
                if (DownFill is not null && DownFill != Paint.Default)
                    DownFill.AddGeometryToPaintTask(cartesianChart.Canvas, visual);
                if (DownStroke is not null && DownStroke != Paint.Default)
                    DownStroke.AddGeometryToPaintTask(cartesianChart.Canvas, visual);
                if (UpFill is not null && UpFill != Paint.Default)
                    UpFill.RemoveGeometryFromPaintTask(cartesianChart.Canvas, visual);
                if (UpStroke is not null && UpStroke != Paint.Default)
                    UpStroke.RemoveGeometryFromPaintTask(cartesianChart.Canvas, visual);
            }

            var x = secondary - uwm;

            visual.X = x;
            visual.Width = uw;
            visual.Y = high;
            visual.Open = open;
            visual.Close = close;
            visual.Low = low;
            visual.RemoveOnCompleted = false;

            if (point.Context.HoverArea is not RectangleHoverArea ha)
                point.Context.HoverArea = ha = new RectangleHoverArea();

            _ = ha.SetDimensions(secondary - uwm, high, uw, Math.Abs(low - high));

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

            pointsCleanup.Clean(point);

            if (ShowDataLabels && DataLabelsPaint is not null && DataLabelsPaint != Paint.Default)
            {
                var label = (TLabel?)point.Context.Label;

                if (label is null)
                {
                    var l = new TLabel { X = secondary - uwm, Y = high, RotateTransform = (float)DataLabelsRotation, MaxWidth = (float)DataLabelsMaxWidth };
                    l.Animate(
                        GetAnimation(cartesianChart),
                        BaseLabelGeometry.XProperty,
                        BaseLabelGeometry.YProperty);
                    label = l;
                    point.Context.Label = l;
                }

                DataLabelsPaint.AddGeometryToPaintTask(cartesianChart.Canvas, label);

                label.Text = DataLabelsFormatter(new ChartPoint<TModel, TVisual, TLabel>(point));
                label.TextSize = dls;
                label.Padding = DataLabelsPadding;
                label.Paint = DataLabelsPaint;

                if (isFirstDraw)
                    label.CompleteTransition(
                        BaseLabelGeometry.TextSizeProperty,
                        BaseLabelGeometry.XProperty,
                        BaseLabelGeometry.YProperty,
                        BaseLabelGeometry.RotateTransformProperty);

                var m = label.Measure();
                var labelPosition = GetLabelPosition(
                    x, high, uw, Math.Abs(low - high), m, DataLabelsPosition,
                    SeriesProperties, coordinate.PrimaryValue > Pivot, drawLocation, drawMarginSize);
                if (DataLabelsTranslate is not null) label.TranslateTransform =
                        new LvcPoint(m.Width * DataLabelsTranslate.Value.X, m.Height * DataLabelsTranslate.Value.Y);

                label.X = labelPosition.X;
                label.Y = labelPosition.Y;
            }

            OnPointMeasured(point);
        }

        pointsCleanup.CollectPoints(
            everFetched, cartesianChart.View, primaryScale, secondaryScale, SoftDeleteOrDisposePoint);
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
