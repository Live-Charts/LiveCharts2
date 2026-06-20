using System.Collections.Generic;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.CoreObjectsTests;

[TestClass]
public class SeriesMeasurementEventsTests
{
    [TestMethod]
    public void PointMeasuredFiresOncePerPointPerMeasure()
    {
        var series = new ColumnSeries<int> { Values = [10, 20, 30] };
        var chart = NewChart(series);
        var measured = new List<ChartPoint<int, RoundedRectangleGeometry, LabelGeometry>>();
        series.PointMeasured += p => measured.Add(p);

        _ = ChangingPaintTasks.DrawChart(chart);

        Assert.AreEqual(3, measured.Count);
    }

    [TestMethod]
    public void PointCreatedFiresOncePerNewPointOnFirstMeasure()
    {
        var series = new ColumnSeries<int> { Values = [10, 20, 30] };
        var chart = NewChart(series);
        var created = new List<ChartPoint<int, RoundedRectangleGeometry, LabelGeometry>>();
        series.PointCreated += p => created.Add(p);

        _ = ChangingPaintTasks.DrawChart(chart);

        Assert.AreEqual(3, created.Count);
    }

    [TestMethod]
    public void PointCreatedDoesNotRefireForExistingPoints()
    {
        // PointCreated is for visual realization (first time the geometry is built),
        // not measurement. A second Measure over the same data must not refire it.
        var series = new ColumnSeries<int> { Values = [10, 20, 30] };
        var chart = NewChart(series);
        var createdAfterFirstMeasure = 0;
        var createdAfterSecondMeasure = 0;
        series.PointCreated += _ => createdAfterFirstMeasure++;

        _ = ChangingPaintTasks.DrawChart(chart);

        // attach a second counter for clarity; the first stays as the baseline.
        series.PointCreated += _ => createdAfterSecondMeasure++;
        _ = ChangingPaintTasks.DrawChart(chart);

        Assert.AreEqual(3, createdAfterFirstMeasure);
        Assert.AreEqual(0, createdAfterSecondMeasure);
    }

    [TestMethod]
    public void PointMeasuredRefiresForEveryPointOnEveryMeasure()
    {
        var series = new ColumnSeries<int> { Values = [10, 20, 30] };
        var chart = NewChart(series);
        var measuredCount = 0;
        series.PointMeasured += _ => measuredCount++;

        _ = ChangingPaintTasks.DrawChart(chart);
        _ = ChangingPaintTasks.DrawChart(chart);

        Assert.AreEqual(6, measuredCount);
    }

    [TestMethod]
    public void EventArgsCarryTheChartPointInstance()
    {
        var series = new ColumnSeries<int> { Values = [10, 20, 30] };
        var chart = NewChart(series);
        ChartPoint<int, RoundedRectangleGeometry, LabelGeometry>? lastMeasured = null;
        ChartPoint<int, RoundedRectangleGeometry, LabelGeometry>? lastCreated = null;
        series.PointMeasured += p => lastMeasured = p;
        series.PointCreated += p => lastCreated = p;

        _ = ChangingPaintTasks.DrawChart(chart);

        Assert.IsNotNull(lastMeasured);
        Assert.IsNotNull(lastCreated);
        Assert.AreSame(series, lastMeasured!.Context.Series);
        Assert.AreSame(series, lastCreated!.Context.Series);
    }

    private static SKCartesianChart NewChart(ColumnSeries<int> series) => new()
    {
        Series = [series],
        XAxes = [new Axis()],
        YAxes = [new Axis()],
    };
}
