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
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.Painting;

namespace LiveChartsCore;

/// <summary>
/// Defines a column-shaped series whose rectangles span <c>[Low, High]</c> on the
/// value axis instead of <c>[Pivot, Value]</c>. The point model is expected to
/// carry the low endpoint in <see cref="Kernel.Coordinate.TertiaryValue"/> and the
/// high endpoint in <see cref="Kernel.Coordinate.PrimaryValue"/> (see
/// <see cref="Defaults.RangeValue"/>). Stacking is not supported — ranges have
/// no natural baseline to accumulate from.
/// </summary>
public abstract class CoreRangeColumnSeries<TModel, TVisual, TLabel, TErrorGeometry>
    : VerticalBarSeries<TModel, TVisual, TLabel, TErrorGeometry>
        where TVisual : BoundedDrawnGeometry, new()
        where TLabel : BaseLabelGeometry, new()
        where TErrorGeometry : BaseLineGeometry, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CoreRangeColumnSeries{TModel, TVisual, TLabel, TErrorGeometry}"/> class.
    /// </summary>
    protected CoreRangeColumnSeries(IReadOnlyCollection<TModel>? values)
        : base(values, isStacked: false)
    {
    }

    /// <inheritdoc cref="BarSeries{TModel, TVisual, TLabel}.MeasureBarLayout(ChartPoint, in BarMeasureContext)"/>
    protected override BarLayout MeasureBarLayout(ChartPoint point, in BarMeasureContext ctx)
    {
        var coordinate = point.Coordinate;
        var helper = ctx.Helper;

        // PrimaryValue carries High, TertiaryValue carries Low (see RangeValue.cs).
        // For a non-inverted Y axis, "high" maps to a smaller pixel Y and "low" to
        // a larger one — bar Y is the smaller pixel value.
        var highPx = ctx.PrimaryScale.ToPixels(coordinate.PrimaryValue);
        var lowPx = ctx.PrimaryScale.ToPixels(coordinate.TertiaryValue);
        var secondary = ctx.SecondaryScale.ToPixels(coordinate.SecondaryValue);

        var y = Math.Min(highPx, lowPx);
        var h = Math.Abs(highPx - lowPx);
        var x = secondary - helper.uwm + helper.cp;

        return new BarLayout(
            x: x, y: y, width: helper.uw, height: h,
            categoryHoverX: secondary - helper.actualUw * 0.5f, categoryHoverY: y,
            categoryHoverWidth: helper.actualUw, categoryHoverHeight: h);
    }

    /// <inheritdoc cref="BarSeries{TModel, TVisual, TLabel}.EnsureVisualForPoint(ChartPoint, in BarMeasureContext)"/>
    protected override TVisual EnsureVisualForPoint(ChartPoint point, in BarMeasureContext ctx)
    {
        var visual = point.Context.Visual as TVisual;
        var coordinate = point.Coordinate;

        // Range bars don't carry error visuals in this initial implementation;
        // a re-attach (visual already exists from a prior frame) is a no-op.
        if (visual is not null) return visual;

        // Range bars have no natural baseline (pivot is irrelevant — a bar from
        // [10, 20] has no reason to "grow from 0"). Enter at the midpoint of
        // [Low, High] at zero height so the bar expands symmetrically outward to
        // its real endpoints as the animation completes.
        var helper = ctx.IsFirstDraw ? ctx.Helper : ctx.PreviousHelper;
        var secondaryScale = ctx.IsFirstDraw ? ctx.SecondaryScale : ctx.PreviousSecondaryScale;
        var primaryScale = ctx.IsFirstDraw ? ctx.PrimaryScale : ctx.PreviousPrimaryScale;

        var highPx = primaryScale.ToPixels(coordinate.PrimaryValue);
        var lowPx = primaryScale.ToPixels(coordinate.TertiaryValue);
        var midPx = (highPx + lowPx) * 0.5f;
        var xi = secondaryScale.ToPixels(coordinate.SecondaryValue) - helper.uwm + helper.cp;
        var uwi = helper.uw;

        var r = new TVisual
        {
            X = xi,
            Y = midPx,
            Width = uwi,
            Height = 0f,
        };

        if (r is BaseRoundedRectangleGeometry rg)
            rg.BorderRadius = new LvcPoint(ctx.Rx, ctx.Ry);

        point.Context.Visual = r;
        OnPointCreated(point);

        _ = everFetched.Add(point);

        return r;
    }

    /// <inheritdoc cref="VerticalBarSeries{TModel, TVisual, TLabel, TErrorGeometry}.GetBounds(Chart, ICartesianAxis, ICartesianAxis)"/>
    public override SeriesBounds GetBounds(Chart chart, ICartesianAxis secondaryAxis, ICartesianAxis primaryAxis)
    {
        var sb = base.GetBounds(chart, secondaryAxis, primaryAxis);
        if (sb.HasData) return sb;

        // base.GetBounds builds the value-axis bounds (PrimaryBounds = Y axis
        // for a column series) from PrimaryValue alone — that's High. For range
        // bars the Low endpoint lives in TertiaryValue and is captured into
        // TertiaryBounds; merge it back so the auto axis covers both ends.
        // Without this, a Waterfall step whose Low sits above min(High) gets
        // clipped (a "Costs" 180->130 bar wouldn't be visible if min(High) is
        // 110 elsewhere in the series and no MinLimit/MaxLimit is set).
        var b = sb.Bounds;
        b.PrimaryBounds.AppendValue(b.TertiaryBounds);
        b.VisiblePrimaryBounds.AppendValue(b.VisibleTertiaryBounds);

        return sb;
    }

    /// <inheritdoc cref="VerticalBarSeries{TModel, TVisual, TLabel, TErrorGeometry}.SoftDeleteOrDisposePoint(ChartPoint, Scaler, Scaler)"/>
    protected internal override void SoftDeleteOrDisposePoint(ChartPoint point, Scaler primaryScale, Scaler secondaryScale)
    {
        var visual = (TVisual?)point.Context.Visual;
        if (visual is null) return;
        if (DataFactory is null) throw new Exception("Data provider not found");

        // Collapse to the midpoint of the bar's own range — symmetric counterpart
        // to the EnsureVisualForPoint entry seed.
        var coordinate = point.Coordinate;
        var highPx = primaryScale.ToPixels(coordinate.PrimaryValue);
        var lowPx = primaryScale.ToPixels(coordinate.TertiaryValue);
        var midPx = (highPx + lowPx) * 0.5f;
        var secondary = secondaryScale.ToPixels(coordinate.SecondaryValue);

        visual.X = secondary - visual.Width * 0.5f;
        visual.Y = midPx;
        visual.Height = 0;
        visual.RemoveOnCompleted = true;

        DataFactory.DisposePoint(point);

        var label = (TLabel?)point.Context.Label;
        if (label is null) return;

        label.TextSize = 1;
        label.RemoveOnCompleted = true;
    }

    /// <summary>
    /// Range column tooltip lists both endpoints — defaults to "{low} → {high}" using
    /// the Y axis labeler for each value, so a <see cref="SkiaSharpView.DateTimeAxis"/>
    /// renders dates and a numeric axis renders numbers. Users can override the whole
    /// format via <see cref="CartesianSeries{TModel, TVisual, TLabel}.YToolTipLabelFormatter"/>.
    /// </summary>
    /// <inheritdoc cref="ISeries.GetPrimaryToolTipText(ChartPoint)"/>
    public override string? GetPrimaryToolTipText(ChartPoint point)
    {
        if (YToolTipLabelFormatter is not null)
            return YToolTipLabelFormatter(new ChartPoint<TModel, TVisual, TLabel>(point));

        var chart = (CartesianChartEngine)point.Context.Chart.CoreChart;
        var series = (ICartesianSeries)point.Context.Series;
        var valueAxis = chart.YAxes[series.ScalesYAt];

        var low = valueAxis.Labels is not null
            ? Labelers.BuildNamedLabeler(valueAxis.Labels)(point.Coordinate.TertiaryValue)
            : valueAxis.Labeler(point.Coordinate.TertiaryValue);
        var high = valueAxis.Labels is not null
            ? Labelers.BuildNamedLabeler(valueAxis.Labels)(point.Coordinate.PrimaryValue)
            : valueAxis.Labeler(point.Coordinate.PrimaryValue);

        return $"{low} → {high}";
    }
}
