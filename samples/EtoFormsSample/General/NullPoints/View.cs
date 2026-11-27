using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;

namespace EtoFormsSample.General.NullPoints;

public class View : Panel
{
    public View()
    {
        var values1 = new double?[] { 5, 4, null, 3, 2, 6, 5, 6, 2 };
        var values2 = new double?[] { 2, 6, 5, 3, null, 5, 2, 4, null };
        var values3 = new ObservablePoint?[]
        {
            new() { X = 0, Y = 1 },
            new() { X = 1, Y = 4 },
            null,
            new() { X = 4, Y = 5 },
            new() { X = 6, Y = 1 },
            new() { X = 8, Y = 6 },
        };

        var series = new ISeries[]
        {
            new LineSeries<double?> { Values = values1 },
            new LineSeries<double?> { Values = values2 },
            new LineSeries<ObservablePoint?> { Values = values3 }
        };

        var cartesianChart = new CartesianChart
        {
            Series = series,
        };

        Content = cartesianChart;
    }
}
