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

namespace LiveChartsCore.Kernel;

// ToDo: This should be a struct.
/// <summary>
/// Defines the a stacked value.
/// </summary>
public class StackedValue
{
    /// <summary>
    /// Gets or sets the start.
    /// </summary>
    /// <value>
    /// The start.
    /// </value>
    public double Start { get; set; }

    /// <summary>
    /// Gets or sets the end.
    /// </summary>
    /// <value>
    /// The end.
    /// </value>
    public double End { get; set; }

    /// <summary>
    /// Gets or sets the total stacked.
    /// </summary>
    /// <value>
    /// The total.
    /// </value>
    public double Total { get; set; }

    /// <summary>
    /// Gets or sets the start.
    /// </summary>
    /// <value>
    /// The start.
    /// </value>
    public double NegativeStart { get; set; }

    /// <summary>
    /// Gets or sets the end.
    /// </summary>
    /// <value>
    /// The end.
    /// </value>
    public double NegativeEnd { get; set; }

    /// <summary>
    /// Gets or sets the total stacked.
    /// </summary>
    /// <value>
    /// The total.
    /// </value>
    public double NegativeTotal { get; set; }

    /// <summary>
    /// Gets or sets the cumulative start. Unlike <see cref="Start"/> and <see cref="NegativeStart"/>,
    /// this value accumulates every point regardless of its sign and is the offset stacked
    /// area/line series use to keep a continuous baseline across mixed-sign values.
    /// </summary>
    public double CumulativeStart { get; set; }

    /// <summary>
    /// Gets or sets the cumulative end. See <see cref="CumulativeStart"/>.
    /// </summary>
    public double CumulativeEnd { get; set; }

    /// <summary>
    /// Gets the share in the total stack.
    /// </summary>
    public double Share => IsNegative
        ? (NegativeEnd - NegativeStart) / NegativeTotal
        : (End - Start) / Total;

    internal bool IsNegative { get; set; }
}
