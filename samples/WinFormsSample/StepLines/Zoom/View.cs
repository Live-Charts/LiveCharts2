using System.Windows.Forms;
using LiveChartsCore.SkiaSharpView.WinForms;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using ViewModelsSamples.StepLines.Zoom;

namespace WinFormsSample.StepLines.Zoom;

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

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
            ZoomMode = LiveChartsCore.Measure.ZoomAndPanMode.X,
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(50, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(cartesianChart);
    }
}
