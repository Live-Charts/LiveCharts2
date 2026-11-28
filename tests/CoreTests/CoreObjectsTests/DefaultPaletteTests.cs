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

// Colorblind readers tell colors apart mostly by how light they are. The light palette of the default theme put
// light green 3rd and amber 6th, two light colors that look alike with red-green color blindness, and the dark one
// was pastel, every color about as light as the others.
[TestClass]
public class DefaultPaletteTests
{
    // OKLab distance x100 under the simulation; under 6 two colors look alike, 8 is a safe margin
    private const double MinDistance = 8;

    [TestMethod]
    [DataRow(LvcThemeKind.Light)]
    [DataRow(LvcThemeKind.Dark)]
    public void TheFirstSixColorsStayApartWithRedGreenColorBlindness(LvcThemeKind kind)
    {
        var colors = ColorsOf(kind).Take(6).ToArray();

        for (var i = 0; i < colors.Length; i++)
            for (var j = i + 1; j < colors.Length; j++)
            {
                var distance = Math.Min(Distance(colors[i], colors[j], s_protan), Distance(colors[i], colors[j], s_deutan));
                Assert.IsTrue(
                    distance >= MinDistance,
                    $"{kind}: colors {i + 1} and {j + 1} look alike with red-green color blindness ({distance:0.0})");
            }
    }

    [TestMethod]
    public void ASeriesKeepsItsColorBetweenLightAndDark()
    {
        var light = ColorsOf(LvcThemeKind.Light);
        var dark = ColorsOf(LvcThemeKind.Dark);

        CollectionAssert.AreEqual(light, dark);
    }

    private static LvcColor[] ColorsOf(LvcThemeKind kind)
    {
        try
        {
            LiveCharts.Configure(s => s.AddDefaultTheme(requestedTheme: kind));
            _ = new SKCartesianChart { Width = 100, Height = 100, Series = [new LineSeries<double> { Values = [1] }] }
                .GetImage();

            return LiveCharts.DefaultSettings.GetTheme().Colors.ToArray();
        }
        finally
        {
            LiveCharts.Configure(s => s.AddDefaultTheme());
        }
    }

    // Machado, Oliveira and Fernandes (2009), severity 1
    private static readonly double[,] s_protan =
        { { 0.152286, 1.052583, -0.204868 }, { 0.114503, 0.786281, 0.099216 }, { -0.003882, -0.048116, 1.051998 } };
    private static readonly double[,] s_deutan =
        { { 0.367322, 0.860646, -0.227968 }, { 0.280085, 0.672501, 0.047413 }, { -0.011820, 0.042940, 0.968881 } };

    private static double Distance(LvcColor a, LvcColor b, double[,] simulation)
    {
        var x = OkLab(Simulate(a, simulation));
        var y = OkLab(Simulate(b, simulation));
        return 100 * Math.Sqrt(Math.Pow(x[0] - y[0], 2) + Math.Pow(x[1] - y[1], 2) + Math.Pow(x[2] - y[2], 2));
    }

    private static double[] Simulate(LvcColor c, double[,] m)
    {
        double r = Linear(c.R), g = Linear(c.G), b = Linear(c.B);
        static double Clamp(double v) => Math.Max(0, Math.Min(1, v));
        return
        [
            Clamp(m[0, 0] * r + m[0, 1] * g + m[0, 2] * b),
            Clamp(m[1, 0] * r + m[1, 1] * g + m[1, 2] * b),
            Clamp(m[2, 0] * r + m[2, 1] * g + m[2, 2] * b)
        ];
    }

    private static double Linear(byte channel)
    {
        var v = channel / 255d;
        return v <= 0.04045 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
    }

    private static double[] OkLab(double[] rgb)
    {
        var l = Math.Pow(0.4122214708 * rgb[0] + 0.5363325363 * rgb[1] + 0.0514459929 * rgb[2], 1d / 3);
        var m = Math.Pow(0.2119034982 * rgb[0] + 0.6806995451 * rgb[1] + 0.1073969566 * rgb[2], 1d / 3);
        var s = Math.Pow(0.0883024619 * rgb[0] + 0.2817188376 * rgb[1] + 0.6299787005 * rgb[2], 1d / 3);
        return
        [
            0.2104542553 * l + 0.7936177850 * m - 0.0040720468 * s,
            1.9779984951 * l - 2.4285922050 * m + 0.4505937099 * s,
            0.0259040371 * l + 0.7827717662 * m - 0.8086757660 * s
        ];
    }
}
