using System;
using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;

namespace EtoFormsSample.Bars.Gantt;

public class View : Panel
{
    private readonly CartesianChart cartesianChart;

    public View()
    {
        // Eto is code-only — no XAML wrappers. Pull raw data + formatters from
        // the shared VM and build the chart objects in code.
        var vm = new ViewModelsSamples.Bars.Gantt.ViewModel();

        var series = new ISeries[]
        {
            new RangeRowSeries<RangeValue>
            {
                Name = "Project",
                Values = vm.Tasks,
                YToolTipLabelFormatter = vm.TaskTooltipFormatter,
            },
        };

        cartesianChart = new CartesianChart
        {
            Series = series,
            XAxes = [new DateTimeAxis(TimeSpan.FromDays(2), vm.DateFormatter) { Name = "Date" }],
            YAxes = [new Axis { Labels = vm.TaskNames }],
        };

        Content = cartesianChart;
    }
}
