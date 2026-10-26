using LiveChartsCore.Drawing;
using Noesis;

namespace LiveChartsCore.Noesis.Drawing.Geometries;

/// <inheritdoc />
public class DoughnutGeometry : BaseDoughnutGeometry, IDrawnElement<NoesisDrawingContext>
{
    /// <inheritdoc />
    public void Draw(NoesisDrawingContext context)
    {
        var sweep = Math.Clamp(SweepAngle, -359.999f, 359.999f);
        var radius = Width / 2;

        if (radius <= 0 || Math.Abs(sweep) < 0.001f)
        {
            return;
        }

        var geometry = new StreamGeometry();

        var middle = (StartAngle + sweep / 2) * Math.PI / 180;
        var center = new Point(CenterX + PushOut * (float)Math.Cos(middle), CenterY + PushOut * (float)Math.Sin(middle));
        var outerStart = PointAt(center, radius, StartAngle);
        var outerEnd = PointAt(center, radius, StartAngle + sweep);
        var innerEnd = PointAt(center, InnerRadius, StartAngle + sweep);
        var innerStart = PointAt(center, InnerRadius, StartAngle);
        var direction = sweep >= 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise;
        var reverse = sweep >= 0 ? SweepDirection.Counterclockwise : SweepDirection.Clockwise;

        using (var path = geometry.Open())
        {
            path.BeginFigure(innerStart, true, true);
            path.LineTo(outerStart, true, false);
            path.ArcTo(outerEnd, new Size(radius, radius), 0, Math.Abs(sweep) > 180, direction, true, false);
            path.LineTo(innerEnd, true, false);

            if (InnerRadius > 0)
            {
                path.ArcTo(innerStart, new Size(InnerRadius, InnerRadius), 0, Math.Abs(sweep) > 180, reverse, true, false);
            }
        }

        context.Retain(geometry);
        context.Canvas.DrawGeometry(context.Fill, context.Stroke, geometry);
    }

    private static Point PointAt(Point center, float radius, float degrees)
    {
        var angle = degrees * Math.PI / 180;

        return new Point(center.X + radius * (float)Math.Cos(angle), center.Y + radius * (float)Math.Sin(angle));
    }
}
