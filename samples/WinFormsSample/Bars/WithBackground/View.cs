using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace WinFormsSample.Bars.WithBackground;

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

        var values1 = new double[] { 20, 50, 40, 20, 40, 30, 50, 20, 50, 40 };
        var values2 = new double[] { 3, 10, 5, 3, 7, 3, 8 };

        var series = new ISeries[]
        {
            new ColumnSeries<double>
            {
                Values = values1,
                Fill = new SolidColorPaint(new SKColor(180, 180, 180, 50)),
                IgnoresBarPosition = true,
                IsHoverable = false
            },
            new ColumnSeries<double>
            {
                Values = values2,
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
