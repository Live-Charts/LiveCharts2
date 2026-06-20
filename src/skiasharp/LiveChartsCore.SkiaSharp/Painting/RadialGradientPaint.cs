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
/// Defines a set of geometries that will be painted using a radial gradient shader.
/// </summary>
/// <seealso cref="SkiaPaint" />
public class RadialGradientPaint : SkiaPaint
{
    private SKShader? _shader;
    internal SKColorFilter? _opacityFilter;
    internal float _opacityFilterAlpha = -1f;
    private SKRect _activeClip = new();
    private readonly SKColor[] _gradientStops;
    private SKPoint _center;
    private float _radius;
    private readonly float[]? _colorPos;
    private readonly SKShaderTileMode _tileMode;

    /// <summary>
    /// Initializes a new instance of the <see cref="RadialGradientPaint"/> class.
    /// </summary>
    /// <param name="gradientStops">The gradient stops.</param>
    /// <param name="center">
    /// The center point of the gradient, both X and Y in the range of 0 to 1, where 0 is the start of the axis and 1 the end,
    /// default is (0.5, 0.5).
    /// </param>
    /// <param name="radius">
    /// The radius, in the range of 0 to 1, where 1 is the minimum of both Width and Height of the chart, default is 0.5.
    /// </param>
    /// <param name="colorPos">
    /// An array of integers in the range of 0 to 1.
    /// These integers indicate the relative positions of the colors, You can set that argument to null to equally
    /// space the colors, default is null.
    /// </param>
    /// <param name="tileMode">
    /// The shader tile mode, default is <see cref="SKShaderTileMode.Clamp"/>.
    /// </param>
    public RadialGradientPaint(
        SKColor[] gradientStops,
        SKPoint? center = null,
        float radius = 0.5f,
        float[]? colorPos = null,
        SKShaderTileMode tileMode = SKShaderTileMode.Clamp)
    {
        _gradientStops = gradientStops;
        center ??= new SKPoint(0.5f, 0.5f);
        _center = center.Value;
        _radius = radius;
        _colorPos = colorPos;
        _tileMode = tileMode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RadialGradientPaint"/> class.
    /// </summary>
    /// <param name="centerColor">Color of the center.</param>
    /// <param name="outerColor">Color of the outer.</param>
    public RadialGradientPaint(SKColor centerColor, SKColor outerColor)
        : this([centerColor, outerColor]) { }

    /// <inheritdoc cref="Paint.CloneTask" />
    public override Paint CloneTask()
    {
        var clone = new RadialGradientPaint(_gradientStops, _center, _radius, _colorPos, _tileMode);
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

        // See LinearGradientPaint.ApplyOpacityMask — same leak fix: previously a fresh
        // SKColorFilter was created every call and Restore dropped the reference without
        // disposing. Cache by opacity; for steady-state opacity the native filter is
        // created once and reused.
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
        if (target is not RadialGradientPaint toPaint) return target;

        if (toPaint._gradientStops.Length != _gradientStops.Length)
            throw new ArgumentException("The gradient stops must be the same length.");

        for (var i = 0; i < _gradientStops.Length; i++)
            _gradientStops[i] = new SKColor(
                (byte)(_gradientStops[i].Red + progress * (toPaint._gradientStops[i].Red - _gradientStops[i].Red)),
                (byte)(_gradientStops[i].Green + progress * (toPaint._gradientStops[i].Green - _gradientStops[i].Green)),
                (byte)(_gradientStops[i].Blue + progress * (toPaint._gradientStops[i].Blue - _gradientStops[i].Blue)),
                (byte)(_gradientStops[i].Alpha + progress * (toPaint._gradientStops[i].Alpha - _gradientStops[i].Alpha)));

        _center = new SKPoint(
            _center.X + progress * (toPaint._center.X - _center.X),
            _center.Y + progress * (toPaint._center.Y - _center.Y));

        _radius = _radius + progress * (toPaint._radius - _radius);

        if (_colorPos is not null && toPaint._colorPos is not null)
        {
            if (_colorPos is null || _colorPos.Length != _colorPos.Length)
                throw new ArgumentException("The color positions must be the same length.");

            for (var i = 0; i < _colorPos.Length; i++)
                _colorPos[i] = _colorPos[i] + progress * (_colorPos[i] - _colorPos[i]);
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

        var center = new SKPoint(_activeClip.Location.X + _center.X * _activeClip.Width, _activeClip.Location.Y + _center.Y * _activeClip.Height);
        var r = _activeClip.Location.X + _activeClip.Width > _activeClip.Location.Y + _activeClip.Height
            ? _activeClip.Location.Y + _activeClip.Height
            : _activeClip.Location.X + _activeClip.Width;
        r *= _radius;

        return
            _shader = SKShader.CreateRadialGradient(center, r, _gradientStops, _colorPos, _tileMode);
    }
}
