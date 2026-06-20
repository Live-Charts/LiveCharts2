using LiveChartsCore.SkiaSharpView.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.OtherTests;

// Regression coverage for issue #1523: Maui CPU/GPU render modes used to cache
// DeviceDisplay.MainDisplayInfo.Density at init and only refresh it when the
// MAIN display info changed. Moving the window to an extended display with a
// different scale produced misscaled output. The fix derives the per-paint
// scale from the surface SkiaSharp gives us, via this helper.
[TestClass]
public class RenderScaleTests
{
    [TestMethod]
    public void PixelDensityFromSurface_returns_one_when_view_not_yet_sized()
    {
        // Before first layout the view's DIP width can be 0 or -1 (Maui default).
        // We must not produce 0 or a negative scale — the canvas would collapse.
        Assert.AreEqual(1f, RenderScale.PixelDensityFromSurface(800, 0));
        Assert.AreEqual(1f, RenderScale.PixelDensityFromSurface(800, -1));
    }

    [TestMethod]
    public void PixelDensityFromSurface_matches_actual_surface_to_view_ratio()
    {
        // 800px surface for a 400-DIP view means the OS is rendering at 2x.
        Assert.AreEqual(2f, RenderScale.PixelDensityFromSurface(800, 400));

        // 1000px / 400dip = 2.5x (e.g. external monitor with non-integer scale,
        // exactly the case from issue #1523 that the cached MainDisplayInfo path missed).
        Assert.AreEqual(2.5f, RenderScale.PixelDensityFromSurface(1000, 400));

        // 1x display: surface and DIP sizes match.
        Assert.AreEqual(1f, RenderScale.PixelDensityFromSurface(400, 400));
    }
}
