using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WinForms;
using SkiaSharp;

#pragma warning disable IDE1006 // Naming Styles

namespace WinFormsSample.Axes.NamedLabels;

public partial class View : UserControl
{
    private readonly CartesianChart cartesianChart;

    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

        var values1 = new int[] { 200, 558, 458, 249 };
        var values2 = new int[] { 300, 450, 400, 280 };
        var labels = new string[] { "Anne", "Johnny", "Zac", "Rosa" };
        static string Labeler(double value) => value.ToString("C");

        var series = new ISeries[]
        {
            new ColumnSeries<int> { Name = "Sales", Values = values1 },
            new LineSeries<int> { Name = "Projected", Values = values2, Fill = null }
        };

        var xAxis = new Axis
        {
            Labels = labels
        };
        var yAxis = new Axis
        {
            Labeler = Labeler
        };

        cartesianChart = new CartesianChart
        {
            Series = series,
            XAxes = [xAxis],
            YAxes = [yAxis],
            TooltipTextSize = 16,
            TooltipTextPaint = new SolidColorPaint(new SKColor(242, 244, 255)) { SKTypeface = SKTypeface.FromFamilyName("Courier New") },
            TooltipBackgroundPaint = new SolidColorPaint(new SKColor(72, 0, 50)),
            TooltipPosition = LiveChartsCore.Measure.TooltipPosition.Left,
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(50, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(cartesianChart);
    }
}
