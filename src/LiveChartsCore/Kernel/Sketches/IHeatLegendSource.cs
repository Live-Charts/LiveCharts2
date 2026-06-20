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

using LiveChartsCore.Drawing;
using LiveChartsCore.Measure;

namespace LiveChartsCore.Kernel.Sketches;

/// <summary>
/// Exposes the metadata a heat-gradient legend needs to render, decoupled from
/// any specific series surface. Cartesian <see cref="IHeatSeries"/> and the
/// geographic heat-land series both implement it so the same legend renderer
/// works against either chart family.
/// </summary>
public interface IHeatLegendSource
{
    /// <summary>
    /// Gets the heat map color stops, from low to high.
    /// </summary>
    LvcColor[] HeatMap { get; }

    /// <summary>
    /// Gets the optional color stop positions in [0, 1]. When null, the stops
    /// are evenly distributed across <see cref="HeatMap"/>.
    /// </summary>
    double[]? ColorStops { get; }

    /// <summary>
    /// Gets the data weight bounds (min/max) that the gradient maps over.
    /// </summary>
    Bounds WeightBounds { get; }

    /// <summary>
    /// Gets the optional minimum-value override used for color mapping. When
    /// null, <see cref="WeightBounds"/>.Min is used.
    /// </summary>
    double? MinValue { get; }

    /// <summary>
    /// Gets the optional maximum-value override used for color mapping. When
    /// null, <see cref="WeightBounds"/>.Max is used.
    /// </summary>
    double? MaxValue { get; }

    /// <summary>
    /// Gets a value indicating whether this source should contribute to the
    /// chart's legend.
    /// </summary>
    bool IsVisibleAtLegend { get; }
}
