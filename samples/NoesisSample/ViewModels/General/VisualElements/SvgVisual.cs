using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Noesis.Drawing.Geometries;
using LiveChartsCore.Noesis.Painting;
using LiveChartsCore.VisualElements;

namespace ViewModelsSamples.General.VisualElements;

public class SvgVisual : Visual
{
    protected override VariableSVGPathGeometry DrawnElement { get; } =
        new VariableSVGPathGeometry
        {
            Stroke = new SolidColorPaint(DrawingColors.Blue),
            Path = global::Noesis.Geometry.Parse(SVGPoints.Star)
        };

    protected override void Measure(Chart chart)
    {
        DrawnElement.X = 200;
        DrawnElement.Y = 100;
        DrawnElement.Width = 40;
        DrawnElement.Height = 40;
    }
}
