using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LiveChartsCore.Defaults;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace AvaloniaSample.VisualTest.Issue2131Repro;

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();

        ConstrainedChart.Series = BuildGauge();
        ConstrainedChart.CoreChart.Update();

        ControlChart.Series = BuildGauge();
        ControlChart.CoreChart.Update();
    }

    public PieChart ConstrainedChart => this.Find<PieChart>("constrainedChart")!;

    public PieChart ControlChart => this.Find<PieChart>("controlChart")!;

    // Mirrors the exact code-behind from issue #2131: a value GaugeItem at 100
    // (value == MaxValue) plus a background ring, built via GaugeGenerator.BuildSolidGauge.
    private static PieSeries<ObservableValue>[] BuildGauge() =>
        GaugeGenerator.BuildSolidGauge(
            new GaugeItem(
                100,
                series =>
                {
                    series.Fill = new SolidColorPaint(new SKColor(0x4D, 0xD0, 0xC8));
                    series.DataLabelsFormatter = point => $"{point.Model?.Value}%";
                    series.DataLabelsSize = 18;
                    series.DataLabelsPaint = new SolidColorPaint(SKColors.White);
                    series.DataLabelsPosition = PolarLabelsPosition.ChartCenter;
                    series.InnerRadius = 38;
                    series.RelativeInnerRadius = -4;
                    series.MaxRadialColumnWidth = 5;
                }),
            new GaugeItem(
                GaugeItem.Background,
                series =>
                {
                    series.InnerRadius = 40;
                    series.Fill = new SolidColorPaint(new SKColor(0xB3, 0x9D, 0xDB));
                    series.DataLabelsSize = 0;
                    series.RelativeInnerRadius = 4;
                    series.OuterRadiusOffset = -20;
                    series.MaxRadialColumnWidth = 5;
                }));

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
