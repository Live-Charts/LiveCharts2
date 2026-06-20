using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using SkiaSharp;

namespace SnapshotTests;

[TestClass]
public sealed class ColumnSeriesTests
{
    [TestMethod]
    public void BasicColumn()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new ColumnSeries<int> { Values = [1, 5, 6] },
                new ColumnSeries<int> { Values = [4, 2, 3] }
            ],
            XAxes = [
                new Axis
                {
                    Labels = ["A", "B", "C", "D", "E"],
                    TicksAtCenter = true,
                    MinStep = 1,
                    ForceStepToMin = true
                }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(ColumnSeriesTests)}_{nameof(BasicColumn)}");
    }

    [TestMethod]
    public void LayeredColumns()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new ColumnSeries<int>
                {
                    Values = [6, 3, 5, 7, 3, 4, 6, 3],
                    MaxBarWidth = 999999,
                    IgnoresBarPosition = true
                },
                new ColumnSeries<int>
                {
                    Values = [2, 4, 8, 9, 5, 2, 4, 7],
                    MaxBarWidth = 30,
                    IgnoresBarPosition = true
                },
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(ColumnSeriesTests)}_{nameof(LayeredColumns)}");
    }

    [TestMethod]
    public void SpacingColumns()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new ColumnSeries<int>
                {
                    Values = [6, 3, 5],
                    Padding = 0,
                    MaxBarWidth = 999999
                }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(ColumnSeriesTests)}_{nameof(SpacingColumns)}");
    }

    [TestMethod]
    public void BackgroundColumns()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new ColumnSeries<int>
                {
                    Values = [10, 10, 10],
                    Fill = new SolidColorPaint(new SKColor(180, 180, 180, 50)),
                    IgnoresBarPosition = true,
                    IsHoverable = false
                },
                new ColumnSeries<int>
                {
                    Values = [6, 3, 5],
                    IgnoresBarPosition = true
                }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(ColumnSeriesTests)}_{nameof(BackgroundColumns)}");
    }
}
