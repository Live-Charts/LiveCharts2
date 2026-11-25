using Eto.Forms;
using LiveChartsCore.Defaults;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.Eto;
using ViewModelsSamples.Pies.Gauge3;

namespace EtoFormsSample.Pies.Gauge3;

public class View : Panel
{
    private readonly PieChart pieChart;

    public View()
    {
        var viewModel = new ViewModel();

        pieChart = new PieChart
        {
            Series = GaugeGenerator.BuildSolidGauge(
                new GaugeItem(viewModel.Vanesa, series => SetStyle("Vanesa", series)),
                new GaugeItem(viewModel.Charles, series => SetStyle("Charles", series)),
                new GaugeItem(viewModel.Ana, series => SetStyle("Ana", series)),
                new GaugeItem(GaugeItem.Background, series =>
                {
                    series.InnerRadius = 20;
                })),
            InitialRotation = 45,
            MaxAngle = 270,
            MinValue = 0,
            MaxValue = 100
        };

        Content = pieChart;
    }

    public static void SetStyle(string name, PieSeries<ObservableValue> series)
    {
        series.Name = name;
        series.DataLabelsPosition = PolarLabelsPosition.Start;
        series.DataLabelsFormatter =
                point => $"{point.Coordinate.PrimaryValue} {point.Context.Series.Name}";
        series.InnerRadius = 20;
        series.RelativeOuterRadius = 8;
        series.RelativeInnerRadius = 8;
    }
}
