using Eto.Forms;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Eto;
using SkiaSharp;

namespace EtoFormsSample.Pies.Gauge2;

public class View : Panel
{
    private readonly PieChart pieChart;

    public View()
    {
        pieChart = new PieChart
        {
            Series = GaugeGenerator.BuildSolidGauge(
                new GaugeItem(30, series =>
                {
                    series.Fill = new SolidColorPaint(SKColors.YellowGreen);
                    series.DataLabelsSize = 50;
                    series.ShowDataLabels = true;
                    series.DataLabelsPosition = PolarLabelsPosition.ChartCenter;
                    series.InnerRadius = 75;
                }),
                new GaugeItem(GaugeItem.Background, series =>
                {
                    series.InnerRadius = 75;
                    series.Fill = new SolidColorPaint(new SKColor(100, 181, 246, 90));
                })),
            InitialRotation = -225,
            MaxAngle = 270,
            MinValue = 0,
            MaxValue = 100
        };

        Content = pieChart;
    }
}
