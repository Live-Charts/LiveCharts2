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

using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;

namespace LiveChartsCore;

/// <summary>
/// Computes the per-series bar-position primitives (unit-width, half-unit-width,
/// center-position, pivot pixel) for a bar-shaped series in a single frame.
/// Promoted from a nested type on <see cref="BarSeries{TModel, TVisual, TLabel}"/>
/// to a top-level type so the template-method orchestration in
/// <see cref="BarSeries{TModel, TVisual, TLabel}.Invalidate"/> can pass it via
/// <see cref="BarMeasureContext"/> to per-point hooks without bumping into
/// nested-generic visibility.
/// </summary>
public sealed class BarMeasureHelper
{
    /// <summary>
    /// Initializes a new instance of <see cref="BarMeasureHelper"/>.
    /// </summary>
    /// <param name="scaler">The scaler for the axis carrying bar position.</param>
    /// <param name="cartesianChart">The chart.</param>
    /// <param name="barSeries">The series.</param>
    /// <param name="axis">The axis the helper measures against.</param>
    /// <param name="p">The pivot pixel.</param>
    /// <param name="minP">The minimum allowed pivot pixel (drawn-margin lower edge).</param>
    /// <param name="maxP">The maximum allowed pivot pixel (drawn-margin upper edge).</param>
    /// <param name="isStacked">Whether the series is stacked.</param>
    /// <param name="isRow">Whether the series is horizontal (row-shaped).</param>
    public BarMeasureHelper(
        Scaler scaler,
        CartesianChartEngine cartesianChart,
        IBarSeries barSeries,
        ICartesianAxis axis,
        float p,
        float minP,
        float maxP,
        bool isStacked,
        bool isRow)
    {
        this.p = p;
        if (p < minP) this.p = minP;
        if (p > maxP) this.p = maxP;

        uw = scaler.MeasureInPixels(axis.UnitWidth);
        actualUw = uw;

        var gp = (float)barSeries.Padding;

        if (uw - gp < 1) gp -= uw - gp;

        uw -= gp;
        uwm = 0.5f * uw;

        int pos, count;

        if (isStacked)
        {
            pos = isRow
                ? cartesianChart.SeriesContext.GetStackedRowPostion(barSeries)
                : cartesianChart.SeriesContext.GetStackedColumnPostion(barSeries);
            count = isRow
                ? cartesianChart.SeriesContext.GetStackedRowSeriesCount()
                : cartesianChart.SeriesContext.GetStackedColumnSeriesCount();
        }
        else
        {
            pos = isRow
                ? cartesianChart.SeriesContext.GetRowPosition(barSeries)
                : cartesianChart.SeriesContext.GetColumnPostion(barSeries);
            count = isRow
                ? cartesianChart.SeriesContext.GetRowSeriesCount()
                : cartesianChart.SeriesContext.GetColumnSeriesCount();
        }

        cp = 0f;

        var padding = (float)barSeries.Padding;
        if (barSeries.IgnoresBarPosition) count = 1;

        uw /= count;
        var mw = (float)barSeries.MaxBarWidth;
        if (uw > mw) uw = mw;
        uwm = 0.5f * uw;
        cp = barSeries.IgnoresBarPosition
            ? 0
            : (pos - count / 2f) * uw + uwm;

        // apply the padding
        uw -= padding;
        cp += padding * 0.5f;

        if (uw < 1)
        {
            uw = 1;
            uwm = 0.5f;
        }
    }

    /// <summary>Effective bar unit-width (after padding + position).</summary>
    public float uw;
    /// <summary>Half the effective bar width.</summary>
    public float uwm;
    /// <summary>Center position offset for this series within the category.</summary>
    public float cp;
    /// <summary>Pivot pixel position (clamped to draw margin).</summary>
    public float p;
    /// <summary>Raw axis unit-width (before padding), used for category-wide hover rects.</summary>
    public float actualUw;
}
