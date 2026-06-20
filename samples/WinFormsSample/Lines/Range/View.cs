using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;

namespace WinFormsSample.Lines.Range;

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

        // WinForms is code-only — no XAML wrappers. Pull raw data + formatters
        // from the shared VM and build the chart objects in code.
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

        var cartesianChart = new CartesianChart
        {
            Series = series,
            XAxes = [new Axis { Labels = vm.Months }],
            YAxes = [new Axis { Labeler = vm.TempLabeler }],
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(50, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(cartesianChart);
    }
}
