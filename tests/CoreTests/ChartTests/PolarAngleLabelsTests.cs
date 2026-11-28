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
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.ChartTests;

// A closed polar line ends its angle axis one step after its last point, so on a full circle the last angle label
// landed on the angle of the first one and covered it: a chart of 8 points showed "8" where "0" should be.
[TestClass]
public class PolarAngleLabelsTests
{
    [TestMethod]
    public void OnAFullCircleTheLastAngleLabelDoesNotCoverTheFirst()
    {
        var labeled = Render(totalAngle: 360);

        Assert.IsTrue(labeled.Contains(0), "the first label is drawn");
        Assert.IsFalse(labeled.Contains(8), "8 lands on the angle of 0, it is not drawn");
        Assert.IsTrue(labeled.Contains(7), "the last point keeps its label");
    }

    [TestMethod]
    public void OnAPartialCircleTheLastAngleLabelIsDrawn()
    {
        var labeled = Render(totalAngle: 270);

        Assert.IsTrue(labeled.Contains(0));
        Assert.IsTrue(labeled.Contains(8), "8 has an angle of its own when the circle is not complete");
    }

    private static HashSet<double> Render(double totalAngle)
    {
        var labeled = new HashSet<double>();
        var angle = new PolarAxis { Labeler = value => { _ = labeled.Add(value); return value.ToString(); } };

        var chart = new SKPolarChart
        {
            Width = 600,
            Height = 400,
            TotalAngle = totalAngle,
            AngleAxes = [angle],
            Series = [new PolarLineSeries<double> { Values = [15, 14, 13, 12, 11, 10, 9, 8], IsClosed = true }]
        };

        _ = chart.GetImage();

        return labeled;
    }
}
