using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;
using ViewModelsSamples.General.TemplatedLegends;

namespace EtoFormsSample.General.TemplatedLegends;

public class View : Panel
{
    private readonly CartesianChart cartesianChart;

    public View()
    {
        var series = new ISeries[]
        {
            new ColumnSeries<double> { Values = [2, 1, 3, 5, 3, 4, 6], Name = "Roger" },
            new ColumnSeries<double> { Values = [1, 3, 2, 7, 3, 5, 2], Name = "Susan" }
        };

        cartesianChart = new CartesianChart
        {
            Series = series,
            LegendPosition = LiveChartsCore.Measure.LegendPosition.Right,
            Legend = new CustomLegend()
        };

        Content = cartesianChart;
    }
}
