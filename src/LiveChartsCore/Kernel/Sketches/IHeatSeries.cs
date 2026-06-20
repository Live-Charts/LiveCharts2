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

namespace LiveChartsCore.Kernel.Sketches;

/// <summary>
/// Defines a heat series plot. Inherits the heat-legend metadata contract
/// (<see cref="IHeatLegendSource"/>) and adds the cartesian-side setters.
/// </summary>
/// <seealso cref="ISeries" />
public interface IHeatSeries : ICartesianSeries, IHeatLegendSource
{
    /// <summary>
    /// Gets or sets the heat map.
    /// </summary>
    new LvcColor[] HeatMap { get; set; }

    /// <summary>
    /// Gets or sets the color stops.
    /// </summary>
    new double[]? ColorStops { get; set; }

    /// <summary>
    /// Gets or sets the padding for each point.
    /// </summary>
    Padding PointPadding { get; set; }

    /// <summary>
    /// Gets or sets the minimum value in the weight axis,
    /// this value will be used to calculate the color of the point.
    /// Default is null and means the minimum value in the series.
    /// </summary>
    new double? MinValue { get; set; }

    /// <summary>
    /// Gets or sets the maximum value in the weight axis,
    /// this value will be used to calculate the color of the point.
    /// Default is null and means the maximum value in the series.
    /// </summary>
    new double? MaxValue { get; set; }
}
