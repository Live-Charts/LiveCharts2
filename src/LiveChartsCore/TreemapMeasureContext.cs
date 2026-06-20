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
/// orchestration on <see cref="CoreTreemapSeries{TModel, TVisual, TLabel}"/>
/// and the per-node hooks. Treemap has no axes — the only inputs are the
/// draw-margin rectangle and per-series cosmetic settings.
/// </summary>
public readonly ref struct TreemapMeasureContext
{
    /// <summary>Initializes a new instance of <see cref="TreemapMeasureContext"/>.</summary>
    public TreemapMeasureContext(
        TreemapChartEngine chart,
        LvcPoint drawLocation,
        LvcSize drawMarginSize,
        float padding,
        float cornerRadius,
        bool isFirstDraw)
    {
        Chart = chart;
        DrawLocation = drawLocation;
        DrawMarginSize = drawMarginSize;
        Padding = padding;
        CornerRadius = cornerRadius;
        IsFirstDraw = isFirstDraw;
    }

    /// <summary>The treemap chart engine.</summary>
    public TreemapChartEngine Chart { get; }
    /// <summary>Top-left of the draw margin region.</summary>
    public LvcPoint DrawLocation { get; }
    /// <summary>Size of the draw margin region.</summary>
    public LvcSize DrawMarginSize { get; }
    /// <summary>Padding (px) inset on every node before laying out its children.</summary>
    public float Padding { get; }
    /// <summary>Corner radius (px) applied to rounded-rectangle visuals.</summary>
    public float CornerRadius { get; }
    /// <summary>True if this is the first draw of the series.</summary>
    public bool IsFirstDraw { get; }
}
