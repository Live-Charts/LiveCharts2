using LiveChartsCore.Drawing;
using LiveChartsCore.Noesis.Drawing;

namespace ViewModelsSamples.Scatter.Custom;

public class MyGeometry : BoundedDrawnGeometry, IDrawnElement<NoesisDrawingContext>
{
    public void Draw(NoesisDrawingContext context)
    {
        var canvas = context.Canvas;
        var y = Y;

        while (y < Y + Height)
        {
            canvas.DrawLine(context.Stroke ?? new global::Noesis.Pen { Brush = context.Fill, Thickness = 1 }, new global::Noesis.Point(X, y), new global::Noesis.Point(X + Width, y));
            y += 10;
        }
    }
}
