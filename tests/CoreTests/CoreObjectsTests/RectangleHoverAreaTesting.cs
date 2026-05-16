using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Drawing;
using LiveChartsCore.Measure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.CoreObjectsTests;

[TestClass]
public class RectangleHoverAreaTesting
{
    // Contract that the documented #1847 sentinel-coordinate recipe relies on:
    // a zero-height column hover area (produced by Mapping non-finite values
    // to Coordinate(index, 0)) must still trigger a tooltip when the chart
    // resolves the find via an X-only strategy. Internally the rectangle is
    // clamped to at least 1 px in each dimension before the X containment
    // check so the user can hover anywhere in the bar's column and still hit
    // the area.
    //
    // If this test breaks, the docs section "NaN and Infinity" -> "Showing
    // the infinity sign in the tooltip" needs to be revisited.
    [TestMethod]
    public void IsPointerOver_ZeroHeight_Tooltipable_OnXOnlyStrategy()
    {
        // Bar's column at x in [100, 150], collapsed to zero height at y=200.
        var ha = new RectangleHoverArea();
        _ = ha.SetDimensions(x: 100, y: 200, width: 50, height: 0);

        // Pointer anywhere within the column on the X-only strategy must hit.
        Assert.IsTrue(ha.IsPointerOver(new LvcPoint(125, 50), FindingStrategy.CompareOnlyXTakeClosest),
            "X within column, Y far above the zero-height area should still hit.");
        Assert.IsTrue(ha.IsPointerOver(new LvcPoint(125, 200), FindingStrategy.CompareOnlyXTakeClosest),
            "X within column, Y at the area should hit.");
        Assert.IsTrue(ha.IsPointerOver(new LvcPoint(125, 350), FindingStrategy.CompareOnlyXTakeClosest),
            "X within column, Y far below the zero-height area should still hit.");

        // Pointer outside the column must miss.
        Assert.IsFalse(ha.IsPointerOver(new LvcPoint(50, 200), FindingStrategy.CompareOnlyXTakeClosest),
            "X to the left of the column should miss.");
        Assert.IsFalse(ha.IsPointerOver(new LvcPoint(180, 200), FindingStrategy.CompareOnlyXTakeClosest),
            "X to the right of the column should miss.");
    }

    [TestMethod]
    public void IsPointerOver_ZeroHeight_NotTooltipable_OnYOnlyStrategy()
    {
        // The Y-only strategy intentionally rejects the zero-height area,
        // even after the 1 px clamp - the clamp is just enough to give an
        // exact-match a chance, not enough to span the chart vertically.
        var ha = new RectangleHoverArea();
        _ = ha.SetDimensions(x: 100, y: 200, width: 50, height: 0);

        Assert.IsFalse(ha.IsPointerOver(new LvcPoint(125, 50), FindingStrategy.CompareOnlyYTakeClosest));
        Assert.IsFalse(ha.IsPointerOver(new LvcPoint(125, 350), FindingStrategy.CompareOnlyYTakeClosest));

        // The clamp does still gives an exact-Y hit at the area's row.
        Assert.IsTrue(ha.IsPointerOver(new LvcPoint(125, 200.5f), FindingStrategy.CompareOnlyYTakeClosest));
    }
}
