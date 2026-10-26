using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Noesis.Drawing.Geometries;
using LiveChartsCore.Noesis.Painting;
using LiveChartsCore.VisualElements;

namespace ViewModelsSamples.General.VisualElements;

public class ScaledRectangleVisual : Visual
{
    protected override RectangleGeometry DrawnElement { get; } =
        new RectangleGeometry { Fill = new SolidColorPaint(DrawingColors.Red) };

    protected override void Measure(Chart chart)
    {
        var cartesianChart = (ICartesianChartView)chart.View;

        // use the ScaleDataToPixels function to scale data.
        var locationInDataScale = new LvcPointD(5, 5);
        var locationInPixels = cartesianChart.ScaleDataToPixels(locationInDataScale);

        DrawnElement.X = (float)locationInPixels.X;
        DrawnElement.Y = (float)locationInPixels.Y;
        DrawnElement.Width = 40;
        DrawnElement.Height = 40;
    }
}
