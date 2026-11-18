using LiveChartsCore.Drawing;
using Noesis;

namespace LiveChartsCore.Noesis.Drawing.Geometries;

/// <inheritdoc />
public class SankeyChordRibbonGeometry : BaseSankeyChordRibbonGeometry, IDrawnElement<NoesisDrawingContext>
{
    /// <inheritdoc />
    public void Draw(NoesisDrawingContext context)
    {
        var radius = (float)Math.Sqrt(Math.Pow(SourceP0X - CenterX, 2) + Math.Pow(SourceP0Y - CenterY, 2));
        var center = new Point(CenterX, CenterY);
        var geometry = new StreamGeometry();

        using (var path = geometry.Open())
        {
            path.BeginFigure(new Point(SourceP0X, SourceP0Y), true, true);
            path.ArcTo(new Point(SourceP1X, SourceP1Y), new Size(radius, radius), 0, false, SweepDirection.Clockwise, true, false);
            path.BezierTo(center, center, new Point(TargetP0X, TargetP0Y), true, false);
            path.ArcTo(new Point(TargetP1X, TargetP1Y), new Size(radius, radius), 0, false, SweepDirection.Clockwise, true, false);
            path.BezierTo(center, center, new Point(SourceP0X, SourceP0Y), true, false);
        }

        context.Retain(geometry);
        context.Canvas.DrawGeometry(context.Fill, context.Stroke, geometry);
    }

    /// <inheritdoc />
    public override LvcSize Measure() => new();
}
