using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;

namespace SnapshotTests;

[TestClass]
public sealed class StackedRowSeriesTests
{
    [TestMethod]
    public void Issue2152_MixedSigns()
    {
        // Regression test for https://github.com/Live-Charts/LiveCharts2/issues/2152.
        // Mirrors the docs sample (samples.stackedBars.basic) where StackedRowSeries
        // mixes positive and negative values. Bars at each index must grow from 0
        // upward for positives and from 0 downward for negatives — sign-segregated
        // so a positive value never starts inside a previous series' negative range.
        var values1 = new int[] { 3, 5, -3, 2, 5, -4, -2 };
        var values2 = new int[] { 4, 2, -3, 2, 3, 4, -2 };
        var values3 = new int[] { -2, 6, 6, 5, 4, 3, -2 };

        var series = new ISeries[]
        {
            new StackedRowSeries<int> { Values = values1 },
            new StackedRowSeries<int> { Values = values2 },
            new StackedRowSeries<int> { Values = values3 }
        };

        var chart = new SKCartesianChart
        {
            Series = series,
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(StackedRowSeriesTests)}_{nameof(Issue2152_MixedSigns)}");
    }
}
