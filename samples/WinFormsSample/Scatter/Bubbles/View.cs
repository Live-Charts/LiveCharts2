using System.Windows.Forms;
using LiveChartsCore.SkiaSharpView.WinForms;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using ViewModelsSamples.Scatter.Bubbles;

namespace WinFormsSample.Scatter.Bubbles;

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

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
            Series = series,
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(50, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(cartesianChart);
    }
}
