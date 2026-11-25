using Eto.Forms;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView.Eto;
using LiveChartsCore.SkiaSharpView;
using ViewModelsSamples.General.ChartToImage;
using LiveChartsCore;

namespace EtoFormsSample.General.ChartToImage;

public class View : Panel
{
    private readonly CartesianChart _cartesian;
    private readonly PieChart _pie;

    public View()
    {
        var viewModel = new ViewModel();

        // Adding a cartesian chart to the UI...
        _cartesian = new CartesianChart
        {
            Series =
            [
                new LineSeries<double> { Values = viewModel.Values1 },
                new ColumnSeries<double> { Values = viewModel.Values2 }
            ]
        };

        // Adding a pie chart to the UI...
        _pie = new PieChart
        {
            Series =
            [
                new PieSeries<double> { Values = viewModel.PieValues1 },
                new PieSeries<double> { Values = viewModel.PieValues2 },
                new PieSeries<double> { Values = viewModel.PieValues3 }
            ]
        };

        Content = new DynamicLayout(
            new DynamicRow(new DynamicControl { Control = _cartesian, YScale = true }),
            new DynamicRow(new DynamicControl { Control = _pie, YScale = true })
        );

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
