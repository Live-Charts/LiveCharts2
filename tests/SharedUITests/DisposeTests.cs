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

    // Smoke test: a chart removed and immediately re-added (same instance) must
    // survive the round-trip. Specifically guards the Apple-side fix in
    // SourceGenChart.OnUnloaded — `Handler?.DisconnectHandler()` plus the
    // platform's auto-disconnect default must leave the element ready for a
    // fresh handler on the next attach. If we ever reuse a handler with a
    // disposed PointerController (recognizers nulled by DisposeController on
    // Mac/iOS), this test crashes in InitializeController.
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
    }
#endif
}
