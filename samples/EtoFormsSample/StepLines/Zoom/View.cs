using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;
using ViewModelsSamples.StepLines.Zoom;

namespace EtoFormsSample.StepLines.Zoom;

public class View : Panel
{
    public View()
    {
        var viewModel = new ViewModel();

        var series = new ISeries[]
        {
            new StepLineSeries<int>
            {
                Values = viewModel.Values
            }
        };

        var cartesianChart = new CartesianChart
        {
            Series = series,
            ZoomMode = LiveChartsCore.Measure.ZoomAndPanMode.X
        };

        Content = cartesianChart;
    }
}
