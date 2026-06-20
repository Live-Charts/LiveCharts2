// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Drawing;

namespace LiveChartsCore.Console.Drawing.Geometries;

/// <summary>
/// Pie/doughnut wedge — fills pixels of the bounding box that fall within
/// (a) the annulus between <see cref="BaseDoughnutGeometry.InnerRadius"/> and the outer radius
///     (= Width / 2), and
/// (b) the angular range [StartAngle, StartAngle + SweepAngle].
///
/// Angles are in degrees, with 0° at the +X axis (right) and positive sweep going clockwise on
/// screen — same convention as SkiaSharp's SKPath.AddArc, which CorePieSeries targets.
/// CornerRadius is intentionally ignored (rounded corners are barely perceptible at terminal
/// pixel densities and the math is hairy); PushOut translates the wedge along its bisector for
/// hover effects.
/// </summary>
public class DoughnutGeometry : BaseDoughnutGeometry, IDrawnElement<ConsoleDrawingContext>
{
    public virtual void Draw(ConsoleDrawingContext context)
    {
        var paint = context.ActiveLvcPaint;
        if (paint is null) return;

        var sweep = SweepAngle;
        if (sweep <= 0) return;
        // Cap at full circle — the angular test below normalizes to [0, 360) and would
        // exclude one edge of a 360° wedge otherwise.
        if (sweep >= 360) sweep = 360;

        var color = context.ActiveColor;
        var pushOut = PushOut;

        // PushOut shifts the entire wedge along the bisector angle. Compute the offset once and
        // apply to both the center and the bounding box.
        float pushX = 0, pushY = 0;
        if (Math.Abs(pushOut) > 1e-6f)
        {
            var midRad = (StartAngle + sweep / 2) * Math.PI / 180.0;
            pushX = (float)(Math.Cos(midRad) * pushOut);
            pushY = (float)(Math.Sin(midRad) * pushOut);
        }

        var cx = CenterX + pushX;
        var cy = CenterY + pushY;
        var outer = Width * 0.5f;
        var inner = InnerRadius;
        if (outer <= 0) return;

        var outerSq = outer * outer;
        var innerSq = inner * inner;

        var x0 = Math.Max(0, (int)(X + pushX));
        var y0 = Math.Max(0, (int)(Y + pushY));
        var x1 = Math.Min(context.Surface.Width, (int)(X + Width + pushX) + 1);
        var y1 = Math.Min(context.Surface.Height, (int)(Y + Height + pushY) + 1);

        var startAngle = StartAngle;
        var sweepAngle = sweep;
        var fullCircle = sweepAngle >= 360;

        for (var py = y0; py < y1; py++)
        {
            var dy = py - cy;
            for (var px = x0; px < x1; px++)
            {
                var dx = px - cx;
                var rSq = dx * dx + dy * dy;
                if (rSq < innerSq || rSq > outerSq) continue;

                if (!fullCircle)
                {
                    var angleDeg = Math.Atan2(dy, dx) * 180.0 / Math.PI;
                    var delta = (angleDeg - startAngle) % 360.0;
                    if (delta < 0) delta += 360.0;
                    if (delta >= sweepAngle) continue;
                }

                context.Surface.SetPixel(px, py, color);
            }
        }
    }
}
