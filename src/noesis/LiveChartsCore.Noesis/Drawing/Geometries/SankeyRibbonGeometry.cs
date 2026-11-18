using LiveChartsCore.Drawing;
using Noesis;

namespace LiveChartsCore.Noesis.Drawing.Geometries;

/// <inheritdoc />
public class SankeyRibbonGeometry : BaseSankeyRibbonGeometry, IDrawnElement<NoesisDrawingContext>
{
    /// <inheritdoc />
    public void Draw(NoesisDrawingContext context)
    {
        var middle = (SourceX + TargetX) / 2;
        var geometry = new StreamGeometry();

        using (var path = geometry.Open())
        {
            path.BeginFigure(new Point(SourceX, SourceY0), true, true);
            path.BezierTo(new Point(middle, SourceY0), new Point(middle, TargetY0), new Point(TargetX, TargetY0), true, false);
            path.LineTo(new Point(TargetX, TargetY1), true, false);
            path.BezierTo(new Point(middle, TargetY1), new Point(middle, SourceY1), new Point(SourceX, SourceY1), true, false);
        }

        context.Retain(geometry);
        context.Canvas.DrawGeometry(context.Fill, context.Stroke, geometry);
    }

    /// <inheritdoc />
    public override LvcSize Measure() => new(Math.Max(0, TargetX - SourceX), Math.Max(SourceY1, TargetY1) - Math.Min(SourceY0, TargetY0));
}
