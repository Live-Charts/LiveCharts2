using System;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Drawing;
using LiveChartsCore.Measure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.OtherTests;

// Pie slice hit testing (SemicircleHoverArea.IsPointerOver). Angles are absolute, in degrees:
// a slice goes from StartAngle to EndAngle = StartAngle + SweepAngle.
[TestClass]
public class SemicircleHoverAreaTests
{
    // https://github.com/Live-Charts/LiveCharts2/issues/2394
    // GetActualEndAngle subtracted 0.01 degrees from the end, so a slice narrower than that
    // (less than 1/36000 of the pie) ended before it started, the hit test read it as a slice
    // wrapping almost the whole circle, and its tooltip showed wherever the pointer was.
    [TestMethod]
    [DataRow(359.999f, 360f, 179d, DisplayName = "tiny last slice (the issue: values 359.999 and 0.001)")]
    [DataRow(0f, 0.001f, 179d, DisplayName = "tiny first slice")]
    [DataRow(90f, 90.005f, 179d, DisplayName = "tiny middle slice")]
    [DataRow(359.999f, 360.00003f, 181d, DisplayName = "tiny slice whose end comes out over 360 by float rounding")]
    public void TinySliceIsNotHitAwayFromIt(float start, float end, double pointerAngle)
    {
        Assert.IsFalse(IsOver(start, end, pointerAngle));
    }

    [TestMethod]
    public void TinySliceIsHitOnIt()
    {
        Assert.IsTrue(IsOver(359.999f, 360f, 359.9995));
    }

    // the cases the old 0.01 degree workaround was there for, and the usual ones, keep working.
    [TestMethod]
    [DataRow(0f, 359.999f, 179d, true, DisplayName = "big slice next to a tiny one")]
    [DataRow(0f, 360f, 359.5, true, DisplayName = "full pie, near 360")]
    [DataRow(0f, 360f, 0.5, true, DisplayName = "full pie, near 0")]
    [DataRow(180f, 270f, 225d, true, DisplayName = "last slice of a pie rotated -90")]
    [DataRow(-90f, 0f, 300d, true, DisplayName = "slice ending exactly at 0 (rotated -90), inside")]
    [DataRow(-90f, 0f, 10d, false, DisplayName = "slice ending exactly at 0 (rotated -90), outside")]
    [DataRow(350f, 370f, 5d, true, DisplayName = "slice wrapping past 360, inside")]
    [DataRow(350f, 370f, 20d, false, DisplayName = "slice wrapping past 360, outside")]
    [DataRow(315f, 405f, 20d, true, DisplayName = "pie rotated 45, inside")]
    [DataRow(315f, 405f, 60d, false, DisplayName = "pie rotated 45, outside")]
    public void RegularSlices(float start, float end, double pointerAngle, bool expected)
    {
        Assert.AreEqual(expected, IsOver(start, end, pointerAngle));
    }

    // a pie centered at (100, 100) with radius 100, the pointer 50 px from the center at the given angle.
    private static bool IsOver(float startAngle, float endAngle, double pointerAngle)
    {
        var area = new SemicircleHoverArea().SetDimensions(100, 100, startAngle, endAngle, 0, 200);
        var rad = pointerAngle * Math.PI / 180;
        var pointer = new LvcPoint((float)(100 + 50 * Math.Cos(rad)), (float)(100 + 50 * Math.Sin(rad)));
        return area.IsPointerOver(pointer, FindingStrategy.Automatic);
    }
}
