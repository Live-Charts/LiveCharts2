using System.Collections.ObjectModel;
using CoreTests.CoreObjectsTests;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;

namespace CoreTests.SeriesTests;

// Regression coverage for https://github.com/Live-Charts/LiveCharts2/issues/1970.
//
// The reporter saw old markers/geometries piling up after a real-time data update.
// Both reported flavors are exercised here:
//   (a) Series.Values reassigned to a fresh, shorter collection.
//   (b) ObservableCollection.Clear() + Add() shrinking the bound collection in place.
// And both reported series types (Line + Scatter).
//
// Each test asserts two things after the shrink:
//   1. Series.everFetched.Count equals the new dataset size — points rendered last
//      frame but no longer in Values must be soft-deleted and pruned from the set.
//      This is the contract enforced by ChartPointCleanupContext.CollectPoints,
//      invoked at the end of every Core{Line,Scatter}Series.Measure pass.
//   2. CoreCanvas.CountGeometries() equals what a freshly-built chart with the
//      smaller dataset produces — i.e. no zombie shapes on the canvas, which is
//      what the user actually sees.
[TestClass]
public class Issue1970Tests
{
    private const int InitialCount = 20;
    private const int UpdatedCount = 5;

    [TestMethod]
    public void LineSeries_ValuesReassignedToSmallerArray_PrunesOldPoints()
    {
        var sutSeries = new LineSeries<ObservableValue> { Values = MakeCollection(InitialCount) };
        var sutChart = NewChart(sutSeries);

        _ = ChangingPaintTasks.DrawChart(sutChart);
        Assert.AreEqual(InitialCount, sutSeries.everFetched.Count, "first draw should fetch all initial points");

        sutSeries.Values = MakeCollection(UpdatedCount);
        _ = ChangingPaintTasks.DrawChart(sutChart);

        Assert.AreEqual(
            UpdatedCount, sutSeries.everFetched.Count,
            "after Values reassignment, stale ChartPoints must be pruned from everFetched");

        var referenceChart = NewChart(new LineSeries<ObservableValue> { Values = MakeCollection(UpdatedCount) });
        _ = ChangingPaintTasks.DrawChart(referenceChart);

        Assert.AreEqual(
            referenceChart.CoreCanvas.CountGeometries(),
            sutChart.CoreCanvas.CountGeometries(),
            "after the shrink, the canvas should hold the same number of geometries as a chart " +
            "freshly built with the smaller dataset (no zombie shapes left over)");
    }

    [TestMethod]
    public void LineSeries_ObservableCollectionClearedAndRepopulatedSmaller_PrunesOldPoints()
    {
        var values = MakeCollection(InitialCount);
        var sutSeries = new LineSeries<ObservableValue> { Values = values };
        var sutChart = NewChart(sutSeries);

        _ = ChangingPaintTasks.DrawChart(sutChart);
        Assert.AreEqual(InitialCount, sutSeries.everFetched.Count);

        // Exact pattern from the issue body: mutate the same bound collection in place.
        values.Clear();
        for (var i = 0; i < UpdatedCount; i++) values.Add(new ObservableValue(i + 1));
        _ = ChangingPaintTasks.DrawChart(sutChart);

        Assert.AreEqual(
            UpdatedCount, sutSeries.everFetched.Count,
            "after Clear()+Add() on the bound ObservableCollection, stale ChartPoints must be pruned");

        var referenceChart = NewChart(new LineSeries<ObservableValue> { Values = MakeCollection(UpdatedCount) });
        _ = ChangingPaintTasks.DrawChart(referenceChart);

        Assert.AreEqual(
            referenceChart.CoreCanvas.CountGeometries(),
            sutChart.CoreCanvas.CountGeometries(),
            "no zombie shapes should remain on the canvas after the in-place shrink");
    }

    [TestMethod]
    public void ScatterSeries_ValuesReassignedToSmallerArray_PrunesOldPoints()
    {
        var sutSeries = new ScatterSeries<ObservableValue> { Values = MakeCollection(InitialCount) };
        var sutChart = NewChart(sutSeries);

        _ = ChangingPaintTasks.DrawChart(sutChart);
        Assert.AreEqual(InitialCount, sutSeries.everFetched.Count);

        sutSeries.Values = MakeCollection(UpdatedCount);
        _ = ChangingPaintTasks.DrawChart(sutChart);

        Assert.AreEqual(
            UpdatedCount, sutSeries.everFetched.Count,
            "ScatterSeries: after Values reassignment, stale ChartPoints must be pruned");

        var referenceChart = NewChart(new ScatterSeries<ObservableValue> { Values = MakeCollection(UpdatedCount) });
        _ = ChangingPaintTasks.DrawChart(referenceChart);

        Assert.AreEqual(
            referenceChart.CoreCanvas.CountGeometries(),
            sutChart.CoreCanvas.CountGeometries(),
            "ScatterSeries: no zombie shapes should remain on the canvas after the shrink");
    }

    [TestMethod]
    public void ScatterSeries_ObservableCollectionClearedAndRepopulatedSmaller_PrunesOldPoints()
    {
        var values = MakeCollection(InitialCount);
        var sutSeries = new ScatterSeries<ObservableValue> { Values = values };
        var sutChart = NewChart(sutSeries);

        _ = ChangingPaintTasks.DrawChart(sutChart);
        Assert.AreEqual(InitialCount, sutSeries.everFetched.Count);

        values.Clear();
        for (var i = 0; i < UpdatedCount; i++) values.Add(new ObservableValue(i + 1));
        _ = ChangingPaintTasks.DrawChart(sutChart);

        Assert.AreEqual(
            UpdatedCount, sutSeries.everFetched.Count,
            "ScatterSeries: after Clear()+Add() on the bound ObservableCollection, stale ChartPoints must be pruned");

        var referenceChart = NewChart(new ScatterSeries<ObservableValue> { Values = MakeCollection(UpdatedCount) });
        _ = ChangingPaintTasks.DrawChart(referenceChart);

        Assert.AreEqual(
            referenceChart.CoreCanvas.CountGeometries(),
            sutChart.CoreCanvas.CountGeometries(),
            "ScatterSeries: no zombie shapes should remain on the canvas after the in-place shrink");
    }

    private static ObservableCollection<ObservableValue> MakeCollection(int count)
    {
        var values = new ObservableCollection<ObservableValue>();
        for (var i = 0; i < count; i++) values.Add(new ObservableValue(i + 1));
        return values;
    }

    private static SKCartesianChart NewChart(ISeries series) => new()
    {
        Series = [series],
        Width = 300,
        Height = 200
    };
}
