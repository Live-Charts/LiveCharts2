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
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.OtherTests;

// Characterizes the bounds CONVENTIONS each cartesian series reports — which
// axis carries the value vs the category, which Coordinate slot each endpoint
// is read from, and whether the value axis auto-fits the data (no forced zero
// baseline). These were previously implicit and untested; pinning them protects
// against silent transpositions / field mix-ups (e.g. a range series reading
// its low from the wrong slot, or a row series swapping value/category).
//
// Method: feed a value cluster around ~1000 at categories index 0..2 and read
// each axis' DataBounds after a measure. The value axis must carry the ~1000
// cluster (and capture the correct low/high endpoints); the category axis must
// carry the small ~[0,2] indices. A transposition or wrong-slot read moves the
// cluster onto the wrong axis (or drops the low to 0) and is caught.
//
// Summary of the conventions pinned here (Coordinate slots):
//   Column / Line / StepLine / Scatter : PrimaryValue = value (Y),
//       SecondaryValue = X. No forced zero baseline.
//   Row                                : PrimaryValue = value (X),
//       SecondaryValue = category (Y). No forced zero baseline.
//   Stacked Column / Row / Area        : value axis carries the stacked SUM.
//   RangeColumn / RangeRow / RangeLine : PrimaryValue = high, TertiaryValue = low.
//   Candlesticks / Ohlc                : PrimaryValue = high, TertiaryValue = open,
//       QuaternaryValue = close, QuinaryValue = low.
//   Box                                : PrimaryValue = max, QuinaryValue = min.
//   Heat                               : PrimaryValue = Y, SecondaryValue = X,
//       TertiaryValue = weight (kept in WeightBounds, not the X/Y bounds).
[TestClass]
public class SeriesBoundsConventionTests
{
    private const double Tol = 30; // absorbs each series' offset/padding machinery

    private static (Axis x, Axis y) Measure(params ISeries[] series)
    {
        var x = new Axis();
        var y = new Axis();
        var chart = new SKCartesianChart
        {
            Width = 800,
            Height = 400,
            XAxes = [x],
            YAxes = [y],
            Series = series
        };
        _ = chart.GetImage(); // force a measure pass
        return (x, y);
    }

    // The value axis must carry the ~1000 cluster: both endpoints captured, and
    // never dragged down to 0 (no forced baseline).
    private static void AssertValueAxis(Axis a, double low, double high, string label)
    {
        Assert.IsTrue(a.DataBounds.Min >= 900,
            $"{label}: value axis must auto-fit (no forced zero, low endpoint captured); Min={a.DataBounds.Min}");
        AssertNear(a.DataBounds.Min, low, Tol, $"{label} value-axis Min");
        AssertNear(a.DataBounds.Max, high, Tol, $"{label} value-axis Max");
    }

    // The category axis must carry the small ~[0,2] indices, NOT the value cluster.
    private static void AssertCategoryAxis(Axis a, string label) =>
        Assert.IsTrue(a.DataBounds.Min >= -5 && a.DataBounds.Max <= 10,
            $"{label}: category axis should carry indices ~[0,2], not the value cluster; got [{a.DataBounds.Min}, {a.DataBounds.Max}]");

    private static void AssertNear(double actual, double expected, double tol, string label) =>
        Assert.IsTrue(Math.Abs(actual - expected) <= tol,
            $"{label}: expected ~{expected} (±{tol}), got {actual}");

    // ---- Single-value cartesian: value on Y, category/X on Secondary ---------

    [TestMethod]
    public void Line_ValueOnY_NoForcedBaseline()
    {
        var (x, y) = Measure(new LineSeries<double> { Values = [1000d, 1010d, 990d], GeometrySize = 0 });
        AssertValueAxis(y, 990, 1010, "Line");
        AssertCategoryAxis(x, "Line");
    }

    [TestMethod]
    public void StepLine_ValueOnY_NoForcedBaseline()
    {
        var (x, y) = Measure(new StepLineSeries<double> { Values = [1000d, 1010d, 990d], GeometrySize = 0 });
        AssertValueAxis(y, 990, 1010, "StepLine");
        AssertCategoryAxis(x, "StepLine");
    }

    [TestMethod]
    public void Column_ValueOnY_NoForcedBaseline()
    {
        var (x, y) = Measure(new ColumnSeries<double> { Values = [1000d, 1010d, 990d] });
        AssertValueAxis(y, 990, 1010, "Column");
        AssertCategoryAxis(x, "Column");
    }

    [TestMethod]
    public void Row_ValueOnX_NoForcedBaseline()
    {
        // Horizontal bars: value moves to X, category to Y (the transposition).
        var (x, y) = Measure(new RowSeries<double> { Values = [1000d, 1010d, 990d] });
        AssertValueAxis(x, 990, 1010, "Row");
        AssertCategoryAxis(y, "Row");
    }

    [TestMethod]
    public void Scatter_BothAxesDataDriven()
    {
        var (x, y) = Measure(new ScatterSeries<ObservablePoint>
        {
            Values = [new(10, 1000), new(11, 1010), new(12, 990)],
            GeometrySize = 0
        });
        AssertValueAxis(y, 990, 1010, "Scatter Y");
        AssertNear(x.DataBounds.Min, 10, 5, "Scatter X Min");
        AssertNear(x.DataBounds.Max, 12, 5, "Scatter X Max");
    }

    // ---- Stacked: the value axis carries the stacked SUM ---------------------

    [TestMethod]
    public void StackedColumn_ValueAxisCarriesStackedSum()
    {
        // Two series of ~500 stack to ~1000 on Y.
        var (x, y) = Measure(
            new StackedColumnSeries<double> { Values = [500d, 510d, 490d] },
            new StackedColumnSeries<double> { Values = [500d, 510d, 490d] });
        Assert.IsTrue(y.DataBounds.Max >= 900,
            $"StackedColumn: Y must reach the stacked sum (~1000); got Max={y.DataBounds.Max}");
        AssertCategoryAxis(x, "StackedColumn");
    }

    [TestMethod]
    public void StackedRow_ValueAxisCarriesStackedSum()
    {
        var (x, y) = Measure(
            new StackedRowSeries<double> { Values = [500d, 510d, 490d] },
            new StackedRowSeries<double> { Values = [500d, 510d, 490d] });
        Assert.IsTrue(x.DataBounds.Max >= 900,
            $"StackedRow: X must reach the stacked sum (~1000); got Max={x.DataBounds.Max}");
        AssertCategoryAxis(y, "StackedRow");
    }

    [TestMethod]
    public void StackedArea_ValueAxisCarriesStackedSum()
    {
        var (x, y) = Measure(
            new StackedAreaSeries<double> { Values = [500d, 510d, 490d], GeometrySize = 0 },
            new StackedAreaSeries<double> { Values = [500d, 510d, 490d], GeometrySize = 0 });
        Assert.IsTrue(y.DataBounds.Max >= 900,
            $"StackedArea: Y must reach the stacked sum (~1000); got Max={y.DataBounds.Max}");
        AssertCategoryAxis(x, "StackedArea");
    }

    // ---- Range: high = PrimaryValue, low = TertiaryValue ---------------------

    [TestMethod]
    public void RangeColumn_ValueExtentOnY_LowFromTertiary()
    {
        var (x, y) = Measure(new RangeColumnSeries<RangeValue>
        {
            Values = [new(950, 1050), new(950, 1050), new(950, 1050)]
        });
        AssertValueAxis(y, 950, 1050, "RangeColumn");
        AssertCategoryAxis(x, "RangeColumn");
    }

    [TestMethod]
    public void RangeRow_ValueExtentOnX_LowFromTertiary()
    {
        var (x, y) = Measure(new RangeRowSeries<RangeValue>
        {
            Values = [new(950, 1050), new(950, 1050), new(950, 1050)]
        });
        AssertValueAxis(x, 950, 1050, "RangeRow");
        AssertCategoryAxis(y, "RangeRow");
    }

    [TestMethod]
    public void RangeLine_ValueExtentOnY_LowFromTertiary()
    {
        var (x, y) = Measure(new RangeLineSeries<RangeValue>
        {
            Values = [new(950, 1050), new(950, 1050), new(950, 1050)]
        });
        AssertValueAxis(y, 950, 1050, "RangeLine");
        AssertCategoryAxis(x, "RangeLine");
    }

    // ---- Financial: high = Primary, low = Quinary (open/close in between) ----

    [TestMethod]
    public void Candlesticks_ValueExtentOnY_LowFromQuinary()
    {
        var (x, y) = Measure(new CandlesticksSeries<FinancialPointI>
        {
            Values =
            [
                new(1050, 1010, 1000, 950),
                new(1050, 1010, 1000, 950),
                new(1050, 1010, 1000, 950)
            ]
        });
        AssertValueAxis(y, 950, 1050, "Candlesticks");
        AssertCategoryAxis(x, "Candlesticks");
    }

    [TestMethod]
    public void Ohlc_ValueExtentOnY_LowFromQuinary()
    {
        var (x, y) = Measure(new OhlcSeries<FinancialPointI>
        {
            Values =
            [
                new(1050, 1010, 1000, 950),
                new(1050, 1010, 1000, 950),
                new(1050, 1010, 1000, 950)
            ]
        });
        AssertValueAxis(y, 950, 1050, "Ohlc");
        AssertCategoryAxis(x, "Ohlc");
    }

    // ---- Box: max = Primary, min = Quinary -----------------------------------

    [TestMethod]
    public void Box_ValueExtentOnY_MinFromQuinary()
    {
        // BoxValue(max, thirdQuartile, firstQuartile, min, median)
        var (x, y) = Measure(new BoxSeries<BoxValue>
        {
            Values =
            [
                new(1050, 1020, 1000, 950, 1010),
                new(1050, 1020, 1000, 950, 1010),
                new(1050, 1020, 1000, 950, 1010)
            ]
        });
        AssertValueAxis(y, 950, 1050, "Box");
        AssertCategoryAxis(x, "Box");
    }

    // ---- Heat: value Y, X data-driven, weight kept out of the X/Y bounds -----

    [TestMethod]
    public void Heat_ValueOnY_XDataDriven()
    {
        var (x, y) = Measure(new HeatSeries<WeightedPoint>
        {
            Values =
            [
                new(10, 1000, 1),
                new(11, 1010, 5),
                new(12, 990, 9)
            ]
        });
        AssertValueAxis(y, 990, 1010, "Heat Y");
        AssertNear(x.DataBounds.Min, 10, 5, "Heat X Min");
        AssertNear(x.DataBounds.Max, 12, 5, "Heat X Max");
    }
}
