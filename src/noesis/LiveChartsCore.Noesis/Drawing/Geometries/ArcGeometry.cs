using LiveChartsCore.Drawing;
using Noesis;

namespace LiveChartsCore.Noesis.Drawing.Geometries;

/// <inheritdoc />
public class ArcGeometry : BaseArcGeometry, IDrawnElement<NoesisDrawingContext>
{
    /// <inheritdoc />
    public void Draw(NoesisDrawingContext context)
    {
        var radius = Width / 2;
        var start = StartAngle * Math.PI / 180;
        var sweep = Math.Clamp(SweepAngle, -359.999f, 359.999f);
        var end = (StartAngle + sweep) * Math.PI / 180;
        var geometry = new StreamGeometry();

        using (var path = geometry.Open())
        {
            path.BeginFigure(new Point(CenterX + radius * (float)Math.Cos(start), CenterY + radius * (float)Math.Sin(start)), false, false);
            path.ArcTo(new Point(CenterX + radius * (float)Math.Cos(end), CenterY + radius * (float)Math.Sin(end)), new Size(radius, radius), 0, Math.Abs(sweep) > 180, sweep >= 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise, true, false);
        }

        context.Retain(geometry);
        context.Canvas.DrawGeometry(context.Fill, context.Stroke, geometry);
    }
}
