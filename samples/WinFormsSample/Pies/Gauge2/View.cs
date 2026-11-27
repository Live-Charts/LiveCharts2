using System.Windows.Forms;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WinForms;
using SkiaSharp;

namespace WinFormsSample.Pies.Gauge2;

public partial class View : UserControl
{
    private readonly PieChart pieChart;

    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

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
            MaxValue = 100,
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(50, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(pieChart);
    }
}
