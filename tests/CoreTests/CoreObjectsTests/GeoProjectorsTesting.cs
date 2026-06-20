using System;
using LiveChartsCore.Geo;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.CoreObjectsTests;

[TestClass]
public class GeoProjectorsTesting
{
    [TestMethod]
    public void MercatorProjectorProjectsOriginToCenter()
    {
        // The default clip (-65° to +85°) is asymmetric — the equator no
        // longer sits at the visual center. Use the symmetric ±85° full-earth
        // bounds here to assert the classic origin-at-center invariant.
        var projector = new MercatorProjector(360, 180, 0, 0, -85, 85, -180, 180);
        var result = projector.ToMap(new double[] { 0, 0 });

        Assert.IsTrue(Math.Abs(result[0] - 180) < 0.1f);
        Assert.IsTrue(Math.Abs(result[1] - 90) < 0.1f);
    }

    [TestMethod]
    public void MercatorProjectorProjectsTopLeft()
    {
        var projector = new MercatorProjector(360, 180, 0, 0);
        // lon=-180, lat=0 should be at left edge
        var result = projector.ToMap(new double[] { -180, 0 });

        Assert.IsTrue(Math.Abs(result[0] - 0) < 0.1f);
    }

    [TestMethod]
    public void MercatorProjectorProjectsTopRight()
    {
        var projector = new MercatorProjector(360, 180, 0, 0);
        // lon=180, lat=0 should be at right edge
        var result = projector.ToMap(new double[] { 180, 0 });

        Assert.IsTrue(Math.Abs(result[0] - 360) < 0.1f);
    }

    [TestMethod]
    public void MercatorProjectorRespectsOffset()
    {
        var projector = new MercatorProjector(360, 180, 50, 25);
        var result = projector.ToMap(new double[] { -180, 0 });

        Assert.IsTrue(Math.Abs(result[0] - 50) < 0.1f);
    }

    [TestMethod]
    public void MercatorProjectorProperties()
    {
        var projector = new MercatorProjector(800, 600, 10, 20);
        Assert.IsTrue(Math.Abs(projector.MapWidth - 800) < 0.001f);
        Assert.IsTrue(Math.Abs(projector.MapHeight - 600) < 0.001f);
        Assert.IsTrue(Math.Abs(projector.XOffset - 10) < 0.001f);
        Assert.IsTrue(Math.Abs(projector.YOffset - 20) < 0.001f);
    }

    [TestMethod]
    public void MercatorProjectorPreferredRatio()
    {
        // Default asymmetric clip (-65° → +85°, full longitude) →
        // aspect ≈ 2π / (mercN(85°) − mercN(−65°)) ≈ 1.355
        // (slightly landscape, fits typical dashboard canvases well).
        var ratio = MercatorProjector.PreferredRatio;
        Assert.IsTrue(ratio.Length == 2);
        Assert.IsTrue(Math.Abs(ratio[0] - 1.355f) < 0.01f, $"width ratio: {ratio[0]}");
        Assert.IsTrue(Math.Abs(ratio[1] - 1f) < 0.001f);

        // Symmetric ±85° gives the classic ~1:1 (square) Mercator aspect.
        var fullEarth = MercatorProjector.GetPreferredRatio(-85, 85, -180, 180);
        Assert.IsTrue(Math.Abs(fullEarth[0] - 1f) < 0.01f, $"width ratio @±85°: {fullEarth[0]}");
        Assert.IsTrue(Math.Abs(fullEarth[1] - 1f) < 0.001f);

        // NaN args fall back to the projection's defaults — same as the
        // PreferredRatio property above.
        var defaulted = MercatorProjector.GetPreferredRatio(double.NaN, double.NaN, double.NaN, double.NaN);
        Assert.IsTrue(Math.Abs(defaulted[0] - 1.355f) < 0.01f);
    }

    [TestMethod]
    public void MercatorProjectorIsAlwaysVisible()
    {
        var projector = new MercatorProjector(360, 180, 0, 0);
        Assert.IsTrue(projector.IsVisible(0, 0));
        Assert.IsTrue(projector.IsVisible(180, 90));
        Assert.IsTrue(projector.IsVisible(-180, -90));
    }

    // Both projectors normalize degenerate / inverted bounds so the public
    // Min/Max Lat/Lon API never crashes the chart on user error. Inverted
    // bounds swap; equal bounds (which would otherwise divide by zero / a
    // zero mercN range) fall back to the projection defaults.
    [TestMethod]
    public void MercatorProjectorNormalizesDegenerateAndInvertedBounds()
    {
        // Inverted lat (min > max) should swap silently — same render as the
        // non-inverted pair.
        var inverted = MercatorProjector.GetPreferredRatio(72, 35, -25, 45);
        var normal = MercatorProjector.GetPreferredRatio(35, 72, -25, 45);
        Assert.AreEqual(normal[0], inverted[0], 0.001f);
        Assert.AreEqual(normal[1], inverted[1], 0.001f);

        // Equal lat → fall back to defaults; no NaN/Infinity in the result.
        var degenerate = MercatorProjector.GetPreferredRatio(50, 50, -25, 45);
        Assert.IsTrue(IsFinite(degenerate[0]));
        Assert.IsTrue(IsFinite(degenerate[1]));

        // ToMap stays finite for the degenerate constructor too.
        var projector = new MercatorProjector(360, 180, 0, 0, 50, 50, -25, 45);
        projector.ToMap(0, 0, out var x, out var y);
        Assert.IsTrue(IsFinite(x));
        Assert.IsTrue(IsFinite(y));
    }

    [TestMethod]
    public void ControlCoordinatesProjectorNormalizesDegenerateAndInvertedBounds()
    {
        var inverted = ControlCoordinatesProjector.GetPreferredRatio(72, 35, 45, -25);
        var normal = ControlCoordinatesProjector.GetPreferredRatio(35, 72, -25, 45);
        Assert.AreEqual(normal[0], inverted[0], 0.001f);
        Assert.AreEqual(normal[1], inverted[1], 0.001f);

        var degenerate = ControlCoordinatesProjector.GetPreferredRatio(50, 50, -25, 45);
        Assert.IsTrue(IsFinite(degenerate[0]));
        Assert.IsTrue(IsFinite(degenerate[1]));

        var projector = new ControlCoordinatesProjector(360, 180, 0, 0, 50, 50, -25, 45);
        projector.ToMap(0, 0, out var x, out var y);
        Assert.IsTrue(IsFinite(x));
        Assert.IsTrue(IsFinite(y));
    }

    // float.IsFinite is netstandard2.1+; targeting net462 means hand-rolling it.
    private static bool IsFinite(float v) => !float.IsNaN(v) && !float.IsInfinity(v);

    [TestMethod]
    public void OrthographicProjectorProjectsCenterToScreenCenter()
    {
        var projector = new OrthographicProjector(500, 500, 0, 0, 0, 0);
        var result = projector.ToMap(new double[] { 0, 0 });

        Assert.IsTrue(Math.Abs(result[0] - 250) < 0.1f);
        Assert.IsTrue(Math.Abs(result[1] - 250) < 0.1f);
    }

    [TestMethod]
    public void OrthographicProjectorCenterPointIsVisible()
    {
        var projector = new OrthographicProjector(500, 500, 0, 0, 0, 0);
        Assert.IsTrue(projector.IsVisible(0, 0));
    }

    [TestMethod]
    public void OrthographicProjectorOppositePointIsNotVisible()
    {
        var projector = new OrthographicProjector(500, 500, 0, 0, 0, 0);
        Assert.IsTrue(!projector.IsVisible(180, 0));
    }

    [TestMethod]
    public void OrthographicProjectorPointJustBeyondHorizonIsNotVisible()
    {
        var projector = new OrthographicProjector(500, 500, 0, 0, 0, 0);
        Assert.IsTrue(!projector.IsVisible(91, 0));
    }

    [TestMethod]
    public void OrthographicProjectorPointJustBeforeHorizonIsVisible()
    {
        var projector = new OrthographicProjector(500, 500, 0, 0, 0, 0);
        Assert.IsTrue(projector.IsVisible(89, 0));
    }

    [TestMethod]
    public void OrthographicProjectorProperties()
    {
        var projector = new OrthographicProjector(800, 600, 10, 20, 45, 30);
        Assert.IsTrue(Math.Abs(projector.MapWidth - 800) < 0.001f);
        Assert.IsTrue(Math.Abs(projector.MapHeight - 600) < 0.001f);
        Assert.IsTrue(Math.Abs(projector.XOffset - 10) < 0.001f);
        Assert.IsTrue(Math.Abs(projector.YOffset - 20) < 0.001f);
        Assert.IsTrue(Math.Abs(projector.CenterLongitude - 45) < 0.001);
        Assert.IsTrue(Math.Abs(projector.CenterLatitude - 30) < 0.001);
        Assert.IsTrue(projector.Radius > 0);
    }

    [TestMethod]
    public void OrthographicProjectorPreferredRatio()
    {
        var ratio = OrthographicProjector.PreferredRatio;
        Assert.IsTrue(ratio.Length == 2);
        Assert.IsTrue(Math.Abs(ratio[0] - 1) < 0.001f);
        Assert.IsTrue(Math.Abs(ratio[1] - 1) < 0.001f);
    }

    [TestMethod]
    public void OrthographicProjectorRadiusIsHalfMinDimension()
    {
        var projector = new OrthographicProjector(800, 600, 0, 0, 0, 0);
        Assert.IsTrue(Math.Abs(projector.Radius - 300) < 0.001f);
    }

    [TestMethod]
    public void OrthographicProjectorScreenCenter()
    {
        var projector = new OrthographicProjector(800, 600, 50, 30, 0, 0);
        Assert.IsTrue(Math.Abs(projector.ScreenCenterX - 450) < 0.001f);
        Assert.IsTrue(Math.Abs(projector.ScreenCenterY - 330) < 0.001f);
    }

    [TestMethod]
    public void OrthographicProjectorVisibilityWithDifferentCenter()
    {
        // Globe facing New York (lon=-74, lat=40.7)
        var projector = new OrthographicProjector(500, 500, 0, 0, -74, 40.7);

        // Nearby should be visible
        Assert.IsTrue(projector.IsVisible(-74, 40.7));  // center
        Assert.IsTrue(projector.IsVisible(-80, 35));     // nearby

        // Tokyo (far side) should not be visible
        Assert.IsTrue(!projector.IsVisible(139.7, 35.7));
    }

    [TestMethod]
    public void OrthographicProjectorSymmetry()
    {
        var projector = new OrthographicProjector(500, 500, 0, 0, 0, 0);

        // Points at equal positive and negative longitude should mirror
        var left = projector.ToMap(new double[] { -45, 0 });
        var right = projector.ToMap(new double[] { 45, 0 });

        // Both should be equal distance from center
        var centerX = 250f;
        Assert.IsTrue(Math.Abs(Math.Abs(left[0] - centerX) - Math.Abs(right[0] - centerX)) < 0.1f);

        // Y coordinates should be equal
        Assert.IsTrue(Math.Abs(left[1] - right[1]) < 0.1f);
    }

    // Round-trip tests for the new ToCoordinates inverse on each projector.
    // ToMap → ToCoordinates → original lon/lat (within tolerance).
    [TestMethod]
    public void MercatorProjectorRoundTrips()
    {
        var projector = new MercatorProjector(800, 600, 50, 30);
        // 1e-4 tolerance: ToMap returns float (~7 decimal-digit precision),
        // so a round trip through float pixel coords inherently loses ~5e-6
        // degrees at the equator. 1e-4 is loose enough to absorb the float
        // loss and tight enough to catch any real bug in the inverse math.
        foreach (var (lon, lat) in new[] { (0d, 0d), (-74d, 40.7d), (139.69d, 35.69d), (-3.7d, 40.4d), (45d, -25d) })
        {
            projector.ToMap(lon, lat, out var x, out var y);
            Assert.IsTrue(projector.ToCoordinates(x, y, out var lon2, out var lat2));
            Assert.AreEqual(lon, lon2, 1e-4, $"lon round-trip @ ({lon}, {lat})");
            Assert.AreEqual(lat, lat2, 1e-4, $"lat round-trip @ ({lon}, {lat})");
        }
    }

    [TestMethod]
    public void ControlCoordinatesProjectorRoundTrips()
    {
        var projector = new ControlCoordinatesProjector(800, 600, 50, 30);
        foreach (var (lon, lat) in new[] { (0d, 0d), (-74d, 40.7d), (139.69d, 35.69d), (-3.7d, 40.4d), (45d, -25d) })
        {
            projector.ToMap(lon, lat, out var x, out var y);
            Assert.IsTrue(projector.ToCoordinates(x, y, out var lon2, out var lat2));
            Assert.AreEqual(lon, lon2, 1e-4);
            Assert.AreEqual(lat, lat2, 1e-4);
        }
    }

    [TestMethod]
    public void OrthographicProjectorRoundTripsOnVisibleHemisphere()
    {
        // Globe centered on the Atlantic; only visible-side coordinates
        // round-trip. The back hemisphere is two-to-one with the disc so
        // there's nothing to invert.
        var projector = new OrthographicProjector(500, 500, 0, 0, -30, 20);

        // Visible points: round-trip exactly.
        foreach (var (lon, lat) in new[] { (-30d, 20d), (-74d, 40.7d), (-3.7d, 40.4d), (15d, 50d) })
        {
            Assert.IsTrue(projector.IsVisible(lon, lat), $"({lon}, {lat}) should be visible");
            projector.ToMap(lon, lat, out var x, out var y);
            Assert.IsTrue(projector.ToCoordinates(x, y, out var lon2, out var lat2));
            Assert.AreEqual(lon, lon2, 1e-2, $"lon round-trip @ ({lon}, {lat})");
            Assert.AreEqual(lat, lat2, 1e-2, $"lat round-trip @ ({lon}, {lat})");
        }
    }

    [TestMethod]
    public void OrthographicProjectorRejectsClicksOutsideDisc()
    {
        var projector = new OrthographicProjector(500, 500, 0, 0, 0, 0);

        // A pixel far outside the disc (top-left corner of a much larger area)
        // must report "not a real coordinate" rather than returning garbage.
        Assert.IsFalse(projector.ToCoordinates(0, 0, out _, out _));
        Assert.IsFalse(projector.ToCoordinates(500, 500, out _, out _));

        // The exact disc center maps back to (centerLon, centerLat).
        Assert.IsTrue(projector.ToCoordinates(250, 250, out var lon, out var lat));
        Assert.AreEqual(0d, lon, 1e-6);
        Assert.AreEqual(0d, lat, 1e-6);
    }
}
