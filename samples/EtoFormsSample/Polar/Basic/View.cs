using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using SkiaSharp;

namespace EtoFormsSample.Polar.Basic;

public class View : Panel
{
    public PolarChart Chart;

    public View()
    {
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
            Title = title
        };

        Content = Chart = polarChart;
    }
}
