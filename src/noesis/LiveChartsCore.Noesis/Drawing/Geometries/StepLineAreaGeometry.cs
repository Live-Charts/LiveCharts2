using LiveChartsCore.Drawing;
using LiveChartsCore.Drawing.Segments;
using Noesis;

namespace LiveChartsCore.Noesis.Drawing.Geometries;

/// <inheritdoc />
public class StepLineAreaGeometry : BaseVectorGeometry, IDrawnElement<NoesisDrawingContext>
{
    /// <inheritdoc />
    public void Draw(NoesisDrawingContext context)
    {
        if (Commands.First is null)
        {
            return;
        }

        var geometry = new StreamGeometry();
        var completed = new List<Segment>();

        using (var path = geometry.Open())
        {
            var first = Commands.First.Value;
            var closed = ClosingMethod == VectorClosingMethod.CloseToPivot;

            path.BeginFigure(new Point(first.Xj, closed ? Pivot : first.Yj), closed, closed);

            foreach (var segment in Commands)
            {
                segment.IsValid = true;
                path.LineTo(new Point(segment.Xj, segment.Yi), true, false);
                path.LineTo(new Point(segment.Xj, segment.Yj), true, false);
                IsValid &= segment.IsValid;

                if (segment.IsValid && segment.RemoveOnCompleted)
                {
                    completed.Add(segment);
                }
            }

            if (closed)
            {
                path.LineTo(new Point(Commands.Last!.Value.Xj, Pivot), true, false);
            }
        }

        context.Retain(geometry);
        context.Canvas.DrawGeometry(context.Fill, context.Stroke, geometry);

        foreach (var segment in completed)
        {
            Commands.Remove(segment);
            IsValid = false;
        }
    }
}
