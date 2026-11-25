using System.Windows.Forms;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView.WinForms;

namespace WinFormsSample.General.ChartToImage;

public partial class View : UserControl
{
    private readonly CartesianChart _cartesian;
    private readonly PieChart _pie;

    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(420, 620);

        // Adding a cartesian chart to the UI...
        _cartesian = new CartesianChart
        {
            Series =
            [
                new LiveChartsCore.SkiaSharpView.LineSeries<double> { Values = [1, 5, 4, 6] },
                new LiveChartsCore.SkiaSharpView.ColumnSeries<double> { Values = [4, 8, 2, 4] }
            ],
            Location = new System.Drawing.Point(10, 10),
            Size = new System.Drawing.Size(400, 180)
        };
        Controls.Add(_cartesian);

        // Adding a pie chart to the UI...
        _pie = new PieChart
        {
            Series =
            [
                new LiveChartsCore.SkiaSharpView.PieSeries<double> { Values = [10] },
                new LiveChartsCore.SkiaSharpView.PieSeries<double> { Values = [6] },
                new LiveChartsCore.SkiaSharpView.PieSeries<double> { Values = [4] }
            ],
            Location = new System.Drawing.Point(10, 210),
            Size = new System.Drawing.Size(400, 180)
        };
        Controls.Add(_pie);

        // now lets create the images
        CreateImageFromCartesianControl();
        CreateImageFromPieControl();
    }

    private void CreateImageFromCartesianControl()
    {
        var chartControl = _cartesian;
        var skChart = new SKCartesianChart(chartControl);
        skChart.SaveImage("CartesianImageFromControl.png");
    }

    private void CreateImageFromPieControl()
    {
        var chartControl = _pie;
        var skChart = new SKPieChart(chartControl);
        skChart.SaveImage("PieImageFromControl.png");
    }
}
