using LiveChartsCore.Drawing;
using LiveChartsCore.Measure;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.ChartTests;

[TestClass]
public class CollapsedDrawMarginTests
{
    // When the reserved margins exceed the control (a tiny resize, or an explicit DrawMargin wider
    // than the control), the draw margin collapses (Width <= 0 || Height <= 0). Measure used to just
    // `return` there — but the canvas keeps painting the previous frame's geometry at its old
    // transform, so the series looks frozen at the prior size. The fix clips every plot zone to a
    // zero-area (but non-Empty) rectangle so the stale content is hidden, and the next valid measure
    // restores the real clip via RegisterClipZones(). These pin that contract. An explicit oversized
    // DrawMargin is used to force the collapse deterministically (auto-margins clamp to a tiny
    // positive size instead).

    private static SKCartesianChart NewChart() => new()
    {
        Width = 300,
        Height = 300,
        AnimationsSpeed = System.TimeSpan.Zero,
        Series = [new LineSeries<double> { Values = [1, 2, 3, 4, 5] }],
        XAxes = [new Axis()],
        YAxes = [new Axis()],
        ExplicitDisposing = true,
    };

    // left + right (and top + bottom) exceed the 300px control, so DrawMarginSize goes negative.
    private static readonly Margin s_collapsing = new(200, 200, 200, 200);

    private static LvcRectangle DrawMarginClip(SKCartesianChart chart) =>
        chart.CoreCanvas.Zones[CanvasZone.DrawMargin].Clip;

    [TestMethod]
    public void CollapsedMargin_HidesPlotZone_NotEmptyClip()
    {
        var chart = NewChart();
        _ = chart.GetImage();

        // sanity: a healthy chart clips the draw-margin zone to a real, positive-area rect.
        var healthy = DrawMarginClip(chart);
        Assert.IsTrue(healthy.Width > 0 && healthy.Height > 0,
            $"expected a positive draw-margin clip when healthy, got {healthy.Width}x{healthy.Height}");

        // force the draw margin to collapse.
        chart.DrawMargin = s_collapsing;
        _ = chart.GetImage();

        var collapsed = DrawMarginClip(chart);

        // the zone is clipped to nothing (no plot pixels), so the stale frame can't show...
        Assert.IsTrue(collapsed.Width <= 0 && collapsed.Height <= 0,
            $"a collapsed draw margin must clip the plot zone to zero area, got {collapsed.Width}x{collapsed.Height}");

        // ...but it must NOT be LvcRectangle.Empty, which the drawing context treats as "no clip"
        // (draw everywhere) and would instead LEAVE the stale frame fully visible.
        Assert.AreNotEqual(LvcRectangle.Empty, collapsed,
            "the hide-clip must be a constructed zero-area rect, not LvcRectangle.Empty (= no clip)");
    }

    [TestMethod]
    public void RecoversToRealClip_AfterMarginValidAgain()
    {
        var chart = NewChart();
        _ = chart.GetImage();

        chart.DrawMargin = s_collapsing;
        _ = chart.GetImage();
        var collapsed = DrawMarginClip(chart);
        Assert.IsTrue(collapsed.Width <= 0 && collapsed.Height <= 0, "precondition: margin collapsed");

        // restore a valid layout: nothing was disposed, so a plain re-measure must restore a real
        // positive clip (RegisterClipZones overwrites the hide-clip).
        chart.DrawMargin = null;
        _ = chart.GetImage();

        var restored = DrawMarginClip(chart);
        Assert.IsTrue(restored.Width > 0 && restored.Height > 0,
            $"the draw-margin clip must recover after the margin is valid again, got {restored.Width}x{restored.Height}");
    }
}
