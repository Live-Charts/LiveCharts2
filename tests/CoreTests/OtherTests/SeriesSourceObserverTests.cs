using System.Collections.ObjectModel;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.Kernel.Observers;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.OtherTests;

[TestClass]
public class SeriesSourceObserverTests
{
    // A plain sealed class rather than a `record` — `record` requires
    // System.Runtime.CompilerServices.IsExternalInit which is missing on net462.
    private sealed class Source
    {
        public Source(double[] values) => Values = values;
        public double[] Values { get; }
    }

    private static SeriesSourceObserver CreateObserverFor(SKCartesianChart chart, bool canBuild = true) =>
        new(chart,
            o => new LineSeries<double> { Values = ((Source)o).Values },
            () => canBuild);

    [TestMethod]
    public void InitializeWithCanBuildFalseDoesNotTrackOrSetSeries()
    {
        var chart = new SKCartesianChart { Width = 200, Height = 200 };
        var observer = CreateObserverFor(chart, canBuild: false);

        observer.Initialize(new ObservableCollection<object> { new Source([1, 2, 3]) });

        Assert.AreEqual(0, chart.Series.Count());
    }

    [TestMethod]
    public void InitializeWithNonEnumerableDoesNothing()
    {
        var chart = new SKCartesianChart { Width = 200, Height = 200 };
        var observer = CreateObserverFor(chart);

        observer.Initialize(instance: 42);

        Assert.AreEqual(0, chart.Series.Count());
    }

    [TestMethod]
    public void InitializeWithPlainEnumerableMapsEachItemOnce()
    {
        var chart = new SKCartesianChart { Width = 200, Height = 200 };
        var observer = CreateObserverFor(chart);

        // A plain array is IEnumerable<object> but NOT INotifyCollectionChanged,
        // so the observer just maps once and does not subscribe.
        observer.Initialize(new object[] { new Source([1, 2]), new Source([3, 4]) });

        Assert.AreEqual(2, chart.Series.Count());
    }

    [TestMethod]
    public void ObservableSourceAddRemoveAndReplaceAreReflectedInChartSeries()
    {
        var chart = new SKCartesianChart { Width = 200, Height = 200 };
        var observer = CreateObserverFor(chart);

        var source = new ObservableCollection<object>();
        observer.Initialize(source);

        Assert.AreEqual(0, chart.Series.Count());

        var first = new Source([1, 2, 3]);
        var second = new Source([4, 5, 6]);

        // Add fires a CollectionChanged-Add — OnItemsAdded must run.
        source.Add(first);
        source.Add(second);
        Assert.AreEqual(2, chart.Series.Count());

        // Remove fires a CollectionChanged-Remove — OnItemsRemoved must run.
        _ = source.Remove(first);
        Assert.AreEqual(1, chart.Series.Count());

        // Replace fires a CollectionChanged-Replace — both Remove and Add branches run.
        source[0] = new Source([7, 8, 9]);
        Assert.AreEqual(1, chart.Series.Count());
    }

    [TestMethod]
    public void ResetActionIsHandledWithoutCrashing()
    {
        var chart = new SKCartesianChart { Width = 200, Height = 200 };
        var observer = CreateObserverFor(chart);

        var source = new ObservableCollection<object> { new Source([1]), new Source([2]) };
        observer.Initialize(source);
        Assert.AreEqual(2, chart.Series.Count());

        // Clear() on ObservableCollection raises a Reset action. The switch hits the
        // Reset case and calls Initialize(sender), which early-returns because the same
        // instance is already tracked. No visible change, but the Reset branch is run.
        source.Clear();
        // (No assertion on count — the important thing is the code path was exercised
        // without throwing.)
    }

    [TestMethod]
    public void DisposeUnsubscribesAndClearsChartSeries()
    {
        var chart = new SKCartesianChart { Width = 200, Height = 200 };
        var observer = CreateObserverFor(chart);

        var source = new ObservableCollection<object> { new Source([1]), new Source([2]) };
        observer.Initialize(source);
        Assert.AreEqual(2, chart.Series.Count());

        observer.Dispose();

        // After dispose the chart's series collection should be emptied and further
        // mutations on the source should no longer propagate.
        Assert.AreEqual(0, chart.Series.Count());

        source.Add(new Source([99]));
        Assert.AreEqual(0, chart.Series.Count());
    }

    [TestMethod]
    public void ReInitializingWithSameInstanceIsANoOp()
    {
        var chart = new SKCartesianChart { Width = 200, Height = 200 };
        var observer = CreateObserverFor(chart);

        var source = new ObservableCollection<object> { new Source([1]) };
        observer.Initialize(source);
        Assert.AreEqual(1, chart.Series.Count());

        // Passing the same tracked collection should early-return without rebuilding.
        observer.Initialize(source);
        Assert.AreEqual(1, chart.Series.Count());
    }
}
