using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;

using SkiaSharp;

namespace SnapshotTests;

[TestClass]
public sealed class RowSeriesTests
{
    [TestMethod]
    public void BasicRow()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new RowSeries<int> { Values = [1, 5, 6] },
                new RowSeries<int> { Values = [4, 2, 3] }
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

        chart.AssertSnapshotMatches($"{nameof(RowSeriesTests)}_{nameof(BasicRow)}");
    }

    [TestMethod]
    public void LayeredRows()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new RowSeries<int>
                {
                    Values = [6, 3, 5, 7, 3, 4, 6, 3],
                    MaxBarWidth = 999999,
                    IgnoresBarPosition = true
                },
                new RowSeries<int>
                {
                    Values = [2, 4, 8, 9, 5, 2, 4, 7],
                    MaxBarWidth = 30,
                    IgnoresBarPosition = true
                },
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(RowSeriesTests)}_{nameof(LayeredRows)}");
    }

    [TestMethod]
    public void SpacingRows()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new RowSeries<int>
                {
                    Values = [6, 3, 5 ],
                    Padding = 0,
                    MaxBarWidth = 999999
                }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(RowSeriesTests)}_{nameof(SpacingRows)}");
    }

    [TestMethod]
    public void BackgroundRows()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new RowSeries<int>
                {
                    Values = [10, 10, 10],
                    Fill = new SolidColorPaint(new SKColor(180, 180, 180, 50)),
                    IgnoresBarPosition = true,
                    IsHoverable = false
                },
                new RowSeries<int>
                {
                    Values = [6, 3, 5],
                    IgnoresBarPosition = true
                }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(RowSeriesTests)}_{nameof(BackgroundRows)}");
    }
}
