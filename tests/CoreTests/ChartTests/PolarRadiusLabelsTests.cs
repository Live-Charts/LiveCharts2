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
using CoreTests.CoreObjectsTests;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.ChartTests;

// The radius labels were drawn along the line that points right, a spoke whenever the angle axis has a separator
// there: the labels sat under the points of the series and the last one on top of an angle label.
[TestClass]
public class PolarRadiusLabelsTests
{
    [TestMethod]
    public void TheRadiusLabelsGoHalfwayBetweenTheFirstTwoSpokes()
    {
        // 8 points from the top (InitialRotation -90), the angle axis has a spoke at every point, every 45 degrees:
        // the first gap is centered 22.5 degrees clockwise from the top
        var angles = LabelAngles(new PolarAxis { LabelsPaint = new SolidColorPaint(SKColors.Black) });

        Assert.IsTrue(angles.Length > 2, "the radius labels are drawn");
        foreach (var angle in angles) Assert.AreEqual(-67.5, angle, 1, "every label is on the line of the gap");
    }

    [TestMethod]
    public void ALabelsAngleSetByTheUserIsKept()
    {
        var angles = LabelAngles(new PolarAxis { LabelsPaint = new SolidColorPaint(SKColors.Black), LabelsAngle = 0 });

        Assert.IsTrue(angles.Length > 2);
        foreach (var angle in angles) Assert.AreEqual(0, angle, 1, "every label is on the line that points right");
    }

    // the angle in degrees of each radius label, seen from the center of the chart
    private static double[] LabelAngles(PolarAxis radius)
    {
        var chart = new SKPolarChart
        {
            Width = 600,
            Height = 600,
            AngleAxes = [new PolarAxis { MinStep = 1, ForceStepToMin = true }],
            RadiusAxes = [radius],
            Series = [new PolarLineSeries<double> { Values = [15, 14, 13, 12, 11, 10, 9, 8], IsClosed = true }]
        };

        _ = ChangingPaintTasks.DrawChart(chart);

        var core = chart.CoreChart;
        var center = new LvcPoint(
            core.DrawMarginLocation.X + core.DrawMarginSize.Width / 2,
            core.DrawMarginLocation.Y + core.DrawMarginSize.Height / 2);

        return radius.LabelsPaint!.GetGeometries(chart.CoreCanvas)
            .Cast<LabelGeometry>()
            .Select(l => (dx: l.X - center.X, dy: l.Y - center.Y))
            .Where(d => Math.Sqrt(d.dx * d.dx + d.dy * d.dy) > 10) // the label at the center has no direction
            .Select(d => Math.Atan2(d.dy, d.dx) * 180 / Math.PI)
            .ToArray();
    }
}
