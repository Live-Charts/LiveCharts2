using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.OtherTests;

[TestClass]
public class SkiaSharpDrawingContextTests
{
    // Regression for #2303. When an application hosts a LiveCharts chart from both
    // Avalonia and WPF (or any other backend) in the same process, the Avalonia
    // chart used to "poison" the WPF chart so that animation frames stacked on top
    // of each other instead of clearing. Root cause: clearCanvasOnNewFrame used to
    // be a process-static field, and Avalonia/Uno-Skia static initializers flipped
    // it to false because their host clears the surface before invoking Render.
    // Once flipped, every other SkiaSharpDrawingContext in the process (WPF, MAUI,
    // WinForms, Blazor, in-memory) silently skipped its own clear too.
    //
    // The fix moved the flag onto the instance via a constructor parameter. This
    // test pins that contract: constructing an opt-out context must not affect a
    // sibling default-constructed context. Verified by pre-filling a surface with
    // a known colour, constructing an opt-out context (the Avalonia/Uno-Skia
    // path), then constructing a default context and calling OnBeginDraw on it —
    // the surface pixel must be the default context's Background, not the
    // pre-fill colour.
    [TestMethod]
    public void OnBeginDraw_OnDefaultContext_ClearsCanvas_EvenWhenAnotherContextOptedOut()
    {
        using var surface = SKSurface.Create(new SKImageInfo(10, 10));
        var motion = new CoreMotionCanvas();

        // mimic the Avalonia / Uno-Skia call site that opts out of clearing
        _ = new SkiaSharpDrawingContext(
            motion, surface.Canvas, SKColor.Empty, clearCanvasOnNewFrame: false);

        // pre-fill with a colour distinct from the upcoming Background
        surface.Canvas.Clear(SKColors.Blue);

        // a default-construct context (WPF / MAUI / WinForms / Blazor) MUST still clear
        var defaultCtx = new SkiaSharpDrawingContext(motion, surface.Canvas, SKColors.Red);
        defaultCtx.OnBeginDraw();

        using var snapshot = surface.Snapshot();
        using var pixmap = snapshot.PeekPixels();

        Assert.AreEqual(
            SKColors.Red,
            pixmap.GetPixelColor(5, 5),
            "Avalonia/Uno-Skia opt-out from clearCanvasOnNewFrame must be per-instance " +
            "— a default context in the same process must still clear on OnBeginDraw.");
    }

    [TestMethod]
    public void OnBeginDraw_OnOptedOutContext_DoesNotClearCanvas()
    {
        using var surface = SKSurface.Create(new SKImageInfo(10, 10));
        var motion = new CoreMotionCanvas();

        surface.Canvas.Clear(SKColors.Blue);

        var optedOut = new SkiaSharpDrawingContext(
            motion, surface.Canvas, SKColors.Red, clearCanvasOnNewFrame: false);
        optedOut.OnBeginDraw();

        using var snapshot = surface.Snapshot();
        using var pixmap = snapshot.PeekPixels();

        Assert.AreEqual(
            SKColors.Blue,
            pixmap.GetPixelColor(5, 5),
            "A context constructed with clearCanvasOnNewFrame:false must not touch " +
            "the canvas in OnBeginDraw — the host is responsible for clearing.");
    }
}
