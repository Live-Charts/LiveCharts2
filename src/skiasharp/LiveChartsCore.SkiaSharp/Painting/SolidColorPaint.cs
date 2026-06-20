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
using LiveChartsCore.Painting;
using LiveChartsCore.SkiaSharpView.Drawing;
using SkiaSharp;

namespace LiveChartsCore.SkiaSharpView.Painting;

/// <summary>
/// Defines a set of geometries that will be painted using a solid color.
/// </summary>
/// <seealso cref="Paint" />
public class SolidColorPaint : SkiaPaint
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SolidColorPaint"/> class.
    /// </summary>
    public SolidColorPaint()
        : base()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SolidColorPaint"/> class.
    /// </summary>
    /// <param name="color">The color.</param>
    public SolidColorPaint(SKColor color)
        : base()
    {
        Color = color;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SolidColorPaint"/> class.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <param name="strokeWidth">Width of the stroke.</param>
    public SolidColorPaint(SKColor color, float strokeWidth)
        : base(strokeWidth)
    {
        Color = color;
    }

    /// <summary>
    /// Gets or sets the color.
    /// </summary>
    /// <value>
    /// The color.
    /// </value>
    public SKColor Color { get; set; }

    /// <inheritdoc cref="Paint.CloneTask" />
    public override Paint CloneTask()
    {
        var clone = new SolidColorPaint { Color = Color };
        Map(this, clone);

        return clone;
    }

    internal override void OnPaintStarted(DrawingContext drawingContext, IDrawnElement? drawnElement)
    {
        var skiaContext = (SkiaSharpDrawingContext)drawingContext;
        _skiaPaint = UpdateSkiaPaint(skiaContext, drawnElement);

        // SKPaint.Color is a managed property that marshals to the native paint on next use;
        // skipping the write when the source color hasn't moved avoids that pair on every
        // paint-task selection. Most paints carry a static color in steady state.
        if (_skiaPaint.Color != Color) _skiaPaint.Color = Color;
    }

    internal override Paint Transitionate(float progress, Paint target)
    {
        if (target is not SolidColorPaint toPaint) return target;

        Color = new SKColor(
            (byte)(Color.Red + progress * (toPaint.Color.Red - Color.Red)),
            (byte)(Color.Green + progress * (toPaint.Color.Green - Color.Green)),
            (byte)(Color.Blue + progress * (toPaint.Color.Blue - Color.Blue)),
            (byte)(Color.Alpha + progress * (toPaint.Color.Alpha - Color.Alpha)));

        _skiaPaint?.Color = Color;

        return this;
    }

    internal override void ApplyOpacityMask(DrawingContext context, float opacity, IDrawnElement? drawnElement)
    {
        var skiaContext = (SkiaSharpDrawingContext)context;
        var baseColor = Color;
        skiaContext.ActiveSkiaPaint.Color =
            new SKColor(
                baseColor.Red,
                baseColor.Green,
                baseColor.Blue,
                (byte)(baseColor.Alpha * opacity));
    }

    internal override void RestoreOpacityMask(DrawingContext context, float opacity, IDrawnElement? drawnElement)
    {
        var skiaContext = (SkiaSharpDrawingContext)context;
        skiaContext.ActiveSkiaPaint.Color = Color;
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>a string.</returns>
    public override string ToString() =>
        $"({Color.Red}, {Color.Green}, {Color.Blue})";

}
