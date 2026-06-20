using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;

namespace SnapshotTests;

[TestClass]
public sealed class StackedRowSeriesTests
{
    [TestMethod]
    public void Basic()
    {
        var values1 = new double[] { 3, 2, 3, 5, 3, 4, 6 };
        var values2 = new double[] { 6, 5, 6, 3, 8, 5, 2 };
        var values3 = new double[] { 4, 8, 2, 8, 9, 5, 3 };

        var series = new ISeries[]
        {
            new StackedRowSeries<double> { Values = values1 },
            new StackedRowSeries<double> { Values = values2 },
            new StackedRowSeries<double> { Values = values3 }
        };

        var chart = new SKCartesianChart
        {
            Series = series,
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(StackedRowSeriesTests)}_{nameof(Basic)}");
    }

    [TestMethod]
    public void StackGroup()
    {
        var values1 = new int[] { 3, 5, 3 };
        var values2 = new int[] { 4, 2, 3 };
        var values3 = new int[] { 4, 6, 6 };
        var values4 = new int[] { 2, 5, 4 };

        static string formatter(ChartPoint p) =>
            $"{p.Coordinate.PrimaryValue:N} ({p.StackedValue!.Share:P})";

        var series = new ISeries[]
        {
            new StackedRowSeries<int>
            {
                Values = values1,
                StackGroup = 0,
                ShowDataLabels = true,
                YToolTipLabelFormatter = formatter
            },
            new StackedRowSeries<int>
            {
                Values = values2,
                StackGroup = 0,
                ShowDataLabels = true,
                YToolTipLabelFormatter = formatter
            },
            new StackedRowSeries<int>
            {
                Values = values3,
                StackGroup = 1,
                ShowDataLabels = true,
                YToolTipLabelFormatter = formatter
            },
            new StackedRowSeries<int>
            {
                Values = values4,
                StackGroup = 1,
                ShowDataLabels = true,
                YToolTipLabelFormatter = formatter
            }
        };

        var chart = new SKCartesianChart
        {
            Series = series,
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(StackedRowSeriesTests)}_{nameof(StackGroup)}");
    }

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
