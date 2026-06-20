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
/// Abstract base for bar-shaped series that grow horizontally (rows and their
/// stacked / range variants). Owns orientation-specific concerns that are
/// independent of the per-point sizing math: primary axis is X, bars enter from
/// the pivot at zero width, the soft-delete collapse animates Width -> 0, and
/// <see cref="GetBounds"/> swaps the X / Y axis roles when delegating to
/// <see cref="Kernel.Providers.DataFactory.GetCartesianBounds"/>.
/// </summary>
/// <typeparam name="TModel">The model type.</typeparam>
/// <typeparam name="TVisual">The visual type.</typeparam>
/// <typeparam name="TLabel">The label type.</typeparam>
/// <typeparam name="TErrorGeometry">The error-bar geometry type.</typeparam>
public abstract class HorizontalBarSeries<TModel, TVisual, TLabel, TErrorGeometry>
    : BarSeries<TModel, TVisual, TLabel>
        where TVisual : BoundedDrawnGeometry, new()
        where TLabel : BaseLabelGeometry, new()
        where TErrorGeometry : BaseLineGeometry, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HorizontalBarSeries{TModel, TVisual, TLabel, TErrorGeometry}"/> class.
    /// </summary>
    /// <param name="values">The values.</param>
    /// <param name="isStacked">Whether the series is stacked.</param>
    protected HorizontalBarSeries(IReadOnlyCollection<TModel>? values, bool isStacked = false)
        : base(GetProperties(isStacked), values)
    {
        DataPadding = new LvcPoint(1, 0);
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

    /// <inheritdoc cref="BarSeries{TModel, TVisual, TLabel}.BeginMeasure(CartesianChartEngine, bool)"/>
    protected override BarMeasureContext BeginMeasure(CartesianChartEngine chart, bool isStacked)
    {
        // PrimaryAxis = value-direction axis = X for horizontal bars.
        // SecondaryAxis = category-direction axis = Y for horizontal bars.
        var primaryAxis = chart.GetXAxis(this);
        var secondaryAxis = chart.GetYAxis(this);

        var secondaryScale = secondaryAxis.GetNextScaler(chart);
        var primaryScale = primaryAxis.GetNextScaler(chart);
        var previousPrimaryScale = primaryAxis.GetActualScaler(chart);
        var previousSecondaryScale = secondaryAxis.GetActualScaler(chart);

        // BarMeasureHelper computes the bar's row height as
        //   scaler.MeasureInPixels(axis.UnitWidth)
        // so the (scaler, axis) pair must be the SAME axis — the category axis,
        // which for horizontal bars is secondaryAxis (Y). The original CoreRowSeries
        // passed primaryAxis here, which appeared to work only because the typical
        // value axis ships with UnitWidth = 1 — RangeRowSeries with a DateTimeAxis
        // (UnitWidth = unit.Ticks) immediately exposes the bug as a sky-high helper.uw
        // value that wrecks bar geometry, hover areas, and tooltip positioning.
        var helper = new BarMeasureHelper(
            secondaryScale, chart, this, secondaryAxis, primaryScale.ToPixels(pivot),
            chart.DrawMarginLocation.X,
            chart.DrawMarginLocation.X + chart.DrawMarginSize.Width, isStacked, isRow: true);
        var pHelper = new BarMeasureHelper(
            previousSecondaryScale, chart, this, secondaryAxis, previousPrimaryScale.ToPixels(pivot),
            chart.DrawMarginLocation.X,
            chart.DrawMarginLocation.X + chart.DrawMarginSize.Width, isStacked, isRow: true);

        var stacker = isStacked ? chart.SeriesContext.GetStackPosition(this, GetStackGroup()) : null;
        var hasSvg = this.HasVariableSvgGeometry();
        var isFirstDraw = !((Chart)chart).IsDrawn(((ISeries)this).SeriesId);

        return new BarMeasureContext(
            chart, primaryAxis, secondaryAxis,
            primaryScale, secondaryScale,
            previousPrimaryScale, previousSecondaryScale,
            helper, pHelper, stacker,
            isFirstDraw, hasSvg,
            chart.DrawMarginLocation, chart.DrawMarginSize,
            (float)Rx, (float)Ry, (float)DataLabelsSize);
    }

    /// <inheritdoc cref="BarSeries{TModel, TVisual, TLabel}.MeasureBarLayout(ChartPoint, in BarMeasureContext)"/>
    protected override BarLayout MeasureBarLayout(ChartPoint point, in BarMeasureContext ctx)
    {
        var coordinate = point.Coordinate;
        var helper = ctx.Helper;

        var primary = ctx.PrimaryScale.ToPixels(coordinate.PrimaryValue);
        var secondary = ctx.SecondaryScale.ToPixels(coordinate.SecondaryValue);
        var b = Math.Abs(primary - helper.p);

        var cx = ctx.PrimaryAxis.IsInverted
            ? (coordinate.PrimaryValue > pivot ? primary : primary - b)
            : (coordinate.PrimaryValue > pivot ? primary - b : primary);
        var y = secondary - helper.uwm + helper.cp;

        if (ctx.Stacker is not null)
        {
            var sx = ctx.Stacker.GetStack(point);

            float primaryI, primaryJ;
            if (coordinate.PrimaryValue >= 0)
            {
                primaryI = ctx.PrimaryScale.ToPixels(sx.Start);
                primaryJ = ctx.PrimaryScale.ToPixels(sx.End);
            }
            else
            {
                primaryI = ctx.PrimaryScale.ToPixels(sx.NegativeStart);
                primaryJ = ctx.PrimaryScale.ToPixels(sx.NegativeEnd);
            }

            cx = primaryJ;
            b = primaryI - primaryJ;
        }

        return new BarLayout(
            x: cx, y: y, width: b, height: helper.uw,
            categoryHoverX: cx, categoryHoverY: secondary - helper.actualUw * 0.5f,
            categoryHoverWidth: b, categoryHoverHeight: helper.actualUw);
    }

    /// <inheritdoc cref="BarSeries{TModel, TVisual, TLabel}.EnsureVisualForPoint(ChartPoint, in BarMeasureContext)"/>
    protected override TVisual EnsureVisualForPoint(ChartPoint point, in BarMeasureContext ctx)
    {
        var visual = point.Context.Visual as TVisual;
        var coordinate = point.Coordinate;

        if (visual is not null)
        {
            AttachErrorVisualsToPaint(point.Context.AdditionalVisuals as ErrorVisual<TErrorGeometry>, ctx.Chart.Canvas);
            return visual;
        }

        var helper = ctx.IsFirstDraw ? ctx.Helper : ctx.PreviousHelper;
        var secondaryScale = ctx.IsFirstDraw ? ctx.SecondaryScale : ctx.PreviousSecondaryScale;

        var yi = secondaryScale.ToPixels(coordinate.SecondaryValue) - helper.uwm + helper.cp;
        var pi = helper.p;
        var uwi = helper.uw;

        var r = new TVisual
        {
            X = pi,
            Y = yi,
            Width = 0f,
            Height = uwi,
        };

        if (r is BaseRoundedRectangleGeometry rg)
            rg.BorderRadius = new LvcPoint(ctx.Rx, ctx.Ry);

        ErrorVisual<TErrorGeometry>? e = null;
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

        point.Context.Visual = r;
        OnPointCreated(point);

        _ = everFetched.Add(point);

        AttachErrorVisualsToPaint(e, ctx.Chart.Canvas);

        return r;
    }

    /// <inheritdoc cref="BarSeries{TModel, TVisual, TLabel}.MeasureErrorBars(ChartPoint, in BarLayout, in BarMeasureContext)"/>
    protected override void MeasureErrorBars(ChartPoint point, in BarLayout layout, in BarMeasureContext ctx)
    {
        var coordinate = point.Coordinate;
        if (coordinate.PointError.IsEmpty) return;
        if (!ShowError || ErrorPaint is null || ErrorPaint == Paint.Default) return;
        if (point.Context.AdditionalVisuals is not ErrorVisual<TErrorGeometry> e) return;

        var pe = coordinate.PointError;
        var primary = ctx.PrimaryScale.ToPixels(coordinate.PrimaryValue);
        var secondary = ctx.SecondaryScale.ToPixels(coordinate.SecondaryValue);
        var helper = ctx.Helper;
        var ye = secondary - helper.uwm + helper.cp + helper.uw * 0.5f;

        // Note #20231608: row series swaps the roles of XError and YError relative
        // to the column convention. YError carries the HORIZONTAL whisker (drawn
        // along the value axis), XError carries the VERTICAL caps. Preserved from
        // the original CoreRowSeries.Invalidate implementation.
        e.YError!.X = primary + ctx.PrimaryScale.MeasureInPixels(pe.Yi);
        e.YError.X1 = primary - ctx.PrimaryScale.MeasureInPixels(pe.Yj);
        e.YError.Y = ye;
        e.YError.Y1 = ye;
        e.YError.RemoveOnCompleted = false;

        e.XError!.X = primary;
        e.XError.X1 = primary;
        e.XError.Y = ye - ctx.SecondaryScale.MeasureInPixels(pe.Xi);
        e.XError.Y1 = ye + ctx.SecondaryScale.MeasureInPixels(pe.Xj);
        e.XError.RemoveOnCompleted = false;
    }

    /// <inheritdoc cref="BarSeries{TModel, TVisual, TLabel}.CollapseEmptyVisual(ChartPoint, in BarMeasureContext)"/>
    protected override void CollapseEmptyVisual(ChartPoint point, in BarMeasureContext ctx)
    {
        var helper = ctx.Helper;
        var secondary = ctx.SecondaryScale.ToPixels(point.Coordinate.SecondaryValue);

        if (point.Context.Visual is TVisual visual)
        {
            visual.X = helper.p;
            visual.Y = secondary - helper.uwm + helper.cp;
            visual.Width = 0;
            visual.Height = helper.uw;
            visual.RemoveOnCompleted = true;
            point.Context.Visual = null;
        }

        if (point.Context.Label is TLabel label)
        {
            label.X = helper.p;
            label.Y = secondary - helper.uwm + helper.cp;
            label.Opacity = 0;
            label.RemoveOnCompleted = true;
            point.Context.Label = null;
        }
    }

    private void AttachErrorVisualsToPaint(ErrorVisual<TErrorGeometry>? e, CoreMotionCanvas canvas)
    {
        if (e is null) return;
        if (ErrorPaint is null || ErrorPaint == Paint.Default) return;
        ErrorPaint.AddGeometryToPaintTask(canvas, e.YError);
        ErrorPaint.AddGeometryToPaintTask(canvas, e.XError);
    }

    private static SeriesProperties GetProperties(bool isStacked) =>
        SeriesProperties.Bar | SeriesProperties.PrimaryAxisHorizontalOrientation |
        SeriesProperties.Solid | SeriesProperties.PrefersYStrategyTooltips |
        (isStacked ? SeriesProperties.Stacked : 0);
}
