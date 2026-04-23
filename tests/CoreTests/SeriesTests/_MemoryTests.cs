using System;
using System.Collections.Generic;
using System.Linq;
using CoreTests.CoreObjectsTests;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsGeneratedCode;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.SeriesTests;

// Leak detection for series point instances.
//
// The old version of these tests stressed each series with ~150,000 operations and asserted
// on a fuzzy GC.GetTotalMemory() threshold. That caught leaks but was slow (~3 min per test
// method) and imprecise. This version uses WeakReference: create N points, wire them into a
// chart, detach, force GC, assert nothing survives. Deterministic, precise, and runs in
// seconds. See https://github.com/Live-Charts/LiveCharts2/pull/2160 for the benchmarks that
// separately track allocation-per-operation regressions.
[TestClass]
public class _MemoryTests
{
    // Small enough to keep the test fast, large enough that a leak is obvious.
    private const int PointCount = 20;

    // --- Cartesian series ------------------------------------------------------------

    [TestMethod] public void BoxSeries_Releases() => AssertCartesianReleases(() => new BoxSeries<ObservableValue>());
    [TestMethod] public void CandlesticksSeries_Releases() => AssertCartesianReleases(() => new CandlesticksSeries<ObservableValue>());
    [TestMethod] public void ColumnSeries_Releases() => AssertCartesianReleases(() => new ColumnSeries<ObservableValue>());
    [TestMethod] public void HeatSeries_Releases() => AssertCartesianReleases(() => new HeatSeries<ObservableValue>());
    [TestMethod] public void LineSeries_Releases() => AssertCartesianReleases(() => new LineSeries<ObservableValue>());
    [TestMethod] public void RowSeries_Releases() => AssertCartesianReleases(() => new RowSeries<ObservableValue>());
    [TestMethod] public void ScatterSeries_Releases() => AssertCartesianReleases(() => new ScatterSeries<ObservableValue>());
    [TestMethod] public void StepLineSeries_Releases() => AssertCartesianReleases(() => new StepLineSeries<ObservableValue>());
    [TestMethod] public void StackedAreaSeries_Releases() => AssertCartesianReleases(() => new StackedAreaSeries<ObservableValue>());
    [TestMethod] public void StackedColumnSeries_Releases() => AssertCartesianReleases(() => new StackedColumnSeries<ObservableValue>());

    [TestMethod] public void PieSeries_Releases() => AssertPieReleases();
    [TestMethod] public void PolarLineSeries_Releases() => AssertPolarReleases();

    // --- Indexed-values path ---------------------------------------------------------

    // Exercises DataFactory.EnumerateIndexedEntities and its _chartIndexEntityMap — the
    // path taken when Values is a collection of a reference type that does NOT implement
    // IChartEntity. The model instances are referenced via ChartPoint.Context.DataSource;
    // the test asserts they are released once Values is detached.
    [TestMethod]
    public void MappedModel_Releases_OnIndexedPath()
    {
        var series = new LineSeries<MappedModel>
        {
            Mapping = (model, index) => new Coordinate(index, model.Value)
        };
        var chart = new SKCartesianChart
        {
            Series = [series],
            Width = 300,
            Height = 200
        };
        var refs = WireAndDetachMapped(series, chart);
        AssertAllDead(refs, "mapped");
    }

    private sealed class MappedModel
    {
        public double Value { get; set; }
    }

    private static List<WeakReference> WireAndDetachMapped(
        LineSeries<MappedModel> series, SourceGenSKChart chart)
    {
        var refs = new List<WeakReference>(PointCount);
        var values = new MappedModel[PointCount];
        for (var i = 0; i < PointCount; i++)
        {
            values[i] = new MappedModel { Value = i + 1 };
            refs.Add(new WeakReference(values[i]));
        }
        series.Values = values;

        _ = ChangingPaintTasks.DrawChart(chart, animated: false);

        series.Values = Array.Empty<MappedModel>();

        _ = ChangingPaintTasks.DrawChart(chart, animated: false);

        return refs;
    }

    // --- Assertion helpers -----------------------------------------------------------

    private static void AssertCartesianReleases(Func<ISeries> factory)
    {
        var series = factory();
        var chart = new SKCartesianChart
        {
            Series = [series],
            Width = 300,
            Height = 200
        };
        var refs = WireAndDetach(series, chart);
        AssertAllDead(refs, series.Name ?? series.GetType().Name);
    }

    private static void AssertPieReleases()
    {
        var series = new PieSeries<ObservableValue>();
        var chart = new SKPieChart
        {
            Series = [series],
            Width = 300,
            Height = 200
        };
        var refs = WireAndDetach(series, chart);
        AssertAllDead(refs, "pie");
    }

    private static void AssertPolarReleases()
    {
        var series = new PolarLineSeries<ObservableValue>();
        var chart = new SKPolarChart
        {
            Series = [series],
            Width = 300,
            Height = 200
        };
        var refs = WireAndDetach(series, chart);
        AssertAllDead(refs, "polar");
    }

    // Create N observables, render once so they become ChartPoints, then replace Values
    // with an empty collection and render again so the cleanup pass runs. Returns weak
    // references that should be dead after the caller forces GC. The local `values`
    // array is on the stack of this method and goes away when the method returns.
    private static List<WeakReference> WireAndDetach(ISeries series, SourceGenSKChart chart)
    {
        var refs = new List<WeakReference>(PointCount);
        var values = new ObservableValue[PointCount];
        for (var i = 0; i < PointCount; i++)
        {
            values[i] = new ObservableValue(i + 1);
            refs.Add(new WeakReference(values[i]));
        }
        series.Values = values;

        // First draw: points get materialized into the chart's everFetched set.
        _ = ChangingPaintTasks.DrawChart(chart, animated: false);

        // Detach — a fresh empty array so no hidden alias to the original.
        series.Values = Array.Empty<ObservableValue>();

        // Second draw: CollectPoints runs during Invalidate and prunes stale ChartPoints,
        // releasing their entity references.
        _ = ChangingPaintTasks.DrawChart(chart, animated: false);

        return refs;
    }

    private static void AssertAllDead(List<WeakReference> refs, string label)
    {
        GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
        GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);

        var alive = refs.Count(r => r.IsAlive);
        Assert.AreEqual(
            0, alive,
            $"[{label}] {alive}/{refs.Count} point instances were not released after Values were replaced.");
    }
}
