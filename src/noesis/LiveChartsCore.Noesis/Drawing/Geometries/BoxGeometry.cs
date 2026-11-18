using LiveChartsCore.Drawing;
using Noesis;

namespace LiveChartsCore.Noesis.Drawing.Geometries;

/// <inheritdoc />
public class BoxGeometry : BaseBoxGeometry, IDrawnElement<NoesisDrawingContext>
{
    /// <inheritdoc />
    public virtual void Draw(NoesisDrawingContext context)
    {
        var cx = X + Width / 2;
        var top = Math.Min(First, Third);
        var bottom = Math.Max(First, Third);

        context.Canvas.DrawRectangle(context.Fill, context.Stroke, new Rect(X, top, Width, bottom - top));

        if (context.Stroke is not null)
        {
            context.Canvas.DrawLine(context.Stroke, new Point(cx, Y), new Point(cx, top));
            context.Canvas.DrawLine(context.Stroke, new Point(cx, bottom), new Point(cx, Min));
            context.Canvas.DrawLine(context.Stroke, new Point(X, Median), new Point(X + Width, Median));
        }
    }
}
