using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;

namespace WinFormsSample.Bars.Waterfall;

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

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

        var cartesianChart = new CartesianChart
        {
            Series = series,
            XAxes = [new Axis { Labels = vm.StepNames }],
            YAxes = [new Axis { Name = "Balance", MinLimit = 0 }],
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(50, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(cartesianChart);
    }
}
