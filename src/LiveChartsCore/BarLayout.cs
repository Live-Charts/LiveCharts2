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

namespace LiveChartsCore;

/// <summary>
/// The final per-frame geometry of a single bar, returned from
/// <c>MeasureBarLayout</c>. Carries enough information for the orchestration
/// template in <see cref="BarSeries{TModel, TVisual, TLabel}.Invalidate"/> to
/// position the visual, configure the hover area and place the data label
/// without any further orientation knowledge.
/// </summary>
public readonly struct BarLayout
{
    /// <summary>
    /// Initializes a new instance of <see cref="BarLayout"/>.
    /// </summary>
    /// <param name="x">Visual rect X.</param>
    /// <param name="y">Visual rect Y.</param>
    /// <param name="width">Visual rect width.</param>
    /// <param name="height">Visual rect height.</param>
    /// <param name="categoryHoverX">Category-wide hover rect X (for non-ExactMatch finding).</param>
    /// <param name="categoryHoverY">Category-wide hover rect Y.</param>
    /// <param name="categoryHoverWidth">Category-wide hover rect width.</param>
    /// <param name="categoryHoverHeight">Category-wide hover rect height.</param>
    public BarLayout(
        float x, float y, float width, float height,
        float categoryHoverX, float categoryHoverY,
        float categoryHoverWidth, float categoryHoverHeight)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        CategoryHoverX = categoryHoverX;
        CategoryHoverY = categoryHoverY;
        CategoryHoverWidth = categoryHoverWidth;
        CategoryHoverHeight = categoryHoverHeight;
    }

    /// <summary>Visual rect X.</summary>
    public float X { get; }
    /// <summary>Visual rect Y.</summary>
    public float Y { get; }
    /// <summary>Visual rect width.</summary>
    public float Width { get; }
    /// <summary>Visual rect height.</summary>
    public float Height { get; }
    /// <summary>Category-wide hover rect X (used when FindingStrategy != ExactMatch).</summary>
    public float CategoryHoverX { get; }
    /// <summary>Category-wide hover rect Y.</summary>
    public float CategoryHoverY { get; }
    /// <summary>Category-wide hover rect width.</summary>
    public float CategoryHoverWidth { get; }
    /// <summary>Category-wide hover rect height.</summary>
    public float CategoryHoverHeight { get; }
}
