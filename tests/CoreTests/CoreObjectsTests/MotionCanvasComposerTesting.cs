using LiveChartsCore.Motion;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.CoreObjectsTests;

[TestClass]
public class MotionCanvasComposerTesting
{
    // Regression for #2029. WPF's TabControl raises Loaded on a chart that lives
    // in a not-yet-selected tab (size 0), then raises Loaded *again* — with no
    // Unloaded between them — once the tab is reparented into the active content
    // host. Without an idempotency guard the composer's Initialize re-subscribed
    // OnPaintSurface and FrameRequest → DrawFrame, so every paint applied the
    // HiDPI Canvas.Scale twice (e.g. 1.75² ≈ 3) and the chart rendered at
    // multiple-times the intended scale.
    [TestMethod]
    public void Initialize_IsIdempotent_WithoutInterveningDispose()
    {
        var renderMode = new SpyRenderMode();
        var ticker = new SpyFrameTicker();
        var composer = new MotionCanvasComposer(renderMode, ticker);
        var canvas = new CoreMotionCanvas();

        composer.Initialize(canvas);
        composer.Initialize(canvas);

        Assert.AreEqual(1, renderMode.InitializeCount, "InitializeRenderMode must run once.");
        Assert.AreEqual(1, ticker.InitializeCount, "InitializeTicker must run once.");
        Assert.AreEqual(1, renderMode.FrameRequestSubscriberCount, "FrameRequest must have a single subscriber.");
    }

    [TestMethod]
    public void Dispose_AfterDoubleInitialize_RestoresCleanState()
    {
        var renderMode = new SpyRenderMode();
        var ticker = new SpyFrameTicker();
        var composer = new MotionCanvasComposer(renderMode, ticker);
        var canvas = new CoreMotionCanvas();

        composer.Initialize(canvas);
        composer.Initialize(canvas);
        composer.Dispose(canvas);

        Assert.AreEqual(0, renderMode.FrameRequestSubscriberCount, "Dispose must unsubscribe FrameRequest.");
        Assert.AreEqual(1, renderMode.DisposeCount, "DisposeRenderMode must run once.");
        Assert.AreEqual(1, ticker.DisposeCount, "DisposeTicker must run once.");

        // After a full Dispose, Initialize must work again (re-entering the
        // Initialize → Dispose → Initialize cycle on a future tab switch).
        composer.Initialize(canvas);

        Assert.AreEqual(2, renderMode.InitializeCount);
        Assert.AreEqual(1, renderMode.FrameRequestSubscriberCount);
    }

    private sealed class SpyRenderMode : IRenderMode
    {
        private CoreMotionCanvas.FrameRequestHandler? _frameRequest;

        public int InitializeCount { get; private set; }
        public int DisposeCount { get; private set; }
        public int FrameRequestSubscriberCount =>
            _frameRequest?.GetInvocationList().Length ?? 0;

        public event CoreMotionCanvas.FrameRequestHandler FrameRequest
        {
            add => _frameRequest += value;
            remove => _frameRequest -= value;
        }

        public void InitializeRenderMode(CoreMotionCanvas canvas) => InitializeCount++;
        public void DisposeRenderMode() => DisposeCount++;
        public void InvalidateRenderer() { }
    }

    private sealed class SpyFrameTicker : IFrameTicker
    {
        public int InitializeCount { get; private set; }
        public int DisposeCount { get; private set; }

        public void InitializeTicker(CoreMotionCanvas canvas, IRenderMode renderMode) => InitializeCount++;
        public void DisposeTicker() => DisposeCount++;
    }
}
