using System.Windows.Forms;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;
using ViewModelsSamples.General.MultiThreading;

namespace WinFormsSample.General.MultiThreading;

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(600, 400);

        // the view model adds points from 10 background tasks,
        // it locks the Sync object every time it changes the values.
        var viewModel = new ViewModel();

        var cartesianChart = new CartesianChart
        {
            SyncContext = viewModel.Sync, // the chart locks the same object while it reads the values
            Series = [
                new LineSeries<int>
                {
                    Values = viewModel.Values,
                    GeometryFill = null,
                    GeometryStroke = null,
                    LineSmoothness = 0
                }
            ],
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(600, 400),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(cartesianChart);
    }
}
