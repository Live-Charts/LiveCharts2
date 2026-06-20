// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Drawing;
using LiveChartsCore.Painting;

namespace LiveChartsCore.Console.Drawing.Geometries;

/// <summary>
/// OHLC candlestick: vertical wicks from high→open and close→low, plus a body rectangle
/// spanning open↔close. Mirrors the SkiaSharp port — uses Y as the high price (smallest screen
/// coord) and <see cref="BaseCandlestickGeometry.Low"/> as the low price (largest screen coord).
/// </summary>
public class CandlestickGeometry : BaseCandlestickGeometry, IDrawnElement<ConsoleDrawingContext>
{
    public virtual void Draw(ConsoleDrawingContext context)
    {
        var paint = context.ActiveLvcPaint;
        if (paint is null) return;

        var w = (int)Width;
        var x = (int)X;
        var cx = x + w / 2;
        var h = (int)Y;       // high → top of wick (smallest Y in screen coords)
        var o = (int)Open;
        var c = (int)Close;
        var l = (int)Low;

        int yi, yj;
        if (o > c) { yi = c; yj = o; }
        else { yi = o; yj = c; }

        var color = context.ActiveColor;

        // Upper wick
        context.Surface.DrawLine(cx, h, cx, yi, color);
        // Lower wick
        context.Surface.DrawLine(cx, yj, cx, l, color);

        // Body — fill or outline depending on the active paint style.
        var bodyHeight = Math.Abs(o - c);
        if (bodyHeight <= 0) bodyHeight = 1;

        if (paint.PaintStyle.HasFlag(PaintStyle.Stroke))
            context.Surface.StrokeRect(x, yi, w, bodyHeight, color);
        else
            context.Surface.FillRect(x, yi, w, bodyHeight, color);
    }
}
