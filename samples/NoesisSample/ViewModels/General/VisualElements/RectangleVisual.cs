using LiveChartsCore;
using LiveChartsCore.Noesis.Drawing.Geometries;
using LiveChartsCore.Noesis.Painting;
using LiveChartsCore.VisualElements;
using LiveChartsCore.Drawing;

namespace ViewModelsSamples.General.VisualElements;

public class RectangleVisual : Visual
{
    protected override RectangleGeometry DrawnElement { get; } =
        new RectangleGeometry { Fill = new SolidColorPaint(DrawingColors.Red) };

    protected override void Measure(Chart chart)
    {
        DrawnElement.X = 100;
        DrawnElement.Y = 100;
        DrawnElement.Width = 40;
        DrawnElement.Height = 40;
    }
}
