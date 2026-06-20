using System;
using LiveChartsCore.Drawing;
using LiveChartsCore.Themes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.CoreObjectsTests;

[TestClass]
public class ThemeTesting
{
    [TestMethod]
    public void FluentDesignHasNineColors()
    {
        Assert.IsTrue(ColorPalletes.FluentDesign.Length == 9);
    }

    [TestMethod]
    public void MaterialDesign500HasNineColors()
    {
        Assert.IsTrue(ColorPalletes.MaterialDesign500.Length == 9);
    }

    [TestMethod]
    public void MaterialDesign200HasNineColors()
    {
        Assert.IsTrue(ColorPalletes.MaterialDesign200.Length == 9);
    }

    [TestMethod]
    public void MaterialDesign800HasNineColors()
    {
        Assert.IsTrue(ColorPalletes.MaterialDesign800.Length == 9);
    }

    [TestMethod]
    public void FluentDesignFirstColorIsCorrect()
    {
        var color = ColorPalletes.FluentDesign[0];
        Assert.IsTrue(color.R == 116);
        Assert.IsTrue(color.G == 77);
        Assert.IsTrue(color.B == 169);
        Assert.IsTrue(color.A == 255);
    }

    [TestMethod]
    public void FluentDesignSecondColorIsCorrect()
    {
        var color = ColorPalletes.FluentDesign[1];
        Assert.IsTrue(color.R == 231);
        Assert.IsTrue(color.G == 72);
        Assert.IsTrue(color.B == 86);
        Assert.IsTrue(color.A == 255);
    }

    [TestMethod]
    public void MaterialDesign500FirstColorIsCorrect()
    {
        var color = ColorPalletes.MaterialDesign500[0];
        Assert.IsTrue(color.R == 33);
        Assert.IsTrue(color.G == 150);
        Assert.IsTrue(color.B == 243);
        Assert.IsTrue(color.A == 255);
    }

    [TestMethod]
    public void MaterialDesign200FirstColorIsCorrect()
    {
        var color = ColorPalletes.MaterialDesign200[0];
        Assert.IsTrue(color.R == 144);
        Assert.IsTrue(color.G == 202);
        Assert.IsTrue(color.B == 249);
        Assert.IsTrue(color.A == 255);
    }

    [TestMethod]
    public void MaterialDesign800FirstColorIsCorrect()
    {
        var color = ColorPalletes.MaterialDesign800[0];
        Assert.IsTrue(color.R == 21);
        Assert.IsTrue(color.G == 101);
        Assert.IsTrue(color.B == 192);
        Assert.IsTrue(color.A == 255);
    }

    [TestMethod]
    public void AllPaletteColorsAreOpaque()
    {
        var palettes = new[]
        {
            ColorPalletes.FluentDesign,
            ColorPalletes.MaterialDesign500,
            ColorPalletes.MaterialDesign200,
            ColorPalletes.MaterialDesign800
        };

        foreach (var palette in palettes)
        {
            foreach (var color in palette)
            {
                Assert.IsTrue(color.A == 255, $"Color ({color.R},{color.G},{color.B}) has alpha {color.A}");
            }
        }
    }

    [TestMethod]
    public void AllPaletteColorsAreDistinct()
    {
        var palettes = new[]
        {
            ColorPalletes.FluentDesign,
            ColorPalletes.MaterialDesign500,
            ColorPalletes.MaterialDesign200,
            ColorPalletes.MaterialDesign800
        };

        foreach (var palette in palettes)
        {
            // Verify each color in the palette is different from the others
            for (var i = 0; i < palette.Length; i++)
            {
                for (var j = i + 1; j < palette.Length; j++)
                {
                    Assert.IsTrue(
                        palette[i] != palette[j],
                        $"Colors at index {i} and {j} are equal");
                }
            }
        }
    }

    [TestMethod]
    public void LvcColorFromArgb()
    {
        var color = LvcColor.FromArgb(200, 100, 150, 50);
        Assert.IsTrue(color.A == 200);
        Assert.IsTrue(color.R == 100);
        Assert.IsTrue(color.G == 150);
        Assert.IsTrue(color.B == 50);
    }

    [TestMethod]
    public void LvcColorDefaultHasZeroValues()
    {
        var color = new LvcColor();
        Assert.IsTrue(color.R == 0);
        Assert.IsTrue(color.G == 0);
        Assert.IsTrue(color.B == 0);
        Assert.IsTrue(color.A == 0);
    }

    [TestMethod]
    public void LvcColorConstructor()
    {
        var color = new LvcColor(100, 150, 200, 255);
        Assert.IsTrue(color.R == 100);
        Assert.IsTrue(color.G == 150);
        Assert.IsTrue(color.B == 200);
        Assert.IsTrue(color.A == 255);
    }

    [TestMethod]
    public void LvcColorRGBConstructor()
    {
        var color = new LvcColor(50, 100, 150);
        Assert.IsTrue(color.R == 50);
        Assert.IsTrue(color.G == 100);
        Assert.IsTrue(color.B == 150);
        Assert.IsTrue(color.A == 255);
    }

    [TestMethod]
    public void LvcPointConstructor()
    {
        var point = new LvcPoint(10, 20);
        Assert.IsTrue(Math.Abs(point.X - 10) < 0.001f);
        Assert.IsTrue(Math.Abs(point.Y - 20) < 0.001f);
    }

    [TestMethod]
    public void LvcPointDefaultIsZero()
    {
        var point = new LvcPoint();
        Assert.IsTrue(Math.Abs(point.X - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(point.Y - 0) < 0.001f);
    }

    [TestMethod]
    public void LvcSizeConstructor()
    {
        var size = new LvcSize(100, 50);
        Assert.IsTrue(Math.Abs(size.Width - 100) < 0.001f);
        Assert.IsTrue(Math.Abs(size.Height - 50) < 0.001f);
    }

    [TestMethod]
    public void LvcPointDConstructor()
    {
        var point = new LvcPointD(3.14, 2.71);
        Assert.IsTrue(Math.Abs(point.X - 3.14) < 0.001);
        Assert.IsTrue(Math.Abs(point.Y - 2.71) < 0.001);
    }

    [TestMethod]
    public void LvcRectangleConstructor()
    {
        var rect = new LvcRectangle(new LvcPoint(10, 20), new LvcSize(100, 50));
        Assert.IsTrue(Math.Abs(rect.Location.X - 10) < 0.001f);
        Assert.IsTrue(Math.Abs(rect.Location.Y - 20) < 0.001f);
        Assert.IsTrue(Math.Abs(rect.Size.Width - 100) < 0.001f);
        Assert.IsTrue(Math.Abs(rect.Size.Height - 50) < 0.001f);
    }

    [TestMethod]
    public void LvcRectangleXYWidthHeight()
    {
        var rect = new LvcRectangle(new LvcPoint(5, 15), new LvcSize(200, 100));
        Assert.IsTrue(Math.Abs(rect.X - 5) < 0.001f);
        Assert.IsTrue(Math.Abs(rect.Y - 15) < 0.001f);
        Assert.IsTrue(Math.Abs(rect.Width - 200) < 0.001f);
        Assert.IsTrue(Math.Abs(rect.Height - 100) < 0.001f);
    }

    [TestMethod]
    public void LvcRectangleContains()
    {
        var rect = new LvcRectangle(new LvcPoint(10, 20), new LvcSize(100, 50));

        Assert.IsTrue(rect.Contains(new LvcPoint(50, 40)));
        Assert.IsTrue(rect.Contains(new LvcPoint(10, 20)));  // top-left
        Assert.IsTrue(rect.Contains(new LvcPoint(110, 70))); // bottom-right

        Assert.IsTrue(!rect.Contains(new LvcPoint(5, 40)));   // left of rect
        Assert.IsTrue(!rect.Contains(new LvcPoint(115, 40))); // right of rect
        Assert.IsTrue(!rect.Contains(new LvcPoint(50, 15)));  // above rect
        Assert.IsTrue(!rect.Contains(new LvcPoint(50, 75)));  // below rect
    }

    [TestMethod]
    public void LvcRectangleEquality()
    {
        var rect1 = new LvcRectangle(new LvcPoint(10, 20), new LvcSize(100, 50));
        var rect2 = new LvcRectangle(new LvcPoint(10, 20), new LvcSize(100, 50));
        var rect3 = new LvcRectangle(new LvcPoint(11, 20), new LvcSize(100, 50));

        Assert.IsTrue(rect1 == rect2);
        Assert.IsTrue(rect1 != rect3);
    }

    [TestMethod]
    public void LvcColorParseHex6()
    {
        var color = LvcColor.Parse("#FF0000");
        Assert.IsTrue(color.R == 255);
        Assert.IsTrue(color.G == 0);
        Assert.IsTrue(color.B == 0);
        Assert.IsTrue(color.A == 255);
    }

    [TestMethod]
    public void LvcColorParseHex8()
    {
        var color = LvcColor.Parse("#80FF0000");
        Assert.IsTrue(color.A == 128);
        Assert.IsTrue(color.R == 255);
        Assert.IsTrue(color.G == 0);
        Assert.IsTrue(color.B == 0);
    }

    [TestMethod]
    public void LvcColorParseHex3()
    {
        var color = LvcColor.Parse("#F00");
        Assert.IsTrue(color.R == 255);
        Assert.IsTrue(color.G == 0);
        Assert.IsTrue(color.B == 0);
        Assert.IsTrue(color.A == 255);
    }

    [TestMethod]
    public void LvcColorTryParseInvalid()
    {
        var result = LvcColor.TryParse("invalid", out var color);
        Assert.IsTrue(!result);
    }

    [TestMethod]
    public void LvcColorTryParseEmpty()
    {
        var result = LvcColor.TryParse("", out var color);
        Assert.IsTrue(!result);
    }

    [TestMethod]
    public void LvcColorTryParseNull()
    {
        var result = LvcColor.TryParse(null!, out var color);
        Assert.IsTrue(!result);
    }

    [TestMethod]
    public void LvcColorParseInvalidThrows()
    {
        Assert.ThrowsExactly<ArgumentException>(() => LvcColor.Parse("invalid"));
    }

    [TestMethod]
    public void LvcColorEquality()
    {
        var c1 = new LvcColor(100, 150, 200, 255);
        var c2 = new LvcColor(100, 150, 200, 255);
        var c3 = new LvcColor(100, 150, 200, 128);

        Assert.IsTrue(c1 == c2);
        Assert.IsTrue(c1 != c3);
        Assert.IsTrue(c1.Equals(c2));
        Assert.IsTrue(!c1.Equals(c3));
    }

    [TestMethod]
    public void LvcColorGetHashCodeConsistency()
    {
        var c1 = new LvcColor(100, 150, 200, 255);
        var c2 = new LvcColor(100, 150, 200, 255);

        Assert.IsTrue(c1.GetHashCode() == c2.GetHashCode());
    }

    [TestMethod]
    public void LvcColorFromRGB()
    {
        var color = LvcColor.FromRGB(50, 100, 150);
        Assert.IsTrue(color.R == 50);
        Assert.IsTrue(color.G == 100);
        Assert.IsTrue(color.B == 150);
        Assert.IsTrue(color.A == 255);
    }

    [TestMethod]
    public void LvcColorFromArgbWithBaseColor()
    {
        var baseColor = new LvcColor(100, 150, 200, 255);
        var color = LvcColor.FromArgb(128, baseColor);

        Assert.IsTrue(color.R == 100);
        Assert.IsTrue(color.G == 150);
        Assert.IsTrue(color.B == 200);
        Assert.IsTrue(color.A == 128);
    }
}
