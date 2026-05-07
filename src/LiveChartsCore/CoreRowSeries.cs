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
using LiveChartsCore.Drawing.Segments;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.Motion;
using LiveChartsCore.Painting;

namespace LiveChartsCore;

/// <summary>
/// Defines the row series 
/// </summary>
/// <typeparam name="TModel">The type of the model.</typeparam>
/// <typeparam name="TVisual">The type of the visual.</typeparam>
/// <typeparam name="TLabel">The type of the label.</typeparam>
/// <seealso cref="BarSeries{TModel, TVisual, TLabel}" />
/// <typeparam name="TErrorGeometry">The type of the error geometry.</typeparam>
public abstract class CoreRowSeries<TModel, TVisual, TLabel, TErrorGeometry>
    : BarSeries<TModel, TVisual, TLabel>
        where TVisual : BoundedDrawnGeometry, new()
        where TLabel : BaseLabelGeometry, new()
        where TErrorGeometry : BaseLineGeometry, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CoreRowSeries{TModel, TVisual, TLabel, TErrorGeometry}"/> class.
    /// </summary>
    /// <param name="isStacked">if set to <c>true</c> [is stacked].</param>
    /// <param name="values">The values.</param>
    public CoreRowSeries(IReadOnlyCollection<TModel>? values, bool isStacked = false)
        : base(GetProperties(isStacked), values)
    {
        DataPadding = new LvcPoint(1, 0);
    }

    /// <inheritdoc cref="ChartElement.Invalidate(Chart)"/>
    public override void Invalidate(Chart chart)
    {
        var cartesianChart = (CartesianChartEngine)chart;
        _ = GetAnimation(cartesianChart);

        var primaryAxis = cartesianChart.GetYAxis(this);
        var secondaryAxis = cartesianChart.GetXAxis(this);

        var drawLocation = cartesianChart.DrawMarginLocation;
        var drawMarginSize = cartesianChart.DrawMarginSize;
        var secondaryScale = primaryAxis.GetNextScaler(cartesianChart);
        var primaryScale = secondaryAxis.GetNextScaler(cartesianChart);
        var previousPrimaryScale = secondaryAxis.GetActualScaler(cartesianChart);
        var previousSecondaryScale = primaryAxis.GetActualScaler(cartesianChart);

        var isStacked = (SeriesProperties & SeriesProperties.Stacked) == SeriesProperties.Stacked;

        var helper = new MeasureHelper(
            secondaryScale, cartesianChart, this, secondaryAxis, primaryScale.ToPixels(pivot),
            cartesianChart.DrawMarginLocation.X,
            cartesianChart.DrawMarginLocation.X + cartesianChart.DrawMarginSize.Width, isStacked, true);

        var pHelper = new MeasureHelper(
                previousSecondaryScale, cartesianChart, this, secondaryAxis, previousPrimaryScale.ToPixels(pivot),
                cartesianChart.DrawMarginLocation.X,
                cartesianChart.DrawMarginLocation.X + cartesianChart.DrawMarginSize.Width, isStacked, true);

        var actualZIndex = ZIndex == 0 ? ((ISeries)this).SeriesId : ZIndex;
        if (Fill is not null && Fill != Paint.Default)
        {
            Fill.ZIndex = actualZIndex + PaintConstants.SeriesFillZIndexOffset;
            cartesianChart.Canvas.AddDrawableTask(Fill, zone: CanvasZone.DrawMargin);
        }
        if (Stroke is not null && Stroke != Paint.Default)
        {
            Stroke.ZIndex = actualZIndex + PaintConstants.SeriesStrokeZIndexOffset;
            cartesianChart.Canvas.AddDrawableTask(Stroke, zone: CanvasZone.DrawMargin);
        }
        if (ShowError && ErrorPaint is not null && ErrorPaint != Paint.Default)
        {
            ErrorPaint.ZIndex = actualZIndex + PaintConstants.SeriesGeometryFillZIndexOffset;
            cartesianChart.Canvas.AddDrawableTask(ErrorPaint, zone: CanvasZone.DrawMargin);
        }
        if (ShowDataLabels && DataLabelsPaint is not null && DataLabelsPaint != Paint.Default)
        {
            DataLabelsPaint.ZIndex = actualZIndex + PaintConstants.SeriesGeometryStrokeZIndexOffset;
            cartesianChart.Canvas.AddDrawableTask(DataLabelsPaint, zone: CanvasZone.DrawMargin);
        }

        var dls = (float)DataLabelsSize;
        var pointsCleanup = ChartPointCleanupContext.For(everFetched);

        var rx = (float)Rx;
        var ry = (float)Ry;

        var stacker = isStacked ? cartesianChart.SeriesContext.GetStackPosition(this, GetStackGroup()) : null;
        var hasSvg = this.HasVariableSvgGeometry();

        var isFirstDraw = !chart.IsDrawn(((ISeries)this).SeriesId);

        foreach (var point in Fetch(cartesianChart))
        {
            var coordinate = point.Coordinate;

            var visual = point.Context.Visual as TVisual;
            var e = point.Context.AdditionalVisuals as ErrorVisual<TErrorGeometry>;

            var primary = primaryScale.ToPixels(coordinate.PrimaryValue);
            var secondary = secondaryScale.ToPixels(coordinate.SecondaryValue);
            var b = Math.Abs(primary - helper.p);

            if (point.IsEmpty || !IsVisible)
            {
                if (visual is not null)
                {
                    visual.X = secondary - helper.uwm + helper.cp;
                    visual.Y = helper.p;
                    visual.Width = helper.uw;
                    visual.Height = 0;
                    visual.RemoveOnCompleted = true;
                    point.Context.Visual = null;
                }

                if (point.Context.Label is not null)
                {
                    var label = (TLabel)point.Context.Label;

                    label.X = secondary - helper.uwm + helper.cp;
                    label.Y = helper.p;
                    label.Opacity = 0;
                    label.RemoveOnCompleted = true;

                    point.Context.Label = null;
                }

                pointsCleanup.Clean(point);

                continue;
            }

            if (visual is null)
            {
                var yi = secondary - helper.uwm + helper.cp;
                var pi = helper.p;
                var uwi = helper.uw;

                if (!isFirstDraw)
                {
                    var previousPrimary = previousPrimaryScale.ToPixels(coordinate.PrimaryValue);
                    var bp = Math.Abs(previousPrimary - pHelper.p);
                    var cyp = coordinate.PrimaryValue > pivot ? previousPrimary : previousPrimary - bp;

                    yi = previousSecondaryScale.ToPixels(coordinate.SecondaryValue) - pHelper.uwm + pHelper.cp;
                    pi = pHelper.p;
                    uwi = pHelper.uw;
                }

                var r = new TVisual
                {
                    X = pi,
                    Y = yi,
                    Width = 0f,
                    Height = uwi
                };

                if (r is BaseRoundedRectangleGeometry rg)
                    rg.BorderRadius = new LvcPoint(rx, ry);

                if (ShowError && ErrorPaint is not null && ErrorPaint != Paint.Default)
                {
                    e = new ErrorVisual<TErrorGeometry>();

                    e.YError.X = pi;
                    e.YError.X1 = pi;
                    e.YError.Y = yi;
                    e.YError.Y1 = yi;

                    e.XError.X = pi;
                    e.XError.X1 = pi;
                    e.XError.Y = yi;
                    e.XError.Y1 = yi;

                    point.Context.AdditionalVisuals = e;
                }

                visual = r;
                point.Context.Visual = visual;
                OnPointCreated(point);

                _ = everFetched.Add(point);
            }

            if (hasSvg)
            {
                var svgVisual = (IVariableSvgPath)visual;
                if (_geometrySvgChanged || svgVisual.SVGPath is null)
                    svgVisual.SVGPath = GeometrySvg ?? throw new Exception("svg path is not defined");
            }

            if (Fill is not null && Fill != Paint.Default)
                Fill.AddGeometryToPaintTask(cartesianChart.Canvas, visual);
            if (Stroke is not null && Stroke != Paint.Default)
                Stroke.AddGeometryToPaintTask(cartesianChart.Canvas, visual);
            if (ErrorPaint is not null && ErrorPaint != Paint.Default && e is not null)
            {
                ErrorPaint.AddGeometryToPaintTask(cartesianChart.Canvas, e!.YError);
                ErrorPaint.AddGeometryToPaintTask(cartesianChart.Canvas, e!.XError);
            }

            var cx = secondaryAxis.IsInverted
                ? (coordinate.PrimaryValue > pivot ? primary : primary - b)
                : (coordinate.PrimaryValue > pivot ? primary - b : primary);
            var y = secondary - helper.uwm + helper.cp;

            if (stacker is not null)
            {
                var sx = stacker.GetStack(point);

                float primaryI, primaryJ;
                if (coordinate.PrimaryValue >= 0)
                {
                    primaryI = primaryScale.ToPixels(sx.Start);
                    primaryJ = primaryScale.ToPixels(sx.End);
                }
                else
                {
                    primaryI = primaryScale.ToPixels(sx.NegativeStart);
                    primaryJ = primaryScale.ToPixels(sx.NegativeEnd);
                }

                cx = primaryJ;
                b = primaryI - primaryJ;
            }

            visual.X = cx;
            visual.Y = y;
            visual.Width = b;
            visual.Height = helper.uw;

            if (!coordinate.PointError.IsEmpty && ShowError && ErrorPaint is not null && ErrorPaint != Paint.Default)
            {
                var pe = coordinate.PointError;
                var ye = secondary - helper.uwm + helper.cp + helper.uw * 0.5f;

                // Note #20231608
                // Because coordinates are inverted in the row series
                // we use the XError as the YError and vice versa
                // strange.. this should be improved.

                e!.YError!.X = primary + primaryScale.MeasureInPixels(pe.Yi);
                e.YError.X1 = primary - primaryScale.MeasureInPixels(pe.Yi);
                e.YError.Y = ye;
                e.YError.Y1 = ye;
                e.YError.RemoveOnCompleted = false;

                e.XError!.X = primary;
                e.XError.X1 = primary;
                e.XError.Y = ye - secondaryScale.MeasureInPixels(pe.Xi);
                e.XError.Y1 = ye + secondaryScale.MeasureInPixels(pe.Xj);
                e.XError.RemoveOnCompleted = false;
            }

            if (visual is BaseRoundedRectangleGeometry rrg)
                rrg.BorderRadius = new LvcPoint(rx, ry);

            visual.RemoveOnCompleted = false;

            if (point.Context.HoverArea is not RectangleHoverArea ha)
                point.Context.HoverArea = ha = new RectangleHoverArea();

            _ = ha.SetDimensions(cx, secondary - helper.actualUw * 0.5f, b, helper.actualUw)
                .CenterXToolTip().StartYToolTip();

            if (chart.FindingStrategy == FindingStrategy.ExactMatch)
                _ = ha
                    .SetDimensions(cx, y, b, helper.uw)
                    .CenterXToolTip().StartYToolTip();

            pointsCleanup.Clean(point);

            if (ShowDataLabels && DataLabelsPaint is not null && DataLabelsPaint != Paint.Default)
            {
                var label = (TLabel?)point.Context.Label;

                if (label is null)
                {
                    var l = new TLabel { X = helper.p, Y = secondary - helper.uwm + helper.cp, RotateTransform = (float)DataLabelsRotation, MaxWidth = (float)DataLabelsMaxWidth };
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
                    cx, y, b, helper.uw, m, DataLabelsPosition, SeriesProperties,
                    coordinate.PrimaryValue > Pivot, drawLocation, drawMarginSize);
                if (DataLabelsTranslate is not null) label.TranslateTransform =
                        new LvcPoint(m.Width * DataLabelsTranslate.Value.X, m.Height * DataLabelsTranslate.Value.Y);

                label.X = labelPosition.X;
                label.Y = labelPosition.Y;
            }

            OnPointMeasured(point);
        }

        pointsCleanup.CollectPoints(
            everFetched, cartesianChart.View, primaryScale, secondaryScale, SoftDeleteOrDisposePoint);
        _geometrySvgChanged = false;
    }

    /// <inheritdoc cref="CartesianSeries{TModel, TVisual, TLabel}.GetRequestedSecondaryOffset"/>
    protected override double GetRequestedSecondaryOffset() => 0.5f;

    /// <inheritdoc cref="Series{TModel, TVisual, TLabel}.SetDefaultPointTransitions(ChartPoint)"/>
    protected override void SetDefaultPointTransitions(ChartPoint chartPoint)
    {
        var chart = chartPoint.Context.Chart;
        if (chartPoint.Context.Visual is not TVisual visual) throw new Exception("Unable to initialize the point instance.");

        var animation = GetAnimation(chart.CoreChart);

        visual.Animate(animation);

        if (chartPoint.Context.AdditionalVisuals is not null)
        {
            var e = (ErrorVisual<TErrorGeometry>)chartPoint.Context.AdditionalVisuals;
            e.YError.Animate(animation);
            e.XError.Animate(animation);
        }
    }

    /// <inheritdoc cref="CartesianSeries{TModel, TVisual, TLabel}.SoftDeleteOrDisposePoint(ChartPoint, Scaler, Scaler)"/>
    protected internal override void SoftDeleteOrDisposePoint(ChartPoint point, Scaler primaryScale, Scaler secondaryScale)
    {
        var visual = (TVisual?)point.Context.Visual;
        if (visual is null) return;
        if (DataFactory is null) throw new Exception("Data provider not found");

        var p = primaryScale.ToPixels(pivot);
        var secondary = secondaryScale.ToPixels(point.Coordinate.SecondaryValue);

        visual.X = p;
        visual.Y = secondary - visual.Height * 0.5f;
        visual.Width = 0;
        visual.RemoveOnCompleted = true;

        if (point.Context.AdditionalVisuals is not null)
        {
            var e = (ErrorVisual<TErrorGeometry>)point.Context.AdditionalVisuals;

            e.YError.Y = secondary - visual.Height * 0.5f;
            e.YError.Y1 = secondary - visual.Height * 0.5f;
            e.YError.RemoveOnCompleted = true;

            e.XError.X = p;
            e.XError.X1 = p;
            e.XError.RemoveOnCompleted = true;
        }

        DataFactory.DisposePoint(point);

        var label = (TLabel?)point.Context.Label;
        if (label is null) return;

        label.TextSize = 1;
        label.RemoveOnCompleted = true;
    }

    /// <inheritdoc cref="CartesianSeries{TModel, TVisual, TLabel}.GetBounds(Chart, ICartesianAxis, ICartesianAxis)"/>
    public override SeriesBounds GetBounds(Chart chart, ICartesianAxis secondaryAxis, ICartesianAxis primaryAxis)
    {
        var rawBounds = DataFactory.GetCartesianBounds(chart, this, primaryAxis, secondaryAxis);
        if (rawBounds.HasData) return rawBounds;

        var rawBaseBounds = rawBounds.Bounds;

        var tickPrimary = primaryAxis.GetTick(chart.ControlSize, rawBaseBounds.VisibleSecondaryBounds);
        var tickSecondary = secondaryAxis.GetTick(chart.ControlSize, rawBaseBounds.VisiblePrimaryBounds);

        var ts = tickSecondary.Value * DataPadding.X;
        var tp = tickPrimary.Value * DataPadding.Y;

        var rgs = GetRequestedGeometrySize();
        var rso = GetRequestedSecondaryOffset();
        var rpo = GetRequestedPrimaryOffset();

        var dimensionalBounds = new DimensionalBounds
        {
            SecondaryBounds = new Bounds
            {
                Max = rawBaseBounds.PrimaryBounds.Max + rpo * secondaryAxis.UnitWidth,
                Min = rawBaseBounds.PrimaryBounds.Min - rpo * secondaryAxis.UnitWidth,
                MinDelta = rawBaseBounds.PrimaryBounds.MinDelta,
                PaddingMax = ts,
                PaddingMin = ts,
                RequestedGeometrySize = rgs
            },
            PrimaryBounds = new Bounds
            {
                Max = rawBaseBounds.SecondaryBounds.Max + rso * primaryAxis.UnitWidth,
                Min = rawBaseBounds.SecondaryBounds.Min - rso * primaryAxis.UnitWidth,
                MinDelta = rawBaseBounds.SecondaryBounds.MinDelta,
                PaddingMax = tp,
                PaddingMin = tp,
                RequestedGeometrySize = rgs
            },
            VisibleSecondaryBounds = new Bounds
            {
                Max = rawBaseBounds.VisiblePrimaryBounds.Max + rpo * secondaryAxis.UnitWidth,
                Min = rawBaseBounds.VisiblePrimaryBounds.Min - rpo * secondaryAxis.UnitWidth
            },
            VisiblePrimaryBounds = new Bounds
            {
                Max = rawBaseBounds.VisibleSecondaryBounds.Max + rso * primaryAxis.UnitWidth,
                Min = rawBaseBounds.VisibleSecondaryBounds.Min - rso * primaryAxis.UnitWidth,
            },
            TertiaryBounds = rawBaseBounds.TertiaryBounds,
            VisibleTertiaryBounds = rawBaseBounds.VisibleTertiaryBounds
        };

        return new SeriesBounds(dimensionalBounds, false);
    }

    private static SeriesProperties GetProperties(bool isStacked)
    {
        return SeriesProperties.Bar | SeriesProperties.PrimaryAxisHorizontalOrientation |
              SeriesProperties.Solid | SeriesProperties.PrefersYStrategyTooltips |
              (isStacked ? SeriesProperties.Stacked : 0);
    }
}
