using LiveChartsCore.Drawing;
using Noesis;

namespace LiveChartsCore.Noesis.Drawing.Geometries;

/// <inheritdoc />
public class RectangleGeometry : BoundedDrawnGeometry, IDrawnElement<NoesisDrawingContext>
{
    /// <inheritdoc />
    public virtual void Draw(NoesisDrawingContext context)
    {
        var bounds = new Rect(
            Math.Min(X, X + Width),
            Math.Min(Y, Y + Height),
            Math.Abs(Width),
            Math.Abs(Height));

        context.Canvas.DrawRectangle(context.Fill, context.Stroke, bounds);
    }
}
