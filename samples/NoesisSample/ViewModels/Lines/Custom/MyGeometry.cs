using LiveChartsCore.Drawing;
using LiveChartsCore.Noesis.Drawing;

namespace ViewModelsSamples.Lines.Custom;

public class MyGeometry : BoundedDrawnGeometry, IDrawnElement<NoesisDrawingContext>
{
    public void Draw(NoesisDrawingContext context)
    {
        var canvas = context.Canvas;

        canvas.DrawRectangle(context.Fill, context.Stroke, new global::Noesis.Rect(X, Y, Width, Height));
        canvas.DrawLine(context.Stroke ?? new global::Noesis.Pen { Brush = context.Fill, Thickness = 1 }, new global::Noesis.Point(X, Y), new global::Noesis.Point(X + Width, Y + Height));
        canvas.DrawLine(context.Stroke ?? new global::Noesis.Pen { Brush = context.Fill, Thickness = 1 }, new global::Noesis.Point(X + Width, Y), new global::Noesis.Point(X, Y + Height));
    }
}
