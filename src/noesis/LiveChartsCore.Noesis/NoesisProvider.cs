using LiveChartsCore.Drawing;
using LiveChartsCore.Geo;
using LiveChartsCore.Kernel.Providers;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Motion;
using LiveChartsCore.Noesis.Drawing.Geometries;
using LiveChartsCore.Noesis.Painting;
using LiveChartsCore.Painting;
using LiveChartsCore.Themes;

namespace LiveChartsCore.Noesis;

/// <inheritdoc />
public sealed class NoesisProvider : ChartEngine
{
    /// <inheritdoc />
    public override ICartesianAxis GetDefaultCartesianAxis() => new Axis();

    /// <inheritdoc />
    public override IPolarAxis GetDefaultPolarAxis() => new PolarAxis();

    /// <inheritdoc />
    public override IMapFactory GetDefaultMapFactory() => new MapFactory();

    /// <inheritdoc />
    public override Paint GetSolidColorPaint(LvcColor color) => new SolidColorPaint(color);

    /// <inheritdoc />
    public override BoundedDrawnGeometry InitializeZoommingSection(CoreMotionCanvas canvas)
    {
        var rectangle = new RectangleGeometry();
        var paint = new SolidColorPaint(LvcColor.FromArgb(50, ColorPalletes.MaterialDesign500[0]));

        paint.AddGeometryToPaintTask(canvas, rectangle);
        canvas.AddDrawableTask(paint, PaintStyle.Fill);

        return rectangle;
    }
}
