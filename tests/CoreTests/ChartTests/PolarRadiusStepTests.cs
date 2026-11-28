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

using System.Linq;
using CoreTests.CoreObjectsTests;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.ChartTests;

// The step of a radius axis did not look at its labels: 90px per step, counted on the shorter side of the control
// instead of the radius, and MinSeparators asked for more. A 380x240 chart drew 7 labels in a 100px radius.
[TestClass]
public class PolarRadiusStepTests
{
    [TestMethod]
    [DataRow(380, 240, false)]
    [DataRow(600, 400, false)]
    [DataRow(900, 600, false)]
    [DataRow(380, 240, true)]
    [DataRow(600, 400, true)]
    public void TheRadiusLabelsDoNotOverlap(int width, int height, bool horizontal)
    {
        // horizontal, the labels are side by side and their width counts, otherwise they are stacked between the
        // first two spokes and mostly their height counts
        var radius = new PolarAxis { LabelsPaint = new SolidColorPaint(SKColors.Black) };
        if (horizontal) radius.LabelsAngle = 0;
        var chart = new SKPolarChart
        {
            Width = width,
            Height = height,
            RadiusAxes = [radius],
            Series = [
                new PolarLineSeries<double> { Values = [15, 14, 13, 12, 11, 10, 9, 8] },
                new PolarLineSeries<double> { Values = [8, 9, 11, 13, 12, 10, 9, 10] }]
        };

        _ = ChangingPaintTasks.DrawChart(chart);

        // the text of each label, centered on its position, without its padding
        var boxes = radius.LabelsPaint!.GetGeometries(chart.CoreCanvas)
            .Cast<LabelGeometry>()
            .Select(l =>
            {
                var size = l.Measure();
                var w = size.Width - (float)(l.Padding.Left + l.Padding.Right);
                var h = size.Height - (float)(l.Padding.Top + l.Padding.Bottom);
                return SKRect.Create(l.X - w / 2, l.Y - h / 2, w, h);
            })
            .ToArray();

        Assert.IsTrue(boxes.Length >= 2, "the radius axis draws labels");

        for (var i = 0; i < boxes.Length; i++)
            for (var j = i + 1; j < boxes.Length; j++)
                Assert.IsFalse(boxes[i].IntersectsWith(boxes[j]), $"labels {i} and {j} overlap at {width}x{height}, horizontal: {horizontal}");
    }
}
