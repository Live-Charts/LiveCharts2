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
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.OtherTests;

// Pins the "auto-fit the non-zoomed axis to the visible window" behavior that
// the library has but never tested: with ZoomMode.X, constraining the X view
// to a sub-window must shrink the Y axis to the Y range of the data actually
// inside that window (and symmetrically for ZoomMode.Y). The unzoomed axis
// must NOT keep the full-series range.
//
// Canonical case (Beto's): values [100, 10, 100] at X = 0,1,2. Zoom X to show
// only the middle point and Y must collapse toward 10, ignoring the 100s.
[TestClass]
public class ZoomVisibleBoundsAutoFitTests
{
    private static (Axis x, Axis y, SKCartesianChart chart) CreateLineChart(
        ZoomAndPanMode zoomMode, double[] values)
    {
        var x = new Axis();
        var y = new Axis();

        var chart = new SKCartesianChart
        {
            Width = 800,
            Height = 400,
            ZoomMode = zoomMode,
            XAxes = [x],
            YAxes = [y],
            Series =
            [
                new LineSeries<double> { Values = values, GeometrySize = 0 }
            ]
        };

        _ = chart.GetImage(); // force a measure pass

        return (x, y, chart);
    }

    [TestMethod]
    public void ZoomModeX_ConstrainingX_FitsYToVisibleWindow()
    {
        // [100, 10, 100] at X = 0,1,2. Full Y range is [10, 100].
        var (x, y, chart) = CreateLineChart(ZoomAndPanMode.X, [100d, 10d, 100d]);

        // Sanity: before zoom the Y visible range reaches the 100s.
        Assert.IsTrue(y.VisibleDataBounds.Max >= 99,
            $"Unzoomed Y should see the full data; got Max={y.VisibleDataBounds.Max}.");

        // Zoom X to isolate the middle point (X=1, Y=10).
        x.MinLimit = 0.5;
        x.MaxLimit = 1.5;
        _ = chart.GetImage();

        Assert.IsTrue(y.VisibleDataBounds.Max < 50,
            $"With only the middle point (Y=10) visible, Y must fit toward 10 and " +
            $"ignore the off-window 100s; got Max={y.VisibleDataBounds.Max}.");
    }

    [TestMethod]
    public void ZoomModeY_ConstrainingY_FitsXToVisibleWindow()
    {
        // Scatter so X and Y are independent. Three points:
        //   (0, 100), (1, 10), (2, 100)
        // Constraining Y to isolate the middle point (Y=10) must collapse the
        // X visible range toward X=1.
        var x = new Axis();
        var y = new Axis();

        var chart = new SKCartesianChart
        {
            Width = 800,
            Height = 400,
            ZoomMode = ZoomAndPanMode.Y,
            XAxes = [x],
            YAxes = [y],
            Series =
            [
                new ScatterSeries<LiveChartsCore.Defaults.ObservablePoint>
                {
                    Values =
                    [
                        new(0, 100),
                        new(1, 10),
                        new(2, 100)
                    ],
                    GeometrySize = 0
                }
            ]
        };
        _ = chart.GetImage();

        Assert.IsTrue(x.VisibleDataBounds.Max >= 1.99,
            $"Unzoomed X should see all three points; got Max={x.VisibleDataBounds.Max}.");

        y.MinLimit = 5;
        y.MaxLimit = 15;
        _ = chart.GetImage();

        Assert.IsTrue(x.VisibleDataBounds.Min > 0.5 && x.VisibleDataBounds.Max < 1.5,
            $"With only the middle point (X=1) visible, X must fit toward 1 and " +
            $"ignore the off-window points at X=0 and X=2; got " +
            $"[{x.VisibleDataBounds.Min}, {x.VisibleDataBounds.Max}].");
    }

    [TestMethod]
    public void ZoomModeX_EmptyWindow_DoesNotThrow_AndYStaysFinite()
    {
        var (x, y, chart) = CreateLineChart(ZoomAndPanMode.X, [100d, 10d, 100d]);

        // A window between points 1 and 2 that contains NO data point.
        x.MinLimit = 1.2;
        x.MaxLimit = 1.8;
        _ = chart.GetImage();

        Assert.IsFalse(double.IsNaN(y.VisibleDataBounds.Min), "Y min went NaN on an empty window.");
        Assert.IsFalse(double.IsNaN(y.VisibleDataBounds.Max), "Y max went NaN on an empty window.");
    }
}
