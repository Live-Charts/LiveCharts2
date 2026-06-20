// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Drawing;
using LiveChartsCore.Generators;

namespace LiveChartsCore.Console.Drawing.Geometries;

/// <summary>
/// Rectangle with a per-instance color motion property — used by HeatSeries where every cell
/// has its own gradient-mapped color, set on the geometry rather than on the active paint.
/// The Color property is generated as a motion property by LiveChartsGenerators so it tweens
/// between heat values smoothly when data changes.
/// </summary>
public partial class ColoredRectangleGeometry
    : BoundedDrawnGeometry, IColoredGeometry, IDrawnElement<ConsoleDrawingContext>
{
    public ColoredRectangleGeometry()
    {
        _ColorMotionProperty = new(LvcColor.Empty);
    }

    [MotionProperty]
    public partial LvcColor Color { get; set; }

    public void Draw(ConsoleDrawingContext context)
    {
        var c = Color;
        if (c.A == 0) return;
        context.Surface.FillRect((int)X, (int)Y, (int)Width, (int)Height, c);
    }
}
