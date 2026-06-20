// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Drawing;

namespace LiveChartsCore.Console.Drawing.Geometries;

public class LineGeometry : BaseLineGeometry, IDrawnElement<ConsoleDrawingContext>
{
    public virtual void Draw(ConsoleDrawingContext context) =>
        context.Surface.DrawLine((int)X, (int)Y, (int)X1, (int)Y1, context.ActiveColor);
}
