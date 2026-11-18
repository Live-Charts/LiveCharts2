using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.Motion;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.ChartTests;

[TestClass]
public class TooltipZoneTests
{
    // Issue #2304: the tooltip used to register its DrawnTask in CanvasZone.NoClip
    // (1), but Y-axis ticks live in CanvasZone.YCrosshair (3) and zones draw in
    // declaration order. Result: axis ticks paint on top of the tooltip on any
    // multi-Y-axis chart, regardless of the tooltip's ZIndex = 10100.
    //
    // The pixel manifestation depends on whether the tooltip's auto-placement
    // happens to overlap a tick path in the rendered layout, so this test pins
    // the contract directly: the tooltip's task lands in the canvas zone that is
    // DRAWN last, after every axis-related zone (the last one in CanvasZone.DrawOrder,
    // not the last one in the zones array: Underlay, #2364, is drawn first). The test
    // does not name CanvasZone.Overlay so it would still fail behaviourally (delta = 0,
    // not 1) if the new constant were renamed; it only asserts the routing.

    [TestMethod]
    public void TooltipLandsInLastCanvasZone_Issue2304()
    {
        var chart = new SKCartesianChart
        {
            Width = 300,
            Height = 300,
            Series = [new LineSeries<double> { Values = [1, 2, 3, 4, 5] }],
            XAxes = [new Axis()],
            YAxes = [new Axis()],
            ExplicitDisposing = true
        };

        // first render — no pointer set, tooltip not shown yet.
        _ = chart.GetImage();
        var lastZoneIndex = CanvasZone.DrawOrder[CanvasZone.DrawOrder.Length - 1];
        var lastZoneBefore = chart.CoreCanvas.Zones[lastZoneIndex].CountTasks();

        // open the tooltip
        chart.CoreChart._isPointerIn = true;
        chart.CoreChart._isToolTipOpen = true;
        chart.CoreChart._pointerPosition = new(150, 150);
        _ = chart.GetImage();

        var lastZoneAfter = chart.CoreCanvas.Zones[lastZoneIndex].CountTasks();

        Assert.AreEqual(
            1,
            lastZoneAfter - lastZoneBefore,
            $"Tooltip task must land in the canvas zone drawn last (index {lastZoneIndex}). " +
            $"Before tooltip: {lastZoneBefore} tasks, after: {lastZoneAfter}. " +
            "If the delta is 0 the tooltip went into an earlier zone and axis ticks will paint over it.");
    }

    [TestMethod]
    public void AddGeometry_ThrowsForMissingZone()
    {
        var canvas = new CoreMotionCanvas();
        var missingZone = canvas.Zones.Length;

        _ = Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => canvas.AddGeometry(missingZone));
    }
}
