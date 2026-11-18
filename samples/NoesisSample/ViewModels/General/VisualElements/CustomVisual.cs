using LiveChartsCore;
using LiveChartsCore.Noesis.Painting;
using LiveChartsCore.VisualElements;
using LiveChartsCore.Drawing;

namespace ViewModelsSamples.General.VisualElements;

public class CustomVisual : Visual
{
    protected override CustomSkiaShape DrawnElement { get; } =
        new CustomSkiaShape { Stroke = new SolidColorPaint(DrawingColors.Red) };

    protected override void Measure(Chart chart)
    {
        DrawnElement.X = 250;
        DrawnElement.Y = 100;
        DrawnElement.Width = 40;
        DrawnElement.Height = 40;
    }
}
