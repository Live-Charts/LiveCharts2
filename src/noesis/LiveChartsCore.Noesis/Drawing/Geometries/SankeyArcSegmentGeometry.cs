using LiveChartsCore.Drawing;
using Noesis;

namespace LiveChartsCore.Noesis.Drawing.Geometries;

/// <inheritdoc />
public class SankeyArcSegmentGeometry : BaseSankeyArcSegmentGeometry, IDrawnElement<NoesisDrawingContext>
{
    /// <inheritdoc />
    public void Draw(NoesisDrawingContext context)
    {
        var sweep = Math.Clamp(SweepAngle, -359.999f, 359.999f);
        var geometry = new StreamGeometry();
        var direction = sweep >= 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise;
        var reverse = sweep >= 0 ? SweepDirection.Counterclockwise : SweepDirection.Clockwise;

        using (var path = geometry.Open())
        {
            path.BeginFigure(PointAt(InnerRadius, StartAngle), true, true);
            path.LineTo(PointAt(OuterRadius, StartAngle), true, false);
            path.ArcTo(PointAt(OuterRadius, StartAngle + sweep), new Size(OuterRadius, OuterRadius), 0, Math.Abs(sweep) > 180, direction, true, false);
            path.LineTo(PointAt(InnerRadius, StartAngle + sweep), true, false);
            path.ArcTo(PointAt(InnerRadius, StartAngle), new Size(InnerRadius, InnerRadius), 0, Math.Abs(sweep) > 180, reverse, true, false);
        }

        context.Retain(geometry);
        context.Canvas.DrawGeometry(context.Fill, context.Stroke, geometry);
    }

    /// <inheritdoc />
    public override LvcSize Measure() => new(OuterRadius * 2, OuterRadius * 2);

    private Point PointAt(float radius, float degrees)
    {
        var angle = degrees * Math.PI / 180;

        return new Point(CenterX + radius * (float)Math.Cos(angle), CenterY + radius * (float)Math.Sin(angle));
    }
}
