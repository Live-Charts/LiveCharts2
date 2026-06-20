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
    [TestMethod] public void TreemapSeries_Releases() => AssertTreemapReleases();
    [TestMethod] public void TreemapSeries_Releases_Hierarchical() => AssertTreemapHierarchicalReleases();
    [TestMethod] public void SankeySeries_Releases_Vertical() => AssertSankeyReleases(LiveChartsCore.Kernel.Sketches.SankeyLayoutKind.Vertical);
    [TestMethod] public void SankeySeries_Releases_BipartiteArc() => AssertSankeyReleases(LiveChartsCore.Kernel.Sketches.SankeyLayoutKind.BipartiteArc);

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

    // Treemap can't share WireAndDetach: its model contract is TreemapNode (not
    // ObservableValue) and its retained-state shape is per-node visual/label
    // dictionaries keyed by reference, not the ChartPoint round-trip. The shape
    // of the assertion is the same — draw, detach, redraw, force GC, no survivors.
    private static void AssertTreemapReleases()
    {
        var series = new TreemapSeries<TreemapNode>();
        var chart = new SKTreemapChart
        {
            Series = [series],
            Width = 300,
            Height = 200
        };
        var refs = WireAndDetachTreemap(series, chart);
        AssertAllDead(refs, "treemap");
    }

    private static List<WeakReference> WireAndDetachTreemap(
        TreemapSeries<TreemapNode> series, SKTreemapChart chart)
    {
        var refs = new List<WeakReference>(PointCount);
        var values = new TreemapNode[PointCount];
        for (var i = 0; i < PointCount; i++)
        {
            values[i] = new TreemapNode(i + 1, $"N{i}");
            refs.Add(new WeakReference(values[i]));
        }
        series.Values = values;

        _ = ChangingPaintTasks.DrawChart(chart, animated: false);

        series.Values = Array.Empty<TreemapNode>();

        _ = ChangingPaintTasks.DrawChart(chart, animated: false);

        return refs;
    }

    // Hierarchical variant: descendants reach _nodeVisuals via the recursive
    // EmitTile → LayoutSiblings(children) path, not via the top-level Values
    // foreach. If the reaper misses descendants, this would catch it.
    private static void AssertTreemapHierarchicalReleases()
    {
        var series = new TreemapSeries<TreemapNode>();
        var chart = new SKTreemapChart
        {
            Series = [series],
            Width = 300,
            Height = 200
        };
        var refs = WireAndDetachTreemapHierarchical(series, chart);
        AssertAllDead(refs, "treemap-hierarchical");
    }

    private static List<WeakReference> WireAndDetachTreemapHierarchical(
        TreemapSeries<TreemapNode> series, SKTreemapChart chart)
    {
        // One root with PointCount children → PointCount + 1 instances under
        // weak references. The root has no own Value so the children-rollup
        // path (ResolveValue → ChildrenMapper) is exercised too.
        var children = new TreemapNode[PointCount];
        var refs = new List<WeakReference>(PointCount + 1);
        for (var i = 0; i < PointCount; i++)
        {
            children[i] = new TreemapNode(i + 1, $"N{i}");
            refs.Add(new WeakReference(children[i]));
        }
        var root = new TreemapNode("Root", children);
        refs.Add(new WeakReference(root));
        series.Values = [root];

        _ = ChangingPaintTasks.DrawChart(chart, animated: false);

        series.Values = Array.Empty<TreemapNode>();

        _ = ChangingPaintTasks.DrawChart(chart, animated: false);

        return refs;
    }

    // Sankey can't share WireAndDetach: its data shape is two parallel
    // collections (Values=nodes, Links=edges) and its retained state spans
    // four reference-keyed dictionaries — _nodeVisuals + _linkVisuals for
    // Vertical or _arcNodeVisuals + _chordLinkVisuals for BipartiteArc, plus
    // _nodeLabels + _nodePoints. Run the same draw-detach-redraw dance,
    // assert no node/link instance survives. Parameterized over layout so
    // both reaper paths are exercised.
    private static void AssertSankeyReleases(LiveChartsCore.Kernel.Sketches.SankeyLayoutKind layout)
    {
        var series = new SankeySeries<SankeyNode> { Layout = layout };
        var chart = new SKSankeyChart
        {
            Series = [series],
            Width = 300,
            Height = 200
        };
        var refs = WireAndDetachSankey(series, chart);
        AssertAllDead(refs, layout == LiveChartsCore.Kernel.Sketches.SankeyLayoutKind.BipartiteArc
            ? "sankey-bipartiteArc"
            : "sankey-vertical");
    }

    private static List<WeakReference> WireAndDetachSankey(
        SankeySeries<SankeyNode> series, SKSankeyChart chart)
    {
        // Strict bipartite layout: first half of the nodes are sources, second
        // half sinks — works for both Vertical (any column count) and
        // BipartiteArc (rejects multi-column flows).
        var nodes = new SankeyNode[PointCount];
        var refs = new List<WeakReference>(PointCount + PointCount / 2);
        for (var i = 0; i < PointCount; i++)
        {
            nodes[i] = new SankeyNode($"N{i}");
            refs.Add(new WeakReference(nodes[i]));
        }
        var links = new SankeyLink<SankeyNode>[PointCount / 2];
        for (var i = 0; i < links.Length; i++)
        {
            links[i] = new SankeyLink<SankeyNode>(nodes[i], nodes[PointCount / 2 + i], i + 1);
            refs.Add(new WeakReference(links[i]));
        }
        series.Values = nodes;
        series.Links = links;

        _ = ChangingPaintTasks.DrawChart(chart, animated: false);

        series.Values = Array.Empty<SankeyNode>();
        series.Links = Array.Empty<SankeyLink<SankeyNode>>();

        _ = ChangingPaintTasks.DrawChart(chart, animated: false);

        return refs;
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
