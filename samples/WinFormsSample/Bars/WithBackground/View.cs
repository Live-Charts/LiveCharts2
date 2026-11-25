using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using ViewModelsSamples.Bars.WithBackground;

namespace WinFormsSample.Bars.WithBackground;

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

        var viewModel = new ViewModel();

        var series = new ISeries[]
        {
            new ColumnSeries<double>
            {
                Values = viewModel.Values1,
                Fill = new SolidColorPaint(new SKColor(180, 180, 180, 50)),
                IgnoresBarPosition = true,
                IsHoverable = false
            },
            new ColumnSeries<double>
            {
                Values = viewModel.Values2,
                IgnoresBarPosition = true
            }
        };

        var yAxis = new Axis
        {
            MinLimit = 0,
            MaxLimit = 10
        };

        var cartesianChart = new CartesianChart
        {
            Series = series,
            YAxes = [yAxis],
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(50, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(cartesianChart);
    }
}
