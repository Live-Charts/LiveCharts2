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
/// Final per-frame geometry + heat color of a single heat cell, returned from
/// <c>MeasureHeatLayout</c>. Heat cells differ from other rect-shaped series in
/// that the dimensions are static (set once at the cell's grid position) while
/// the color animates through the gradient based on the point's weight.
/// </summary>
public readonly struct HeatLayout
{
    /// <summary>Initializes a new instance of <see cref="HeatLayout"/>.</summary>
    /// <param name="x">Cell rect top-left X (centered on the grid intersection minus padding).</param>
    /// <param name="y">Cell rect top-left Y.</param>
    /// <param name="width">Cell width in pixels.</param>
    /// <param name="height">Cell height in pixels.</param>
    /// <param name="hoverX">Hover-area top-left X (matches the gridded cell, before padding).</param>
    /// <param name="hoverY">Hover-area top-left Y.</param>
    /// <param name="hoverWidth">Hover-area width (pre-padding cell width).</param>
    /// <param name="hoverHeight">Hover-area height (pre-padding cell height).</param>
    /// <param name="color">Final heat color (interpolated from the gradient at the point's weight).</param>
    public HeatLayout(
        float x, float y, float width, float height,
        float hoverX, float hoverY, float hoverWidth, float hoverHeight,
        LvcColor color)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        HoverX = hoverX;
        HoverY = hoverY;
        HoverWidth = hoverWidth;
        HoverHeight = hoverHeight;
        Color = color;
    }

    /// <summary>Cell visual rect top-left X.</summary>
    public float X { get; }
    /// <summary>Cell visual rect top-left Y.</summary>
    public float Y { get; }
    /// <summary>Cell width.</summary>
    public float Width { get; }
    /// <summary>Cell height.</summary>
    public float Height { get; }
    /// <summary>Hover-area top-left X (cell grid without padding).</summary>
    public float HoverX { get; }
    /// <summary>Hover-area top-left Y.</summary>
    public float HoverY { get; }
    /// <summary>Hover-area width.</summary>
    public float HoverWidth { get; }
    /// <summary>Hover-area height.</summary>
    public float HoverHeight { get; }
    /// <summary>Interpolated heat color.</summary>
    public LvcColor Color { get; }
}
