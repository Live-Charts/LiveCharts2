using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Noesis.Drawing.Geometries;
using LiveChartsCore.Noesis.Drawing.Layouts;
using LiveChartsCore.Noesis.Painting;
using LiveChartsCore.VisualElements;

namespace ViewModelsSamples.General.VisualElements;

public class ContainerVisual : Visual
{
    private readonly Container _container;

    public ContainerVisual()
    {
        _container = new Container
        {
            Content = new LabelGeometry
            {
                Text = "Hello",
                TextSize = 20,
                Padding = new(10),
                Paint = new SolidColorPaint(DrawingColors.Black),
                VerticalAlign = Align.Start,
                HorizontalAlign = Align.Start
            }
        };

        _container.Geometry.Stroke = new SolidColorPaint(DrawingColors.Black, 3);
        _container.Geometry.Fill = new SolidColorPaint(DrawingColors.LightGray);
    }

    protected override Container DrawnElement => _container;

    protected override void Measure(Chart chart)
    {
        DrawnElement.X = 500;
        DrawnElement.Y = 100;
    }
}
