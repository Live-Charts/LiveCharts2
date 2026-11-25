using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;
using ViewModelsSamples.General.NullPoints;

namespace WinFormsSample.General.NullPoints;

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(600, 400);

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
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(600, 400),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(cartesianChart);
    }
}
