using LiveChartsCore.Drawing;
using Noesis;

namespace LiveChartsCore.Noesis.Drawing.Geometries;

/// <inheritdoc />
public class CircleGeometry : BoundedDrawnGeometry, IDrawnElement<NoesisDrawingContext>
{
    /// <inheritdoc />
    public virtual void Draw(NoesisDrawingContext context)
    {
        context.Canvas.DrawEllipse(context.Fill, context.Stroke, new Point(X + Width / 2, Y + Height / 2), Width / 2, Height / 2);
    }
}
