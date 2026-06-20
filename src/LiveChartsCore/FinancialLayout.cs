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
/// Final per-frame geometry of a single candle, returned from
/// <c>MeasureFinancialLayout</c>. The candle body spans (X, Y=high) →
/// (X+Width, Low); Open / Close are intermediate pixel-Y values bounded
/// by High (smallest) and Low (largest) since the Y axis grows downward.
/// </summary>
public readonly struct FinancialLayout
{
    /// <summary>Initializes a new instance of <see cref="FinancialLayout"/>.</summary>
    /// <param name="x">Body left edge (secondary - half-width).</param>
    /// <param name="y">High pixel-Y (top of the candle).</param>
    /// <param name="width">Candle body width.</param>
    /// <param name="open">Open pixel-Y.</param>
    /// <param name="close">Close pixel-Y.</param>
    /// <param name="low">Low pixel-Y (bottom of the candle).</param>
    /// <param name="isBullish">
    /// True when the candle is bullish (close value &gt; open value). In pixel-Y
    /// space — where lower Y means higher value — this is <c>open > close</c>.
    /// Drives which paint pair (Up* vs Down*) the visual attaches to.
    /// </param>
    /// <param name="categoryHoverX">Category-wide hover rect X (centered on the category, ignores MaxBarWidth clamp).</param>
    /// <param name="categoryHoverWidth">Category-wide hover rect width (axis unit-width, not clamped to MaxBarWidth).</param>
    public FinancialLayout(
        float x, float y, float width,
        float open, float close, float low, bool isBullish,
        float categoryHoverX, float categoryHoverWidth)
    {
        X = x;
        Y = y;
        Width = width;
        Open = open;
        Close = close;
        Low = low;
        IsBullish = isBullish;
        CategoryHoverX = categoryHoverX;
        CategoryHoverWidth = categoryHoverWidth;
    }

    /// <summary>Body left edge.</summary>
    public float X { get; }
    /// <summary>High pixel-Y (top of the candle's vertical extent).</summary>
    public float Y { get; }
    /// <summary>Candle body width.</summary>
    public float Width { get; }
    /// <summary>Open pixel-Y.</summary>
    public float Open { get; }
    /// <summary>Close pixel-Y.</summary>
    public float Close { get; }
    /// <summary>Low pixel-Y (bottom of the candle's vertical extent).</summary>
    public float Low { get; }
    /// <summary>True when the close value is greater than the open value.</summary>
    public bool IsBullish { get; }
    /// <summary>Category-wide hover-area top-left X (centered on the category, ignores MaxBarWidth clamp).</summary>
    public float CategoryHoverX { get; }
    /// <summary>Category-wide hover-area width (axis unit-width, used when FindingStrategy != ExactMatch).</summary>
    public float CategoryHoverWidth { get; }

    /// <summary>Total vertical extent in pixels (Low - Y, since Y axis grows down).</summary>
    public float Height => Low - Y;
}
