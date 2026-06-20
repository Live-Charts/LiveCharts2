using System.Windows.Forms;
using LiveChartsCore.SkiaSharpView.WinForms;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;

namespace WinFormsSample.Polar.Basic;

public partial class View : UserControl
{
    public PolarChart Chart;

    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

        var values = new double[] { 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
        var cotangentAngle = LiveCharts.CotangentAngle;
        var tangentAngle = LiveCharts.TangentAngle;

        var series = new ISeries[]
        {
            new PolarLineSeries<double>
            {
                Values = values,
                ShowDataLabels = true,
                GeometrySize = 15,
                DataLabelsSize = 8,
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                DataLabelsRotation = cotangentAngle,
                IsClosed = true
            }
        };

        var radiusAxes = new PolarAxis[]
        {
            new() {
                LabelsAngle = -60,
                MaxLimit = 30
            }
        };

        var angleAxes = new PolarAxis[]
        {
            new() {
                LabelsRotation = tangentAngle
            }
        };

        var title = new DrawnLabelVisual(
            new LabelGeometry
            {
                Text = "My chart title",
                Paint = new SolidColorPaint(SKColor.Parse("#303030")),
                TextSize = 25,
                Padding = new LiveChartsCore.Drawing.Padding(15)
            });

        var polarChart = new PolarChart
        {
            Series = series,
            RadiusAxes = radiusAxes,
            AngleAxes = angleAxes,
            Title = title,
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(50, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Chart = polarChart;

        Controls.Add(polarChart);
    }
}
