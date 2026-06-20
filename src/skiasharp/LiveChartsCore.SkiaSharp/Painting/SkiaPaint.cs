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
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using LiveChartsCore.SkiaSharpView.Painting.ImageFilters;
using SkiaSharp;

namespace LiveChartsCore.SkiaSharpView.Painting;

/// <summary>
/// Initializes a new instance of the <see cref="SkiaPaint"/> class.
/// </summary>
/// <param name="strokeThickness">The stroke thickness.</param>
/// <param name="strokeMiter">The stroke miter.</param>
public abstract class SkiaPaint(float strokeThickness = 1f, float strokeMiter = 0f)
    : Paint(strokeThickness, strokeMiter)
{
    /// <summary>
    /// Represents a method that builds a <see cref="SKFont"/> from a <see cref="SKPaint"/>,
    /// a <see cref="SKTypeface"/>, and a size.
    /// </summary>
    /// <param name="paint">The paint instance that skia will use to draw the text.</param>
    /// <param name="typeface">The typefaced requested by the <see cref="SkiaPaint"/> instance or the
    /// <see cref="Drawing.Geometries.LabelGeometry"/>.</param>
    /// <param name="size">The text size requested by the <see cref="Drawing.Geometries.LabelGeometry"/>.</param>
    /// <returns>A <see cref="SKFont"/> instance that will be used to draw and shape the label.</returns>
    public delegate SKFont FontBuilderDelegate(SKPaint paint, SKTypeface typeface, float size);

    internal FontBuilderDelegate _fontBuilder = LiveChartsSkiaSharp.DefaultTextSettings.FontBuilder;
    internal SKPaint? _skiaPaint;

    /// <summary>
    /// Gets or sets the font family.
    /// </summary>
    [Obsolete($"Use the {nameof(SKTypeface)} property and assign it to {nameof(SKTypeface)}.{nameof(SKTypeface.FromFamilyName)}(fontFamily, fontStyle)")]
    public string? FontFamily
    {
        get => field;
        set
        {
            field = value;
            SKTypeface = value is null
                ? null
                : SKTypeface.FromFamilyName(value, SKFontStyle ?? SKTypeface.Default.FontStyle);
        }
    }

    /// <summary>
    /// Gets or sets the font style.
    /// </summary>
    [Obsolete($"Use the {nameof(SKTypeface)} property and assign it to {nameof(SKTypeface)}.{nameof(SKTypeface.FromFamilyName)}(fontFamily, fontStyle)")]
    public SKFontStyle? SKFontStyle
    {
        get => field;
        set
        {
            field = value;
            SKTypeface = value is null
                ? null
                : SKTypeface.FromFamilyName(SKTypeface.Default.FamilyName ?? "Arial", value);
        }
    }

    /// <summary>
    /// Gets or sets the SKTypeface.
    /// </summary>
    public SKTypeface? SKTypeface { get; set; }

    /// <summary>
    /// Gets or sets the stroke cap.
    /// </summary>
    /// <value>
    /// The stroke cap.
    /// </value>
    public SKStrokeCap StrokeCap { get; set; }

    /// <summary>
    /// Gets or sets the stroke join.
    /// </summary>
    /// <value>
    /// The stroke join.
    /// </value>
    public SKStrokeJoin StrokeJoin { get; set; }

    private readonly PathEffectMotionProperty _pathEffectMotion = new();

    /// <summary>
    /// Gets or sets the path effect. Backed by a motion property so the effect can animate on
    /// the rail: assigning a new effect can soft-transition, and a self-animating effect (one
    /// carrying a looping <see cref="PathEffect.Animation"/>) marches indefinitely — the looping
    /// lives in the effect, the paint just reads the current value each frame.
    /// </summary>
    /// <value>
    /// The path effect.
    /// </value>
    public PathEffect? PathEffect
    {
        get => _pathEffectMotion.GetMovement(this);
        set
        {
            // The effect owns its animation (null = static, no transition). The motion uses it,
            // so an effect with a looping animation keeps re-evaluating + invalidating forever.
            _pathEffectMotion.Animation = value?.Animation;
            _pathEffectMotion.SetMovement(value, this);
        }
    }

    private readonly ImageFilterMotionProperty _imageFilterMotion = new();

    /// <summary>
    /// Gets or sets the image filter. Backed by a motion property (like <see cref="PathEffect"/>)
    /// so a self-animating filter (one carrying a looping <see cref="ImageFilters.ImageFilter.Animation"/>)
    /// animates on the rail; the paint just reads the current value each frame.
    /// </summary>
    /// <value>
    /// The image filter.
    /// </value>
    public ImageFilter? ImageFilter
    {
        get => _imageFilterMotion.GetMovement(this);
        set
        {
            _imageFilterMotion.Animation = value?.Animation;
            _imageFilterMotion.SetMovement(value, this);
        }
    }

    /// <summary>
    /// Configures the SkiaSharp font manually.
    /// </summary>
    /// <param name="fontBuilder"></param>
    public SkiaPaint ConfigureSkiaSharpFont(FontBuilderDelegate fontBuilder)
    {
        _fontBuilder = fontBuilder;
        return this;
    }

    internal static SKTypeface FallbackTypeface =>
        field ??= (
               LiveChartsSkiaSharp.DefaultTextSettings.DefaultTypeface
            ?? SKTypeface.Default           // let SkiaSharp decide
            ?? TryCreateFont("Arial")       // common fallback
            ?? TryCreateFont("Helvetica")   // macOS/iOS
            ?? TryCreateFont("Roboto")      // Android
            ?? TryCreateFont("DejaVu Sans") // Linux
            ?? throw new InvalidOperationException(
                "LiveCharts could not find a default typeface, please set the DefaultTypeface property in the TextSettings. " +
                "LiveCharts could not find a default typeface. Please set the DefaultTypeface property using HasTextSettings, e.g.: " +
                "LiveCharts.Configure(config => config.HasTextSettings(new TextSettings { DefaultTypeface = SKTypeface.FromFamilyName(\"Arial\") }));")
        );

    internal bool IsGlobalSKTypeface =>
        GetSKTypeface() == FallbackTypeface;

    internal static void Map(SkiaPaint from, SkiaPaint to, float progress = 1)
    {
        to.PaintStyle = from.PaintStyle;
        to.IsAntialias = from.IsAntialias;
        to.StrokeCap = from.StrokeCap;
        to.StrokeJoin = from.StrokeJoin;
        to.SKTypeface = from.SKTypeface;

        to.StrokeThickness = from.StrokeThickness + progress * (to.StrokeThickness - from.StrokeThickness);
        to.StrokeMiter = from.StrokeMiter + progress * (to.StrokeMiter - from.StrokeMiter);
        to.PathEffect = PathEffect.Transitionate(from.PathEffect, to.PathEffect, progress);
        to.ImageFilter = ImageFilter.Transitionate(from.ImageFilter, to.ImageFilter, progress);
    }

    internal SKPaint UpdateSkiaPaint(SkiaSharpDrawingContext? context, IDrawnElement? drawnElement)
    {
        SKPaint paint;

        if (_skiaPaint is null)
        {
            paint = new SKPaint();
            _skiaPaint = paint;

            paint.Style = PaintStyle.HasFlag(PaintStyle.Stroke)
                ? SKPaintStyle.Stroke
                : SKPaintStyle.Fill;
        }
        else
        {
            paint = _skiaPaint;
        }

        paint.IsAntialias = IsAntialias;
        paint.StrokeCap = StrokeCap;
        paint.StrokeJoin = StrokeJoin;
        paint.StrokeMiter = StrokeMiter;
        paint.StrokeWidth = StrokeThickness;

        // Read the effect ONCE: when animating, the motion returns a fresh interpolated effect
        // per call, so re-reading would build a different instance each access.
        var pathEffect = PathEffect;
        if (pathEffect is not null)
        {
            // A fresh animated effect arrives each frame with no native effect yet → build it.
            // A static effect is built once and cached, exactly as before.
            if (pathEffect._sKPathEffect is null)
                pathEffect.CreateEffect();

            paint.PathEffect = pathEffect._sKPathEffect;
        }

        // Read once (the motion returns a fresh interpolated filter per call while animating).
        var imageFilter = ImageFilter;
        if (imageFilter is not null)
        {
            if (imageFilter._sKImageFilter is null)
                imageFilter.CreateFilter();

            paint.ImageFilter = imageFilter._sKImageFilter;
        }
        else
        {
            paint.ImageFilter = null; // filter removed → clear it from the cached/reused SKPaint
        }

        if (drawnElement is not null)
            paint.StrokeWidth = drawnElement.StrokeThickness;

        // special case for text paints.
        // when  the label is mesured, we do not have a context yet.
        if (context is null)
            return paint;

        context.ActiveSkiaPaint = paint;

        return paint;
    }

    internal SKTypeface GetSKTypeface() => SKTypeface ?? FallbackTypeface;

    internal override void OnPaintFinished(DrawingContext context, IDrawnElement? drawnElement)
    {
        // This method is intentionally left empty.
        // No additional actions are required after painting is finished in this derived class.
    }

    internal override void DisposeTask()
    {
        if (_skiaPaint is not null && !IsGlobalSKTypeface)
            _skiaPaint.Typeface?.Dispose();

        PathEffect?.Dispose();
        ImageFilter?.Dispose();

        _skiaPaint?.Dispose();
        _skiaPaint = null;
    }

    private static SKTypeface? TryCreateFont(string family)
    {
        var tf = SKTypeface.FromFamilyName(family);
        return tf?.FamilyName == family ? tf : null;
    }

}
