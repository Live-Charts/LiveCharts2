// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Drawing;
using LiveChartsCore.Painting;

namespace LiveChartsCore.Console.Drawing.Geometries;

/// <summary>
/// Box-and-whisker plot geometry: vertical whiskers from max→Q3 and Q1→min, a box spanning
/// Q1↔Q3, and a horizontal line at the median. Stroke style draws all four segments outline-only;
/// fill style fills the box.
/// </summary>
public class BoxGeometry : BaseBoxGeometry, IDrawnElement<ConsoleDrawingContext>
{
    public virtual void Draw(ConsoleDrawingContext context)
    {
        var paint = context.ActiveLvcPaint;
        if (paint is null) return;

        var w = (int)Width;
        var x = (int)X;
        var cx = x + w / 2;
        var h = (int)Y;        // max → top of upper whisker
        var q3 = (int)Third;
        var q1 = (int)First;
        var min = (int)Min;
        var med = (int)Median;

        int yi, yj;
        if (q3 > q1) { yi = q1; yj = q3; }
        else { yi = q3; yj = q1; }

        var color = context.ActiveColor;
        var boxHeight = Math.Abs(q3 - q1);
        if (boxHeight <= 0) boxHeight = 1;

        // Whiskers (always drawn).
        context.Surface.DrawLine(cx, h, cx, yi, color);
        context.Surface.DrawLine(cx, yj, cx, min, color);

        // Box body.
        if (paint.PaintStyle.HasFlag(PaintStyle.Stroke))
            context.Surface.StrokeRect(x, yi, w, boxHeight, color);
        else
            context.Surface.FillRect(x, yi, w, boxHeight, color);

        // Median tick — always a stroke line across the box.
        context.Surface.DrawLine(x, med, x + w - 1, med, color);
    }
}
