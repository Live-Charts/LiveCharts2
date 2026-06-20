using System;
using LiveChartsCore.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.CoreObjectsTests;

[TestClass]
public class ColorTesting
{
    [TestMethod]
    public void Compare()
    {
        Assert.IsTrue(new LvcColor(123, 30, 29) != new LvcColor(123, 32, 23));
        Assert.IsTrue(new LvcColor(123, 30, 29) == new LvcColor(123, 30, 29));
        Assert.IsTrue(LvcColor.Empty != new LvcColor(123, 30, 29));
        Assert.IsTrue(LvcColor.Empty == LvcColor.Empty);

        var a = LvcColor.Empty;
        a.R = 1;
        a.G = 1;
        a.B = 1;
        a.A = 1;

        Assert.IsTrue(a != new LvcColor(1, 1, 1, 1));
    }

    [TestMethod]
    public void FromRgbAndArgbProduceExpectedChannels()
    {
        var rgb = LvcColor.FromRGB(10, 20, 30);
        Assert.AreEqual((byte)10, rgb.R);
        Assert.AreEqual((byte)20, rgb.G);
        Assert.AreEqual((byte)30, rgb.B);
        Assert.AreEqual((byte)255, rgb.A);

        var argb = LvcColor.FromArgb(64, 10, 20, 30);
        Assert.AreEqual((byte)64, argb.A);
        Assert.AreEqual((byte)10, argb.R);
        Assert.AreEqual((byte)20, argb.G);
        Assert.AreEqual((byte)30, argb.B);

        var overrideAlpha = LvcColor.FromArgb(32, rgb);
        Assert.AreEqual((byte)32, overrideAlpha.A);
        Assert.AreEqual(rgb.R, overrideAlpha.R);
        Assert.AreEqual(rgb.G, overrideAlpha.G);
        Assert.AreEqual(rgb.B, overrideAlpha.B);
    }

    [TestMethod]
    public void TryParseThreeDigitHexExpandsEachNibble()
    {
        // "#F80" expands each nibble: R=FF, G=88, B=00, A defaults to FF.
        Assert.IsTrue(LvcColor.TryParse("#F80", out var c));
        Assert.AreEqual((byte)0xFF, c.R);
        Assert.AreEqual((byte)0x88, c.G);
        Assert.AreEqual((byte)0x00, c.B);
        Assert.AreEqual((byte)0xFF, c.A);
    }

    [TestMethod]
    public void TryParseFourDigitHexExpandsEachNibbleIncludingAlpha()
    {
        // "#8F80" uses the first nibble as alpha: A=88, R=FF, G=88, B=00.
        Assert.IsTrue(LvcColor.TryParse("#8F80", out var c));
        Assert.AreEqual((byte)0x88, c.A);
        Assert.AreEqual((byte)0xFF, c.R);
        Assert.AreEqual((byte)0x88, c.G);
        Assert.AreEqual((byte)0x00, c.B);
    }

    [TestMethod]
    public void TryParseThreeDigitHexWithoutHash()
    {
        Assert.IsTrue(LvcColor.TryParse("F80", out var c));
        Assert.AreEqual((byte)0xFF, c.R);
        Assert.AreEqual((byte)0x88, c.G);
        Assert.AreEqual((byte)0x00, c.B);
        Assert.AreEqual((byte)0xFF, c.A);
    }

    [TestMethod]
    public void TryParseSixDigitHex()
    {
        Assert.IsTrue(LvcColor.TryParse("#FF8040", out var c));
        Assert.AreEqual((byte)255, c.R);
        Assert.AreEqual((byte)128, c.G);
        Assert.AreEqual((byte)64, c.B);
        Assert.AreEqual((byte)255, c.A);
    }

    [TestMethod]
    public void TryParseSixDigitHexWithoutHash()
    {
        Assert.IsTrue(LvcColor.TryParse("FF8040", out var c));
        Assert.AreEqual((byte)255, c.R);
        Assert.AreEqual((byte)128, c.G);
        Assert.AreEqual((byte)64, c.B);
        Assert.AreEqual((byte)255, c.A);
    }

    [TestMethod]
    public void TryParseEightDigitHexReadsAlpha()
    {
        Assert.IsTrue(LvcColor.TryParse("#80FF8040", out var c));
        Assert.AreEqual((byte)128, c.A);
        Assert.AreEqual((byte)255, c.R);
        Assert.AreEqual((byte)128, c.G);
        Assert.AreEqual((byte)64, c.B);
    }

    [TestMethod]
    public void TryParseRejectsNullOrWhitespace()
    {
        Assert.IsFalse(LvcColor.TryParse(null!, out _));
        Assert.IsFalse(LvcColor.TryParse("", out _));
        Assert.IsFalse(LvcColor.TryParse("   ", out _));
    }

    [TestMethod]
    public void TryParseRejectsUnsupportedLengths()
    {
        Assert.IsFalse(LvcColor.TryParse("12", out _));
        Assert.IsFalse(LvcColor.TryParse("12345", out _));
        Assert.IsFalse(LvcColor.TryParse("1234567", out _));
    }

    [TestMethod]
    public void TryParseRejectsNonHexCharacters()
    {
        Assert.IsFalse(LvcColor.TryParse("#GGGGGG", out _));
    }

    [TestMethod]
    public void ParseThrowsOnInvalidInput()
    {
        Assert.ThrowsExactly<ArgumentException>(() => LvcColor.Parse("not-a-color"));
    }
}
