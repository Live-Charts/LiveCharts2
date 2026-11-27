using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace EtoFormsSample.Axes.CustomSeparatorsInterval;

public class View : Panel
{
    private readonly CartesianChart cartesianChart;

    public View()
    {
        var values = new int[] { 10, 55, 45, 68, 60, 70, 75, 78 };
        var customSeparators = new double[] { 0, 10, 25, 50, 100 };

        var series = new ISeries[]
        {
            new LineSeries<int> { Values = values }
        };

        var yAxis = new Axis
        {
            CustomSeparators = customSeparators,
            MinLimit = 0,
            MaxLimit = 100,
            SeparatorsPaint = new SolidColorPaint(new SKColor(180, 180, 180, 100))
        };

        cartesianChart = new CartesianChart
        {
            Series = series,
            YAxes = [yAxis],
        };

        Content = cartesianChart;
    }
}
