// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Drawing;
using LiveChartsCore.Drawing.Segments;
using LiveChartsCore.Painting;

namespace LiveChartsCore.Console.Drawing.Geometries;

/// <summary>
/// Cubic-bezier path geometry. Strokes by sampling each segment into a polyline. Fills by walking
/// the closed polygon (top curve + closing baseline emitted by <c>CoreStackedAreaSeries</c>) and
/// taking the min/max Y per X column — for any "X-monotonic at the boundary" closed polygon (which
/// stacked areas always are), the fill is just the vertical strip between those two extents.
///
/// Mirrors SkiaSharp's CubicBezierAreaGeometry semantics: the path's "current point" carries
/// across segments, the first segment opens with an implicit MoveTo(Xi, Yi), and within each
/// segment Skia's <c>CubicTo(Xi, Yi, Xm, Ym, Xj, Yj)</c> means (Xi, Yi) is the FIRST control
/// point and (Xj, Yj) is the endpoint — the start is the previous segment's end.
/// </summary>
public class VectorAreaGeometry : BaseVectorGeometry, IDrawnElement<ConsoleDrawingContext>
{
    private const int SamplesPerSegment = 16;

    public void Draw(ConsoleDrawingContext context)
    {
        if (Commands.Count == 0) return;
        var paint = context.ActiveLvcPaint;
        if (paint is null) return;

        var color = context.ActiveColor;

        if (paint.PaintStyle.HasFlag(PaintStyle.Stroke))
        {
            DrawStroke(context.Surface, color);
        }
        else if (paint.PaintStyle.HasFlag(PaintStyle.Fill))
        {
            DrawFill(context.Surface, color);
        }
    }

    private void DrawStroke(ConsoleSurface surface, LvcColor color)
    {
        var first = true;
        float currentX = 0, currentY = 0;

        foreach (var segment in Commands)
        {
            segment.IsValid = true;

            float startX, startY;
            if (first)
            {
                startX = segment.Xi;
                startY = segment.Yi;
            }
            else
            {
                startX = currentX;
                startY = currentY;
            }

            if (segment is CubicBezierSegment cubic)
            {
                StrokeCubic(surface,
                    startX, startY,
                    cubic.Xi, cubic.Yi,
                    cubic.Xm, cubic.Ym,
                    cubic.Xj, cubic.Yj,
                    color);
            }
            else
            {
                surface.DrawLine(
                    (int)startX, (int)startY,
                    (int)segment.Xj, (int)segment.Yj,
                    color);
            }

            currentX = segment.Xj;
            currentY = segment.Yj;
            first = false;
        }
    }

    private void DrawFill(ConsoleSurface surface, LvcColor color)
    {
        var w = surface.Width;
        var h = surface.Height;
        if (w == 0) return;

        var minY = new int[w];
        var maxY = new int[w];
        var hasData = new bool[w];

        var first = true;
        float currentX = 0, currentY = 0;

        foreach (var segment in Commands)
        {
            segment.IsValid = true;

            float startX, startY;
            if (first)
            {
                startX = segment.Xi;
                startY = segment.Yi;
            }
            else
            {
                startX = currentX;
                startY = currentY;
            }

            if (segment is CubicBezierSegment cubic)
            {
                BucketCubic(
                    startX, startY,
                    cubic.Xi, cubic.Yi,
                    cubic.Xm, cubic.Ym,
                    cubic.Xj, cubic.Yj,
                    w, minY, maxY, hasData);
            }
            else
            {
                BucketLine((int)startX, (int)startY, (int)segment.Xj, (int)segment.Yj, w, minY, maxY, hasData);
            }

            currentX = segment.Xj;
            currentY = segment.Yj;
            first = false;
        }

        for (var x = 0; x < w; x++)
        {
            if (!hasData[x]) continue;
            var y0 = Math.Max(0, minY[x]);
            var y1 = Math.Min(h - 1, maxY[x]);
            for (var y = y0; y <= y1; y++)
                surface.SetPixel(x, y, color);
        }
    }

    private static void StrokeCubic(
        ConsoleSurface surface,
        float x0, float y0,
        float c1x, float c1y,
        float c2x, float c2y,
        float x1, float y1,
        LvcColor color)
    {
        var prevX = (int)x0;
        var prevY = (int)y0;

        for (var i = 1; i <= SamplesPerSegment; i++)
        {
            var t = i / (float)SamplesPerSegment;
            var u = 1 - t;
            var b0 = u * u * u;
            var b1 = 3 * u * u * t;
            var b2 = 3 * u * t * t;
            var b3 = t * t * t;

            var px = (int)(b0 * x0 + b1 * c1x + b2 * c2x + b3 * x1);
            var py = (int)(b0 * y0 + b1 * c1y + b2 * c2y + b3 * y1);

            surface.DrawLine(prevX, prevY, px, py, color);
            prevX = px;
            prevY = py;
        }
    }

    private static void BucketCubic(
        float x0, float y0,
        float c1x, float c1y,
        float c2x, float c2y,
        float x1, float y1,
        int w, int[] minY, int[] maxY, bool[] hasData)
    {
        var prevX = (int)x0;
        var prevY = (int)y0;
        BucketPoint(prevX, prevY, w, minY, maxY, hasData);

        for (var i = 1; i <= SamplesPerSegment; i++)
        {
            var t = i / (float)SamplesPerSegment;
            var u = 1 - t;
            var b0 = u * u * u;
            var b1 = 3 * u * u * t;
            var b2 = 3 * u * t * t;
            var b3 = t * t * t;

            var px = (int)(b0 * x0 + b1 * c1x + b2 * c2x + b3 * x1);
            var py = (int)(b0 * y0 + b1 * c1y + b2 * c2y + b3 * y1);

            BucketLine(prevX, prevY, px, py, w, minY, maxY, hasData);
            prevX = px;
            prevY = py;
        }
    }

    private static void BucketLine(
        int x0, int y0, int x1, int y1,
        int w, int[] minY, int[] maxY, bool[] hasData)
    {
        // Bresenham — visit every (x, y) on the line and update per-column min/max.
        var dx = Math.Abs(x1 - x0);
        var dy = -Math.Abs(y1 - y0);
        var sx = x0 < x1 ? 1 : -1;
        var sy = y0 < y1 ? 1 : -1;
        var err = dx + dy;

        while (true)
        {
            BucketPoint(x0, y0, w, minY, maxY, hasData);
            if (x0 == x1 && y0 == y1) break;
            var e2 = 2 * err;
            if (e2 >= dy) { err += dy; x0 += sx; }
            if (e2 <= dx) { err += dx; y0 += sy; }
        }
    }

    private static void BucketPoint(int x, int y, int w, int[] minY, int[] maxY, bool[] hasData)
    {
        if ((uint)x >= (uint)w) return;
        if (!hasData[x]) { minY[x] = y; maxY[x] = y; hasData[x] = true; }
        else
        {
            if (y < minY[x]) minY[x] = y;
            if (y > maxY[x]) maxY[x] = y;
        }
    }
}
