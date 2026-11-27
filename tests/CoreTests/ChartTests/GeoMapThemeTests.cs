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

using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.Themes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.ChartTests;

// The land of a map had the same near white fill in light and dark, the theme did not reach it.
[TestClass]
public class GeoMapThemeTests
{
    private static readonly SKColor s_lightLand = new(240, 240, 240);
    private static readonly SKColor s_darkLand = new(55, 55, 55);

    [TestMethod]
    public void TheLandFollowsTheTheme()
    {
        try
        {
            LiveCharts.Configure(s => s.AddDefaultTheme(requestedTheme: LvcThemeKind.Dark));
            var dark = Count(new SKGeoMap { Width = 400, Height = 300 });

            LiveCharts.Configure(s => s.AddDefaultTheme(requestedTheme: LvcThemeKind.Light));
            var light = Count(new SKGeoMap { Width = 400, Height = 300 });

            Assert.AreEqual(0, dark.Light, "dark: no land is drawn in the light color");
            Assert.IsTrue(dark.Dark > 1000, "dark: the land is drawn in the dark color");
            Assert.AreEqual(0, light.Dark, "light: no land is drawn in the dark color");
            Assert.IsTrue(light.Light > 1000, "light: the land keeps its light color");
        }
        finally
        {
            LiveCharts.Configure(s => s.AddDefaultTheme());
        }
    }

    [TestMethod]
    public void ALandFillSetByTheUserWins()
    {
        var mine = new SKColor(200, 0, 0);

        try
        {
            LiveCharts.Configure(s => s.AddDefaultTheme(requestedTheme: LvcThemeKind.Dark));

            var map = new SKGeoMap { Width = 400, Height = 300, Fill = new SolidColorPaint(mine) };
            using var image = map.GetImage();
            using var bitmap = SKBitmap.FromImage(image);

            Assert.AreEqual(mine, ((SolidColorPaint)map.Fill!).Color);
            Assert.AreEqual(0, CountColor(bitmap, s_darkLand), "the theme did not paint over the user's fill");
        }
        finally
        {
            LiveCharts.Configure(s => s.AddDefaultTheme());
        }
    }

    private static (int Light, int Dark) Count(SKGeoMap map)
    {
        using var image = map.GetImage();
        using var bitmap = SKBitmap.FromImage(image);
        return (CountColor(bitmap, s_lightLand), CountColor(bitmap, s_darkLand));
    }

    private static int CountColor(SKBitmap bitmap, SKColor color)
    {
        var count = 0;
        for (var x = 0; x < bitmap.Width; x++)
            for (var y = 0; y < bitmap.Height; y++)
                if (bitmap.GetPixel(x, y) == color) count++;
        return count;
    }
}
