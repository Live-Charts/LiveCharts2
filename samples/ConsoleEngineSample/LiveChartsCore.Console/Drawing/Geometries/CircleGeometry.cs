// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Drawing;
using LiveChartsCore.Painting;

namespace LiveChartsCore.Console.Drawing.Geometries;

/// <summary>
/// Bounded circular point geometry — used for ScatterSeries point markers and any other place
/// the chart engine asks for a round visual. Diameter = min(Width, Height); we draw a true
/// circle inscribed in the bounding box rather than an ellipse, which looks better at the
/// resolutions we operate at.
/// </summary>
public class CircleGeometry : BoundedDrawnGeometry, IDrawnElement<ConsoleDrawingContext>
{
    public void Draw(ConsoleDrawingContext context)
    {
        var paint = context.ActiveLvcPaint;
        if (paint is null) return;

        var cx = (int)(X + Width / 2f);
        var cy = (int)(Y + Height / 2f);
        var r = (int)Math.Min(Width, Height) / 2;
        if (r <= 0) return;

        var color = context.ActiveColor;
        if (paint.PaintStyle.HasFlag(PaintStyle.Stroke))
            StrokeCircle(context.Surface, cx, cy, r, color);
        else
            FillCircle(context.Surface, cx, cy, r, color);
    }

    /// <summary>Bresenham/midpoint circle — 1-pixel-thick outline, 8-way symmetric.</summary>
    private static void StrokeCircle(ConsoleSurface s, int cx, int cy, int r, LvcColor color)
    {
        var x = r;
        var y = 0;
        var err = 1 - r;
        while (x >= y)
        {
            s.SetPixel(cx + x, cy + y, color);
            s.SetPixel(cx + y, cy + x, color);
            s.SetPixel(cx - y, cy + x, color);
            s.SetPixel(cx - x, cy + y, color);
            s.SetPixel(cx - x, cy - y, color);
            s.SetPixel(cx - y, cy - x, color);
            s.SetPixel(cx + y, cy - x, color);
            s.SetPixel(cx + x, cy - y, color);

            y++;
            if (err <= 0) err += 2 * y + 1;
            else { x--; err += 2 * (y - x) + 1; }
        }
    }

    /// <summary>Solid disc via per-row scanline using the circle equation.</summary>
    private static void FillCircle(ConsoleSurface s, int cx, int cy, int r, LvcColor color)
    {
        var rSq = r * r;
        for (var dy = -r; dy <= r; dy++)
        {
            var dxMax = (int)Math.Sqrt(rSq - dy * dy);
            for (var dx = -dxMax; dx <= dxMax; dx++)
                s.SetPixel(cx + dx, cy + dy, color);
        }
    }
}
