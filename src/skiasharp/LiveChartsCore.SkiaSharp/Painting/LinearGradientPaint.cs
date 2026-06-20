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
/// Defines a set of geometries that will be painted using a linear gradient shader.
/// </summary>
/// <seealso cref="SkiaPaint" />
/// <remarks>
/// Initializes a new instance of the <see cref="LinearGradientPaint"/> class.
/// </remarks>
/// <param name="gradientStops">The gradient stops.</param>
/// <param name="startPoint">
/// The start point, both X and Y in the range of 0 to 1, where 0 is the start of the axis and 1 the end.
/// </param>
/// <param name="endPoint">
/// The end point, both X and Y in the range of 0 to 1, where 0 is the start of the axis and 1 the end.
/// </param>
/// <param name="colorPos">
/// An array of floats in the range of 0 to 1.
/// These floats indicate the relative positions of the colors, you can set that argument to null to equally
/// space the colors, default is null.
/// </param>
/// <param name="tileMode">
/// The shader tile mode, default is <see cref="SKShaderTileMode.Clamp"/>.
/// </param>
public class LinearGradientPaint(
    SKColor[] gradientStops,
    SKPoint startPoint,
    SKPoint endPoint,
    float[]? colorPos = null,
    SKShaderTileMode tileMode = SKShaderTileMode.Clamp)
        : SkiaPaint
{
    private SKShader? _shader;
    internal SKColorFilter? _opacityFilter;
    internal float _opacityFilterAlpha = -1f;
    private SKRect _activeClip = new();

    private SKColor[] GradientStops { get; set; } = gradientStops;
    private SKPoint StartPoint { get; set; } = startPoint;
    private SKPoint EndPoint { get; set; } = endPoint;
    private float[]? ColorPos { get; set; } = colorPos;

    /// <summary>
    /// Default start point.
    /// </summary>
    public static readonly SKPoint DefaultStartPoint = new(0, 0.5f);

    /// <summary>
    /// Default end point.
    /// </summary>
    public static readonly SKPoint DefaultEndPoint = new(1, 0.5f);

    /// <summary>
    /// Initializes a new instance of the <see cref="LinearGradientPaint"/> class.
    /// </summary>
    /// <param name="gradientStops">The gradient stops.</param>
    public LinearGradientPaint(SKColor[] gradientStops)
        : this(gradientStops, DefaultStartPoint, DefaultEndPoint) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LinearGradientPaint"/> class.
    /// </summary>
    /// <param name="startColor">The start color.</param>
    /// <param name="endColor">The end color.</param>
    /// <param name="startPoint">
    /// The start point, both X and Y in the range of 0 to 1, where 0 is the start of the axis and 1 the end.
    /// </param>
    /// <param name="endPoint">
    /// The end point, both X and Y in the range of 0 to 1, where 0 is the start of the axis and 1 the end.
    /// </param>
    public LinearGradientPaint(SKColor startColor, SKColor endColor, SKPoint startPoint, SKPoint endPoint)
        : this([startColor, endColor], startPoint, endPoint) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LinearGradientPaint"/> class.
    /// </summary>
    /// <param name="start">The start.</param>
    /// <param name="end">The end.</param>
    public LinearGradientPaint(SKColor start, SKColor end)
        : this(start, end, DefaultStartPoint, DefaultEndPoint) { }

    /// <inheritdoc cref="Paint.CloneTask" />
    public override Paint CloneTask()
    {
        var clone = new LinearGradientPaint(GradientStops, StartPoint, EndPoint, ColorPos, tileMode);
        Map(this, clone);

        return clone;
    }

    internal override void OnPaintStarted(DrawingContext drawingContext, IDrawnElement? drawnElement)
    {
        var skiaContext = (SkiaSharpDrawingContext)drawingContext;
        _skiaPaint = UpdateSkiaPaint(skiaContext, drawnElement);

        var bounds = skiaContext.Canvas.LocalClipBounds;
        _activeClip = new SKRect(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);

        _skiaPaint.Shader = GetShader();
    }

    internal override void ApplyOpacityMask(DrawingContext context, float opacity, IDrawnElement? drawnElement)
    {
        if (_skiaPaint is null || opacity > 0.99) return;

        // Previously this allocated an SKColorFilter every call and `RestoreOpacityMask`
        // dropped the reference without disposing — the native handle leaked to GC
        // finalization. Cache by opacity value; for static or coarsely-quantized opacity
        // (most cases) the native filter is created once and reused.
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
        if (target is not LinearGradientPaint toPaint) return target;

        if (toPaint.GradientStops.Length != GradientStops.Length)
            throw new NotImplementedException(
                $"Transitions between {nameof(GradientStops)} must be of the same length.");

        for (var i = 0; i < GradientStops.Length; i++)
            GradientStops[i] = new SKColor(
                (byte)(GradientStops[i].Red + progress * (toPaint.GradientStops[i].Red - GradientStops[i].Red)),
                (byte)(GradientStops[i].Green + progress * (toPaint.GradientStops[i].Green - GradientStops[i].Green)),
                (byte)(GradientStops[i].Blue + progress * (toPaint.GradientStops[i].Blue - GradientStops[i].Blue)),
                (byte)(GradientStops[i].Alpha + progress * (toPaint.GradientStops[i].Alpha - GradientStops[i].Alpha)));

        StartPoint = new SKPoint(
            StartPoint.X + progress * (toPaint.StartPoint.X - StartPoint.X),
            StartPoint.Y + progress * (toPaint.StartPoint.Y - StartPoint.Y));

        EndPoint = new SKPoint(
            EndPoint.X + progress * (toPaint.EndPoint.X - EndPoint.X),
            EndPoint.Y + progress * (toPaint.EndPoint.Y - EndPoint.Y));

        if (ColorPos is not null && toPaint.ColorPos is not null)
        {
            if (ColorPos is null || ColorPos.Length != toPaint.ColorPos.Length)
                throw new NotImplementedException(
                    $"Transitions between {nameof(ColorPos)} must be of the same length.");

            for (var i = 0; i < ColorPos.Length; i++)
                ColorPos[i] = ColorPos[i] + progress * (toPaint.ColorPos[i] - ColorPos[i]);
        }

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

    private SKShader GetShader()
    {
        if (_shader is not null)
            return _shader;

        var xf = _activeClip.Location.X;
        var xt = xf + _activeClip.Width;

        var yf = _activeClip.Location.Y;
        var yt = yf + _activeClip.Height;

        var start = new SKPoint(xf + (xt - xf) * StartPoint.X, yf + (yt - yf) * StartPoint.Y);
        var end = new SKPoint(xf + (xt - xf) * EndPoint.X, yf + (yt - yf) * EndPoint.Y);

        return
            _shader = SKShader.CreateLinearGradient(start, end, GradientStops, ColorPos, tileMode);
    }
}
