using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;

namespace EtoFormsSample.Bars.Waterfall;

public class View : Panel
{
    private readonly CartesianChart cartesianChart;

    public View()
    {
        var vm = new ViewModelsSamples.Bars.Waterfall.ViewModel();

        var series = new ISeries[]
        {
            new RangeColumnSeries<RangeValue>
            {
                Name = "Cash flow",
                Values = vm.Steps,
                YToolTipLabelFormatter = vm.StepTooltipFormatter,
            },
        };

        cartesianChart = new CartesianChart
        {
            Series = series,
            XAxes = [new Axis { Labels = vm.StepNames }],
            YAxes = [new Axis { Name = "Balance", MinLimit = 0 }],
        };

        Content = cartesianChart;
    }
}
