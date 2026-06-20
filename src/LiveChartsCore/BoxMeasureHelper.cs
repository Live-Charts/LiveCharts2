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
/// Computes the per-series box-position primitives (unit-width, half-unit-width,
/// center-position, pivot pixel) for a single frame. Promoted from a nested type
/// on <see cref="CoreBoxSeries{TModel, TVisual, TLabel, TMiniatureGeometry}"/> so
/// the template-method <see cref="BoxMeasureContext"/> can pass it via <c>in</c>
/// to per-point hooks.
/// </summary>
public sealed class BoxMeasureHelper
{
    /// <summary>Initializes a new instance of <see cref="BoxMeasureHelper"/>.</summary>
    public BoxMeasureHelper(
        Scaler scaler,
        CartesianChartEngine cartesianChart,
        IBoxSeries boxSeries,
        ICartesianAxis axis,
        float p,
        float minP,
        float maxP)
    {
        this.p = p;
        if (p < minP) this.p = minP;
        if (p > maxP) this.p = maxP;

        uw = scaler.MeasureInPixels(axis.UnitWidth);
        actualUw = uw;

        var gp = (float)boxSeries.Padding;

        if (uw - gp < 1) gp -= uw - gp;

        uw -= gp;
        uwm = 0.5f * uw;

        var pos = cartesianChart.SeriesContext.GetBoxPosition(boxSeries);
        var count = cartesianChart.SeriesContext.GetBoxSeriesCount();

        cp = 0f;

        var padding = (float)boxSeries.Padding;

        uw /= count;
        var mw = (float)boxSeries.MaxBarWidth;
        if (uw > mw) uw = mw;
        uwm = 0.5f * uw;
        cp = (pos - count / 2f) * uw + uwm;

        // apply the padding
        uw -= padding;
        cp += padding * 0.5f;

        if (uw < 1)
        {
            uw = 1;
            uwm = 0.5f;
        }
    }

    /// <summary>Effective box body width (after padding + position).</summary>
    public float uw;
    /// <summary>Half the effective box width.</summary>
    public float uwm;
    /// <summary>Center position offset for this series within the category.</summary>
    public float cp;
    /// <summary>Pivot pixel position (clamped to draw margin).</summary>
    public float p;
    /// <summary>Raw axis unit-width (before padding), used for category-wide hover rects.</summary>
    public float actualUw;
}
