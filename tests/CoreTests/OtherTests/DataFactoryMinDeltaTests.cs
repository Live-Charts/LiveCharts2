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
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.OtherTests;

// Characterizes DataBounds.MinDelta — the smallest gap between consecutive points,
// computed by DataFactory (the single data provider in OSS, shared by every
// series). It was previously implicit and untested, yet it drives a loved zoom
// behavior: the engine floors zoom-in at (MinZoomDelta ?? DataBounds.MinDelta * 3),
// so you can't zoom past ~3x the smallest data gap and the series never "gets
// lost" between points. Pinning it protects that floor and gives the batched
// data providers an exact target to match.
[TestClass]
public class DataFactoryMinDeltaTests
{
    private const double Tol = 1e-6;

    // GetCartesianBounds: MinDelta is min |Δsecondary| (X) and min |Δprimary| (Y)
    // over consecutive points — the MIN, not the first or average gap.
    [TestMethod]
    public void CartesianBounds_MinDelta_IsSmallestAdjacentGap()
    {
        var x = new Axis();
        var y = new Axis();
        var chart = new SKCartesianChart
        {
            Width = 800,
            Height = 400,
            XAxes = [x],
            YAxes = [y],
            Series =
            [
                new LineSeries<ObservablePoint>
                {
                    // X gaps: 5, 1, 3  -> min 1   |   Y gaps: 8, 2, 20 -> min 2
                    Values =
                    [
                        new(0, 0), new(5, 8), new(6, 10), new(9, 30)
                    ],
                    GeometrySize = 0
                }
            ]
        };

        _ = chart.GetImage();

        Assert.AreEqual(1, x.DataBounds.MinDelta, Tol, "X MinDelta should be the smallest adjacent gap (1)");
        Assert.AreEqual(2, y.DataBounds.MinDelta, Tol, "Y MinDelta should be the smallest adjacent gap (2)");
    }

    // GetFinancialBounds is a separate code path; it computes MinDelta the same
    // way. Index-based points are spaced by 1, so the secondary MinDelta is 1.
    [TestMethod]
    public void FinancialBounds_MinDelta_IsSmallestAdjacentGap()
    {
        var x = new Axis();
        var y = new Axis();
        var chart = new SKCartesianChart
        {
            Width = 800,
            Height = 400,
            XAxes = [x],
            YAxes = [y],
            Series =
            [
                new CandlesticksSeries<FinancialPointI>
                {
                    Values =
                    [
                        new(1050, 1010, 1000, 950),
                        new(1050, 1010, 1000, 950),
                        new(1050, 1010, 1000, 950)
                    ]
                }
            ]
        };

        _ = chart.GetImage();

        Assert.AreEqual(1, x.DataBounds.MinDelta, Tol, "financial X MinDelta should be 1 (unit-spaced indices)");
    }
}
