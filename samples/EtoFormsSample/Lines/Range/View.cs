using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;

namespace EtoFormsSample.Lines.Range;

public class View : Panel
{
    private readonly CartesianChart cartesianChart;

    public View()
    {
        // Eto is code-only — no XAML wrappers. Pull raw data + formatters from
        // the shared VM and build the chart objects in code.
        var vm = new ViewModelsSamples.Lines.Range.ViewModel();

        var series = new ISeries[]
        {
            new RangeLineSeries<RangeValue>
            {
                Name = "Temperature",
                Values = vm.Temperatures,
                YToolTipLabelFormatter = vm.TempTooltipFormatter,
            },
        };

        cartesianChart = new CartesianChart
        {
            Series = series,
            XAxes = [new Axis { Labels = vm.Months }],
            YAxes = [new Axis { Labeler = vm.TempLabeler }],
        };

        Content = cartesianChart;
    }
}
