using LiveChartsCore.Drawing;
using Noesis;

namespace LiveChartsCore.Noesis.Drawing.Geometries;

/// <inheritdoc />
public class LineGeometry : BaseLineGeometry, IDrawnElement<NoesisDrawingContext>
{
    /// <inheritdoc />
    public void Draw(NoesisDrawingContext context)
    {
        if (context.Stroke is not null)
        {
            context.Canvas.DrawLine(context.Stroke, new Point(X, Y), new Point(X1, Y1));
        }
    }
}
