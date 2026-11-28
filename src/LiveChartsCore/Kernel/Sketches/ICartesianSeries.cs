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
using LiveChartsCore.Drawing;
using LiveChartsCore.Measure;

namespace LiveChartsCore.Kernel.Sketches;

/// <summary>
/// Defines a Cartesian series.
/// </summary>
/// <seealso cref="ISeries" />
public interface ICartesianSeries : ISeries
{
    /// <summary>
    /// Gets or sets the axis index where the series is scaled in the X plane, the index must exist 
    /// in the <see cref="ICartesianChartView.XAxes"/> collection.
    /// </summary>
    /// <value>
    /// The index of the axis.
    /// </value>
    int ScalesXAt { get; set; }

    /// <summary>
    /// Gets or sets the axis index where the series is scaled in the Y plane, the index must exist 
    /// in the <see cref="ICartesianChartView.YAxes"/> collection.
    /// </summary>
    /// <value>
    /// The index of the axis.
    /// </value>
    int ScalesYAt { get; set; }

    /// <summary>
    /// Gets or sets the data labels position.
    /// </summary>
    /// <value>
    /// The data labels position.
    /// </value>
    DataLabelsPosition DataLabelsPosition { get; set; }

    /// <summary>
    /// Gets or sets the data labels translate transform, the property is of type <see cref="LvcPoint"/>,
    /// where the <see cref="LvcPoint.X"/> property is in normalized units (from 0 to 1), where 1 is the width of the label and
    /// the <see cref="LvcPoint.Y"/> property is also in normalized units (from 0 to 1), where 1 is the height of the label.
    /// </summary>
    /// <value>
    /// The data labels trasnlate.
    /// </value>
    LvcPoint? DataLabelsTranslate { get; set; }

    /// <summary>
    /// Gets or sets the clipping mode, clipping restricts the series and labels for being drawn outside of the chart area (DrawMargin),
    /// default is <see cref="ClipMode.XY"/>, and means that anything outside the chart bounds will be ignored.
    /// </summary>
    ClipMode ClippingMode { get; set; }

    /// <summary>
    /// Gets or sets the tooltip label formatter in the X axis, the function receives a <see cref="ChartPoint"/> instance and must return a string.
    /// </summary>
    Func<ChartPoint, string>? XToolTipLabelFormatter { get; set; }

    /// <summary>
    /// Gets or sets the tooltip label formatter in the Y axis, the function receives a <see cref="ChartPoint"/> instance and must return a string.
    /// </summary>
    Func<ChartPoint, string>? YToolTipLabelFormatter { get; set; }

    /// <summary>
    /// Gets the series bounds.
    /// </summary>
    /// <param name="chart">The chart.</param>
    /// <param name="x">The axis the series scales on in the X plane.</param>
    /// <param name="y">The axis the series scales on in the Y plane.</param>
    /// <returns>the series bounds</returns>
    SeriesBounds GetBounds(Chart chart, ICartesianAxis x, ICartesianAxis y);
}
