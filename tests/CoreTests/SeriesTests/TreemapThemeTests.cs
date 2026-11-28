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
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.Themes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.SeriesTests;

// The default theme had no rule for treemaps and a treemap has no fill of its own, so a treemap that did not set
// its paints drew nothing at all.
[TestClass]
public class TreemapThemeTests
{
    [TestMethod]
    public void ATreemapWithoutPaintsIsDrawnInTheFirstColorOfTheTheme()
    {
        foreach (var kind in new[] { LvcThemeKind.Light, LvcThemeKind.Dark })
        {
            try
            {
                LiveCharts.Configure(s => s.AddDefaultTheme(requestedTheme: kind));

                var chart = new SKTreemapChart
                {
                    Width = 300,
                    Height = 300,
                    Series = [new TreemapSeries<TreemapNode> { Values = [new(1, "a")] }]
                };

                using var image = chart.GetImage();
                using var bitmap = SKBitmap.FromImage(image);
                var center = bitmap.GetPixel(bitmap.Width / 2, bitmap.Height / 2);

                var expected = (kind == LvcThemeKind.Dark
                    ? ColorPalletes.DefaultDark
                    : ColorPalletes.DefaultLight)[0];

                Assert.AreEqual(new SKColor(expected.R, expected.G, expected.B), center, $"{kind}: the tile is drawn");
            }
            finally
            {
                LiveCharts.Configure(s => s.AddDefaultTheme());
            }
        }
    }
}
