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

namespace LiveChartsCore;

/// <summary>
/// Bundle of per-Invalidate state shared between the template-method
/// orchestration on <see cref="CoreSankeySeries{TNode, TVisual, TLabel}"/>
/// and the per-element hooks. Captures the draw region plus the cosmetic
/// parameters fed to the d3-sankey layout pass.
/// </summary>
public readonly ref struct SankeyMeasureContext
{
    /// <summary>Initializes a new instance of <see cref="SankeyMeasureContext"/>.</summary>
    public SankeyMeasureContext(
        SankeyChartEngine chart,
        LvcPoint drawLocation,
        LvcSize drawMarginSize,
        float nodeWidth,
        float nodePadding,
        int layoutIterations,
        bool isFirstDraw)
    {
        Chart = chart;
        DrawLocation = drawLocation;
        DrawMarginSize = drawMarginSize;
        NodeWidth = nodeWidth;
        NodePadding = nodePadding;
        LayoutIterations = layoutIterations;
        IsFirstDraw = isFirstDraw;
    }

    /// <summary>The sankey chart engine.</summary>
    public SankeyChartEngine Chart { get; }
    /// <summary>Top-left of the draw margin region.</summary>
    public LvcPoint DrawLocation { get; }
    /// <summary>Size of the draw margin region.</summary>
    public LvcSize DrawMarginSize { get; }
    /// <summary>Per-column node rectangle width (px).</summary>
    public float NodeWidth { get; }
    /// <summary>Vertical gap (px) between sibling nodes within a column.</summary>
    public float NodePadding { get; }
    /// <summary>Sweep-relaxation iteration count.</summary>
    public int LayoutIterations { get; }
    /// <summary>True if this is the first draw of the series.</summary>
    public bool IsFirstDraw { get; }
}
