using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using SkiaSharp;

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

    [TestMethod]
    public void Issue1923_LineOverStacked()
    {
        // Regression test for https://github.com/Live-Charts/LiveCharts2/issues/1923.
        // A LineSeries added after stacked areas must render on top of them with
        // default ZIndex on every series. Pre-2.1, stacked series silently jumped
        // to z-index 1000 - stack-position, hiding the line behind stack 0's fill
        // unless the user set ZIndex > 1000 on the line. The line uses a thick
        // contrasting stroke and no fill so any regression would visibly hide
        // the line stroke pixels behind the stacked fills.
        var stack1 = new double[] { 3, 2, 3, 5, 3, 4, 6 };
        var stack2 = new double[] { 6, 5, 6, 3, 8, 5, 2 };
        var stack3 = new double[] { 4, 8, 2, 8, 9, 5, 3 };
        var line = new double[] { 8, 6, 9, 7, 10, 6, 8 };

        var series = new ISeries[]
        {
            new StackedAreaSeries<double> { Values = stack1 },
            new StackedAreaSeries<double> { Values = stack2 },
            new StackedAreaSeries<double> { Values = stack3 },
            new LineSeries<double>
            {
                Values = line,
                Fill = null,
                GeometryFill = null,
                GeometryStroke = null,
                Stroke = new SolidColorPaint(SKColors.Red, 6),
            }
        };

        var chart = new SKCartesianChart
        {
            Series = series,
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(StackedAreaTests)}_{nameof(Issue1923_LineOverStacked)}");
    }
}
