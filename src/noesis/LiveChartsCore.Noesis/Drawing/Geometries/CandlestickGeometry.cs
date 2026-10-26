using LiveChartsCore.Drawing;
using Noesis;

namespace LiveChartsCore.Noesis.Drawing.Geometries;

/// <inheritdoc />
public class CandlestickGeometry : BaseCandlestickGeometry, IDrawnElement<NoesisDrawingContext>
{
    /// <inheritdoc />
    public virtual void Draw(NoesisDrawingContext context)
    {
        var cx = X + Width / 2;
        var top = Math.Min(Open, Close);
        var bottom = Math.Max(Open, Close);

        context.Canvas.DrawLine(context.Stroke, new Point(cx, Y), new Point(cx, top));
        context.Canvas.DrawRectangle(context.Fill, context.Stroke, new Rect(X, top, Width, bottom - top));
        context.Canvas.DrawLine(context.Stroke, new Point(cx, bottom), new Point(cx, Low));
    }
}
