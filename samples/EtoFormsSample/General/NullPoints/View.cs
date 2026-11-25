using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;
using ViewModelsSamples.General.NullPoints;

namespace EtoFormsSample.General.NullPoints;

public class View : Panel
{
    public View()
    {
        var viewModel = new ViewModel();

        var series = new ISeries[]
        {
            new LineSeries<double?> { Values = viewModel.Values1 },
            new LineSeries<double?> { Values = viewModel.Values2 },
            new LineSeries<ObservablePoint?> { Values = viewModel.Values3 }
        };

        var cartesianChart = new CartesianChart
        {
            Series = series,
        };

        Content = cartesianChart;
    }
}
