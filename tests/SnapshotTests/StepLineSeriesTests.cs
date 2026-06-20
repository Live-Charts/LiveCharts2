using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using SkiaSharp;

namespace SnapshotTests;

[TestClass]
public sealed class StepLineSeriesTests
{
    [TestMethod]
    public void Basic()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new StepLineSeries<int> { Values = [1, 5, 7, 3] },
                new StepLineSeries<int> { Values = [4, 2, 8, 6] },
                new StepLineSeries<int> { Values = [2, 6, 4, 8] }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(StepLineSeriesTests)}_{nameof(Basic)}");
    }

    [TestMethod]
    public void Area()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new StepLineSeries<int>
                {
                    Values = [1, 5, 7, 3],
                    Fill = new SolidColorPaint(SKColor.Parse("#6495ED")),
                    Stroke = null,
                    GeometryFill = null,
                    GeometryStroke = null
                },
                new StepLineSeries<int>
                {
                    Values = [4, 2, 8, 6],
                    Fill = new SolidColorPaint(SKColor.Parse("#F08080")),
                    Stroke = null,
                    GeometryFill = null,
                    GeometryStroke = null
                }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(StepLineSeriesTests)}_{nameof(Area)}");
    }

    [TestMethod]
    public void Styled()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new StepLineSeries<int>
                {
                    Values = [1, 5, 7, 3, 6],
                    Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 4 },
                    Fill = new SolidColorPaint(SKColor.Parse("#336495ED")),
                    GeometrySize = 20,
                    GeometryFill = new SolidColorPaint(SKColors.White),
                    GeometryStroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 3 }
                }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(StepLineSeriesTests)}_{nameof(Styled)}");
    }

    [TestMethod]
    public void WithDataLabels()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new StepLineSeries<int>
                {
                    Values = [3, 7, 2, 5, 4],
                    ShowDataLabels = true,
                    DataLabelsSize = 14,
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    DataLabelsPosition = DataLabelsPosition.Top
                }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(StepLineSeriesTests)}_{nameof(WithDataLabels)}");
    }

    [TestMethod]
    public void Gaps()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new StepLineSeries<int?> { Values = [1, 5, null, 3, 7, null, 2] }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(StepLineSeriesTests)}_{nameof(Gaps)}");
    }
}
