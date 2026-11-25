using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;
using ViewModelsSamples.Scatter.Bubbles;

namespace EtoFormsSample.Scatter.Bubbles;

public class View : Panel
{
    public View()
    {
        var viewModel = new ViewModel();

        var series = new ISeries[]
        {
            new ScatterSeries<WeightedPoint>
            {
                Values = viewModel.Values1,
                GeometrySize = 100,
                MinGeometrySize = 5
            },
            new ScatterSeries<WeightedPoint>
            {
                Values = viewModel.Values2,
                GeometrySize = 100,
                MinGeometrySize = 5,
                StackGroup = 1
            },
            new ScatterSeries<WeightedPoint>
            {
                Values = viewModel.Values3,
                GeometrySize = 100,
                MinGeometrySize = 5,
                StackGroup = 1
            }
        };

        var cartesianChart = new CartesianChart
        {
            Series = series
        };

        Content = cartesianChart;
    }
}
