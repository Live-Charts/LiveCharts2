// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Drawing;
using LiveChartsCore.Painting;

namespace LiveChartsCore.Console.Drawing.Geometries;

/// <summary>
/// Step-line path geometry. Strokes by emitting an L-shape per segment (horizontal first, then
/// vertical). Fills using the same per-column min/max approach as <see cref="VectorAreaGeometry"/>.
/// Mirrors SkiaSharp's StepLineAreaGeometry.
/// </summary>
public class StepLineAreaGeometry : BaseVectorGeometry, IDrawnElement<ConsoleDrawingContext>
{
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

            if (first)
            {
                // Implicit MoveTo to the segment's end (matches SkiaSharp's OnOpen).
                currentX = segment.Xj;
                currentY = segment.Yj;
                first = false;
                continue;
            }

            // Step pattern: horizontal first, then vertical.
            var hX = segment.Xj;
            var hY = currentY;
            surface.DrawLine((int)currentX, (int)currentY, (int)hX, (int)hY, color);
            surface.DrawLine((int)hX, (int)hY, (int)segment.Xj, (int)segment.Yj, color);

            currentX = segment.Xj;
            currentY = segment.Yj;
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

            if (first)
            {
                currentX = segment.Xj;
                currentY = segment.Yj;
                BucketPoint((int)currentX, (int)currentY, w, minY, maxY, hasData);
                first = false;
                continue;
            }

            var hX = segment.Xj;
            var hY = currentY;
            BucketLine((int)currentX, (int)currentY, (int)hX, (int)hY, w, minY, maxY, hasData);
            BucketLine((int)hX, (int)hY, (int)segment.Xj, (int)segment.Yj, w, minY, maxY, hasData);

            currentX = segment.Xj;
            currentY = segment.Yj;
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

    private static void BucketLine(
        int x0, int y0, int x1, int y1,
        int w, int[] minY, int[] maxY, bool[] hasData)
    {
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
