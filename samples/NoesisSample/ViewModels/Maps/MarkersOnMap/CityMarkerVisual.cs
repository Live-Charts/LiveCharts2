using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Noesis.Drawing.Geometries;
using LiveChartsCore.Noesis.Painting;
using LiveChartsCore.VisualElements;

namespace ViewModelsSamples.Maps.MarkersOnMap;

public class CityMarkerVisual : Visual
{
    private const float Size = 14f;

    // GeoVisualElement writes the projected pixel onto this geometry, so do not set X or Y
    // in Measure below or the marker would ignore the coordinate it is anchored to.
    //
    // The translate centers the circle on the projected point; without it the geometry's
    // top-left corner would sit on the coordinate.
    //
    // Declared as IDrawnElement rather than CircleGeometry: net462 does not support covariant
    // returns, and Measure has no need for the derived type here.
    protected override IDrawnElement DrawnElement { get; } = new CircleGeometry
    {
        Width = Size,
        Height = Size,
        Fill = new SolidColorPaint(new LvcColor(255, 87, 51)), // OrangeRed
        Stroke = new SolidColorPaint(DrawingColors.White),
        // A geometry that carries its own paints is drawn with the thickness set here, the one on
        // the paint is ignored, so setting it there would silently draw a 1px hairline instead.
        StrokeThickness = 2,
        TranslateTransform = new(-Size / 2f, -Size / 2f),
    };

    protected override void Measure(Chart chart)
    { }
}
