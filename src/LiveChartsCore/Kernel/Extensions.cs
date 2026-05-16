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
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.Motion;
using LiveChartsCore.VisualElements;

namespace LiveChartsCore.Kernel;

/// <summary>
/// LiveCharts kernel extensions.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Calculates the tooltip location.
    /// </summary>
    /// <param name="foundPoints">The points.</param>
    /// <param name="tooltipSize">The tooltip size.</param>
    /// <param name="chart">The chart.</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static LvcPoint GetTooltipLocation(
        this IEnumerable<ChartPoint> foundPoints,
        LvcSize tooltipSize,
        Chart chart)
    {
        var location = chart.Kind is ChartKind.Cartesian or ChartKind.Polar
            ? _getCartesianTooltipLocation(foundPoints, chart, tooltipSize)
            : _getPieTooltipLocation(foundPoints, chart, tooltipSize);

        var controlSize = chart.ControlSize;

        if (location.Y < 0) location.Y = 0;
        if (location.X < 0) location.X = 0;
        if (location.Y + tooltipSize.Height > controlSize.Height) location.Y = controlSize.Height - tooltipSize.Height;
        if (location.X + tooltipSize.Width > controlSize.Width) location.X = controlSize.Width - tooltipSize.Width;

        return location;
    }

    private static LvcPoint _getCartesianTooltipLocation(
        IEnumerable<ChartPoint> foundPoints, Chart chart, LvcSize tooltipSize)
    {
        var count = 0f;
        var placementContext = new TooltipPlacementContext(chart.TooltipPosition);

        foreach (var point in foundPoints)
        {
            if (point.Context.HoverArea is null) continue;
            point.Context.HoverArea.SuggestTooltipPlacement(placementContext, tooltipSize);
            count++;
        }

        if (count == 0) return new();

        var avrgX = (placementContext.MostRight + placementContext.MostLeft) * 0.5f - tooltipSize.Width * 0.5f;
        var avrgY = (placementContext.MostTop + placementContext.MostBottom) * 0.5f - tooltipSize.Height * 0.5f;

        var position = chart.TooltipPosition;

        var x = avrgX;
        var y = placementContext.MostAutoTop - tooltipSize.Height;
        chart.AutoToolTipsInfo.ToolTipPlacement = placementContext.AutoPopPupPlacement;

        switch (position)
        {
            case TooltipPosition.Top:
                x = avrgX;
                y = placementContext.MostTop - tooltipSize.Height;
                chart.AutoToolTipsInfo.ToolTipPlacement = PopUpPlacement.Top;
                break;
            case TooltipPosition.Bottom:
                x = avrgX;
                y = placementContext.MostBottom;
                chart.AutoToolTipsInfo.ToolTipPlacement = PopUpPlacement.Bottom;
                break;
            case TooltipPosition.Left:
                x = placementContext.MostLeft - tooltipSize.Width;
                y = avrgY;
                chart.AutoToolTipsInfo.ToolTipPlacement = PopUpPlacement.Left;
                break;
            case TooltipPosition.Right:
                x = placementContext.MostRight;
                y = avrgY;
                chart.AutoToolTipsInfo.ToolTipPlacement = PopUpPlacement.Right;
                break;
            case TooltipPosition.Center:
                x = avrgX;
                y = avrgY;
                chart.AutoToolTipsInfo.ToolTipPlacement = PopUpPlacement.Top;
                break;
            case TooltipPosition.Hidden:
            case TooltipPosition.Auto:
            default:
                break;
        }

        if (x < 0)
        {
            // the tooltip is out of the left edge
            // we return TooltipPosition.Right
            chart.AutoToolTipsInfo.ToolTipPlacement = PopUpPlacement.Right;
            return new(placementContext.MostRight, avrgY);
        }

        if (x + tooltipSize.Width > chart.ControlSize.Width)
        {
            // the tooltip is out of the right edge
            // we return TooltipPosition.Left
            chart.AutoToolTipsInfo.ToolTipPlacement = PopUpPlacement.Left;
            return new(placementContext.MostLeft - tooltipSize.Width, avrgY);
        }

        if (y < 0)
        {
            y += tooltipSize.Height;
            chart.AutoToolTipsInfo.ToolTipPlacement = PopUpPlacement.Bottom;
        }
        if (y + tooltipSize.Height > chart.ControlSize.Height)
        {
            y -= tooltipSize.Height;
            chart.AutoToolTipsInfo.ToolTipPlacement = PopUpPlacement.Top;
        }

        return new(x, y);
    }

    private static LvcPoint _getPieTooltipLocation(
        IEnumerable<ChartPoint> foundPoints, Chart chart, LvcSize tooltipSize)
    {
        var placementContext = new TooltipPlacementContext(TooltipPosition.Auto);

        foreach (var foundPoint in foundPoints)
        {
            if (foundPoint.Context.HoverArea is null) continue;
            foundPoint.Context.HoverArea.SuggestTooltipPlacement(placementContext, tooltipSize);
        }

        chart.AutoToolTipsInfo.ToolTipPlacement = PopUpPlacement.Top;

        var p = new LvcPoint(
            placementContext.PieX - tooltipSize.Width * 0.5f,
            placementContext.PieY - tooltipSize.Height);

        if (p.Y < 0)
        {
            chart.AutoToolTipsInfo.ToolTipPlacement = PopUpPlacement.Bottom;

            p = new LvcPoint(
                placementContext.PieX - tooltipSize.Width * 0.5f,
                placementContext.PieY);
        }

        return p;
    }

    /// <summary>
    /// Gets the tick.
    /// </summary>
    /// <param name="axis">The axis.</param>
    /// <param name="controlSize">Size of the control.</param>
    /// <param name="bounds">The bounds.</param>
    /// <returns></returns>
    public static AxisTick GetTick(this ICartesianAxis axis, LvcSize controlSize, Bounds? bounds = null)
    {
        bounds ??= axis.VisibleDataBounds;
        var maxLabelSize = axis.PossibleMaxLabelSize;

        var w = maxLabelSize.Width;
        var h = maxLabelSize.Height;

        var r = Math.Abs(axis.LabelsRotation % 90);

        if (r is >= 20 and <= 70)
        {
            // if the labels are rotated, we assume that they can overlap.
            var d = 0.35f * (float)Math.Sqrt(w * w + h * h);
            w = d;
            h = d;
        }
        else
        {
            // modify the size of the label to avoid overlapping
            // and improve readability.

            var density = 1 + axis.LabelsDensity;

            if (axis.Orientation == AxisOrientation.X) w *= density / 2f;
            if (axis.Orientation == AxisOrientation.Y) h *= density / 2f;
        }

        const float MinLabelSize = 10; // Assume the label size is at least 10px

        if (w < MinLabelSize) w = MinLabelSize;
        if (h < MinLabelSize) h = MinLabelSize;

        var max = axis.MaxLimit is null ? bounds.Max : axis.MaxLimit.Value;
        var min = axis.MinLimit is null ? bounds.Min : axis.MinLimit.Value;

        AxisLimit.ValidateLimits(ref min, ref max, axis.MinStep);

        var unit = axis.UnitWidth;

        max /= unit;
        min /= unit;

        var range = max - min;

        // when the range is 0, we force the range to be 15% of the max value... just a default value.
        if (range == 0) range = 0.15 * max / unit;

        var separations = axis.Orientation == AxisOrientation.Y
            ? Math.Round(controlSize.Height / h, 0)
            : Math.Round(controlSize.Width / w, 0);

        var minimum = range / separations;

        var magnitude = Math.Pow(10, Math.Floor(Math.Log(minimum) / Math.Log(10)));

        var residual = minimum / magnitude;
        var tick = residual > 5 ? 10 * magnitude : residual > 2 ? 5 * magnitude : residual > 1 ? 2 * magnitude : magnitude;

        EnsureMinSeparators(axis.MinSeparators, axis.ForceStepToMin, range, ref tick, ref magnitude);

        return new AxisTick { Value = tick * unit, Magnitude = magnitude * unit };
    }

    /// <summary>
    /// Gets the tick.
    /// </summary>
    /// <param name="axis">The axis.</param>
    /// <param name="chart">The chart.</param>
    /// <param name="bounds">The bounds.</param>
    /// <returns></returns> 
    public static AxisTick GetTick(this IPolarAxis axis, PolarChartEngine chart, Bounds? bounds = null)
    {
        bounds ??= axis.VisibleDataBounds;

        var max = axis.MaxLimit is null ? bounds.Max : axis.MaxLimit.Value;
        var min = axis.MinLimit is null ? bounds.Min : axis.MinLimit.Value;

        var controlSize = chart.ControlSize;
        var minD = controlSize.Width < controlSize.Height ? controlSize.Width : controlSize.Height;
        var radius = minD - chart.InnerRadius;
        var c = minD * chart.TotalAnge / 360;

        var range = max - min;
        var separations = axis.Orientation == PolarAxisOrientation.Angle
            ? Math.Round(c / 30, 0)
            : Math.Round(radius / 90, 0);
        var minimum = range / separations;

        var magnitude = Math.Pow(10, Math.Floor(Math.Log(minimum) / Math.Log(10)));

        var residual = minimum / magnitude;
        var tick = residual > 5 ? 10 * magnitude : residual > 2 ? 5 * magnitude : residual > 1 ? 2 * magnitude : magnitude;

        EnsureMinSeparators(axis.MinSeparators, axis.ForceStepToMin, range, ref tick, ref magnitude);

        return new AxisTick { Value = tick, Magnitude = magnitude };
    }

    // Walks the nice-number ladder downward (10 → 5 → 2 → 1 → 0.5 → 0.2 → ...) until range/tick
    // reaches the requested minimum separator count. Skipped when the user pinned the step via
    // ForceStepToMin, when minSeparators is non-positive, or when range/tick can't be computed.
    private static void EnsureMinSeparators(int minSeparators, bool forceStepToMin, double range, ref double tick, ref double magnitude)
    {
        if (forceStepToMin || minSeparators <= 0 || tick <= 0 || magnitude <= 0 || range <= 0) return;

        var mul = tick / magnitude; // approximately 1, 2, 5, or 10 — fp noise can leave it just above/below
        var safety = 0;

        // each iteration at least doubles the separator count, so 32 covers any int MinSeparators with headroom.
        // thresholds are placed at the midpoints between buckets (7.5, 3.5, 1.5) so a noisy mul like 9.9999 still
        // snaps to "drop one tier" (5) instead of skipping straight to 2.
        while (range / (mul * magnitude) < minSeparators && safety++ < 32)
        {
            if (mul > 7.5) mul = 5;
            else if (mul > 3.5) mul = 2;
            else if (mul > 1.5) mul = 1;
            else
            {
                mul = 5;
                magnitude /= 10;
            }

            // also breaks on NaN since any comparison with NaN is false
            if (!(magnitude > 0)) break;
        }

        tick = mul * magnitude;
    }

    /// <summary>
    /// Sets the transition of the given <paramref name="properties"/> to the <paramref name="animation"/>.
    /// </summary>
    /// <param name="animatable">The animatable object.</param>
    /// <param name="animation">The animation.</param>
    /// <param name="properties">
    /// The properties, if this argument is not set then all the animatable properties in the object will use the given animation.
    /// </param>
    public static void Animate(this Animatable animatable, Animation animation, params PropertyDefinition[]? properties)
    {
        animatable.SetTransition(animation, properties);
        animatable.CompleteTransition(properties);
    }

    /// <summary>
    /// Sets the transition of the given <paramref name="properties"/> to the specified <paramref name="easingFunction"/> and <paramref name="speed"/>.
    /// </summary>
    /// <param name="animatable">The animatable object.</param>
    /// <param name="easingFunction">The animation's easing function.</param>
    /// <param name="speed">The animation's speed.</param>
    /// <param name="properties">
    /// The properties, if this argument is not set then all the animatable properties in the object will use the given animation.
    /// </param>
    public static void Animate(this Animatable animatable, Func<float, float>? easingFunction, TimeSpan speed, params PropertyDefinition[]? properties) =>
        Animate(animatable, new Animation(easingFunction, speed), properties);

    /// <summary>
    /// Points the given <paramref name="properties"/> at the chart's shared
    /// <see cref="Chart.Animation"/> instance. When <paramref name="properties"/> is null or
    /// empty, every animatable property on <paramref name="animatable"/> is bound to the same
    /// instance. Because that instance is mutated in-place when
    /// <see cref="Chart.ActualAnimationsSpeed"/> or <see cref="Chart.ActualEasingFunction"/>
    /// change, callers using this overload pick up chart-level animation changes without
    /// recreating their geometries.
    /// </summary>
    /// <param name="animatable">The animatable object.</param>
    /// <param name="chart">
    /// The chart whose <see cref="Chart.Animation"/> is referenced.
    /// </param>
    /// <param name="properties">
    /// The properties to bind; when null or empty, every animatable property on
    /// <paramref name="animatable"/> is bound to <see cref="Chart.Animation"/>.
    /// </param>
    public static void Animate(this Animatable animatable, Chart chart, params PropertyDefinition[]? properties) =>
        Animate(animatable, chart.Animation, properties);

    /// <summary>
    /// Sets the transition of the given <paramref name="properties"/> to the animations config in the chart
    /// for all the geometries in a <see cref="VisualElement"/>.
    /// </summary>
    /// <param name="visual">The visual.</param>
    /// <param name="animation">The animation.</param>
    /// <param name="properties">The properties.</param>
    public static void Animate(this VisualElement visual, Animation animation, params PropertyDefinition[]? properties)
    {
        foreach (var animatable in visual.GetDrawnGeometries())
        {
            if (animatable is null) continue;
            Animate(animatable, animation, properties);
        }
    }

    /// <summary>
    /// Determines whether is bar series.
    /// </summary>
    /// <param name="series">The series.</param>
    /// <returns>
    ///   <c>true</c> if [is bar series] [the specified series]; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsBarSeries(this ISeries series) => (series.SeriesProperties & SeriesProperties.Bar) != 0;

    /// <summary>
    /// Determines whether is column series.
    /// </summary>
    /// <param name="series">The series.</param>
    /// <returns>
    ///   <c>true</c> if [is column series] [the specified series]; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsColumnSeries(this ISeries series)
    {
        return (series.SeriesProperties & (SeriesProperties.Bar | SeriesProperties.PrimaryAxisVerticalOrientation)) ==
            (SeriesProperties.Bar | SeriesProperties.PrimaryAxisVerticalOrientation);
    }

    /// <summary>
    /// Determines whether is row series.
    /// </summary>
    /// <param name="series">The series.</param>
    /// <returns>
    ///   <c>true</c> if [is row series] [the specified series]; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsRowSeries(this ISeries series)
    {
        return (series.SeriesProperties & (SeriesProperties.Bar | SeriesProperties.PrimaryAxisHorizontalOrientation)) ==
            (SeriesProperties.Bar | SeriesProperties.PrimaryAxisHorizontalOrientation);
    }

    /// <summary>
    /// Determines whether is box series.
    /// </summary>
    /// <param name="series">The series.</param>
    /// <returns>
    ///   <c>true</c> if [is box series] [the specified series]; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsBoxSeries(this ISeries series) => (series.SeriesProperties & (SeriesProperties.BoxSeries)) == SeriesProperties.BoxSeries;

    /// <summary>
    /// Determines whether is stacked series.
    /// </summary>
    /// <param name="series">The series.</param>
    /// <returns>
    ///   <c>true</c> if [is stacked series] [the specified series]; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsStackedSeries(this ISeries series) => (series.SeriesProperties & (SeriesProperties.Stacked)) != 0;

    /// <summary>
    /// Determines whether is vertical series.
    /// </summary>
    /// <param name="series">The series.</param>
    /// <returns>
    ///   <c>true</c> if [is vertical series] [the specified series]; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsVerticalSeries(this ISeries series) => (series.SeriesProperties & (SeriesProperties.PrimaryAxisVerticalOrientation)) != 0;

    /// <summary>
    /// Determines whether is horizontal series.
    /// </summary>
    /// <param name="series">The series.</param>
    /// <returns>
    ///   <c>true</c> if [is horizontal series] [the specified series]; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsHorizontalSeries(this ISeries series) => (series.SeriesProperties & (SeriesProperties.PrimaryAxisHorizontalOrientation)) != 0;

    /// <summary>
    /// Determines whether is a financial series.
    /// </summary>
    /// <param name="series"></param>
    /// <returns></returns>
    public static bool IsFinancialSeries(this ISeries series) => (series.SeriesProperties & SeriesProperties.Financial) != 0;

    /// <summary>
    /// Determines whether is a pie series.
    /// </summary>
    /// <param name="series">The series.</param>
    public static bool IsPieSeries(this ISeries series) => (series.SeriesProperties & SeriesProperties.PieSeries) != 0;

    /// <summary>
    /// Determines whether is bar series.
    /// </summary>
    /// <param name="series">The series.</param>
    /// <returns>
    ///   <c>true</c> if [is bar series] [the specified series]; otherwise, <c>false</c>.
    /// </returns>
    public static bool HasVariableSvgGeometry(this ISeries series) =>
        (series.SeriesProperties & SeriesProperties.IsSVGPath) != 0;

    /// <summary>
    /// Calculates the finding strategy based on the series properties.
    /// </summary>
    /// <param name="seriesCollection">The series collection.</param>
    /// <returns></returns>
    public static FindingStrategy GetFindingStrategy(this IEnumerable<ISeries> seriesCollection)
    {
        var areAllX = true;
        var areAllY = true;

        foreach (var series in seriesCollection)
        {
            areAllX = areAllX && (series.SeriesProperties & SeriesProperties.PrefersXStrategyTooltips) != 0;
            areAllY = areAllY && (series.SeriesProperties & SeriesProperties.PrefersYStrategyTooltips) != 0;
        }

        return areAllX
            ? FindingStrategy.CompareOnlyXTakeClosest
            : (areAllY
                ? FindingStrategy.CompareOnlyYTakeClosest
                : FindingStrategy.CompareAllTakeClosest);
    }

    /// <summary>
    /// Finds the closest point to the specified location [in pixels].
    /// </summary>
    /// <param name="points">The points to look in to.</param>
    /// <param name="point">The location in pixels.</param>
    /// <returns></returns>
    public static ChartPoint<TModel, TVisual, TLabel>? FindClosestTo<TModel, TVisual, TLabel>(
        this IEnumerable<ChartPoint> points, LvcPoint point)
    {
        var closest = FindClosestTo(points, point);
        return closest is null
            ? null
            : new ChartPoint<TModel, TVisual, TLabel>(closest);
    }

    /// <summary>
    /// Finds the closest point to the specified location [in pixels].
    /// </summary>
    /// <param name="points">The points to look into.</param>
    /// <param name="point">The location in pixels.</param>
    /// <returns></returns>
    public static ChartPoint? FindClosestTo(this IEnumerable<ChartPoint> points, LvcPoint point)
    {
        var fp = new LvcPoint((float)point.X, (float)point.Y);

        return points
            .Select(p => new
            {
                distance = p.DistanceTo(fp, FindingStrategy.CompareAll),
                point = p
            })
            .OrderBy(p => p.distance)
            .FirstOrDefault()?.point;
    }

    /// <summary>
    /// Gets a scaler for the given axis with the measured bounds (the target, the final dimension of the chart).
    /// </summary>
    /// <param name="axis"></param>
    /// <param name="chart"></param>
    /// <returns></returns>
    public static Scaler GetNextScaler(this ICartesianAxis axis, CartesianChartEngine chart) =>
        new(chart.DrawMarginLocation, chart.DrawMarginSize, axis);

    /// <summary>
    /// Gets a scaler that is built based on the dimensions of the chart at a given time, the scaler is built based on the
    /// animations that are happening in the chart at the moment this method is called.
    /// </summary>
    /// <param name="axis"></param>
    /// <param name="chart"></param>
    /// <returns></returns>
    public static Scaler GetActualScaler(this ICartesianAxis axis, CartesianChartEngine chart)
    {
        // returns an scaler with the "Motion bounds", which means the bounds but tracked by the motion canvas
        // clock, so this returns the bounds that are being animated at the moment this method is called.
        // DrawMarginLocation and DrawMarginSize are the actual chart size, so they are not completely accurate
        // but this should not be a big issue, since this normally has no big changes during the animations,
        // if necessary, implement motion properties to track the DrawMarginLocation and DrawMarginSize.

        return new Scaler(
            chart.DrawMarginLocation,
            chart.DrawMarginSize,
            axis,
            new Bounds
            {
                Max = axis.MotionMaxLimit,
                Min = axis.MotionMinLimit
            });
    }

    /// <summary>
    /// Returns an enumeration with only the first element.
    /// </summary>
    /// <typeparam name="T">The source type.</typeparam>
    /// <typeparam name="T1">The target type.</typeparam>
    /// <param name="source">The source enumeration.</param>
    /// <param name="predicate">The mapping predicate.</param>
    /// <returns></returns>
    public static IEnumerable<T1> SelectFirst<T, T1>(this IEnumerable<T> source, Func<T, T1> predicate)
    {
        foreach (var item in source)
        {
            yield return predicate(item);
            yield break;
        }
    }

    /// <summary>
    /// Gets the point for the given view.
    /// </summary>
    /// <param name="dictionary">The points dictionary.</param>
    /// <param name="view">The view.</param>
    /// <returns></returns>
    public static ChartPoint? GetPointForView(this Dictionary<IChartView, ChartPoint> dictionary, IChartView view) => dictionary.TryGetValue(view, out var point) ? point : null;

    /// <summary>
    /// Splits an enumerable of chartpoints by each null gap.
    /// </summary>
    /// <param name="points">The points.</param>
    /// <param name="onDeleteNullPoint">Called when a point was deleted.</param>
    /// <returns></returns>
    public static IEnumerable<IEnumerable<ChartPoint>> SplitByNullGaps(
        this IEnumerable<ChartPoint> points,
        Action<ChartPoint> onDeleteNullPoint)
    {
        using var builder = new GapsBuilder(points.GetEnumerator());
        while (!builder.Finished) yield return YieldReturnUntilNextNullChartPoint(builder, onDeleteNullPoint);
    }

    /// <summary>
    /// Builds a enumerator with the necessary data to build an Spline.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    internal static IEnumerable<SplineData> AsSplineData(this IEnumerable<ChartPoint> source)
    {
        using var e = source.Where(x => !x.IsEmpty).GetEnumerator();

        if (!e.MoveNext()) yield break;
        var data = new SplineData(e.Current);

        if (!e.MoveNext())
        {
            data.IsNextEmpty = true;
            yield return data;
            yield break;
        }

        data.GoNext(e.Current);

        while (e.MoveNext())
        {
            yield return data;
            data.IsFirst = false;
            data.GoNext(e.Current);
        }

        data.IsFirst = false;
        yield return data;

        data.GoNext(data.Next);
        yield return data;
    }

    private static IEnumerable<ChartPoint> YieldReturnUntilNextNullChartPoint(
        GapsBuilder builder,
        Action<ChartPoint> onDeleteNullPoint)
    {
        while (builder.Enumerator.MoveNext())
        {
            if (builder.Enumerator.Current.IsEmpty)
            {
                var wasEmpty = builder.IsEmpty;
                builder.IsEmpty = true;
                onDeleteNullPoint(builder.Enumerator.Current);
                if (!wasEmpty) yield break; // if there are no points then do not return an empty enumerable...
            }
            else
            {
                yield return builder.Enumerator.Current;
                builder.IsEmpty = false;
            }
        }

        builder.Finished = true;
    }

    private class GapsBuilder(IEnumerator<ChartPoint> enumerator) : IDisposable
    {
        public IEnumerator<ChartPoint> Enumerator { get; } = enumerator;

        public bool IsEmpty { get; set; } = true;

        public bool Finished { get; set; } = false;

        public void Dispose() => Enumerator.Dispose();
    }

    internal class SplineData(ChartPoint start)
    {
        public ChartPoint Previous { get; set; } = start;

        public ChartPoint Current { get; set; } = start;

        public ChartPoint Next { get; set; } = start;

        public ChartPoint AfterNext { get; set; } = start;

        public bool IsFirst { get; set; } = true;
        public bool IsNextEmpty { get; set; }

        public void GoNext(ChartPoint point)
        {
            Previous = Current;
            Current = Next;
            Next = AfterNext;
            AfterNext = point;
        }
    }
}
