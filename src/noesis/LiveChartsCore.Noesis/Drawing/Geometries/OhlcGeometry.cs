using LiveChartsCore.Drawing;
using Noesis;

namespace LiveChartsCore.Noesis.Drawing.Geometries;

/// <inheritdoc />
public class OhlcGeometry : BaseCandlestickGeometry, IDrawnElement<NoesisDrawingContext>
{
    /// <inheritdoc />
    public virtual void Draw(NoesisDrawingContext context)
    {
        var cx = X + Width / 2;

        context.Canvas.DrawLine(context.Stroke, new Point(cx, Y), new Point(cx, Low));
        context.Canvas.DrawLine(context.Stroke, new Point(X, Open), new Point(cx, Open));
        context.Canvas.DrawLine(context.Stroke, new Point(cx, Close), new Point(X + Width, Close));
    }
}
