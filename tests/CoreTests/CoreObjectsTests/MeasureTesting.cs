using System;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Drawing;
using LiveChartsCore.Measure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.CoreObjectsTests;

[TestClass]
public class MeasureTesting
{
    // ========== Bounds Tests ==========

    [TestMethod]
    public void BoundsDefaultIsEmpty()
    {
        var bounds = new Bounds();
        Assert.IsTrue(bounds.IsEmpty);
        Assert.IsTrue(Math.Abs(bounds.Max - double.MinValue) < 0.001);
        Assert.IsTrue(Math.Abs(bounds.Min - double.MaxValue) < 0.001);
    }

    [TestMethod]
    public void BoundsConstructorWithMinMax()
    {
        var bounds = new Bounds(10, 100);
        Assert.IsTrue(Math.Abs(bounds.Min - 10) < 0.001);
        Assert.IsTrue(Math.Abs(bounds.Max - 100) < 0.001);
    }

    [TestMethod]
    public void BoundsCopyConstructor()
    {
        var original = new Bounds(5, 50);
        original.AppendValue(10);
        original.AppendValue(20);

        var copy = new Bounds(original);
        Assert.IsTrue(Math.Abs(copy.Min - original.Min) < 0.001);
        Assert.IsTrue(Math.Abs(copy.Max - original.Max) < 0.001);
        Assert.IsTrue(copy.IsEmpty == original.IsEmpty);
        Assert.IsTrue(Math.Abs(copy.MinDelta - original.MinDelta) < 0.001);
    }

    [TestMethod]
    public void BoundsAppendValueUpdatesMinAndMax()
    {
        var bounds = new Bounds();
        bounds.AppendValue(5);
        bounds.AppendValue(15);
        bounds.AppendValue(10);

        Assert.IsTrue(!bounds.IsEmpty);
        Assert.IsTrue(Math.Abs(bounds.Min - 5) < 0.001);
        Assert.IsTrue(Math.Abs(bounds.Max - 15) < 0.001);
    }

    [TestMethod]
    public void BoundsAppendSingleValue()
    {
        var bounds = new Bounds();
        bounds.AppendValue(42);

        Assert.IsTrue(!bounds.IsEmpty);
        Assert.IsTrue(Math.Abs(bounds.Min - 42) < 0.001);
        Assert.IsTrue(Math.Abs(bounds.Max - 42) < 0.001);
    }

    [TestMethod]
    public void BoundsDelta()
    {
        var bounds = new Bounds();
        bounds.AppendValue(10);
        bounds.AppendValue(30);

        Assert.IsTrue(Math.Abs(bounds.Delta - 20) < 0.001);
    }

    [TestMethod]
    public void BoundsAppendBounds()
    {
        var bounds1 = new Bounds();
        bounds1.AppendValue(5);
        bounds1.AppendValue(15);

        var bounds2 = new Bounds();
        bounds2.AppendValue(3);
        bounds2.AppendValue(20);

        bounds1.AppendValue(bounds2);

        Assert.IsTrue(Math.Abs(bounds1.Min - 3) < 0.001);
        Assert.IsTrue(Math.Abs(bounds1.Max - 20) < 0.001);
    }

    [TestMethod]
    public void BoundsAppendNegativeValues()
    {
        var bounds = new Bounds();
        bounds.AppendValue(-10);
        bounds.AppendValue(-5);
        bounds.AppendValue(-20);

        Assert.IsTrue(Math.Abs(bounds.Min - (-20)) < 0.001);
        Assert.IsTrue(Math.Abs(bounds.Max - (-5)) < 0.001);
    }

    // ========== Margin Tests ==========

    [TestMethod]
    public void MarginDefaultValues()
    {
        var margin = new Margin();
        Assert.IsTrue(Math.Abs(margin.Left - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(margin.Top - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(margin.Right - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(margin.Bottom - 0) < 0.001f);
    }

    [TestMethod]
    public void MarginFourArgConstructor()
    {
        var margin = new Margin(10, 20, 30, 40);
        Assert.IsTrue(Math.Abs(margin.Left - 10) < 0.001f);
        Assert.IsTrue(Math.Abs(margin.Top - 20) < 0.001f);
        Assert.IsTrue(Math.Abs(margin.Right - 30) < 0.001f);
        Assert.IsTrue(Math.Abs(margin.Bottom - 40) < 0.001f);
    }

    [TestMethod]
    public void MarginSingleArgConstructor()
    {
        var margin = new Margin(15);
        Assert.IsTrue(Math.Abs(margin.Left - 15) < 0.001f);
        Assert.IsTrue(Math.Abs(margin.Top - 15) < 0.001f);
        Assert.IsTrue(Math.Abs(margin.Right - 15) < 0.001f);
        Assert.IsTrue(Math.Abs(margin.Bottom - 15) < 0.001f);
    }

    [TestMethod]
    public void MarginTwoArgConstructor()
    {
        var margin = new Margin(10, 20);
        Assert.IsTrue(Math.Abs(margin.Left - 10) < 0.001f);
        Assert.IsTrue(Math.Abs(margin.Top - 20) < 0.001f);
        Assert.IsTrue(Math.Abs(margin.Right - 10) < 0.001f);
        Assert.IsTrue(Math.Abs(margin.Bottom - 20) < 0.001f);
    }

    [TestMethod]
    public void MarginAutoIsNaN()
    {
        Assert.IsTrue(float.IsNaN(Margin.Auto));
    }

    [TestMethod]
    public void MarginIsAutoReturnsTrueForNaN()
    {
        Assert.IsTrue(Margin.IsAuto(float.NaN));
        Assert.IsTrue(Margin.IsAuto(Margin.Auto));
    }

    [TestMethod]
    public void MarginIsAutoReturnsFalseForRegularValues()
    {
        Assert.IsTrue(!Margin.IsAuto(0));
        Assert.IsTrue(!Margin.IsAuto(10));
        Assert.IsTrue(!Margin.IsAuto(-5));
    }

    // ========== AxisLimit Tests ==========

    [TestMethod]
    public void AxisLimitValidateLimitsWhenBothDefined()
    {
        double min = 10, max = 100;
        AxisLimit.ValidateLimits(ref min, ref max, 0);

        // Both defined, should not change
        Assert.IsTrue(Math.Abs(min - 10) < 0.001);
        Assert.IsTrue(Math.Abs(max - 100) < 0.001);
    }

    [TestMethod]
    public void AxisLimitValidateLimitsWhenOnlyMinDefined()
    {
        double min = 10, max = double.MinValue;
        AxisLimit.ValidateLimits(ref min, ref max, 0);

        Assert.IsTrue(Math.Abs(min - 10) < 0.001);
        Assert.IsTrue(max > min);
    }

    [TestMethod]
    public void AxisLimitValidateLimitsWhenOnlyMaxDefined()
    {
        double min = double.MaxValue, max = 100;
        AxisLimit.ValidateLimits(ref min, ref max, 0);

        Assert.IsTrue(Math.Abs(max - 100) < 0.001);
        Assert.IsTrue(min < max);
    }

    [TestMethod]
    public void AxisLimitValidateLimitsWhenNoneDefined()
    {
        double min = double.MaxValue, max = double.MinValue;
        AxisLimit.ValidateLimits(ref min, ref max, 0);

        Assert.IsTrue(Math.Abs(min - 0) < 0.001);
        Assert.IsTrue(Math.Abs(max - 10) < 0.001);
    }

    [TestMethod]
    public void AxisLimitValidateLimitsWithStep()
    {
        double min = double.MaxValue, max = double.MinValue;
        AxisLimit.ValidateLimits(ref min, ref max, 5);

        Assert.IsTrue(Math.Abs(min - 0) < 0.001);
        Assert.IsTrue(Math.Abs(max - 50) < 0.001); // 5 * 10
    }

    [TestMethod]
    public void AxisLimitStruct()
    {
        var limit = new AxisLimit(0, 100, 1, 5, 95);
        Assert.IsTrue(Math.Abs(limit.Min - 0) < 0.001);
        Assert.IsTrue(Math.Abs(limit.Max - 100) < 0.001);
        Assert.IsTrue(Math.Abs(limit.MinDelta - 1) < 0.001);
        Assert.IsTrue(Math.Abs(limit.DataMin - 5) < 0.001);
        Assert.IsTrue(Math.Abs(limit.DataMax - 95) < 0.001);
    }

    // ========== Coordinate Tests ==========

    [TestMethod]
    public void CoordinateXYConstructor()
    {
        var coord = new Coordinate(10, 20);
        Assert.IsTrue(!coord.IsEmpty);
        Assert.IsTrue(Math.Abs(coord.SecondaryValue - 10) < 0.001);
        Assert.IsTrue(Math.Abs(coord.PrimaryValue - 20) < 0.001);
    }

    [TestMethod]
    public void CoordinateXYWeightConstructor()
    {
        var coord = new Coordinate(10, 20, 5);
        Assert.IsTrue(!coord.IsEmpty);
        Assert.IsTrue(Math.Abs(coord.SecondaryValue - 10) < 0.001);
        Assert.IsTrue(Math.Abs(coord.PrimaryValue - 20) < 0.001);
        Assert.IsTrue(Math.Abs(coord.TertiaryValue - 5) < 0.001);
    }

    [TestMethod]
    public void CoordinateFinancialConstructor()
    {
        var coord = new Coordinate(100, 150, 110, 140, 90);
        Assert.IsTrue(!coord.IsEmpty);
        Assert.IsTrue(Math.Abs(coord.SecondaryValue - 100) < 0.001);
        Assert.IsTrue(Math.Abs(coord.PrimaryValue - 150) < 0.001);
        Assert.IsTrue(Math.Abs(coord.TertiaryValue - 110) < 0.001);
        Assert.IsTrue(Math.Abs(coord.QuaternaryValue - 140) < 0.001);
        Assert.IsTrue(Math.Abs(coord.QuinaryValue - 90) < 0.001);
    }

    [TestMethod]
    public void CoordinateBoxConstructor()
    {
        var coord = new Coordinate(0, 100, 75, 25, 0, 50);
        Assert.IsTrue(!coord.IsEmpty);
    }

    [TestMethod]
    public void CoordinateEmpty()
    {
        var coord = Coordinate.Empty;
        Assert.IsTrue(coord.IsEmpty);
    }

    [TestMethod]
    public void CoordinateToString()
    {
        var coord = new Coordinate(10, 20);
        var str = coord.ToString();
        Assert.IsTrue(str.Contains("10"));
        Assert.IsTrue(str.Contains("20"));
    }

    [TestMethod]
    public void CoordinateFullConstructor()
    {
        var error = new Error(1, 2, 3, 4);
        var coord = new Coordinate(10, 20, 30, 40, 50, 60, error);

        Assert.IsTrue(!coord.IsEmpty);
        Assert.IsTrue(Math.Abs(coord.PrimaryValue - 10) < 0.001);
        Assert.IsTrue(Math.Abs(coord.SecondaryValue - 20) < 0.001);
        Assert.IsTrue(Math.Abs(coord.TertiaryValue - 30) < 0.001);
        Assert.IsTrue(Math.Abs(coord.QuaternaryValue - 40) < 0.001);
        Assert.IsTrue(Math.Abs(coord.QuinaryValue - 50) < 0.001);
        Assert.IsTrue(Math.Abs(coord.SenaryValue - 60) < 0.001);
        Assert.IsTrue(Math.Abs(coord.PointError.Xi - 1) < 0.001);
        Assert.IsTrue(Math.Abs(coord.PointError.Xj - 2) < 0.001);
        Assert.IsTrue(Math.Abs(coord.PointError.Yi - 3) < 0.001);
        Assert.IsTrue(Math.Abs(coord.PointError.Yj - 4) < 0.001);
    }

    // ========== DimensionalBounds Tests ==========

    [TestMethod]
    public void DimensionalBoundsDefaultConstructor()
    {
        var db = new DimensionalBounds();
        Assert.IsTrue(db.PrimaryBounds is not null);
        Assert.IsTrue(db.SecondaryBounds is not null);
        Assert.IsTrue(db.TertiaryBounds is not null);
        Assert.IsTrue(db.VisiblePrimaryBounds is not null);
        Assert.IsTrue(db.VisibleSecondaryBounds is not null);
        Assert.IsTrue(db.VisibleTertiaryBounds is not null);
    }

    // ========== RectangleHoverArea Tests ==========

    [TestMethod]
    public void RectangleHoverAreaSetDimensions()
    {
        var area = new RectangleHoverArea();
        area.SetDimensions(10, 20, 100, 50);

        Assert.IsTrue(Math.Abs(area.X - 10) < 0.001f);
        Assert.IsTrue(Math.Abs(area.Y - 20) < 0.001f);
        Assert.IsTrue(Math.Abs(area.Width - 100) < 0.001f);
        Assert.IsTrue(Math.Abs(area.Height - 50) < 0.001f);
    }

    [TestMethod]
    public void RectangleHoverAreaConstructorWithArgs()
    {
        var area = new RectangleHoverArea(10, 20, 100, 50);

        Assert.IsTrue(Math.Abs(area.X - 10) < 0.001f);
        Assert.IsTrue(Math.Abs(area.Y - 20) < 0.001f);
        Assert.IsTrue(Math.Abs(area.Width - 100) < 0.001f);
        Assert.IsTrue(Math.Abs(area.Height - 50) < 0.001f);
    }

    [TestMethod]
    public void RectangleHoverAreaIsPointerOverCompareAll()
    {
        var area = new RectangleHoverArea(10, 20, 100, 50);

        // Point inside
        Assert.IsTrue(area.IsPointerOver(new LvcPoint(50, 40), FindingStrategy.CompareAll));

        // Point outside
        Assert.IsTrue(!area.IsPointerOver(new LvcPoint(5, 40), FindingStrategy.CompareAll));
        Assert.IsTrue(!area.IsPointerOver(new LvcPoint(50, 5), FindingStrategy.CompareAll));
        Assert.IsTrue(!area.IsPointerOver(new LvcPoint(200, 40), FindingStrategy.CompareAll));
        Assert.IsTrue(!area.IsPointerOver(new LvcPoint(50, 200), FindingStrategy.CompareAll));
    }

    [TestMethod]
    public void RectangleHoverAreaIsPointerOverCompareOnlyX()
    {
        var area = new RectangleHoverArea(10, 20, 100, 50);

        // Point within X range (Y doesn't matter)
        Assert.IsTrue(area.IsPointerOver(new LvcPoint(50, 999), FindingStrategy.CompareOnlyX));
        // Point outside X range
        Assert.IsTrue(!area.IsPointerOver(new LvcPoint(5, 30), FindingStrategy.CompareOnlyX));
    }

    [TestMethod]
    public void RectangleHoverAreaIsPointerOverCompareOnlyY()
    {
        var area = new RectangleHoverArea(10, 20, 100, 50);

        // Point within Y range (X doesn't matter)
        Assert.IsTrue(area.IsPointerOver(new LvcPoint(999, 40), FindingStrategy.CompareOnlyY));
        // Point outside Y range
        Assert.IsTrue(!area.IsPointerOver(new LvcPoint(50, 5), FindingStrategy.CompareOnlyY));
    }

    [TestMethod]
    public void RectangleHoverAreaDistanceToCompareAll()
    {
        var area = new RectangleHoverArea(0, 0, 100, 100);
        var distance = area.DistanceTo(new LvcPoint(50, 50), FindingStrategy.CompareAll);

        // Distance from (50,50) to center of rectangle (50,50) should be 0
        Assert.IsTrue(Math.Abs(distance - 0) < 0.001);
    }

    [TestMethod]
    public void RectangleHoverAreaDistanceToCompareOnlyX()
    {
        var area = new RectangleHoverArea(0, 0, 100, 100);
        var distance = area.DistanceTo(new LvcPoint(75, 0), FindingStrategy.CompareOnlyX);

        // Should be |75 - 50| = 25
        Assert.IsTrue(Math.Abs(distance - 25) < 0.001);
    }

    [TestMethod]
    public void RectangleHoverAreaDistanceToCompareOnlyY()
    {
        var area = new RectangleHoverArea(0, 0, 100, 100);
        var distance = area.DistanceTo(new LvcPoint(0, 75), FindingStrategy.CompareOnlyY);

        // Should be |75 - 50| = 25
        Assert.IsTrue(Math.Abs(distance - 25) < 0.001);
    }

    [TestMethod]
    public void RectangleHoverAreaCenterToolTip()
    {
        var area = new RectangleHoverArea(100, 200, 50, 30);
        area.CenterXToolTip().CenterYToolTip();

        Assert.IsTrue(Math.Abs(area.SuggestedTooltipLocation.X - 125) < 0.001f);
        Assert.IsTrue(Math.Abs(area.SuggestedTooltipLocation.Y - 215) < 0.001f);
    }

    [TestMethod]
    public void RectangleHoverAreaStartToolTip()
    {
        var area = new RectangleHoverArea(100, 200, 50, 30);
        area.StartXToolTip().StartYToolTip();

        Assert.IsTrue(Math.Abs(area.SuggestedTooltipLocation.X - 100) < 0.001f);
        Assert.IsTrue(Math.Abs(area.SuggestedTooltipLocation.Y - 200) < 0.001f);
    }

    [TestMethod]
    public void RectangleHoverAreaEndToolTip()
    {
        var area = new RectangleHoverArea(100, 200, 50, 30);
        area.EndXToolTip().EndYToolTip();

        Assert.IsTrue(Math.Abs(area.SuggestedTooltipLocation.X - 150) < 0.001f);
        Assert.IsTrue(Math.Abs(area.SuggestedTooltipLocation.Y - 230) < 0.001f);
    }

    [TestMethod]
    public void RectangleHoverAreaIsLessThanPivot()
    {
        var area = new RectangleHoverArea(0, 0, 100, 50);
        Assert.IsTrue(!area.LessThanPivot);
        area.IsLessThanPivot();
        Assert.IsTrue(area.LessThanPivot);
    }

    [TestMethod]
    public void RectangleHoverAreaMinWidthAndHeight()
    {
        // When width or height is < 1, IsPointerOver uses 1 as minimum
        var area = new RectangleHoverArea(50, 50, 0, 0);

        // Even with 0 width/height, a point exactly at the location should still work
        // because it uses 1 pixel minimum
        var isOver = area.IsPointerOver(new LvcPoint(50.5f, 50.5f), FindingStrategy.CompareAll);
        Assert.IsTrue(isOver);
    }

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

    // ========== SemicircleHoverArea Tests ==========

    [TestMethod]
    public void SemicircleHoverAreaSetDimensions()
    {
        var area = new SemicircleHoverArea();
        area.SetDimensions(100, 100, 0, 90, 10, 50);

        Assert.IsTrue(Math.Abs(area.CenterX - 100) < 0.001f);
        Assert.IsTrue(Math.Abs(area.CenterY - 100) < 0.001f);
        Assert.IsTrue(Math.Abs(area.StartAngle - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(area.EndAngle - 90) < 0.001f);
        Assert.IsTrue(Math.Abs(area.InnerRadius - 10) < 0.001f);
        Assert.IsTrue(Math.Abs(area.Radius - 50) < 0.001f);
    }

    [TestMethod]
    public void SemicircleHoverAreaDistanceTo()
    {
        var area = new SemicircleHoverArea();
        area.SetDimensions(100, 100, 0, 90, 0, 50);

        var distance = area.DistanceTo(new LvcPoint(100, 100), FindingStrategy.CompareAll);
        Assert.IsTrue(distance >= 0);
    }
}
