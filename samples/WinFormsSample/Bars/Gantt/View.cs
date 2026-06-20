using System;
using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;

namespace WinFormsSample.Bars.Gantt;

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

        // WinForms is code-only — no XAML wrappers. Pull raw data + formatters
        // from the shared VM and build the chart objects in code.
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

        var cartesianChart = new CartesianChart
        {
            Series = series,
            XAxes = [new DateTimeAxis(TimeSpan.FromDays(2), vm.DateFormatter) { Name = "Date" }],
            YAxes = [new Axis { Labels = vm.TaskNames }],
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(50, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(cartesianChart);
    }
}
