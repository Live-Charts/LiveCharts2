using LiveChartsCore.Noesis.Drawing.Geometries;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NoesisTests;

[TestClass]
public sealed class MapHitTestTests
{
    [TestMethod]
    public void EmptyLandDoesNotHit()
    {
        var land = new LandAreaGeometry();

        Assert.IsFalse(land.ContainsPoint(0, 0));
    }

    [TestMethod]
    public void ResetProjectedLandDoesNotHit()
    {
        var path = new PathBuilder();
        var land = new LandAreaGeometry();

        path.MoveTo(0, 0);
        path.LineTo(100, 0);
        path.LineTo(0, 100);
        path.Close();
        land.SetBasePath(path);
        path.Reset();

        Assert.IsFalse(land.ContainsPoint(10, 10));
    }
}
