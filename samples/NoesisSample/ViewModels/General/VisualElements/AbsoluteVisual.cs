using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Noesis.Drawing.Geometries;
using LiveChartsCore.Noesis.Drawing.Layouts;
using LiveChartsCore.Noesis.Painting;
using LiveChartsCore.VisualElements;

namespace ViewModelsSamples.General.VisualElements;

public class AbsoluteVisual : Visual
{
    protected override AbsoluteLayout DrawnElement { get; } =
        new AbsoluteLayout
        {
            // X and Y coordinates are relative to the parent
            Children = [
                new RectangleGeometry
                {
                    X = 0,
                    Y = 0,
                    Width = 40,
                    Height = 40,
                    Fill = new SolidColorPaint(DrawingColors.Gray)
                },
                new LabelGeometry
                {
                    X = 10,
                    Y = 10,
                    Text = "Hello",
                    TextSize = 10,
                    HorizontalAlign = Align.Start,
                    VerticalAlign = Align.Start,
                    Paint = new SolidColorPaint(DrawingColors.Black)
                }
            ]
        };

    protected override void Measure(Chart chart)
    {
        DrawnElement.X = 300;
        DrawnElement.Y = 100;
        DrawnElement.Width = 40;
        DrawnElement.Height = 40;
    }
}
