using System;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Drawing;
using LiveChartsCore.Measure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.OtherTests;

[TestClass]
public class RectangleHoverAreaTests
{
    // Regression for #2165: stacked column/row series with negative values produce
    // RectangleHoverArea instances whose Width or Height is negative. IsPointerOver
    // must treat (X, X+Width) and (Y, Y+Height) as unordered ranges.

    [TestMethod]
    public void RectangleHoverAreaIsPointerOverNegativeHeight()
    {
        // Same rect as a stacked column going downward from baseline:
        // top edge at y=20, bottom edge at y=70, expressed as Y=70 / Height=-50.
        var area = new RectangleHoverArea(10, 70, 100, -50);

        // Point that is geometrically inside the rectangle.
        Assert.IsTrue(area.IsPointerOver(new LvcPoint(50, 40), FindingStrategy.CompareAll));
        Assert.IsTrue(area.IsPointerOver(new LvcPoint(50, 40), FindingStrategy.CompareAllTakeClosest));
        Assert.IsTrue(area.IsPointerOver(new LvcPoint(50, 40), FindingStrategy.CompareOnlyY));

        // Outside in Y on either side of the (unordered) range.
        Assert.IsTrue(!area.IsPointerOver(new LvcPoint(50, 10), FindingStrategy.CompareAll));
        Assert.IsTrue(!area.IsPointerOver(new LvcPoint(50, 80), FindingStrategy.CompareAll));
    }

    [TestMethod]
    public void RectangleHoverAreaIsPointerOverNegativeWidth()
    {
        // Same shape but for a row series: X=110 / Width=-100 spans x in [10, 110].
        var area = new RectangleHoverArea(110, 20, -100, 50);

        Assert.IsTrue(area.IsPointerOver(new LvcPoint(50, 40), FindingStrategy.CompareAll));
        Assert.IsTrue(area.IsPointerOver(new LvcPoint(50, 40), FindingStrategy.CompareOnlyX));

        Assert.IsTrue(!area.IsPointerOver(new LvcPoint(5, 40), FindingStrategy.CompareAll));
        Assert.IsTrue(!area.IsPointerOver(new LvcPoint(120, 40), FindingStrategy.CompareAll));
    }

    [TestMethod]
    public void RectangleHoverAreaIsPointerOverNegativeWidthAndHeight()
    {
        // Both negative — rectangle covers x in [10, 110], y in [20, 70].
        var area = new RectangleHoverArea(110, 70, -100, -50);

        Assert.IsTrue(area.IsPointerOver(new LvcPoint(50, 40), FindingStrategy.CompareAll));
        Assert.IsTrue(!area.IsPointerOver(new LvcPoint(5, 40), FindingStrategy.CompareAll));
        Assert.IsTrue(!area.IsPointerOver(new LvcPoint(50, 80), FindingStrategy.CompareAll));
    }

    [TestMethod]
    public void RectangleHoverAreaDistanceToWithNegativeDimensions()
    {
        // Center is at (X + Width/2, Y + Height/2) regardless of sign.
        var area = new RectangleHoverArea(110, 70, -100, -50); // center: (60, 45)
        var distance = area.DistanceTo(new LvcPoint(60, 45), FindingStrategy.CompareAll);

        Assert.IsTrue(Math.Abs(distance - 0) < 0.001);
    }
}
