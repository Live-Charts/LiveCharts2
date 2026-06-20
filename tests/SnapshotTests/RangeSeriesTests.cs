using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;

namespace SnapshotTests;

[TestClass]
public sealed class RangeSeriesTests
{
    [TestMethod]
    public void RangeColumn()
    {
        var values = new RangeValue[]
        {
            new(10, 35),
            new(20, 25),
            new(5, 45),
            new(15, 40),
            new(8, 22),
            new(28, 50),
            new(12, 18),
        };

        var chart = new SKCartesianChart
        {
            Series = [new RangeColumnSeries<RangeValue> { Values = values }],
            YAxes = [new Axis { MinLimit = 0, MaxLimit = 60 }],
            Width = 600,
            Height = 600,
        };

        chart.AssertSnapshotMatches($"{nameof(RangeSeriesTests)}_{nameof(RangeColumn)}");
    }

    [TestMethod]
    public void RangeRow()
    {
        var values = new RangeValue[]
        {
            new(10, 35),
            new(20, 25),
            new(5, 45),
            new(15, 40),
            new(8, 22),
            new(28, 50),
            new(12, 18),
        };

        var chart = new SKCartesianChart
        {
            Series = [new RangeRowSeries<RangeValue> { Values = values }],
            XAxes = [new Axis { MinLimit = 0, MaxLimit = 60 }],
            Width = 600,
            Height = 600,
        };

        chart.AssertSnapshotMatches($"{nameof(RangeSeriesTests)}_{nameof(RangeRow)}");
    }

    // Low > High is a user error but must not crash — the bar should still render
    // with the rectangle normalized to abs(highPx - lowPx). The MeasureBarLayout
    // implementation uses Math.Min / Math.Abs specifically for this case.
    [TestMethod]
    public void RangeColumn_SwappedEndpoints()
    {
        var values = new RangeValue[]
        {
            new(35, 10),   // low > high intentionally
            new(25, 20),
            new(45, 5),
        };

        var chart = new SKCartesianChart
        {
            Series = [new RangeColumnSeries<RangeValue> { Values = values }],
            YAxes = [new Axis { MinLimit = 0, MaxLimit = 60 }],
            Width = 600,
            Height = 600,
        };

        chart.AssertSnapshotMatches($"{nameof(RangeSeriesTests)}_{nameof(RangeColumn_SwappedEndpoints)}");
    }

    [TestMethod]
    public void RangeLine()
    {
        var values = new RangeValue[]
        {
            new(10, 35),
            new(20, 25),
            new(5, 45),
            new(15, 40),
            new(8, 22),
            new(28, 50),
            new(12, 18),
        };

        var chart = new SKCartesianChart
        {
            Series = [new RangeLineSeries<RangeValue> { Values = values }],
            YAxes = [new Axis { MinLimit = 0, MaxLimit = 60 }],
            Width = 600,
            Height = 600,
        };

        chart.AssertSnapshotMatches($"{nameof(RangeSeriesTests)}_{nameof(RangeLine)}");
    }

    // Low > High at some points: each spline traces whichever endpoint it's
    // built from, so the band crosses over itself. Must not crash; the band
    // fill self-intersects but renders the closed region per even-odd fill.
    [TestMethod]
    public void RangeLine_SwappedEndpoints()
    {
        var values = new RangeValue[]
        {
            new(35, 10),   // low > high intentionally
            new(20, 25),
            new(45, 5),
            new(15, 40),
        };

        var chart = new SKCartesianChart
        {
            Series = [new RangeLineSeries<RangeValue> { Values = values }],
            YAxes = [new Axis { MinLimit = 0, MaxLimit = 60 }],
            Width = 600,
            Height = 600,
        };

        chart.AssertSnapshotMatches($"{nameof(RangeSeriesTests)}_{nameof(RangeLine_SwappedEndpoints)}");
    }
}
