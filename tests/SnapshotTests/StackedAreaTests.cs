using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;

namespace SnapshotTests;

[TestClass]
public sealed class StackedAreaTests
{
    [TestMethod]
    public void Basic()
    {
        var values1 = new double[] { 3, 2, 3, 5, 3, 4, 6 };
        var values2 = new double[] { 6, 5, 6, 3, 8, 5, 2 };
        var values3 = new double[] { 4, 8, 2, 8, 9, 5, 3 };

        var series = new ISeries[]
        {
            new StackedAreaSeries<double> { Values = values1 },
            new StackedAreaSeries<double> { Values = values2 },
            new StackedAreaSeries<double> { Values = values3 }
        };

        var chart = new SKCartesianChart
        {
            Series = series,
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(StackedAreaTests)}_{nameof(Basic)}");
    }

    [TestMethod]
    public void Step()
    {
        var values1 = new double[] { 3, 2, 3, 5, 3, 4, 6 };
        var values2 = new double[] { 6, 5, 6, 3, 8, 5, 2 };
        var values3 = new double[] { 4, 8, 2, 8, 9, 5, 3 };

        var series = new ISeries[]
        {
            new StackedStepAreaSeries<double> { Values = values1 },
            new StackedStepAreaSeries<double> { Values = values2 },
            new StackedStepAreaSeries<double> { Values = values3 }
        };

        var chart = new SKCartesianChart
        {
            Series = series,
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(StackedAreaTests)}_{nameof(Step)}");
    }

    [TestMethod]
    public void Issue2073_MixedSigns()
    {
        // Regression test for https://github.com/Live-Charts/LiveCharts2/issues/2073.
        // The upper series must stack on the cumulative running total of prior series so
        // its baseline stays continuous when prior values cross zero — Excel-like.
        var values1 = new double[] { -52.89, -50.35, -42.11, 100, -34.56, -28.33 };
        var values2 = new double[] { 1404.4, 1404.4, 1327.89, 1383.78, 1365.07, 1235.7 };

        var series = new ISeries[]
        {
            new StackedAreaSeries<double> { Values = values1 },
            new StackedAreaSeries<double> { Values = values2 }
        };

        var chart = new SKCartesianChart
        {
            Series = series,
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(StackedAreaTests)}_{nameof(Issue2073_MixedSigns)}");
    }
}
