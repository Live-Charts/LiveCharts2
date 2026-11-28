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
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.Themes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.SeriesTests;

// Every node of a sankey took the one color the theme gave the series, so the flows could not be told apart.
[TestClass]
public class SankeyThemeTests
{
    [TestMethod]
    public void EachNodeTakesTheNextColorOfThePalette()
    {
        try
        {
            LiveCharts.Configure(s => s.AddDefaultTheme(requestedTheme: LvcThemeKind.Light));

            var (a, b) = SourceNodeColors(Sankey());

            var palette = ColorPalletes.Default;
            Assert.AreEqual(new SKColor(palette[0].R, palette[0].G, palette[0].B), a, "the first node");
            Assert.AreEqual(new SKColor(palette[1].R, palette[1].G, palette[1].B), b, "the second node");
        }
        finally
        {
            LiveCharts.Configure(s => s.AddDefaultTheme());
        }
    }

    [TestMethod]
    public void AFillSetByTheUserColorsEveryNode()
    {
        var mine = new SKColor(120, 60, 200);

        try
        {
            LiveCharts.Configure(s => s.AddDefaultTheme(requestedTheme: LvcThemeKind.Light));

            var series = Sankey();
            series.Fill = new SolidColorPaint(mine);
            var (a, b) = SourceNodeColors(series);

            Assert.AreEqual(mine, a);
            Assert.AreEqual(mine, b);
        }
        finally
        {
            LiveCharts.Configure(s => s.AddDefaultTheme());
        }
    }

    // two source nodes on the left, A on top of B, flowing into X and Y
    private static SankeySeries<SankeyNode> Sankey()
    {
        var a = new SankeyNode("A");
        var b = new SankeyNode("B");
        var x = new SankeyNode("X");
        var y = new SankeyNode("Y");

        return new SankeySeries<SankeyNode>
        {
            Values = [a, b, x, y],
            Links = [new(a, x, 5), new(a, y, 3), new(b, y, 7)],
            NodeWidth = 16
        };
    }

    private static (SKColor A, SKColor B) SourceNodeColors(SankeySeries<SankeyNode> series)
    {
        var chart = new SKSankeyChart { Width = 600, Height = 400, Series = [series] };

        using var image = chart.GetImage();
        using var bitmap = SKBitmap.FromImage(image);

        // A carries 8 of the 15 units, B the other 7
        return (bitmap.GetPixel(8, 40), bitmap.GetPixel(8, bitmap.Height - 40));
    }
}
