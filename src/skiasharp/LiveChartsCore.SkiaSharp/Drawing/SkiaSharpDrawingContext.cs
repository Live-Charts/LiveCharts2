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
using LiveChartsCore.Motion;
using LiveChartsCore.Painting;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace LiveChartsCore.SkiaSharpView.Drawing;

/// <summary>
/// Defines a skia sharp drawing context.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SkiaSharpDrawingContext"/> class.
/// </remarks>
/// <param name="motionCanvas">The motion canvas.</param>
/// <param name="canvas">The canvas.</param>
/// <param name="background">The background color.</param>
public class SkiaSharpDrawingContext(
    CoreMotionCanvas motionCanvas, SKCanvas canvas, SKColor background)
        : DrawingContext
{
    /// <summary>
    /// Gets or sets the motion canvas.
    /// </summary>
    /// <value>
    /// The motion canvas.
    /// </value>
    public CoreMotionCanvas MotionCanvas { get; set; } = motionCanvas;

    /// <summary>
    /// Gets or sets the canvas.
    /// </summary>
    /// <value>
    /// The canvas.
    /// </value>
    public SKCanvas Canvas { get; } = canvas;

    /// <summary>
    /// Gets or sets the paint.
    /// </summary>
    /// <value>
    /// The paint.
    /// </value>
    public SKPaint ActiveSkiaPaint { get; set; } = null!;

    /// <summary>
    /// Gets or sets the background.
    /// </summary>
    public SKColor Background { get; set; } = background;

    /// <inheritdoc cref="DrawingContext.LogOnCanvas(string)"/>
    public override void LogOnCanvas(string log)
    {
        using var textPaint = new SKPaint
        {
            Color = SKColors.White,
            TextSize = 14,
            IsAntialias = true,
            Typeface = SkiaPaint.FallbackTypeface
        };

        using var backgroundPaint = new SKPaint
        {
            Color = SKColors.Black.WithAlpha(180),
            Style = SKPaintStyle.Fill
        };

        var lines = log.Split('`');

        Canvas.DrawRect(new(10, 0, 400, (textPaint.TextSize + 4f) * lines.Length), backgroundPaint);

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;
            Canvas.DrawText(
                line,
                new SKPoint(10, 10 + 2 + (textPaint.TextSize + 4f) * i),
                textPaint);
        }
    }

    internal override void OnBeginDraw()
    {
        // For fully opaque backgrounds, Clear is safe and efficient.
        if (Background.Alpha == 255)
        {
            Canvas.Clear(Background);
            return;
        }

        // For transparent or translucent backgrounds, draw a filled rect with SrcOver
        // to avoid clearing through to the underlying OS surface while still clearing
        // the motion canvas to prevent ghosting/trails from previous frames.
        var clearColor = Background == SKColor.Empty ? SKColors.Transparent : Background;
        
        using var backgroundPaint = new SKPaint
        {
            Color = clearColor,
            Style = SKPaintStyle.Fill,
            BlendMode = SKBlendMode.SrcOver
        };

        var bounds = Canvas.DeviceClipBounds;
        Canvas.DrawRect(bounds, backgroundPaint);
    }

    internal override void OnEndDraw()
    {
        // No cleanup is needed at the end of the draw operation.
        // This method is intentionally left empty, following the pattern in SkiaPaint.OnPaintFinished.
    }

    internal override void OnBeginZone(CanvasZone zone)
    {
        if (zone.Clip == LvcRectangle.Empty) return;

        zone.StateId = Canvas.Save();
        Canvas.ClipRect(new(zone.Clip.X, zone.Clip.Y, zone.Clip.X + zone.Clip.Width, zone.Clip.Y + zone.Clip.Height));
    }

    internal override void OnEndZone(CanvasZone zone)
    {
        if (zone.Clip == LvcRectangle.Empty) return;

        Canvas.RestoreToCount(zone.StateId);
    }

    internal override void Draw(IDrawnElement drawable)
    {
        var opacity = ActiveOpacity;

        var element = (IDrawnElement<SkiaSharpDrawingContext>)drawable;

        var canvasState = 0;
        if (element.HasTransform)
        {
            canvasState = Canvas.Save();
            var transform = BuildTransform(element);
            Canvas.Concat(ref transform);
        }

        if (ActiveLvcPaint is null)
        {
            // if the active paint is null, we need to draw by the element paint

            var elementFill = element.Fill;
            var elementStroke = element.Stroke;
            var elementPaint = element.Paint;

            if (elementFill is not null)
                DrawByPaint(elementFill, element, opacity);

            if (elementStroke is not null)
                DrawByPaint(elementStroke, element, opacity);

            if (elementPaint is not null)
                DrawByPaint(elementPaint, element, opacity);
        }
        else
        {
            // we will draw using the active paint while the element paint is null

            if (ActiveLvcPaint.PaintStyle.HasFlag(PaintStyle.Fill))
            {
                var elementFill = element.Fill;

                if (elementFill is null)
                    DrawElement(element, opacity);
                else
                    DrawByPaint(elementFill, element, opacity);
            }

            if (ActiveLvcPaint.PaintStyle.HasFlag(PaintStyle.Stroke))
            {
                var elementStroke = element.Stroke;

                if (elementStroke is null)
                    DrawElement(element, opacity);
                else
                    DrawByPaint(elementStroke, element, opacity);
            }
        }

        if (element.HasTransform)
        {
            Canvas.RestoreToCount(canvasState);
        }
    }

    internal override void SelectPaint(Paint paint)
    {
        ActiveLvcPaint = paint;
        //ActiveSkiaPaint = paint.SKPaint; set by paint.OnPaintStarted
        PaintMotionProperty.s_activePaint = paint;

        paint.OnPaintStarted(this, null);
    }

    internal override void ClearPaintSelection(Paint paint)
    {
        paint.OnPaintFinished(this, null);

        ActiveLvcPaint = null!;
        ActiveSkiaPaint = null!;
        PaintMotionProperty.s_activePaint = null!;
    }

    private void DrawByPaint(Paint paint, IDrawnElement<SkiaSharpDrawingContext> element, float opacity)
    {
        var originalPaint = ActiveSkiaPaint;
        var originalTask = ActiveLvcPaint;

        // hack for now...
        // ActiveLvcPaint must be null for this kind of draw method...
        // normally used to draw tooltips and legends
        // Improve this? maybe a cleaner way?
        if (paint != MeasureTask.Instance)
        {
            ActiveLvcPaint = paint;
            paint.OnPaintStarted(this, element);
        }

        DrawElement(element, opacity);

        paint.OnPaintFinished(this, element);

        ActiveSkiaPaint = originalPaint;
        ActiveLvcPaint = originalTask;
    }

    private void DrawElement(IDrawnElement<SkiaSharpDrawingContext> element, float opacity)
    {
        var hasGeometryOpacity =
            ActiveLvcPaint is not null &&
            opacity < 1;

        var hasShadow =
            ActiveLvcPaint is not null &&
            element.DropShadow is not null &&
            element.DropShadow != LvcDropShadow.Empty;

        SKImageFilter? originalFilter = null;

        if (hasGeometryOpacity)
        {
            ActiveLvcPaint!.ApplyOpacityMask(this, opacity, element);
        }

        if (hasShadow)
        {
            var shadow = element.DropShadow!;
            originalFilter = ActiveSkiaPaint.ImageFilter;

            ActiveSkiaPaint.ImageFilter = SKImageFilter.CreateDropShadow(
                shadow.Dx, shadow.Dy,
                shadow.SigmaX, shadow.SigmaY,
                new(shadow.Color.R, shadow.Color.G, shadow.Color.B, shadow.Color.A));
        }

        element.Draw(this);

        if (hasShadow)
        {
            ActiveSkiaPaint.ImageFilter!.Dispose();
            ActiveSkiaPaint.ImageFilter = originalFilter;
        }

        if (hasGeometryOpacity)
        {
            ActiveLvcPaint!.RestoreOpacityMask(this, opacity, element);
        }
    }

    private static SKMatrix BuildTransform(IDrawnElement<SkiaSharpDrawingContext> element)
    {
        var m = element.Measure();
        var o = element.TransformOrigin;
        var p = new SKPoint(element.X, element.Y);
        var xo = m.Width * o.X;
        var yo = m.Height * o.Y;

        var origin = new SKPoint(p.X + xo, p.Y + yo);
        var matrix = SKMatrix.CreateIdentity();

        if (element.HasTranslate)
        {
            var t = element.TranslateTransform;
            matrix = SKMatrix.Concat(matrix, SKMatrix.CreateTranslation(t.X, t.Y));
        }

        if (element.HasRotation)
        {
            matrix = SKMatrix.Concat(matrix, SKMatrix.CreateRotationDegrees(
                element.RotateTransform, origin.X, origin.Y));
        }

        if (element.HasScale)
        {
            var s = element.ScaleTransform;
            matrix = SKMatrix.Concat(matrix, SKMatrix.CreateScale(s.X, s.Y, origin.X, origin.Y));
        }

        if (element.HasSkew)
        {
            var skew = element.SkewTransform;
            var skewMatrix = new SKMatrix
            {
                ScaleX = 1,
                SkewX = (float)Math.Tan(skew.X * Math.PI / 180),
                TransX = 0,
                SkewY = (float)Math.Tan(skew.Y * Math.PI / 180),
                ScaleY = 1,
                TransY = 0,
                Persp0 = 0,
                Persp1 = 0,
                Persp2 = 1
            };
            var translateToOrigin = SKMatrix.CreateTranslation(origin.X, origin.Y);
            var translateBack = SKMatrix.CreateTranslation(-origin.X, -origin.Y);
            matrix = SKMatrix.Concat(matrix, translateToOrigin);
            matrix = SKMatrix.Concat(matrix, skewMatrix);
            matrix = SKMatrix.Concat(matrix, translateBack);
        }

        return matrix;
    }

}
