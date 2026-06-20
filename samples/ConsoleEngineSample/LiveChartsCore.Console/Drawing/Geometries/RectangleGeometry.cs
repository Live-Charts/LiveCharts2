// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Drawing;
using LiveChartsCore.Painting;

namespace LiveChartsCore.Console.Drawing.Geometries;

public class RectangleGeometry : BoundedDrawnGeometry, IDrawnElement<ConsoleDrawingContext>
{
    public void Draw(ConsoleDrawingContext context)
    {
        var paint = context.ActiveLvcPaint;
        if (paint is null) return;

        if (paint.PaintStyle.HasFlag(PaintStyle.Stroke))
            context.Surface.StrokeRect((int)X, (int)Y, (int)Width, (int)Height, context.ActiveColor);
        else
            context.Surface.FillRect((int)X, (int)Y, (int)Width, (int)Height, context.ActiveColor);
    }
}
