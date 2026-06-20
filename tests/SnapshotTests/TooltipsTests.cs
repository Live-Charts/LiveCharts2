using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;

namespace SnapshotTests;

[TestClass]
public sealed class TooltipsTests
{
    [TestMethod]
    public void Top()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new ColumnSeries<int> { Name = "aaaa", Values = [1, 5, 6] },
                new ColumnSeries<int> { Name = "bbbb", Values = [4, 2, 3] }
            ],
            TooltipPosition = TooltipPosition.Top,
            Width = 600,
            Height = 600
        };

        chart.PointerAt(300, 300);
        chart.AssertSnapshotMatches($"{nameof(TooltipPosition)}_{nameof(Top)}");
    }

    [TestMethod]
    public void Left()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new ColumnSeries<int> { Name = "aaaa", Values = [1, 5, 6] },
                new ColumnSeries<int> { Name = "bbbb", Values = [4, 2, 3] }
            ],
            TooltipPosition = TooltipPosition.Left,
            Width = 600,
            Height = 600
        };

        chart.PointerAt(300, 300);
        chart.AssertSnapshotMatches($"{nameof(TooltipPosition)}_{nameof(Left)}");
    }

    [TestMethod]
    public void Right()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new ColumnSeries<int> { Name = "aaaa", Values = [1, 5, 6] },
                new ColumnSeries<int> { Name = "bbbb", Values = [4, 2, 3] }
            ],
            TooltipPosition = TooltipPosition.Right,
            Width = 600,
            Height = 600
        };

        chart.PointerAt(300, 300);
        chart.AssertSnapshotMatches($"{nameof(TooltipPosition)}_{nameof(Right)}");
    }

    [TestMethod]
    public void Bottom()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new ColumnSeries<int> { Name = "aaaa", Values = [1, 5, 6] },
                new ColumnSeries<int> { Name = "bbbb", Values = [4, 2, 3] }
            ],
            TooltipPosition = TooltipPosition.Bottom,
            Width = 600,
            Height = 600
        };

        chart.PointerAt(300, 300);
        chart.AssertSnapshotMatches($"{nameof(TooltipPosition)}_{nameof(Bottom)}");
    }
}
