using LiveChartsCore.Drawing;
using LiveChartsCore.Noesis.Drawing;

namespace ViewModelsSamples.General.VisualElements;

public class CustomSkiaShape : BoundedDrawnGeometry, IDrawnElement<NoesisDrawingContext>
{
    public void Draw(NoesisDrawingContext context)
    {
        var canvas = context.Canvas;

        var x = X + Width / 2f;
        var y = Y + Height / 2f;
        var r = (float)Width / 2f;

        canvas.DrawEllipse(context.Fill, context.Stroke, new global::Noesis.Point(x, y), r, r);
        canvas.DrawEllipse(context.Fill, context.Stroke, new global::Noesis.Point(x, y), r * 0.75f, r * 0.75f);
        canvas.DrawEllipse(context.Fill, context.Stroke, new global::Noesis.Point(x, y), r * 0.5f, r * 0.5f);
    }
}
