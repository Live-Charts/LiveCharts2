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

using System.Collections.Generic;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.Themes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.CoreObjectsTests;

// The default theme already has a rule for DrawnLabelVisual, a theme built on it must be able to add its own.
[TestClass]
public class ThemeElementRuleTests
{
    [TestMethod]
    public void ARuleForAnElementTheThemeAlreadyStylesRunsAfterIt()
    {
        var calls = new List<string>();
        var red = new SKColor(255, 0, 0);

        try
        {
            // threw "An item with the same key has already been added" before the fix
            LiveCharts.Configure(s => s.AddDefaultTheme(theme => theme
                .HasRuleFor<DrawnLabelVisual>(label =>
                {
                    calls.Add("mine");
                    label.Paint = new SolidColorPaint(red);
                })));

            var title = new DrawnLabelVisual(new LabelGeometry { Text = "title", TextSize = 20 });
            var chart = new SKCartesianChart
            {
                Width = 300,
                Height = 300,
                Title = title,
                Series = [new LineSeries<double> { Values = [1, 2, 3] }]
            };

            _ = chart.GetImage();

            Assert.IsTrue(calls.Count > 0, "the second rule runs");
            Assert.AreEqual(red, ((SolidColorPaint)title.Paint!).Color, "it runs after the theme's own rule");
        }
        finally
        {
            LiveCharts.Configure(s => s.AddDefaultTheme());
        }
    }

    [TestMethod]
    public void RulesForTheSameElementRunInTheOrderTheyWereAdded()
    {
        var calls = new List<int>();
        var theme = new Theme()
            .HasRuleFor<DrawnLabelVisual>(_ => calls.Add(1))
            .HasRuleFor<DrawnLabelVisual>(_ => calls.Add(2));

        theme.ApplyStyleTo<DrawnLabelVisual>(new DrawnLabelVisual(new LabelGeometry()));

        CollectionAssert.AreEqual(new[] { 1, 2 }, calls);
    }
}
