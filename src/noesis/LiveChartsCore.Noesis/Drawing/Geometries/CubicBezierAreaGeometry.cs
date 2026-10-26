using LiveChartsCore.Drawing;
using LiveChartsCore.Drawing.Segments;
using Noesis;

namespace LiveChartsCore.Noesis.Drawing.Geometries;

/// <inheritdoc />
public class CubicBezierAreaGeometry : BaseVectorGeometry, IDrawnElement<NoesisDrawingContext>
{
    private const int SegmentsPerPath = 128;
    private readonly List<Segment> _completed = new();
    private readonly List<StreamGeometry> _paths = new();

    /// <inheritdoc />
    public void Draw(NoesisDrawingContext context)
    {
        var node = Commands.First;
        var pathIndex = 0;
        var maxComplexity = context.Fill is not null ? 2048 : SegmentsPerPath;
        var previous = node is null ? default : new Point(node.Value.Xi, node.Value.Yi);

        while (node is not null)
        {
            var geometry = GetPath(pathIndex++);
            var toPivot = ClosingMethod == VectorClosingMethod.CloseToPivot;
            var closed = ClosingMethod != VectorClosingMethod.NotClosed;
            var count = 0;

            using (var path = geometry.Open())
            {
                path.BeginFigure(new Point(previous.X, toPivot ? Pivot : previous.Y), closed, closed);

                if (toPivot)
                {
                    path.LineTo(previous, true, false);
                }

                while (node is not null && (count < maxComplexity || ClosingMethod == VectorClosingMethod.CloseToStart))
                {
                    var segment = (CubicBezierSegment)node.Value;

                    segment.IsValid = true;

                    var firstControl = new Point(segment.Xi, segment.Yi);
                    var secondControl = new Point(segment.Xm, segment.Ym);
                    var end = new Point(segment.Xj, segment.Yj);

                    if (IsStraight(previous, firstControl, secondControl, end))
                    {
                        path.LineTo(end, true, false);
                    }
                    else
                    {
                        path.BezierTo(firstControl, secondControl, end, true, false);
                        count += 15;
                    }

                    previous = end;
                    IsValid &= segment.IsValid;

                    if (segment.IsValid && segment.RemoveOnCompleted)
                    {
                        _completed.Add(segment);
                    }

                    node = node.Next;
                    count++;
                }

                if (toPivot)
                {
                    path.LineTo(new Point(previous.X, Pivot), true, false);
                }
            }

            context.Retain(geometry);
            context.Canvas.DrawGeometry(context.Fill, context.Stroke, geometry);
        }

        foreach (var segment in _completed)
        {
            Commands.Remove(segment);
            IsValid = false;
        }

        _completed.Clear();
    }

    private static bool IsStraight(Point start, Point firstControl, Point secondControl, Point end)
    {
        var dx = end.X - start.X;
        var dy = end.Y - start.Y;
        var firstCross = (firstControl.X - start.X) * dy - (firstControl.Y - start.Y) * dx;
        var secondCross = (secondControl.X - start.X) * dy - (secondControl.Y - start.Y) * dx;
        var lengthSquared = dx * dx + dy * dy;

        if (lengthSquared == 0)
        {
            return firstControl == start && secondControl == start;
        }

        var firstAlong = (firstControl.X - start.X) * dx + (firstControl.Y - start.Y) * dy;
        var secondAlong = (secondControl.X - start.X) * dx + (secondControl.Y - start.Y) * dy;
        var tolerance = lengthSquared * 0.000001f;

        return firstCross * firstCross <= tolerance && secondCross * secondCross <= tolerance &&
            firstAlong >= 0 && firstAlong <= lengthSquared &&
            secondAlong >= 0 && secondAlong <= lengthSquared;
    }

    private StreamGeometry GetPath(int index)
    {
        if (index == _paths.Count)
        {
            _paths.Add(new StreamGeometry());
        }

        return _paths[index];
    }

    internal override void OnDisposed()
    {
        foreach (var path in _paths)
        {
            path.Dispose();
        }

        _paths.Clear();

        base.OnDisposed();
    }
}
