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
using LiveChartsCore.Painting;
using LiveChartsCore.SkiaSharpView.Drawing;
using SkiaSharp;

namespace LiveChartsCore.SkiaSharpView.Painting;

/// <summary>
/// Defines a set of geometries that will be painted with a repeating two-tone stripe pattern.
/// You give a single base <see cref="Color"/>; the stripe tone is derived from it by
/// <see cref="StripeBrightness"/> (lighter when positive, darker when negative), so the pattern
/// always stays in the same hue. <see cref="StripeWidth"/> sets the band size in pixels and
/// <see cref="StripeAngle"/> the orientation.
/// </summary>
/// <seealso cref="SkiaPaint" />
public class StripedPaint : SkiaPaint
{
    private SKShader? _shader;
    private SKColorFilter? _opacityFilter;
    private float _opacityFilterAlpha = -1f;

    /// <summary>
    /// Initializes a new instance of the <see cref="StripedPaint"/> class.
    /// </summary>
    public StripedPaint()
        : base()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="StripedPaint"/> class.
    /// </summary>
    /// <param name="color">The base color; the stripe tone is derived from it.</param>
    public StripedPaint(SKColor color)
        : base()
    {
        Color = color;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StripedPaint"/> class.
    /// </summary>
    /// <param name="color">The base color; the stripe tone is derived from it.</param>
    /// <param name="strokeWidth">Width of the stroke.</param>
    public StripedPaint(SKColor color, float strokeWidth)
        : base(strokeWidth)
    {
        Color = color;
    }

    /// <summary>
    /// Gets or sets the base color. This is the wider/background band; the stripe band is a
    /// lighter or darker tone of this same color (see <see cref="StripeBrightness"/>).
    /// </summary>
    public SKColor Color { get; set; }

    /// <summary>
    /// Gets or sets how the stripe tone differs from <see cref="Color"/>, in the range -1 to 1.
    /// A positive value blends the stripe toward white (lighter), a negative value toward black
    /// (darker). Default is <c>0.15</c> (a subtle lighter stripe).
    /// </summary>
    public float StripeBrightness { get; set; } = 0.15f;

    /// <summary>
    /// Gets or sets the width, in pixels, of a single stripe band (each tone occupies one band,
    /// so the full pattern repeats every <c>2 * StripeWidth</c> pixels). Default is <c>8</c>.
    /// </summary>
    public float StripeWidth { get; set; } = 8f;

    /// <summary>
    /// Gets or sets the angle of the stripes, in degrees. <c>0</c> draws vertical stripes,
    /// <c>90</c> horizontal. Default is <c>45</c>.
    /// </summary>
    public float StripeAngle { get; set; } = 45f;

    /// <inheritdoc cref="Paint.CloneTask" />
    public override Paint CloneTask()
    {
        var clone = new StripedPaint
        {
            Color = Color,
            StripeBrightness = StripeBrightness,
            StripeWidth = StripeWidth,
            StripeAngle = StripeAngle,
        };
        Map(this, clone);

        return clone;
    }

    internal override void OnPaintStarted(DrawingContext drawingContext, IDrawnElement? drawnElement)
    {
        var skiaContext = (SkiaSharpDrawingContext)drawingContext;
        _skiaPaint = UpdateSkiaPaint(skiaContext, drawnElement);

        _skiaPaint.Shader = GetShader();
    }

    internal override void ApplyOpacityMask(DrawingContext context, float opacity, IDrawnElement? drawnElement)
    {
        if (_skiaPaint is null || opacity > 0.99) return;

        // Mirrors LinearGradientPaint: the pattern lives in a shader, so opacity must be applied via a
        // color filter (not Color.Alpha). Cache by opacity value so the native filter is built once.
        if (_opacityFilter is null || _opacityFilterAlpha != opacity)
        {
            _opacityFilter?.Dispose();
            _opacityFilter = SKColorFilter.CreateBlendMode(
                new SKColor(255, 255, 255, (byte)(255 * opacity)),
                SKBlendMode.DstIn);
            _opacityFilterAlpha = opacity;
        }

        _skiaPaint.ColorFilter = _opacityFilter;
    }

    internal override void RestoreOpacityMask(DrawingContext context, float opacity, IDrawnElement? drawnElement)
    {
        if (_skiaPaint is null) return;

        _skiaPaint.ColorFilter = null;
    }

    internal override Paint Transitionate(float progress, Paint target)
    {
        if (target is not StripedPaint toPaint) return target;

        Color = new SKColor(
            (byte)(Color.Red + progress * (toPaint.Color.Red - Color.Red)),
            (byte)(Color.Green + progress * (toPaint.Color.Green - Color.Green)),
            (byte)(Color.Blue + progress * (toPaint.Color.Blue - Color.Blue)),
            (byte)(Color.Alpha + progress * (toPaint.Color.Alpha - Color.Alpha)));

        StripeBrightness += progress * (toPaint.StripeBrightness - StripeBrightness);
        StripeWidth += progress * (toPaint.StripeWidth - StripeWidth);
        StripeAngle += progress * (toPaint.StripeAngle - StripeAngle);

        _shader?.Dispose();
        _shader = null;

        _skiaPaint?.Shader = GetShader();

        return this;
    }

    internal override void DisposeTask()
    {
        base.DisposeTask();

        _shader?.Dispose();
        _shader = null;

        _opacityFilter?.Dispose();
        _opacityFilter = null;
        _opacityFilterAlpha = -1f;
    }

    /// <summary>
    /// Derives the stripe tone from <see cref="Color"/> by blending toward white (when
    /// <see cref="StripeBrightness"/> is positive) or black (when negative). Alpha is preserved.
    /// </summary>
    private SKColor GetStripeColor()
    {
        var t = StripeBrightness;
        if (t > 1) t = 1;
        if (t < -1) t = -1;

        // t >= 0 -> blend toward white; t < 0 -> blend toward black (target channel = 0).
        var target = t >= 0 ? 255f : 0f;
        var k = Math.Abs(t);

        return new SKColor(
            (byte)(Color.Red + (target - Color.Red) * k),
            (byte)(Color.Green + (target - Color.Green) * k),
            (byte)(Color.Blue + (target - Color.Blue) * k),
            Color.Alpha);
    }

    private SKShader GetShader()
    {
        if (_shader is not null)
            return _shader;

        var band = Math.Max(1f, StripeWidth);
        var period = band * 2f;

        // Two hard-edged bands per period (base, then stripe tone) tiled forever -> stripes. The local
        // matrix rotates the whole pattern; the gradient runs along X, so 0deg = vertical stripes.
        var colors = new[] { Color, Color, GetStripeColor(), GetStripeColor() };
        var positions = new[] { 0f, 0.5f, 0.5f, 1f };
        var rotation = SKMatrix.CreateRotationDegrees(StripeAngle);

        return _shader = SKShader.CreateLinearGradient(
            new SKPoint(0, 0),
            new SKPoint(period, 0),
            colors,
            positions,
            SKShaderTileMode.Repeat,
            rotation);
    }
}
