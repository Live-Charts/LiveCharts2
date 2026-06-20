// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Console.Drawing.Geometries;
using LiveChartsCore.Console.Painting;
using LiveChartsCore.Drawing;
using LiveChartsCore.Geo;
using LiveChartsCore.Kernel.Providers;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Motion;
using LiveChartsCore.Painting;

namespace LiveChartsCore.Console;

public class ConsoleProvider : ChartEngine
{
    public override IMapFactory GetDefaultMapFactory() => throw new NotImplementedException();

    public override ICartesianAxis GetDefaultCartesianAxis() => new Axis();

    public override IPolarAxis GetDefaultPolarAxis() => new PolarAxis();

    public override Paint GetSolidColorPaint(LvcColor color) => new SolidColorPaint(color);

    public override BoundedDrawnGeometry InitializeZoommingSection(CoreMotionCanvas canvas)
    {
        var rectangle = new RectangleGeometry();
        var paint = new SolidColorPaint
        {
            PaintStyle = PaintStyle.Fill,
            Color = new(33, 150, 243, 50),
            ZIndex = int.MaxValue
        };
        paint.AddGeometryToPaintTask(canvas, rectangle);
        canvas.AddDrawableTask(paint);
        return rectangle;
    }
}
