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
using LiveChartsCore.Measure;
using LiveChartsCore.Motion;
using LiveChartsCore.Painting;

namespace LiveChartsCore;

/// <summary>
/// Abstract base for bar-shaped series that grow vertically (columns and their
/// stacked / range variants). Owns orientation-specific concerns that are
/// independent of the per-point sizing math: primary axis is Y, bars enter from
/// the pivot at zero height, the soft-delete collapse animates Height -> 0.
/// </summary>
/// <typeparam name="TModel">The model type.</typeparam>
/// <typeparam name="TVisual">The visual type.</typeparam>
/// <typeparam name="TLabel">The label type.</typeparam>
/// <typeparam name="TErrorGeometry">The error-bar geometry type.</typeparam>
public abstract class VerticalBarSeries<TModel, TVisual, TLabel, TErrorGeometry>
    : BarSeries<TModel, TVisual, TLabel>
        where TVisual : BoundedDrawnGeometry, new()
        where TLabel : BaseLabelGeometry, new()
        where TErrorGeometry : BaseLineGeometry, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VerticalBarSeries{TModel, TVisual, TLabel, TErrorGeometry}"/> class.
    /// </summary>
    /// <param name="values">The values.</param>
    /// <param name="isStacked">Whether the series is stacked.</param>
    protected VerticalBarSeries(IReadOnlyCollection<TModel>? values, bool isStacked = false)
        : base(GetProperties(isStacked), values)
    {
        DataPadding = new LvcPoint(0, 1);
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

        visual.X = secondary - visual.Width * 0.5f;
        visual.Y = p;
        visual.Height = 0;
        visual.RemoveOnCompleted = true;

        if (point.Context.AdditionalVisuals is not null)
        {
            var e = (ErrorVisual<TErrorGeometry>)point.Context.AdditionalVisuals;

            e.YError.Y = p;
            e.YError.Y1 = p;
            e.YError.RemoveOnCompleted = true;

            e.XError.X = secondary - visual.Width * 0.5f;
            e.XError.X1 = secondary - visual.Width * 0.5f;
            e.XError.RemoveOnCompleted = true;
        }

        DataFactory.DisposePoint(point);

        var label = (TLabel?)point.Context.Label;
        if (label is null) return;

        label.TextSize = 1;
        label.RemoveOnCompleted = true;
    }

    /// <inheritdoc cref="BarSeries{TModel, TVisual, TLabel}.BeginMeasure(CartesianChartEngine, bool)"/>
    protected override BarMeasureContext BeginMeasure(CartesianChartEngine chart, bool isStacked)
    {
        var primaryAxis = chart.GetYAxis(this);
        var secondaryAxis = chart.GetXAxis(this);

        var secondaryScale = secondaryAxis.GetNextScaler(chart);
        var primaryScale = primaryAxis.GetNextScaler(chart);
        var previousPrimaryScale = primaryAxis.GetActualScaler(chart);
        var previousSecondaryScale = secondaryAxis.GetActualScaler(chart);

        var helper = new BarMeasureHelper(
            secondaryScale, chart, this, secondaryAxis, primaryScale.ToPixels(pivot),
            chart.DrawMarginLocation.Y,
            chart.DrawMarginLocation.Y + chart.DrawMarginSize.Height, isStacked, isRow: false);
        var pHelper = new BarMeasureHelper(
            previousSecondaryScale, chart, this, secondaryAxis, previousPrimaryScale.ToPixels(pivot),
            chart.DrawMarginLocation.Y,
            chart.DrawMarginLocation.Y + chart.DrawMarginSize.Height, isStacked, isRow: false);

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

        var cy = ctx.PrimaryAxis.IsInverted
            ? (coordinate.PrimaryValue > pivot ? primary - b : primary)
            : (coordinate.PrimaryValue > pivot ? primary : primary - b);
        var x = secondary - helper.uwm + helper.cp;

        if (ctx.Stacker is not null)
        {
            var sy = ctx.Stacker.GetStack(point);

            float primaryI, primaryJ;
            if (coordinate.PrimaryValue >= 0)
            {
                primaryI = ctx.PrimaryScale.ToPixels(sy.Start);
                primaryJ = ctx.PrimaryScale.ToPixels(sy.End);
            }
            else
            {
                primaryI = ctx.PrimaryScale.ToPixels(sy.NegativeStart);
                primaryJ = ctx.PrimaryScale.ToPixels(sy.NegativeEnd);
            }

            cy = primaryJ;
            b = primaryI - primaryJ;
        }

        return new BarLayout(
            x: x, y: cy, width: helper.uw, height: b,
            categoryHoverX: secondary - helper.actualUw * 0.5f, categoryHoverY: cy,
            categoryHoverWidth: helper.actualUw, categoryHoverHeight: b);
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

        // Initial entry position — the previous helper is used mid-life (a new
        // visual joining a series that has already drawn) so the animation
        // sources from where the bar WOULD have been in the previous frame's
        // scaling. First draw uses the current helper directly.
        var helper = ctx.IsFirstDraw ? ctx.Helper : ctx.PreviousHelper;
        var secondaryScale = ctx.IsFirstDraw ? ctx.SecondaryScale : ctx.PreviousSecondaryScale;

        var xi = secondaryScale.ToPixels(coordinate.SecondaryValue) - helper.uwm + helper.cp;
        var pi = helper.p;
        var uwi = helper.uw;

        var r = new TVisual
        {
            X = xi,
            Y = pi,
            Width = uwi,
            Height = 0f,
        };

        if (r is BaseRoundedRectangleGeometry rg)
            rg.BorderRadius = new LvcPoint(ctx.Rx, ctx.Ry);

        ErrorVisual<TErrorGeometry>? e = null;
        if (ShowError && ErrorPaint is not null && ErrorPaint != Paint.Default)
        {
            e = new ErrorVisual<TErrorGeometry>();

            var cx = xi + uwi * 0.5f;
            e.YError.X = cx;
            e.YError.X1 = cx;
            e.YError.Y = pi;
            e.YError.Y1 = pi;

            e.XError.X = cx;
            e.XError.X1 = cx;
            e.XError.Y = pi;
            e.XError.Y1 = pi;

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
        var xe = secondary - helper.uwm + helper.cp + helper.uw * 0.5f;

        e.YError!.X = xe;
        e.YError.X1 = xe;
        e.YError.Y = primary + ctx.PrimaryScale.MeasureInPixels(pe.Yi);
        e.YError.Y1 = primary - ctx.PrimaryScale.MeasureInPixels(pe.Yj);
        e.YError.RemoveOnCompleted = false;

        e.XError!.X = xe - ctx.SecondaryScale.MeasureInPixels(pe.Xi);
        e.XError.X1 = xe + ctx.SecondaryScale.MeasureInPixels(pe.Xj);
        e.XError.Y = primary;
        e.XError.Y1 = primary;
        e.XError.RemoveOnCompleted = false;
    }

    /// <inheritdoc cref="BarSeries{TModel, TVisual, TLabel}.CollapseEmptyVisual(ChartPoint, in BarMeasureContext)"/>
    protected override void CollapseEmptyVisual(ChartPoint point, in BarMeasureContext ctx)
    {
        var helper = ctx.Helper;
        var secondary = ctx.SecondaryScale.ToPixels(point.Coordinate.SecondaryValue);

        if (point.Context.Visual is TVisual visual)
        {
            visual.X = secondary - helper.uwm + helper.cp;
            visual.Y = helper.p;
            visual.Width = helper.uw;
            visual.Height = 0;
            visual.RemoveOnCompleted = true;
            point.Context.Visual = null;
        }

        if (point.Context.Label is TLabel label)
        {
            label.X = secondary - helper.uwm + helper.cp;
            label.Y = helper.p;
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
        SeriesProperties.Bar | SeriesProperties.PrimaryAxisVerticalOrientation |
        SeriesProperties.Solid | SeriesProperties.PrefersXStrategyTooltips |
        (isStacked ? SeriesProperties.Stacked : 0);
}
