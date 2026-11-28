// The MIT License(MIT)
//
// Copyright(c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.Themes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.CoreObjectsTests;

// The light palette of the default theme had three colors under 3:1 on white (light green 2.1, cyan 2.3, amber
// 1.6): a line or a bar in those colors did not have the contrast WCAG asks for graphical objects (1.4.11).
[TestClass]
public class DefaultPaletteTests
{
    [TestMethod]
    [DataRow(LvcThemeKind.Light)]
    [DataRow(LvcThemeKind.Dark)]
    public void EveryColorOfTheDefaultThemeHasContrastOnItsBackground(LvcThemeKind kind)
    {
        try
        {
            LiveCharts.Configure(s => s.AddDefaultTheme(requestedTheme: kind));
            _ = new SKCartesianChart { Width = 100, Height = 100, Series = [new LineSeries<double> { Values = [1] }] }
                .GetImage();

            var theme = LiveCharts.DefaultSettings.GetTheme();
            var background = theme.VirtualBackroundColor;

            var failing = theme.Colors
                .Where(c => Contrast(c, background) < 3)
                .Select(c => $"({c.R},{c.G},{c.B}) {Contrast(c, background):0.00}")
                .ToArray();

            Assert.AreEqual(9, theme.Colors.Length);
            Assert.AreEqual(0, failing.Length, $"{kind}: under 3:1 on the background: {string.Join(", ", failing)}");
        }
        finally
        {
            LiveCharts.Configure(s => s.AddDefaultTheme());
        }
    }

    [TestMethod]
    public void ASeriesKeepsItsHueBetweenLightAndDark()
    {
        // the same position in both palettes is the same family of color, only its shade changes
        var light = ColorPalletes.DefaultLight;
        var dark = ColorPalletes.DefaultDark;

        Assert.AreEqual(light.Length, dark.Length);
        for (var i = 0; i < light.Length; i++)
            Assert.IsTrue(HueDistance(Hue(light[i]), Hue(dark[i])) < 20, $"color {i} changes its hue");
    }

    // WCAG 2 contrast ratio
    private static double Contrast(LvcColor a, LvcColor b)
    {
        var la = Luminance(a);
        var lb = Luminance(b);
        return (Math.Max(la, lb) + 0.05) / (Math.Min(la, lb) + 0.05);
    }

    private static double Luminance(LvcColor c) =>
        0.2126 * Linear(c.R) + 0.7152 * Linear(c.G) + 0.0722 * Linear(c.B);

    private static double Linear(byte channel)
    {
        var v = channel / 255d;
        return v <= 0.04045 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
    }

    private static double Hue(LvcColor c)
    {
        double r = c.R / 255d, g = c.G / 255d, b = c.B / 255d;
        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var d = max - min;
        if (d == 0) return 0;
        var h = max == r ? (g - b) / d % 6 : max == g ? (b - r) / d + 2 : (r - g) / d + 4;
        return (h * 60 + 360) % 360;
    }

    private static double HueDistance(double a, double b)
    {
        var d = Math.Abs(a - b) % 360;
        return d > 180 ? 360 - d : d;
    }
}
