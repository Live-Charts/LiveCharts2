using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;

namespace SnapshotTests;

[TestClass]
public sealed class LegendsTests
{
    [TestMethod]
    public void Top()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new ColumnSeries<int> { Name = "aaaaaaaaaaaa", Values = [1, 5, 6] },
                new ColumnSeries<int> { Name = "bbbbbbbbbbbb", Values = [4, 2, 3] },
                new ColumnSeries<int> { Name = "cccccccccccc", Values = [2, 6, 4] },
                new ColumnSeries<int> { Name = "dddddddddddd", Values = [3, 4, 5] },
                new ColumnSeries<int> { Name = "eeeeeeeeeeee", Values = [5, 3, 2] },
                new ColumnSeries<int> { Name = "ffffffffffff", Values = [6, 4, 3] }
            ],
            LegendPosition = LiveChartsCore.Measure.LegendPosition.Top,
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(LegendsTests)}_{nameof(Top)}");
    }

    [TestMethod]
    public void Left()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new ColumnSeries<int> { Name = "aaaaaaaaaaaa", Values = [1, 5, 6] },
                new ColumnSeries<int> { Name = "bbbbbbbbbbbb", Values = [4, 2, 3] },
                new ColumnSeries<int> { Name = "cccccccccccc", Values = [2, 6, 4] },
                new ColumnSeries<int> { Name = "dddddddddddd", Values = [3, 4, 5] },
                new ColumnSeries<int> { Name = "eeeeeeeeeeee", Values = [5, 3, 2] },
                new ColumnSeries<int> { Name = "ffffffffffff", Values = [6, 4, 3] }
            ],
            LegendPosition = LiveChartsCore.Measure.LegendPosition.Left,
            Width = 600,
            Height = 600
        };
        chart.AssertSnapshotMatches($"{nameof(LegendsTests)}_{nameof(Left)}");
    }

    [TestMethod]
    public void Right()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new ColumnSeries<int> { Name = "aaaaaaaaaaaa", Values = [1, 5, 6] },
                new ColumnSeries<int> { Name = "bbbbbbbbbbbb", Values = [4, 2, 3] },
                new ColumnSeries<int> { Name = "cccccccccccc", Values = [2, 6, 4] },
                new ColumnSeries<int> { Name = "dddddddddddd", Values = [3, 4, 5] },
                new ColumnSeries<int> { Name = "eeeeeeeeeeee", Values = [5, 3, 2] },
                new ColumnSeries<int> { Name = "ffffffffffff", Values = [6, 4, 3] }
            ],
            LegendPosition = LiveChartsCore.Measure.LegendPosition.Right,
            Width = 600,
            Height = 600
        };
        chart.AssertSnapshotMatches($"{nameof(LegendsTests)}_{nameof(Right)}");
    }

    [TestMethod]
    public void Bottom()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new ColumnSeries<int> { Name = "aaaaaaaaaaaa", Values = [1, 5, 6] },
                new ColumnSeries<int> { Name = "bbbbbbbbbbbb", Values = [4, 2, 3] },
                new ColumnSeries<int> { Name = "cccccccccccc", Values = [2, 6, 4] },
                new ColumnSeries<int> { Name = "dddddddddddd", Values = [3, 4, 5] },
                new ColumnSeries<int> { Name = "eeeeeeeeeeee", Values = [5, 3, 2] },
                new ColumnSeries<int> { Name = "ffffffffffff", Values = [6, 4, 3] }
            ],
            LegendPosition = LiveChartsCore.Measure.LegendPosition.Bottom,
            Width = 600,
            Height = 600
        };
        chart.AssertSnapshotMatches($"{nameof(LegendsTests)}_{nameof(Bottom)}");
    }
}
