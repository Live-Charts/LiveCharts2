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
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.SeriesTests;

// Characterizes BarSeries.MaxBarWidth, which was previously only SET in tests and
// never asserted as a clamp. When the per-category axis width in pixels exceeds
// MaxBarWidth (e.g. deep X zoom), the drawn column must be CAPPED to MaxBarWidth
// and centered on its category — it must NOT grow to fill the whole unit, which
// is what keeps bars distinct at high zoom instead of merging into a solid block.
[TestClass]
public class ColumnMaxBarWidthTests
{
    private static double DrawnColumnWidth(double maxBarWidth)
    {
        var series = new ColumnSeries<double>
        {
            Values = new[] { 10d, 10d, 10d },
            MaxBarWidth = maxBarWidth,
            Padding = 0
        };

        var chart = new SKCartesianChart
        {
            Width = 1000,
            Height = 400,
            Series = new[] { series },
            // Zoom so a single category (~1 unit) spans the full 1000px width, so
            // the raw per-unit width (~1000px) far exceeds any MaxBarWidth tested.
            XAxes = new[] { new Axis { MinLimit = 0.5, MaxLimit = 1.5 } },
            YAxes = new[] { new Axis() }
        };

        _ = chart.GetImage();

        var point = series.DataFactory
            .Fetch(series, chart.CoreChart)
            .Select(series.ConvertToTypedChartPoint)
            .First(p => p.Coordinate.SecondaryValue == 1);

        return point.Visual.Width;
    }

    [TestMethod]
    public void ColumnWidthIsClampedToMaxBarWidth()
    {
        // Raw per-unit width here is ~1000px, so each draw is bound by its cap.
        Assert.IsTrue(Math.Abs(DrawnColumnWidth(50) - 50) < 1.5,
            "column width should clamp to MaxBarWidth = 50");

        // Raising the cap raises the drawn width — proving the cap is the binding
        // constraint, not some other limit.
        Assert.IsTrue(Math.Abs(DrawnColumnWidth(120) - 120) < 1.5,
            "raising MaxBarWidth to 120 should widen the clamp to 120");
    }
}
