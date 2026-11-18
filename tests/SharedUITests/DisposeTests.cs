using Factos;
using SharedUITests.Helpers;
using Xunit;

// to run these tests, see the UITests project, specially the program.cs file.
// to enable IDE intellisense for these tests, go to Directory.Build.props and set UITesting to true.

namespace SharedUITests;

public class DisposeTests
{
    public AppController App => AppController.Current;

#if XAML_UI_TESTING
    // https://github.com/Live-Charts/LiveCharts2/issues/1725
    //
    // A chart removed from the visual tree must be collectable — its inner MotionCanvas /
    // SKCanvasView occupy substantial memory and the original report describes them piling up
    // page-after-page on Maui 8. The symptom no longer reproduces on Maui 10 (Application
    // events are routed through a WeakEventManager) but the same invariant — unloaded chart
    // is GC-able — should hold on every Xaml platform we ship. This canary fires if a future
    // platform release (or our own code) regresses to a strong app-level root on charts:
    // a `this`-capturing event subscription that's never detached, an accumulator field, a
    // static collection, etc.
    //
    // Each platform's Test/Dispose/View exposes `ChangeContent()` that swaps the inner
    // content for a fresh one and returns the chart objects from the swapped-out instance.
    // We weak-ref the charts directly because Maui 10's Element.Parent is a WeakReference,
    // so testing leaks via "did the parent page survive?" silently misses real chart leaks.
    [AppTestMethod]
    public async Task UnloadedChartsShouldBeCollectable_Issue1725()
    {
        var sut = await App.NavigateTo<Samples.Test.Dispose.View>();
        await Task.Delay(1000);

        var weakRefs = new List<WeakReference>();

        const int Iterations = 5;
        // The Test/Dispose sample's swapped-in page hosts a CartesianChart, PieChart,
        // PolarChart, and GeoMap. The probe walks Grid.Children, so any future change to
        // the sample layout (extra children, wrapper panel) will diverge from this number
        // and fail the sanity check below — telling us the probe needs updating instead
        // of silently dropping coverage of the leak we were trying to canary.
        const int ChartsPerPage = 4;
        for (var i = 0; i < Iterations; i++)
        {
            // Add the weak refs in a non-async helper so the swapped-out chart references
            // are never bound to locals that the async state machine could hoist into
            // long-lived fields surviving across the awaits below.
            AddWeakRefs(weakRefs, sut.ChangeContent());
            await Task.Delay(500);
        }

        // One untracked extra swap to push the most-recent tracked instance out of any
        // transient reference the platform's layout/dispatcher holds for the latest unload.
        _ = sut.ChangeContent();

        // Let the platform's dispatcher drain any queued unload work.
        await Task.Delay(2000);

        GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
        GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);

        var alive = weakRefs.Count(r => r.IsAlive);
        var expected = Iterations * ChartsPerPage;
        Assert.True(
            alive == 0 && weakRefs.Count == expected,
            $"{alive}/{weakRefs.Count} unloaded chart instances were still alive after GC " +
            $"(expected {expected} tracked: {Iterations} swaps x {ChartsPerPage} charts/page). " +
            "If the count differs, the visual-tree probe in the platform's ChangeContent() " +
            "is no longer finding every chart and this canary has stopped covering part of " +
            "the leak scenario — update GetCharts() / ChartsPerPage to match the sample. " +
            "If alive > 0, an unloaded chart is rooted by something in the app — likely a " +
            "`this`-capturing event subscription that is never detached, or a static " +
            "collection / accumulator field. See " +
            "https://github.com/Live-Charts/LiveCharts2/issues/1725.");
    }

    private static void AddWeakRefs(List<WeakReference> refs, object[] objects)
    {
        foreach (var o in objects)
            refs.Add(new WeakReference(o));
    }

    // https://github.com/Live-Charts/LiveCharts2/issues/2404
    // Charts whose view leaves the visual tree for a while and is then added back (the same
    // instance, e.g. views reused as tabs by swapping a ContentView's content) must load and draw
    // again. On MAUI iOS / Mac Catalyst the #1725 leak fix disconnected the chart's handler on
    // that Unloaded (Window was null), MAUI never gave it a new one on the re-add, so Loaded
    // never fired again and the chart stayed blank. The test below can't catch it: it removes and
    // re-adds in the same call, before Unloaded runs.
    [AppTestMethod]
    public async Task ChartShouldDrawAfterLeavingTheTreeForAWhile_Issue2404()
    {
        var sut = await App.NavigateTo<Samples.Test.Dispose.View>();
        await Task.Delay(1000);

        for (var i = 0; i < 2; i++)
        {
            sut.RemoveContent();
            await Task.Delay(500); // long enough for the platform to raise Unloaded while the charts are out

            sut.RestoreContent();
            await Task.Delay(1500);

            AssertChartsAreLoaded(sut.GetCurrentCharts());
        }
    }

    // A chart removed and immediately re-added (same instance, in the same call) must survive the
    // round-trip and draw again. Guards the Apple-side #1725 fix in SourceGenDrawnView: releasing
    // and restoring PointerController's gesture recognizers must leave the chart working, and
    // reusing a controller whose recognizers were disposed must not crash in InitializeController.
    // It used to assert nothing, so a chart left blank by a re-add passed (#2404).
    [AppTestMethod]
    public async Task ChartShouldSurviveReattachOfSameInstance_Issue1725()
    {
        var sut = await App.NavigateTo<Samples.Test.Dispose.View>();
        await Task.Delay(1000);

        for (var i = 0; i < 3; i++)
        {
            sut.ReattachSameInstance();
            await Task.Delay(500);
        }

        await Task.Delay(1000);
        AssertChartsAreLoaded(sut.GetCurrentCharts());
    }

    // Stress version of the reattach test above: on CI's WinUI and Uno (Windows App SDK) runners a
    // chart sometimes ended up unloaded after an immediate remove + re-add. Many reattaches, with
    // different waits between them, make a timing-dependent failure show up reliably; on WinUI/Uno
    // the failure message carries the Loaded/Unloaded sequence of each chart and its MotionCanvas.
    [AppTestMethod]
    public async Task ChartsStayLoadedThroughManyImmediateReattaches()
    {
        var sut = await App.NavigateTo<Samples.Test.Dispose.View>();
        await Task.Delay(1000);

        var charts = sut.GetCurrentCharts();
        var log = new LoadedEventsLog(charts);

        for (var round = 0; round < 5; round++)
        {
            for (var i = 0; i < 6; i++)
            {
                sut.ReattachSameInstance();
                log.Mark($"r{round}.{i}");

                switch (i % 5)
                {
                    case 0: break;                          // next reattach right away
                    case 1: await Task.Yield(); break;      // one dispatcher turn
                    case 2: await Task.Delay(16); break;    // about one frame
                    case 3: await Task.Delay(100); break;
                    default: await Task.Delay(300); break;
                }
            }

            await Task.Delay(1500);

            foreach (var chart in charts)
            {
                if (!IsCoreLoaded(chart))
                    Assert.Fail($"round {round}: {chart.GetType().Name} is not loaded after the reattaches.\n{log}");
            }

            AssertChartsAreLoaded(charts);
        }
    }

    private static bool IsCoreLoaded(object chart) =>
        chart is LiveChartsCore.Kernel.Sketches.IChartView view && view.CoreChart.IsLoaded;

    // Records Loaded/Unloaded of each chart and of its MotionCanvas (WinUI and Uno only), with the
    // framework's IsLoaded, whether it has a parent and the chart engine's IsLoaded at that moment.
    private sealed class LoadedEventsLog
    {
        private readonly System.Text.StringBuilder _log = new();
        private readonly System.Diagnostics.Stopwatch _clock = System.Diagnostics.Stopwatch.StartNew();

        public LoadedEventsLog(object[] charts)
        {
#if WINUI_UI_TESTING || UNO_UI_TESTING
            foreach (var chart in charts)
            {
                var name = chart.GetType().Name;
                Watch((Microsoft.UI.Xaml.FrameworkElement)chart, name, chart);
                if (((Microsoft.UI.Xaml.Controls.UserControl)chart).Content is Microsoft.UI.Xaml.FrameworkElement canvas)
                    Watch(canvas, name + ".MotionCanvas", chart);
            }
#endif
        }

        public void Mark(string text) => _log.Append($"\n[{_clock.ElapsedMilliseconds}ms {text}] ");

        public override string ToString() => _log.Length == 0 ? "(no event log on this platform)" : _log.ToString();

#if WINUI_UI_TESTING || UNO_UI_TESTING
        private void Watch(Microsoft.UI.Xaml.FrameworkElement element, string name, object chart)
        {
            element.Loaded += (_, _) => Append(name, "Loaded", element, chart);
            element.Unloaded += (_, _) => Append(name, "Unloaded", element, chart);
        }

        private void Append(string name, string evt, Microsoft.UI.Xaml.FrameworkElement element, object chart) =>
            _log.Append($"{name}.{evt}(fe.IsLoaded={element.IsLoaded},parent={element.Parent is not null},core={IsCoreLoaded(chart)}) ");
#endif
    }

    // every chart of the Dispose test page (Cartesian, Pie, Polar and GeoMap) is loaded and has
    // drawn its points, and on MAUI still has a handler. Fails if a chart is missing or the page
    // holds something else, so no chart goes unchecked.
    private static void AssertChartsAreLoaded(object[] charts)
    {
        Assert.Equal(4, charts.Length);

        var maps = 0;
        foreach (var chart in charts)
        {
#if MAUI_UI_TESTING
            Assert.NotNull(((Microsoft.Maui.Controls.VisualElement)chart).Handler);
#endif
            // IGeoMapView derives from IChartView, so check the map first.
            if (chart is LiveChartsCore.Geo.IGeoMapView mapView)
            {
                Assert.ChartIsLoaded(mapView);
                maps++;
            }
            else if (chart is LiveChartsCore.Kernel.Sketches.IChartView chartView)
            {
                Assert.ChartIsLoaded(chartView);
            }
            else
            {
                Assert.Fail($"unexpected element in the Dispose test page: {chart.GetType().Name}");
            }
        }

        Assert.Equal(1, maps);
    }

#if MAUI_UI_TESTING
    // https://github.com/Live-Charts/LiveCharts2/issues/2297
    //
    // On iOS, MAUI's UITabBarController fires Unloaded for the chart on the tab
    // the user just switched AWAY from, even though the chart's element is still
    // mounted in the window — UIKit just hides the platform UIView. The #1725
    // Apple-side handler-disconnect ran on that Unloaded and set Handler=null;
    // MAUI then never raised Loaded when the user returned to the tab and the
    // chart went permanently blank. The fix gates the disconnect on Window ==
    // null, so transient tab-switch unloads no longer drop the handler.
    //
    // This is a MAUI-only contract test — TabbedPage and the Handler property
    // are MAUI concepts, and the regression view (Issue2297Repro) lives only
    // in MauiSample. On non-iOS MAUI targets the disconnect is gated by
    // `#if IOS || MACCATALYST` so the assertion always holds; the test still
    // runs there as a smoke check that the Window-null gate doesn't regress.
    [AppTestMethod]
    public async Task ChartHandlerSurvivesTabSwitch_Issue2297()
    {
        var sut = await App.NavigateTo<Samples.Test.Issue2297Repro.View>();
        await Task.Delay(1000);

        await sut.PushTabbedPageAsync();
        await Task.Delay(1500);

        var hiddenTabChart = sut.GetChart(0);
        Assert.NotNull(hiddenTabChart.Handler);

        sut.SwitchToTab(1);
        await Task.Delay(1500);

        Assert.NotNull(hiddenTabChart.Handler);

        await sut.PopTabbedPageAsync();
    }
#endif
#endif
}
