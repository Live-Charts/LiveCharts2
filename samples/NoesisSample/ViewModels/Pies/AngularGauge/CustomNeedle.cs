using LiveChartsCore.Noesis.Drawing;
using LiveChartsCore.Noesis.Drawing.Geometries;

namespace ViewModelsSamples.Pies.AngularGauge;

public class CustomNeedle : NeedleGeometry
{
    public override void Draw(NoesisDrawingContext context)
    {

        context.Canvas.DrawRectangle(context.Fill, context.Stroke, new global::Noesis.Rect(X - Width * 0.5f, Y, Width, Radius));
    }
}
