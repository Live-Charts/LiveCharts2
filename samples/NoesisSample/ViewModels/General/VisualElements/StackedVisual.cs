using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Noesis.Drawing.Geometries;
using LiveChartsCore.Noesis.Drawing.Layouts;
using LiveChartsCore.Noesis.Painting;
using LiveChartsCore.VisualElements;

namespace ViewModelsSamples.General.VisualElements;

public class StackedVisual : Visual
{
    protected override StackLayout DrawnElement { get; } =
        new StackLayout
        {
            // X and Y coordinates are relative to the parent
            Orientation = ContainerOrientation.Vertical,
            Children = [
                new CircleGeometry
                {
                    Width = 40,
                    Height = 40,
                    Fill = new SolidColorPaint(DrawingColors.CadetBlue)
                },
                new RectangleGeometry
                {
                    Width = 40,
                    Height = 40,
                    Fill = new SolidColorPaint(DrawingColors.DeepSkyBlue)
                },
                new DiamondGeometry
                {
                    Width = 40,
                    Height = 40,
                    Fill = new SolidColorPaint(DrawingColors.LightSteelBlue)
                }
            ]
        };

    protected override void Measure(Chart chart)
    {
        DrawnElement.X = 350;
        DrawnElement.Y = 100;
    }
}
