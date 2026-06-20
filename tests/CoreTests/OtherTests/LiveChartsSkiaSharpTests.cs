using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.OtherTests;

[TestClass]
public class LiveChartsSkiaSharpTests
{
    [TestMethod]
    public void AsSKColor_NonEmptyLvcColorPreservesAllChannels()
    {
        var color = new LvcColor(10, 20, 30, 200);
        var sk = color.AsSKColor();

        Assert.AreEqual((byte)10, sk.Red);
        Assert.AreEqual((byte)20, sk.Green);
        Assert.AreEqual((byte)30, sk.Blue);
        Assert.AreEqual((byte)200, sk.Alpha);
    }

    [TestMethod]
    public void AsSKColor_EmptyLvcColorMapsToSKColorEmpty()
    {
        var sk = LvcColor.Empty.AsSKColor();
        Assert.AreEqual(SKColor.Empty, sk);
    }

    [TestMethod]
    public void AsSKColor_AlphaOverrideWinsOverColorAlpha()
    {
        var color = new LvcColor(10, 20, 30, 200);
        var sk = color.AsSKColor(alphaOverrides: 50);

        Assert.AreEqual((byte)10, sk.Red);
        Assert.AreEqual((byte)20, sk.Green);
        Assert.AreEqual((byte)30, sk.Blue);
        Assert.AreEqual((byte)50, sk.Alpha);
    }

    [TestMethod]
    public void WithOpacity_ReturnsNewColorAndKeepsRgb()
    {
        var color = new LvcColor(10, 20, 30, 200);
        var faded = color.WithOpacity(64);

        Assert.AreEqual((byte)10, faded.R);
        Assert.AreEqual((byte)20, faded.G);
        Assert.AreEqual((byte)30, faded.B);
        Assert.AreEqual((byte)64, faded.A);
    }

    [TestMethod]
    public void AsLvcColor_PreservesAllChannels()
    {
        var sk = new SKColor(10, 20, 30, 200);
        var color = sk.AsLvcColor();

        Assert.AreEqual((byte)10, color.R);
        Assert.AreEqual((byte)20, color.G);
        Assert.AreEqual((byte)30, color.B);
        Assert.AreEqual((byte)200, color.A);
    }

}
