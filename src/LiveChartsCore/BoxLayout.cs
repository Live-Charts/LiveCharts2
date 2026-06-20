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
/// Final per-frame geometry of a single box-and-whisker visual, returned from
/// <c>MeasureBoxLayout</c>. The body spans (X, Y=max) → (X+Width, Min), with
/// Third / Median / First as intermediate pixel-Y values.
/// </summary>
public readonly struct BoxLayout
{
    /// <summary>Initializes a new instance of <see cref="BoxLayout"/>.</summary>
    public BoxLayout(
        float x, float y, float width,
        float third, float first, float min, float median,
        float categoryHoverX, float categoryHoverWidth)
    {
        X = x;
        Y = y;
        Width = width;
        Third = third;
        First = first;
        Min = min;
        Median = median;
        CategoryHoverX = categoryHoverX;
        CategoryHoverWidth = categoryHoverWidth;
    }

    /// <summary>Body left edge.</summary>
    public float X { get; }
    /// <summary>Max pixel-Y (top of the whisker).</summary>
    public float Y { get; }
    /// <summary>Body width.</summary>
    public float Width { get; }
    /// <summary>Third quartile pixel-Y.</summary>
    public float Third { get; }
    /// <summary>First quartile pixel-Y.</summary>
    public float First { get; }
    /// <summary>Min pixel-Y (bottom of the whisker).</summary>
    public float Min { get; }
    /// <summary>Median pixel-Y (line inside the box).</summary>
    public float Median { get; }
    /// <summary>Category-wide hover-area top-left X (centered on the category, ignores series-stacking offset).</summary>
    public float CategoryHoverX { get; }
    /// <summary>Category-wide hover-area width (axis unit-width, not series-divided width).</summary>
    public float CategoryHoverWidth { get; }

    /// <summary>Total whisker extent in pixels.</summary>
    public float Height => Min - Y;
}
