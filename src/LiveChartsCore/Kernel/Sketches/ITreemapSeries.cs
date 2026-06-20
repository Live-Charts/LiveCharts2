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

namespace LiveChartsCore.Kernel.Sketches;

/// <summary>
/// Common (non-typed) surface of a treemap series. The hierarchical data
/// model is provided via series-level mappers exposed on the typed
/// <c>CoreTreemapSeries&lt;TModel, TVisual, TLabel&gt;</c>.
/// </summary>
public interface ITreemapSeries : ISeries, IStrokedAndFilled
{
    /// <summary>
    /// Gets or sets the gap (in pixels) inset on every node before its
    /// children are laid out. Default is 2.
    /// </summary>
    double Padding { get; set; }

    /// <summary>
    /// Gets or sets the corner radius (in pixels) for tiles that use a rounded
    /// rectangle visual. Default is 0.
    /// </summary>
    double CornerRadius { get; set; }

    /// <summary>
    /// The sub-rectangle of the chart draw margin that this series owns. Set
    /// by <see cref="TreemapChartEngine"/> on every measure pass — the engine
    /// squarifies the draw margin between visible series by their totals.
    /// </summary>
    LvcRectangle AssignedRectangle { get; set; }

    /// <summary>
    /// Returns the sum of root weights for this series (rolling internal nodes
    /// up via the children mapper). Used by the engine to partition the draw
    /// margin between multiple series.
    /// </summary>
    double GetTotalWeight();

    /// <summary>
    /// Builds the tooltip text for a tile. Receives the leaf's <see cref="ChartPoint"/>
    /// (Context.DataSource is the user's node model, Coordinate.PrimaryValue is the
    /// resolved weight). When null the default formatter renders "&lt;label&gt;: &lt;value&gt;".
    /// </summary>
    Func<ChartPoint, string>? TooltipLabelFormatter { get; set; }
}
