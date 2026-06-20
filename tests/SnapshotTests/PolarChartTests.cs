using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using SkiaSharp;

namespace SnapshotTests;

[TestClass]
public sealed class PolarChartTests
{
    [TestMethod]
    public void Basic()
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

        var chart = new SKPolarChart
        {
            Series = series,
            RadiusAxes = radiusAxes,
            AngleAxes = angleAxes,
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(PolarChartTests)}_{nameof(Basic)}");
    }

    [TestMethod]
    public void MultipleSeries()
    {
        var chart = new SKPolarChart
        {
            Series = [
                new PolarLineSeries<double>
                {
                    Values = [10, 8, 12, 6, 14, 4],
                    IsClosed = true
                },
                new PolarLineSeries<double>
                {
                    Values = [6, 14, 4, 10, 8, 12],
                    IsClosed = true
                },
                new PolarLineSeries<double>
                {
                    Values = [12, 6, 10, 14, 4, 8],
                    IsClosed = true
                }
            ],
            RadiusAxes = [new PolarAxis { MaxLimit = 20 }],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(PolarChartTests)}_{nameof(MultipleSeries)}");
    }

    [TestMethod]
    public void FilledArea()
    {
        var chart = new SKPolarChart
        {
            Series = [
                new PolarLineSeries<double>
                {
                    Values = [10, 8, 12, 6, 14, 4],
                    IsClosed = true,
                    Fill = new SolidColorPaint(SKColor.Parse("#336495ED")),
                    GeometrySize = 0
                },
                new PolarLineSeries<double>
                {
                    Values = [6, 14, 4, 10, 8, 12],
                    IsClosed = true,
                    Fill = new SolidColorPaint(SKColor.Parse("#33F08080")),
                    GeometrySize = 0
                }
            ],
            RadiusAxes = [new PolarAxis { MaxLimit = 20 }],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(PolarChartTests)}_{nameof(FilledArea)}");
    }

    [TestMethod]
    public void WithInnerRadius()
    {
        var chart = new SKPolarChart
        {
            Series = [
                new PolarLineSeries<double>
                {
                    Values = [10, 8, 12, 6, 14, 4],
                    IsClosed = true
                }
            ],
            InnerRadius = 50,
            RadiusAxes = [new PolarAxis { MaxLimit = 20 }],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(PolarChartTests)}_{nameof(WithInnerRadius)}");
    }

    [TestMethod]
    public void OpenLine()
    {
        var chart = new SKPolarChart
        {
            Series = [
                new PolarLineSeries<double>
                {
                    Values = [10, 8, 12, 6, 14, 4],
                    IsClosed = false,
                    GeometrySize = 12,
                    GeometryFill = new SolidColorPaint(SKColors.White),
                    GeometryStroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 2 }
                }
            ],
            RadiusAxes = [new PolarAxis { MaxLimit = 20 }],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(PolarChartTests)}_{nameof(OpenLine)}");
    }
}
