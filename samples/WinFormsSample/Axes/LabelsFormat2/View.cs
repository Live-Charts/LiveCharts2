using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;
using SkiaSharp;

#pragma warning disable IDE1006 // Naming Styles

namespace WinFormsSample.Axes.LabelsFormat2;

public partial class View : UserControl
{
    private readonly CartesianChart cartesianChart;

    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

        var values1 = new double[] { 426, 583, 104 };
        var values2 = new double[] { 200, 558, 458 };
        var labels = new string[] { "王", "赵", "张" };
        static string Labeler(double value) => value.ToString("C2");

        var series = new ISeries[]
        {
            new ColumnSeries<double> { Values = values1 },
            new ColumnSeries<double> { Values = values2, Fill = null }
        };

        var xAxis = new Axis
        {
            Name = "姓名",
            Labels = labels
        };

        var yAxis = new Axis
        {
            Name = "销售额",
            NamePadding = new LiveChartsCore.Drawing.Padding(0, 15),
            Labeler = Labeler,
            LabelsPaint = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(SKColors.DarkGreen)
            // You can set FontFamily, FontWeight, etc. if needed for Chinese support
        };

        cartesianChart = new CartesianChart
        {
            Series = series,
            XAxes = [xAxis],
            YAxes = [yAxis],
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(50, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(cartesianChart);
    }
}
