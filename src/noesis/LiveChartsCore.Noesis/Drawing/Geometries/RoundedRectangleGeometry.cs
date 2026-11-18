using LiveChartsCore.Drawing;
using Noesis;

namespace LiveChartsCore.Noesis.Drawing.Geometries;

/// <inheritdoc />
public class RoundedRectangleGeometry : BaseRoundedRectangleGeometry, IDrawnElement<NoesisDrawingContext>
{
    /// <inheritdoc />
    public virtual void Draw(NoesisDrawingContext context)
    {
        var bounds = new Rect(
            Math.Min(X, X + Width),
            Math.Min(Y, Y + Height),
            Math.Abs(Width),
            Math.Abs(Height));

        context.Canvas.DrawRoundedRectangle(context.Fill, context.Stroke, bounds, BorderRadius.X, BorderRadius.Y);
    }
}
