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

using LiveChartsCore.Measure;
using LiveChartsCore.Painting;

namespace LiveChartsCore.Kernel.Sketches;

/// <summary>
/// Defines a stepline series plot.
/// </summary>
/// <seealso cref="ISeries" />
public interface IStepLineSeries : ICartesianSeries, IStrokedAndFilled
{
    /// <summary>
    /// Gets or sets a value indicating whether the line should split every null point, 
    /// enabling it has a performance impact, default is true.
    /// </summary>
    bool EnableNullSplitting { get; set; }

    /// <summary>
    /// Gets or sets the size of the geometry.
    /// </summary>
    /// <value>
    /// The size of the geometry.
    /// </value>
    double GeometrySize { get; set; }

    /// <summary>
    /// Gets or sets when the geometries (the markers on the points) are drawn, default is
    /// <see cref="GeometryVisibility.Auto"/>: they are hidden while the points are so close that they would overlap,
    /// and the point under the pointer always shows its geometry.
    /// </summary>
    GeometryVisibility GeometryVisibility { get; set; }

    /// <summary>
    /// Gets or sets the geometry fill.
    /// </summary>
    /// <value>
    /// The shapes fill.
    /// </value>
    Paint? GeometryFill { get; set; }

    /// <summary>
    /// Gets or sets the geometry stroke.
    /// </summary>
    /// <value>
    /// The shapes stroke.
    /// </value>
    Paint? GeometryStroke { get; set; }
}
