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
using LiveChartsCore.Painting;

namespace LiveChartsCore.Kernel.Sketches;

/// <summary>
/// Common (non-typed) surface of a sankey series. Graph topology is provided
/// via series-level mappers exposed on the typed
/// <c>CoreSankeySeries&lt;TModel, TVisual, TLabel&gt;</c>.
/// </summary>
public interface ISankeySeries : ISeries, IStrokedAndFilled
{
    /// <summary>Width (px) reserved for each node's column rectangle. Default 12.</summary>
    double NodeWidth { get; set; }

    /// <summary>Vertical gap (px) between sibling nodes within the same column. Default 8.</summary>
    double NodePadding { get; set; }

    /// <summary>
    /// Corner radius (px) applied to node rectangles when the visual is a
    /// rounded-rectangle geometry. Default 0 — matches the typical d3-sankey
    /// sharp-corner aesthetic. Ignored when the user picks a custom TVisual
    /// that doesn't derive from <c>BaseRoundedRectangleGeometry</c>.
    /// </summary>
    double NodeCornerRadius { get; set; }

    /// <summary>
    /// Number of relaxation iterations used by the d3-sankey layout to
    /// minimize ribbon crossings. Higher = cleaner but slower. Default 32.
    /// </summary>
    int LayoutIterations { get; set; }

    /// <summary>Where node labels are placed relative to the node rectangle.</summary>
    SankeyLabelPosition NodeLabelPosition { get; set; }

    /// <summary>
    /// How the graph is laid out on the canvas. Default
    /// <see cref="SankeyLayoutKind.Vertical"/> — the classic d3-sankey L→R
    /// column layout. Other modes (e.g. <see cref="SankeyLayoutKind.BipartiteArc"/>)
    /// have their own constraints; see the enum members for details.
    /// </summary>
    SankeyLayoutKind Layout { get; set; }

    /// <summary>
    /// Angular span (degrees) covered by each arc when
    /// <see cref="Layout"/> = <see cref="SankeyLayoutKind.BipartiteArc"/>.
    /// Default 150°: each arc covers 150° leaving a 30° gap at the top and
    /// bottom poles of the ellipse. Ignored in other layout modes.
    /// </summary>
    double ArcSpanDegrees { get; set; }

    /// <summary>
    /// Fill paint applied to flow ribbons. Typically a translucent version
    /// of <see cref="IStrokedAndFilled.Fill"/>; when null the ribbons
    /// inherit <see cref="IStrokedAndFilled.Fill"/> directly.
    /// </summary>
    Paint? LinkFill { get; set; }

    /// <summary>
    /// Builds the tooltip text for a node. Receives the hovered node's
    /// <see cref="ChartPoint"/> (Context.DataSource is the user's node model,
    /// Coordinate.PrimaryValue is the node's resolved value = max(in-sum,
    /// out-sum)). When null the default formatter renders
    /// "&lt;label&gt;: &lt;value&gt;".
    /// </summary>
    Func<ChartPoint, string>? TooltipLabelFormatter { get; set; }
}

/// <summary>Placement of node labels relative to the node rectangle.</summary>
public enum SankeyLabelPosition
{
    /// <summary>
    /// Source nodes (no incoming links) get a label outside-left, sink nodes
    /// (no outgoing links) get a label outside-right, pass-through nodes
    /// (both incoming + outgoing) get a label overlaid on the node — the
    /// d3-sankey "reads naturally" convention. Default.
    /// </summary>
    Auto,
    /// <summary>Always outside the node; side flips per node so the label points away from chart center.</summary>
    Outside,
    /// <summary>Always overlaid on the node, centered.</summary>
    Inside
}

/// <summary>How a sankey series arranges its nodes on the canvas.</summary>
public enum SankeyLayoutKind
{
    /// <summary>
    /// Classic L→R columns of stacked rectangles. Supports any number of
    /// columns (depths). Default.
    /// </summary>
    Vertical,

    /// <summary>
    /// Source nodes on a left arc, target nodes on a right arc; ribbons curve
    /// through the center. Requires bipartite data (every node is either pure
    /// source or pure sink — exactly two depth columns); throws
    /// <see cref="System.InvalidOperationException"/> on multi-column data.
    /// </summary>
    BipartiteArc,
}
