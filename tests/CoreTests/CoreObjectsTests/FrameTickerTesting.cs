using LiveChartsCore.Motion;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.CoreObjectsTests;

[TestClass]
public class FrameTickerTesting
{
    // Regression for #2216. The reporter sees an NRE in
    // CompositionTargetTicker.DisposeTicker on WPF when a Prism-style
    // ContentControl swap unloads a TabControl that hosts a chart while the
    // chart tab is not the active one — Unloaded fires on the MotionCanvas
    // without the ticker ever having been initialized (or with a previous
    // DisposeTicker already having nulled _canvas), and the unsubscribe of
    // _canvas.Invalidated dereferences null.
    //
    // The fix is a defensive null guard on the ticker's DisposeTicker(). The
    // contract is: it must be safe to call DisposeTicker even when
    // InitializeTicker was never called, or when DisposeTicker was already
    // called. CompositionTargetTicker is WPF-internal and not reachable from
    // CoreTests, but AsyncLoopTicker is the parallel core-side implementation
    // and shares the same null-deref shape — fixing one without the other
    // would just relocate the bug.
    //
    // Every IFrameTicker implementation was audited for the same shape:
    // ApplicationIdleTicker (WinForms) and the Android / iOS-MacCatalyst
    // NativeFrameTicker partials had the unguarded _canvas.Invalidated -=
    // (Android / Mac also dereferenced _vsyncTicker / _displayLink) and were
    // guarded the same way. The WinUI and Uno-Skia NativeFrameTicker partials
    // were already null-safe and were normalized to the identical idiom. The
    // NoOS NativeFrameTicker delegates to AsyncLoopTicker. None of those
    // tickers are reachable from CoreTests (platform TFMs / project refs), so
    // this test pins the contract for the whole family via AsyncLoopTicker.
    [TestMethod]
    public void AsyncLoopTicker_DisposeWithoutInitialize_DoesNotThrow()
    {
        var ticker = new AsyncLoopTicker();

        ticker.DisposeTicker();
    }

    [TestMethod]
    public void AsyncLoopTicker_DoubleDispose_DoesNotThrow()
    {
        var ticker = new AsyncLoopTicker();
        var canvas = new CoreMotionCanvas();
        var renderMode = new NoopRenderMode();

        ticker.InitializeTicker(canvas, renderMode);
        ticker.DisposeTicker();
        ticker.DisposeTicker();
    }

    // Regression for #2020/#2333. AsyncLoopTicker is purely event-driven: it
    // only starts its drawing loop when CoreMotionCanvas.Invalidated fires.
    // On Uno's native renderer (e.g. WASM native) the chart engine can
    // invalidate the canvas before the view raises Loaded — that is, before
    // MotionCanvasComposer.Initialize subscribes the ticker. A canvas that is
    // already invalid at InitializeTicker time never raises another event on
    // its own, so the ticker idled forever and the chart froze before its
    // first frame. The contract pinned here: initializing the ticker against
    // an already-invalid canvas must start the drawing loop immediately.
    [TestMethod]
    public void AsyncLoopTicker_CanvasInvalidatedBeforeInitialize_StartsDrawingLoop()
    {
        var ticker = new AsyncLoopTicker();
        var canvas = new CoreMotionCanvas();
        var renderMode = new CountingRenderMode();

        // the chart engine updated before the view loaded
        canvas.Invalidate();

        // the first loop iteration runs synchronously inside InitializeTicker
        ticker.InitializeTicker(canvas, renderMode);

        ticker.DisposeTicker();

        Assert.IsTrue(renderMode.InvalidateCount >= 1);
    }

    private sealed class NoopRenderMode : IRenderMode
    {
        public event CoreMotionCanvas.FrameRequestHandler FrameRequest { add { } remove { } }

        public void InitializeRenderMode(CoreMotionCanvas canvas) { }
        public void DisposeRenderMode() { }
        public void InvalidateRenderer() { }
    }

    private sealed class CountingRenderMode : IRenderMode
    {
        public int InvalidateCount { get; private set; }

        public event CoreMotionCanvas.FrameRequestHandler FrameRequest { add { } remove { } }

        public void InitializeRenderMode(CoreMotionCanvas canvas) { }
        public void DisposeRenderMode() { }
        public void InvalidateRenderer() => InvalidateCount++;
    }
}
