using LiveChartsCore.Drawing;
using LiveChartsCore.Drawing.Segments;
using Noesis;

namespace LiveChartsCore.Noesis.Drawing.Geometries;

/// <inheritdoc />
public class CubicBezierBandGeometry : BaseVectorGeometry, IBandVectorGeometry, IDrawnElement<NoesisDrawingContext>
{
    /// <inheritdoc />
    public LinkedList<Segment> LowCommands { get; } = new();

    /// <inheritdoc />
    public void Draw(NoesisDrawingContext context)
    {
        if (Commands.First is null || LowCommands.Last is null)
        {
            return;
        }

        var geometry = new StreamGeometry();
        var completed = new List<Segment>();

        using (var path = geometry.Open())
        {
            var first = Commands.First.Value;

            path.BeginFigure(new Point(first.Xi, first.Yi), true, true);

            foreach (var segment in Commands)
            {
                segment.IsValid = true;

                var cubic = (CubicBezierSegment)segment;

                path.BezierTo(new Point(cubic.Xi, cubic.Yi), new Point(cubic.Xm, cubic.Ym), new Point(cubic.Xj, cubic.Yj), true, false);
                IsValid &= segment.IsValid;

                if (segment.IsValid && segment.RemoveOnCompleted)
                {
                    completed.Add(segment);
                }
            }

            path.LineTo(new Point(LowCommands.Last.Value.Xj, LowCommands.Last.Value.Yj), true, false);

            for (var node = LowCommands.Last; node is not null; node = node.Previous)
            {
                var segment = (CubicBezierSegment)node.Value;

                segment.IsValid = true;
                path.BezierTo(new Point(segment.Xm, segment.Ym), new Point(segment.Xi, segment.Yi), new Point(node.Previous?.Value.Xj ?? segment.Xi, node.Previous?.Value.Yj ?? segment.Yi), true, false);
                IsValid &= segment.IsValid;

                if (segment.IsValid && segment.RemoveOnCompleted)
                {
                    completed.Add(segment);
                }
            }
        }

        context.Retain(geometry);
        context.Canvas.DrawGeometry(context.Fill, context.Stroke, geometry);

        foreach (var segment in completed)
        {
            Commands.Remove(segment);
            LowCommands.Remove(segment);
            IsValid = false;
        }
    }
}
