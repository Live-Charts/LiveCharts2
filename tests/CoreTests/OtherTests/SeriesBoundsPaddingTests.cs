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
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.OtherTests;

// Characterizes the axis-padding contract that CartesianSeries.GetBounds applies
// on top of the raw data bounds — previously untested. When the axis auto-fits,
// these three properties create the gap between the data and the chart edge:
//   PaddingMax / PaddingMin   = tick * DataPadding (a value-space pad each side),
//   RequestedGeometrySize     = pixels reserved for the point geometry.
// The pad is symmetric (Max == Min) and the geometry size is the same on both
// axes. (DataPadding and GetRequestedGeometrySize/Offset vary per series, which
// is why this checks several series types.)
[TestClass]
public class SeriesBoundsPaddingTests
{
    private static DimensionalBoundsView Measure(ISeries series)
    {
        var x = new Axis();
        var y = new Axis();
        var chart = new SKCartesianChart
        {
            Width = 800,
            Height = 400,
            Series = [series],
            XAxes = [x],
            YAxes = [y]
        };
        _ = chart.GetImage();
        var b = ((ICartesianSeries)series).GetBounds((Chart)chart.CoreChart, x, y).Bounds;
        return new DimensionalBoundsView(b);
    }

    // A plain struct rather than a `record struct` — positional records synthesize
    // `init` accessors which require System.Runtime.CompilerServices.IsExternalInit,
    // missing on net462.
    private readonly struct DimensionalBoundsView(LiveChartsCore.Measure.DimensionalBounds b)
    {
        public LiveChartsCore.Measure.DimensionalBounds B { get; } = b;

        // Universal: padding is symmetric on each axis, and the requested geometry
        // size is the same on both. The per-axis MAGNITUDES vary by series
        // (DataPadding differs, and bar series push the category gap through an
        // offset instead of padding) — those are asserted per test below.
        public void AssertSymmetric(string label)
        {
            Assert.AreEqual(B.PrimaryBounds.PaddingMax, B.PrimaryBounds.PaddingMin, 1e-9,
                $"{label}: primary padding must be symmetric");
            Assert.AreEqual(B.SecondaryBounds.PaddingMax, B.SecondaryBounds.PaddingMin, 1e-9,
                $"{label}: secondary padding must be symmetric");
            Assert.AreEqual(B.PrimaryBounds.RequestedGeometrySize, B.SecondaryBounds.RequestedGeometrySize, 1e-9,
                $"{label}: requested geometry size must match on both axes");
        }
    }

    [TestMethod]
    public void Line_PadsBothAxes_AndReservesGeometry()
    {
        var v = Measure(new LineSeries<double> { Values = [1d, 2d, 4d, 8d] });
        v.AssertSymmetric("Line");
        // DataPadding (0.5, 1): both axes padded; markers reserve geometry size.
        Assert.IsTrue(v.B.SecondaryBounds.PaddingMax > 0, "Line X must be padded");
        Assert.IsTrue(v.B.PrimaryBounds.PaddingMax > 0, "Line Y must be padded");
        Assert.IsTrue(v.B.PrimaryBounds.RequestedGeometrySize > 0, "Line must reserve geometry size for its markers");
    }

    [TestMethod]
    public void Column_PadsValueAxisOnly_CategoryGapIsAnOffset()
    {
        var v = Measure(new ColumnSeries<double> { Values = [1d, 2d, 4d, 8d] });
        v.AssertSymmetric("Column");
        // DataPadding (0, 1): the value axis (Y) is padded; the category axis (X)
        // is NOT padded — its gap comes from the 0.5 unit-width offset instead.
        Assert.IsTrue(v.B.PrimaryBounds.PaddingMax > 0, "Column Y must be padded");
        Assert.AreEqual(0, v.B.SecondaryBounds.PaddingMax, 1e-9, "Column X gap is an offset, not padding");
    }

    [TestMethod]
    public void Scatter_PadsBothAxes()
    {
        var v = Measure(new ScatterSeries<ObservablePoint>
        {
            Values = [new(1, 1), new(2, 4), new(3, 9)]
        });
        v.AssertSymmetric("Scatter");
        // DataPadding (1, 1): both axes padded.
        Assert.IsTrue(v.B.PrimaryBounds.PaddingMax > 0, "Scatter Y must be padded");
        Assert.IsTrue(v.B.SecondaryBounds.PaddingMax > 0, "Scatter X must be padded");
    }
}
