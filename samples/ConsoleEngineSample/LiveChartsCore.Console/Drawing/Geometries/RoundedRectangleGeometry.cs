// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Drawing;
using LiveChartsCore.Painting;

namespace LiveChartsCore.Console.Drawing.Geometries;

public class RoundedRectangleGeometry : BaseRoundedRectangleGeometry, IDrawnElement<ConsoleDrawingContext>
{
    public void Draw(ConsoleDrawingContext context)
    {
        var paint = context.ActiveLvcPaint;
        if (paint is null) return;

        if (paint.PaintStyle.HasFlag(PaintStyle.Stroke))
            context.Surface.StrokeRect((int)X, (int)Y, (int)Width, (int)Height, context.ActiveColor);
        else
            // Bar/column/row/stacked series all use this geometry as their TVisual, so route
            // fills through the stamped path. Pies, lines, areas, candlesticks, etc. use
            // different geometries (DoughnutGeometry, VectorAreaGeometry, etc.) that keep
            // calling plain FillRect — they don't need texture distinction.
            context.Surface.FillRectStamped((int)X, (int)Y, (int)Width, (int)Height, context.ActiveColor);
    }
}
