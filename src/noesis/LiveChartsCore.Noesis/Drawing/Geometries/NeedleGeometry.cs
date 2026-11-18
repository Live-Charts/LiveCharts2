using LiveChartsCore.Drawing;
using Noesis;

namespace LiveChartsCore.Noesis.Drawing.Geometries;

/// <inheritdoc />
public class NeedleGeometry : BaseNeedleGeometry, IDrawnElement<NoesisDrawingContext>
{
    /// <summary>Creates a gauge needle.</summary>
    public NeedleGeometry()
    {
        TransformOrigin = new LvcPoint(0, 0);
    }

    /// <inheritdoc />
    public virtual void Draw(NoesisDrawingContext context)
    {
        var halfWidth = Width / 2;
        var geometry = new StreamGeometry();

        using (var path = geometry.Open())
        {
            path.BeginFigure(new Point(X, Y + Radius), true, true);
            path.LineTo(new Point(X - halfWidth, Y), true, false);
            path.LineTo(new Point(X + halfWidth, Y), true, false);
        }

        context.Retain(geometry);
        context.Canvas.DrawGeometry(context.Fill, context.Stroke, geometry);
        context.Canvas.DrawEllipse(context.Fill, context.Stroke, new Point(X, Y), halfWidth, halfWidth);
    }
}
