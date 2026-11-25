using Eto.Forms;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;
using ViewModelsSamples.General.MultiThreading;

namespace EtoFormsSample.General.MultiThreading;

public class View : Panel
{
    public View()
    {
        // the view model adds points from 10 background tasks,
        // it locks the Sync object every time it changes the values.
        var viewModel = new ViewModel();

        var cartesianChart = new CartesianChart
        {
            SyncContext = viewModel.Sync, // the chart locks the same object while it reads the values
            Series =
            [
                new LineSeries<int>
                {
                    Values = viewModel.Values,
                    GeometryFill = null,
                    GeometryStroke = null,
                    LineSmoothness = 0
                }
            ]
        };

        Content = cartesianChart;
    }
}
