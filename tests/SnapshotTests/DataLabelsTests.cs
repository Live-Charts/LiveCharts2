using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using SkiaSharp;

namespace SnapshotTests;

[TestClass]
public sealed class DataLabelsTests
{
    [TestMethod]
    public void LineWithLabels()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new LineSeries<int>
                {
                    Values = [3, 7, 2, 5],
                    ShowDataLabels = true,
                    DataLabelsSize = 14,
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    DataLabelsPosition = DataLabelsPosition.Top
                }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(DataLabelsTests)}_{nameof(LineWithLabels)}");
    }

    [TestMethod]
    public void ColumnWithLabels()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new ColumnSeries<int>
                {
                    Values = [3, 7, 2, 5],
                    ShowDataLabels = true,
                    DataLabelsSize = 14,
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    DataLabelsPosition = DataLabelsPosition.Top
                },
                new ColumnSeries<int>
                {
                    Values = [5, 3, 8, 1],
                    ShowDataLabels = true,
                    DataLabelsSize = 14,
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    DataLabelsPosition = DataLabelsPosition.Middle
                }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(DataLabelsTests)}_{nameof(ColumnWithLabels)}");
    }

    [TestMethod]
    public void RowWithLabels()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new RowSeries<int>
                {
                    Values = [3, 7, 2, 5],
                    ShowDataLabels = true,
                    DataLabelsSize = 14,
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    DataLabelsPosition = DataLabelsPosition.End
                }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(DataLabelsTests)}_{nameof(RowWithLabels)}");
    }

    [TestMethod]
    public void ScatterWithLabels()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new ScatterSeries<int>
                {
                    Values = [3, 7, 2, 5, 8],
                    ShowDataLabels = true,
                    DataLabelsSize = 14,
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    DataLabelsPosition = DataLabelsPosition.Top
                }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(DataLabelsTests)}_{nameof(ScatterWithLabels)}");
    }

    [TestMethod]
    public void ColumnWithLabelsRotated()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new ColumnSeries<int>
                {
                    Values = [3, 7, 2, 5, 4],
                    ShowDataLabels = true,
                    DataLabelsSize = 14,
                    DataLabelsPaint = new SolidColorPaint(SKColors.DarkRed),
                    DataLabelsRotation = -45,
                    DataLabelsPosition = DataLabelsPosition.Top
                }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(DataLabelsTests)}_{nameof(ColumnWithLabelsRotated)}");
    }

    [TestMethod]
    public void HeatWithLabels()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new HeatSeries<LiveChartsCore.Defaults.WeightedPoint>
                {
                    Values = [
                        new(0, 0, 5),
                        new(0, 1, 10),
                        new(0, 2, 15),
                        new(1, 0, 20),
                        new(1, 1, 25),
                        new(1, 2, 30),
                        new(2, 0, 35),
                        new(2, 1, 40),
                        new(2, 2, 45)
                    ],
                    ShowDataLabels = true,
                    DataLabelsSize = 12,
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black)
                }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(DataLabelsTests)}_{nameof(HeatWithLabels)}");
    }
}
