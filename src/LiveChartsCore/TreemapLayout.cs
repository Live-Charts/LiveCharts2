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

namespace LiveChartsCore;

/// <summary>
/// Squarified treemap layout (Bruls / Huijing / van Wijk, 2000). Used by
/// <see cref="CoreTreemapSeries{TModel, TVisual, TLabel}"/> for per-series
/// node layout and by <see cref="TreemapChartEngine"/> for partitioning the
/// draw margin between multiple series.
/// </summary>
internal static class TreemapLayout
{
    /// <summary>
    /// Partitions <paramref name="rect"/> into sub-rectangles whose areas are
    /// proportional to the items' weights, picking per-row layouts that keep
    /// each rectangle's aspect ratio as close to 1 as possible. Items with
    /// non-positive (or non-finite) weights are skipped. <paramref name="items"/>
    /// is mutated (sorted in place). Returned in descending-weight order.
    /// </summary>
    public static List<(T item, LvcRectangle rect)> Squarify<T>(
        List<(double weight, T item)> items,
        LvcRectangle rect)
    {
        var placements = new List<(T, LvcRectangle)>(items.Count);
        if (items.Count == 0 || rect.Size.Width <= 0 || rect.Size.Height <= 0)
            return placements;

        items.RemoveAll(p => p.weight <= 0 || double.IsNaN(p.weight) || double.IsInfinity(p.weight));
        if (items.Count == 0) return placements;

        // Descending by weight — required for squarify to converge to
        // near-square aspect ratios.
        items.Sort((a, b) => b.weight.CompareTo(a.weight));

        var total = 0.0;
        foreach (var p in items) total += p.weight;
        if (total <= 0) return placements;

        var scale = rect.Size.Width * rect.Size.Height / total;
        var areas = new double[items.Count];
        for (var i = 0; i < items.Count; i++) areas[i] = items[i].weight * scale;

        var idx = 0;
        var current = rect;

        while (idx < items.Count)
        {
            var rowStart = idx;
            var rowSum = 0.0;
            var rowMin = double.MaxValue;
            var rowMax = 0.0;
            var worst = double.MaxValue;
            var shorter = Math.Min(current.Size.Width, current.Size.Height);

            // Greedy row-build: add items while the worst aspect ratio
            // doesn't get worse.
            while (idx < items.Count)
            {
                var a = areas[idx];
                var newSum = rowSum + a;
                var newMin = a < rowMin ? a : rowMin;
                var newMax = a > rowMax ? a : rowMax;
                var newWorst = WorstRatio(newMax, newMin, shorter, newSum);

                if (idx == rowStart || newWorst <= worst)
                {
                    rowSum = newSum;
                    rowMin = newMin;
                    rowMax = newMax;
                    worst = newWorst;
                    idx++;
                }
                else
                {
                    break;
                }
            }

            // Lay out the closed row along the shorter edge.
            var depth = rowSum / shorter;
            var isHorizontalRow = current.Size.Width <= current.Size.Height;
            // isHorizontalRow == true: width is the shorter side. The row spans
            // the full width and grows downward.
            // isHorizontalRow == false: height is the shorter side. The row
            // spans the full height and grows rightward.

            double cursorX = current.Location.X;
            double cursorY = current.Location.Y;

            for (var k = rowStart; k < idx; k++)
            {
                var a = areas[k];
                var extent = a / depth;

                LvcRectangle childRect;
                if (isHorizontalRow)
                {
                    childRect = new LvcRectangle(
                        new LvcPoint((float)cursorX, (float)cursorY),
                        new LvcSize((float)extent, (float)depth));
                    cursorX += extent;
                }
                else
                {
                    childRect = new LvcRectangle(
                        new LvcPoint((float)cursorX, (float)cursorY),
                        new LvcSize((float)depth, (float)extent));
                    cursorY += extent;
                }

                placements.Add((items[k].item, childRect));
            }

            // Shrink the remaining rectangle by the row's depth.
            current = isHorizontalRow
                ? new LvcRectangle(
                    new LvcPoint(current.Location.X, (float)(current.Location.Y + depth)),
                    new LvcSize(current.Size.Width, (float)(current.Size.Height - depth)))
                : new LvcRectangle(
                    new LvcPoint((float)(current.Location.X + depth), current.Location.Y),
                    new LvcSize((float)(current.Size.Width - depth), current.Size.Height));
        }

        return placements;
    }

    private static double WorstRatio(double maxArea, double minArea, double shorter, double rowAreaSum)
    {
        var s2 = shorter * shorter;
        var sum2 = rowAreaSum * rowAreaSum;
        // For a row spanning `shorter`, each tile is (area*shorter/sum, sum/shorter).
        // Biggest tile's longer/shorter is area*shorter^2/sum^2; smallest tile's
        // longer/shorter is sum^2/(area*shorter^2). Max of the two is the row's worst.
        var r1 = maxArea * s2 / sum2;
        var r2 = sum2 / (minArea * s2);
        return r1 > r2 ? r1 : r2;
    }
}
