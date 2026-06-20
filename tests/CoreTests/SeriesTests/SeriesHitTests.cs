using System;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Drawing.Segments;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Drawing;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.SeriesTests;

// Coverage matrix for Series.FindPointsInPosition / ISeries.FindHitPoints
// across every series type and every FindingStrategy. Each series gets:
//   * a "hit at center" probe per supported strategy
//   * a "miss outside any point" probe
//   * one or more contract probes that pin behavior unique to that series
//     (gap padding, ExactMatch precision, axis-only acceptance, etc.)
//
// Probes go through SKChart.GetPointsAt, which is the public path consumers
// touch via DataPointerDown/Hover.
[TestClass]
public class SeriesHitTests
{
    // --- ScatterSeries ----------------------------------------------------
    // Default strategy = CompareAllTakeClosest. RectangleHoverArea is a
    // gs x gs square centred on the point.

    [TestMethod]
    public void Scatter_CompareAll_HitsAtCenter()
    {
        var series = new ScatterSeries<ObservablePoint>
        {
            Values = [new(0, 0), new(1, 1), new(2, 2)],
            GeometrySize = 20,
        };
        var chart = NewCartesianChart(series);
        var center = HoverAreaCenter(series, chart, 1);

        var hits = chart.GetPointsAt(new(center.X, center.Y), FindingStrategy.CompareAll).ToArray();
        Assert.AreEqual(1, hits.Length);
        Assert.AreEqual(1d, hits[0].Coordinate.SecondaryValue);
    }

    [TestMethod]
    public void Scatter_CompareAllTakeClosest_PicksOneOnOverlap()
    {
        var series = new ScatterSeries<ObservablePoint>
        {
            Values = [new(0, 0), new(0.01, 0.01)],
            GeometrySize = 80,
        };
        var chart = NewCartesianChart(series);
        var c0 = HoverAreaCenter(series, chart, 0);

        var hits = chart.GetPointsAt(
            new(c0.X + 1, c0.Y + 1),
            FindingStrategy.CompareAllTakeClosest).ToArray();
        Assert.AreEqual(1, hits.Length, "TakeClosest must collapse the overlap to one point");
    }

    [TestMethod]
    public void Scatter_OutsideArea_Misses()
    {
        var series = new ScatterSeries<ObservablePoint> { Values = [new(1, 1)], GeometrySize = 10 };
        var chart = NewCartesianChart(series);
        _ = chart.GetImage();
        var hits = chart.GetPointsAt(new(0, 0), FindingStrategy.CompareAll).ToArray();
        Assert.AreEqual(0, hits.Length);
    }

    [TestMethod]
    public void Scatter_CompareOnlyX_HitsAnyYInColumn()
    {
        var series = new ScatterSeries<ObservablePoint>
        {
            Values = [new(1, 1)],
            GeometrySize = 20,
        };
        var chart = NewCartesianChart(series);
        _ = chart.GetImage();
        var area = ReadHoverArea(series, chart, 0);
        var midX = area.X + area.Width * 0.5f;

        // Y is far above the marker — X-only must still hit because the area
        // intersects the column on X.
        var hits = chart.GetPointsAt(new(midX, area.Y - 50), FindingStrategy.CompareOnlyX).ToArray();
        Assert.AreEqual(1, hits.Length);
    }

    // --- ColumnSeries (BarSeries with vertical orientation) ---------------
    // Default strategy = CompareOnlyXTakeClosest. HoverArea uses `actualUw`
    // (wider than the visual) so the gap between bars is hoverable.
    // ExactMatch instead probes the *visual* directly via the override.

    [TestMethod]
    public void Column_CompareOnlyX_HitsAtColumnCenter()
    {
        var series = new ColumnSeries<int> { Values = [10, 20, 30] };
        var chart = NewCartesianChart(series);
        var center = HoverAreaCenter(series, chart, 1);

        var hits = chart.GetPointsAt(new(center.X, center.Y), FindingStrategy.CompareOnlyX).ToArray();
        Assert.AreEqual(1, hits.Length);
        Assert.AreEqual(20d, hits[0].Coordinate.PrimaryValue);
    }

    [TestMethod]
    public void Column_CompareOnlyXTakeClosest_VisualGapStillHits()
    {
        // ColumnSeries's HoverArea uses actualUw (the full slot width) while
        // the visual is uw = actualUw - Padding. A probe placed in the
        // visual-only gap (between two column visuals on X) must therefore
        // still be covered by a HoverArea — the "easy to hover" UX contract.
        var series = new ColumnSeries<int> { Values = [10, 20, 30] };
        var chart = NewCartesianChart(series);
        var v0 = ReadVisual<RoundedRectangleGeometry>(series, chart, 0);
        var v1 = ReadVisual<RoundedRectangleGeometry>(series, chart, 1);

        var midGapX = (v0.X + v0.Width + v1.X) * 0.5f;
        var midY = (v0.Y + v1.Y) * 0.5f;

        var hits = chart.GetPointsAt(
            new(midGapX, midY),
            FindingStrategy.CompareOnlyXTakeClosest).ToArray();
        Assert.AreEqual(1, hits.Length, "Column hover area should cover the visual gap (UX contract)");
    }

    [TestMethod]
    public void Column_ExactMatch_HitsInsideVisualOnly()
    {
        var series = new ColumnSeries<int> { Values = [10, 20, 30] };
        var chart = NewCartesianChart(series);
        var v = ReadVisual<RoundedRectangleGeometry>(series, chart, 1);
        var centerX = v.X + v.Width * 0.5f;
        var centerY = v.Y + v.Height * 0.5f;

        var hits = chart.GetPointsAt(new(centerX, centerY), FindingStrategy.ExactMatch).ToArray();
        Assert.AreEqual(1, hits.Length);
        Assert.AreEqual(20d, hits[0].Coordinate.PrimaryValue);
    }

    [TestMethod]
    public void Column_ExactMatch_GapBetweenColumnsMisses()
    {
        // ExactMatch override must use the visual extents, not the wider
        // actualUw hover area. A probe in the gap between two visuals must
        // miss — otherwise ExactMatch degrades to CompareOnlyX behavior.
        var series = new ColumnSeries<int> { Values = [10, 20, 30] };
        var chart = NewCartesianChart(series);
        var v0 = ReadVisual<RoundedRectangleGeometry>(series, chart, 0);
        var v1 = ReadVisual<RoundedRectangleGeometry>(series, chart, 1);

        var gapX = (v0.X + v0.Width + v1.X) * 0.5f;
        var midY = (v0.Y + v1.Y) * 0.5f;

        var hits = chart.GetPointsAt(new(gapX, midY), FindingStrategy.ExactMatch).ToArray();
        Assert.AreEqual(0, hits.Length,
            "ExactMatch must respect visual edges, not the inflated hover area");
    }

    [TestMethod]
    public void Column_OutsideArea_Misses()
    {
        var series = new ColumnSeries<int> { Values = [10, 20, 30] };
        var chart = NewCartesianChart(series);
        _ = chart.GetImage();
        var hits = chart.GetPointsAt(new(-50, -50), FindingStrategy.CompareOnlyXTakeClosest).ToArray();
        Assert.AreEqual(0, hits.Length);
    }

    // --- RowSeries (horizontal Bar) --------------------------------------
    // Default strategy = CompareOnlyYTakeClosest.

    [TestMethod]
    public void Row_CompareOnlyY_HitsAtRowCenter()
    {
        var series = new RowSeries<int> { Values = [10, 20, 30] };
        var chart = NewCartesianChart(series);
        var center = HoverAreaCenter(series, chart, 1);

        var hits = chart.GetPointsAt(new(center.X, center.Y), FindingStrategy.CompareOnlyY).ToArray();
        Assert.AreEqual(1, hits.Length);
        Assert.AreEqual(20d, hits[0].Coordinate.PrimaryValue);
    }

    [TestMethod]
    public void Row_CompareOnlyYTakeClosest_VisualGapStillHits()
    {
        // RowSeries inflates its HoverArea height to `actualUw` (the slot
        // height) while the visual height is `uw = actualUw - Padding`.
        // A probe placed just inside the visual-only gap (between two row
        // visuals on Y) must therefore still be covered by a HoverArea.
        // The probe nudges 1 px off the exact midpoint because the gap
        // midpoint lands on the boundary between the two hover areas, where
        // strict-edge IsPointerOver returns false for both.
        var series = new RowSeries<int> { Values = [10, 20, 30] };
        var chart = NewCartesianChart(series);
        var v0 = ReadVisual<RoundedRectangleGeometry>(series, chart, 0);
        var v1 = ReadVisual<RoundedRectangleGeometry>(series, chart, 1);

        var (top, bottom) = v0.Y < v1.Y ? (v0, v1) : (v1, v0);
        var midX = (top.X + bottom.X) * 0.5f;
        var inGapButTowardTop = (top.Y + top.Height + bottom.Y) * 0.5f - 1;

        var hits = chart.GetPointsAt(
            new(midX, inGapButTowardTop),
            FindingStrategy.CompareOnlyYTakeClosest).ToArray();
        Assert.AreEqual(1, hits.Length, "Row hover area should cover the visual inter-row gap (UX contract)");
    }

    // --- RangeColumnSeries ----------------------------------------------
    // Inherits FindPointsInPosition from BarSeries (ExactMatch = visual rect,
    // others = HoverArea). MeasureBarLayout gives the visual a Y span of
    // |HighPx - LowPx| anchored at min(High,Low)Px — so ExactMatch hits the
    // [Low, High] band and misses the area above High / below Low.

    [TestMethod]
    public void RangeColumn_CompareOnlyX_HitsAtColumnCenter()
    {
        var series = new RangeColumnSeries<RangeValue>
        {
            Values = [new(10, 20), new(15, 25), new(5, 30)],
        };
        var chart = NewCartesianChart(series);
        var center = HoverAreaCenter(series, chart, 1);

        var hits = chart.GetPointsAt(new(center.X, center.Y), FindingStrategy.CompareOnlyX).ToArray();
        Assert.AreEqual(1, hits.Length);
        Assert.AreEqual(25d, hits[0].Coordinate.PrimaryValue);
    }

    [TestMethod]
    public void RangeColumn_ExactMatch_HitsInsideBandVisual()
    {
        // Visual spans [Low, High] vertically (height = |HighPx - LowPx|, not
        // a 0-to-Value column). Probe at the band midpoint must hit; this
        // distinguishes the range column from a regular column whose visual
        // spans [Pivot, Value].
        var series = new RangeColumnSeries<RangeValue>
        {
            Values = [new(10, 30)],
        };
        var chart = NewCartesianChart(series);
        var v = ReadVisual<RoundedRectangleGeometry>(series, chart, 0);
        var probeX = v.X + v.Width * 0.5f;
        var probeY = v.Y + v.Height * 0.5f;

        var hits = chart.GetPointsAt(new(probeX, probeY), FindingStrategy.ExactMatch).ToArray();
        Assert.AreEqual(1, hits.Length, "Probe inside [Low, High] band must hit");
    }

    [TestMethod]
    public void RangeColumn_ExactMatch_BelowBandMisses()
    {
        // A probe below Low (outside the [Low, High] band on the value axis)
        // must miss. Pins that the visual rectangle is the [Low, High] band,
        // not the [Pivot=0, High] column a regular ColumnSeries would draw.
        var series = new RangeColumnSeries<RangeValue>
        {
            Values = [new(10, 30)],
        };
        var chart = NewCartesianChart(series);
        var v = ReadVisual<RoundedRectangleGeometry>(series, chart, 0);
        var probeX = v.X + v.Width * 0.5f;
        var probeY = v.Y + v.Height + 5;  // below the band's bottom edge

        var hits = chart.GetPointsAt(new(probeX, probeY), FindingStrategy.ExactMatch).ToArray();
        Assert.AreEqual(0, hits.Length, "Probe below Low must miss — range column is NOT [0, High]");
    }

    // --- RangeRowSeries -------------------------------------------------
    // Mirror of RangeColumn for the horizontal orientation. Default
    // strategy = CompareOnlyYTakeClosest; ExactMatch hits the [Low, High]
    // band on X.

    [TestMethod]
    public void RangeRow_CompareOnlyY_HitsAtRowCenter()
    {
        var series = new RangeRowSeries<RangeValue>
        {
            Values = [new(10, 20), new(15, 25), new(5, 30)],
        };
        var chart = NewCartesianChart(series);
        var center = HoverAreaCenter(series, chart, 1);

        var hits = chart.GetPointsAt(new(center.X, center.Y), FindingStrategy.CompareOnlyY).ToArray();
        Assert.AreEqual(1, hits.Length);
        Assert.AreEqual(25d, hits[0].Coordinate.PrimaryValue);
    }

    [TestMethod]
    public void RangeRow_ExactMatch_HitsInsideBandVisual()
    {
        var series = new RangeRowSeries<RangeValue>
        {
            Values = [new(10, 30)],
        };
        var chart = NewCartesianChart(series);
        var v = ReadVisual<RoundedRectangleGeometry>(series, chart, 0);
        var probeX = v.X + v.Width * 0.5f;
        var probeY = v.Y + v.Height * 0.5f;

        var hits = chart.GetPointsAt(new(probeX, probeY), FindingStrategy.ExactMatch).ToArray();
        Assert.AreEqual(1, hits.Length);
    }

    [TestMethod]
    public void RangeRow_ExactMatch_LeftOfBandMisses()
    {
        // Horizontal mirror of RangeColumn_ExactMatch_BelowBandMisses: a
        // probe to the LEFT of Low (outside the band on X) must miss.
        var series = new RangeRowSeries<RangeValue>
        {
            Values = [new(10, 30)],
        };
        var chart = NewCartesianChart(series);
        var v = ReadVisual<RoundedRectangleGeometry>(series, chart, 0);
        var probeX = v.X - 5;  // left of the band
        var probeY = v.Y + v.Height * 0.5f;

        var hits = chart.GetPointsAt(new(probeX, probeY), FindingStrategy.ExactMatch).ToArray();
        Assert.AreEqual(0, hits.Length);
    }

    // --- BoxSeries -------------------------------------------------------
    // Default = CompareOnlyXTakeClosest. RectangleHoverArea spans the full
    // whisker rectangle (High → Low). Override goes pixel-precise on ExactMatch.

    [TestMethod]
    public void Box_CompareOnlyX_HitsInWhiskerColumn()
    {
        var series = new BoxSeries<BoxValue>
        {
            Values =
            [
                new(10, 8, 5, 3, 1),
                new(20, 18, 15, 13, 11),
                new(30, 28, 25, 23, 21),
            ],
        };
        var chart = NewCartesianChart(series);
        var center = HoverAreaCenter(series, chart, 1);

        var hits = chart.GetPointsAt(new(center.X, center.Y), FindingStrategy.CompareOnlyX).ToArray();
        Assert.AreEqual(1, hits.Length);
    }

    [TestMethod]
    public void Box_ExactMatch_HitsInsideVisual()
    {
        var series = new BoxSeries<BoxValue>
        {
            Values =
            [
                new(10, 8, 5, 3, 1),
                new(20, 18, 15, 13, 11),
                new(30, 28, 25, 23, 21),
            ],
        };
        var chart = NewCartesianChart(series);
        var v = ReadVisual<BoxGeometry>(series, chart, 1);
        var cx = v.X + v.Width * 0.5f;
        var cy = (v.Y + v.Min) * 0.5f;

        var hits = chart.GetPointsAt(new(cx, cy), FindingStrategy.ExactMatch).ToArray();
        Assert.AreEqual(1, hits.Length);
    }

    // --- CandlesticksSeries (Financial) ----------------------------------
    // Default = CompareOnlyXTakeClosest.

    [TestMethod]
    public void Candlestick_CompareOnlyX_HitsInCandleColumn()
    {
        var series = new CandlesticksSeries<FinancialPointI>
        {
            Values =
            [
                new(10, 8, 5, 3),
                new(20, 18, 15, 13),
                new(30, 28, 25, 23),
            ],
        };
        var chart = NewCartesianChart(series);
        var center = HoverAreaCenter(series, chart, 1);

        var hits = chart.GetPointsAt(new(center.X, center.Y), FindingStrategy.CompareOnlyX).ToArray();
        Assert.AreEqual(1, hits.Length);
    }

    [TestMethod]
    public void Candlestick_CompareOnlyX_GapAroundVisualStillHits()
    {
        // MaxBarWidth (default 25) clamps the visual candle to a thin column
        // inside its axis-unit slot. The hover area must cover the whole
        // slot so a probe in the "gap" between visuals (still inside the
        // category column) still hits — matching BoxSeries / BarSeries.
        // Pre-fix the hover area was the visual width and this probe missed.
        var series = new CandlesticksSeries<FinancialPointI>
        {
            Values = [new(10, 8, 5, 3), new(20, 18, 15, 13)],
        };
        var chart = NewCartesianChart(series);
        var v = ReadVisual<CandlestickGeometry>(series, chart, 0);

        // 5 px past the right edge of the (narrow) candle body — clearly
        // outside the visual but well inside the axis-unit slot on a
        // 1000 px wide 2-candle chart.
        var probeX = v.X + v.Width + 5;
        var probeY = v.Y + 1;

        var hits = chart.GetPointsAt(new(probeX, probeY), FindingStrategy.CompareOnlyX).ToArray();
        Assert.AreEqual(1, hits.Length,
            "Candlestick hover area should span the full category slot, not just the clamped visual");
    }

    // --- OhlcSeries (Financial, I-bar variant) ---------------------------
    // Default = CompareOnlyXTakeClosest. Reuses CoreFinancialSeries hit-test —
    // only the rendered geometry differs. Same probes as CandlesticksSeries
    // pin financial-pipeline parity for the I-bar variant.

    [TestMethod]
    public void Ohlc_CompareOnlyX_HitsInBarColumn()
    {
        var series = new OhlcSeries<FinancialPointI>
        {
            Values =
            [
                new(10, 8, 5, 3),
                new(20, 18, 15, 13),
                new(30, 28, 25, 23),
            ],
        };
        var chart = NewCartesianChart(series);
        var center = HoverAreaCenter(series, chart, 1);

        var hits = chart.GetPointsAt(new(center.X, center.Y), FindingStrategy.CompareOnlyX).ToArray();
        Assert.AreEqual(1, hits.Length);
    }

    [TestMethod]
    public void Ohlc_OutsideChart_Misses()
    {
        var series = new OhlcSeries<FinancialPointI>
        {
            Values = [new(10, 8, 5, 3), new(20, 18, 15, 13)],
        };
        var chart = NewCartesianChart(series);
        _ = chart.GetImage();

        var hits = chart.GetPointsAt(new(0, 0), FindingStrategy.CompareOnlyX).ToArray();
        Assert.AreEqual(0, hits.Length);
    }

    [TestMethod]
    public void Ohlc_CompareOnlyX_GapAroundVisualStillHits()
    {
        // Mirrors the candlestick gap-padding contract — the OHLC visual
        // (BaseCandlestickGeometry) is clamped by MaxBarWidth the same way,
        // so a probe in the inter-bar gap (inside the axis-unit slot) must
        // still hit. Pins parity so a future financial-pipeline tweak can't
        // silently diverge between the two variants.
        var series = new OhlcSeries<FinancialPointI>
        {
            Values = [new(10, 8, 5, 3), new(20, 18, 15, 13)],
        };
        var chart = NewCartesianChart(series);
        var v = ReadVisual<OhlcGeometry>(series, chart, 0);

        var probeX = v.X + v.Width + 5;
        var probeY = v.Y + 1;

        var hits = chart.GetPointsAt(new(probeX, probeY), FindingStrategy.CompareOnlyX).ToArray();
        Assert.AreEqual(1, hits.Length,
            "OHLC hover area should span the full category slot, matching CandlesticksSeries");
    }

    // --- LineSeries ------------------------------------------------------
    // Default = CompareOnlyXTakeClosest. HoverArea is a uwx × geometrySize
    // rectangle around the marker. ExactMatch goes through the override and
    // checks the visual marker rectangle only.

    [TestMethod]
    public void Line_CompareOnlyX_HitsAnyYInColumn()
    {
        var series = new LineSeries<int> { Values = [10, 20, 30], GeometrySize = 12 };
        var chart = NewCartesianChart(series);
        var area = ReadHoverArea(series, chart, 1);
        var midX = area.X + area.Width * 0.5f;
        var probeY = area.Y - 100;

        var hits = chart.GetPointsAt(new(midX, probeY), FindingStrategy.CompareOnlyX).ToArray();
        Assert.AreEqual(1, hits.Length);
        Assert.AreEqual(20d, hits[0].Coordinate.PrimaryValue);
    }

    [TestMethod]
    public void Line_ExactMatch_MissesOffMarker()
    {
        // ExactMatch on LineSeries goes through the override and pins the
        // hit to the marker rectangle only. A probe within the column but
        // far from the marker on Y must miss.
        var series = new LineSeries<int> { Values = [10, 20, 30], GeometrySize = 12 };
        var chart = NewCartesianChart(series);
        var area = ReadHoverArea(series, chart, 1);
        var midX = area.X + area.Width * 0.5f;
        var probeY = area.Y - 100;

        var hits = chart.GetPointsAt(new(midX, probeY), FindingStrategy.ExactMatch).ToArray();
        Assert.AreEqual(0, hits.Length);
    }

    [TestMethod]
    public void Line_ExactMatchTakeClosest_EmptyProbeReturnsNothing()
    {
        // ExactMatchTakeClosest must still respect the visual-containment
        // filter: a probe far from any marker returns empty, not the
        // nearest marker. Pre-fix the override skipped the filter and
        // unconditionally returned one point.
        var series = new LineSeries<int> { Values = [10, 20, 30], GeometrySize = 12 };
        var chart = NewCartesianChart(series);
        _ = chart.GetImage();

        var hits = chart.GetPointsAt(new(0, 0), FindingStrategy.ExactMatchTakeClosest).ToArray();
        Assert.AreEqual(0, hits.Length);
    }

    [TestMethod]
    public void Line_ExactMatchTakeClosest_OnMarker_ReturnsOne()
    {
        // A probe at the marker center DOES hit (one closest point).
        var series = new LineSeries<int> { Values = [10, 20, 30], GeometrySize = 12 };
        var chart = NewCartesianChart(series);
        var v = ReadVisual<CircleGeometry>(series, chart, 1);
        var cx = v.X + v.TranslateTransform.X + v.Width * 0.5f;
        var cy = v.Y + v.TranslateTransform.Y + v.Height * 0.5f;

        var hits = chart.GetPointsAt(new(cx, cy), FindingStrategy.ExactMatchTakeClosest).ToArray();
        Assert.AreEqual(1, hits.Length);
    }

    // --- StepLineSeries --------------------------------------------------
    // Mirrors LineSeries with the same override pattern.

    [TestMethod]
    public void StepLine_CompareOnlyX_HitsAnyYInColumn()
    {
        var series = new StepLineSeries<int> { Values = [10, 20, 30], GeometrySize = 12 };
        var chart = NewCartesianChart(series);
        var area = ReadHoverArea(series, chart, 1);
        var midX = area.X + area.Width * 0.5f;
        var probeY = area.Y - 100;

        var hits = chart.GetPointsAt(new(midX, probeY), FindingStrategy.CompareOnlyX).ToArray();
        Assert.AreEqual(1, hits.Length);
    }

    [TestMethod]
    public void StepLine_ExactMatchTakeClosest_EmptyProbeReturnsNothing()
    {
        var series = new StepLineSeries<int> { Values = [10, 20, 30], GeometrySize = 12 };
        var chart = NewCartesianChart(series);
        _ = chart.GetImage();

        var hits = chart.GetPointsAt(new(0, 0), FindingStrategy.ExactMatchTakeClosest).ToArray();
        Assert.AreEqual(0, hits.Length);
    }

    // --- RangeLineSeries -------------------------------------------------
    // Default = CompareOnlyXTakeClosest (PrefersXStrategyTooltips). HoverArea
    // is uwx × bandHeight, covering the full band column at each X — any Y
    // strategy lands inside the band. ExactMatch goes through the override
    // and hits EITHER the high marker OR the low marker (VisualContains OR).

    [TestMethod]
    public void RangeLine_CompareOnlyX_HitsAnywhereInBand()
    {
        var series = new RangeLineSeries<RangeValue>
        {
            Values = [new(10, 30), new(15, 35), new(5, 25)],
            GeometrySize = 12,
        };
        var chart = NewCartesianChart(series);
        var area = ReadHoverArea(series, chart, 1);
        var midX = area.X + area.Width * 0.5f;
        var midY = area.Y + area.Height * 0.5f;

        var hits = chart.GetPointsAt(new(midX, midY), FindingStrategy.CompareOnlyX).ToArray();
        Assert.AreEqual(1, hits.Length);
        Assert.AreEqual(35d, hits[0].Coordinate.PrimaryValue);
    }

    [TestMethod]
    public void RangeLine_ExactMatchTakeClosest_OnHighMarker_Hits()
    {
        var series = new RangeLineSeries<RangeValue>
        {
            Values = [new(10, 30), new(15, 35), new(5, 25)],
            GeometrySize = 16,
        };
        var chart = NewCartesianChart(series);
        var hi = ReadHighMarker(series, chart, 1);
        var cx = hi.X + hi.TranslateTransform.X + hi.Width * 0.5f;
        var cy = hi.Y + hi.TranslateTransform.Y + hi.Height * 0.5f;

        var hits = chart.GetPointsAt(new(cx, cy), FindingStrategy.ExactMatchTakeClosest).ToArray();
        Assert.AreEqual(1, hits.Length, "ExactMatch must hit a probe at the high marker center");
        Assert.AreEqual(35d, hits[0].Coordinate.PrimaryValue);
    }

    [TestMethod]
    public void RangeLine_ExactMatchTakeClosest_OnLowMarker_Hits()
    {
        // Mirror of the high-marker case — pins that ExactMatch's
        // VisualContains OR's both markers, not just point.Context.Visual.
        var series = new RangeLineSeries<RangeValue>
        {
            Values = [new(10, 30), new(15, 35), new(5, 25)],
            GeometrySize = 16,
        };
        var chart = NewCartesianChart(series);
        var lo = ReadLowMarker(series, chart, 1);
        var cx = lo.X + lo.TranslateTransform.X + lo.Width * 0.5f;
        var cy = lo.Y + lo.TranslateTransform.Y + lo.Height * 0.5f;

        var hits = chart.GetPointsAt(new(cx, cy), FindingStrategy.ExactMatchTakeClosest).ToArray();
        Assert.AreEqual(1, hits.Length, "ExactMatch must hit a probe at the low marker center");
        Assert.AreEqual(15d, hits[0].Coordinate.TertiaryValue);
    }

    [TestMethod]
    public void RangeLine_ExactMatch_BetweenMarkers_Misses()
    {
        // ExactMatch is markers-only — a probe inside the band but vertically
        // between the high and low markers must miss. CompareOnlyX would still
        // hit via the band-column HoverArea; that's what
        // RangeLine_CompareOnlyX_HitsAnywhereInBand covers.
        var series = new RangeLineSeries<RangeValue>
        {
            Values = [new(10, 30), new(15, 35), new(5, 25)],
            GeometrySize = 12,
        };
        var chart = NewCartesianChart(series);
        var hi = ReadHighMarker(series, chart, 1);
        var lo = ReadLowMarker(series, chart, 1);
        var cx = hi.X + hi.TranslateTransform.X + hi.Width * 0.5f;
        var midY = (hi.Y + lo.Y) * 0.5f;  // halfway between marker centers

        var hits = chart.GetPointsAt(new(cx, midY), FindingStrategy.ExactMatch).ToArray();
        Assert.AreEqual(0, hits.Length);
    }

    [TestMethod]
    public void RangeLine_ExactMatchTakeClosest_EmptyProbeReturnsNothing()
    {
        // Mirror of Line_ExactMatchTakeClosest_EmptyProbeReturnsNothing:
        // the visual-containment filter must still apply even with TakeClosest,
        // so a probe far from any marker returns empty, not the nearest marker.
        var series = new RangeLineSeries<RangeValue>
        {
            Values = [new(10, 30), new(15, 35), new(5, 25)],
            GeometrySize = 12,
        };
        var chart = NewCartesianChart(series);
        _ = chart.GetImage();

        var hits = chart.GetPointsAt(new(0, 0), FindingStrategy.ExactMatchTakeClosest).ToArray();
        Assert.AreEqual(0, hits.Length);
    }

    // --- StackedColumnSeries --------------------------------------------
    // Inherits FindPointsInPosition from BarSeries verbatim; the contract
    // worth pinning here is the stacker: each layer's visual is anchored
    // at its CumulativeStart on the value axis. A probe in the layer 1
    // slice must hit layer 1 (and not layer 0); ExactMatch is the strict
    // discriminator because each layer's visual rectangle is distinct.

    [TestMethod]
    public void StackedColumn_CompareOnlyX_HitsAllLayers()
    {
        // CompareOnlyX uses HoverAreas which all share the same X span at
        // a given category — every stacked layer reports a hit there.
        var s1 = new StackedColumnSeries<int> { Values = [3, 5, 4] };
        var s2 = new StackedColumnSeries<int> { Values = [2, 3, 6] };
        var chart = NewCartesianChartMulti(s1, s2);
        var center = HoverAreaCenter(s1, chart, 1);

        var hits = chart.GetPointsAt(new(center.X, center.Y), FindingStrategy.CompareOnlyX).ToArray();
        Assert.AreEqual(2, hits.Length, "CompareOnlyX must report a hit for each stacked layer at the probed X");
    }

    [TestMethod]
    public void StackedColumn_ExactMatch_HitsCorrectLayer()
    {
        // The stacker shifts s2's visual UP to sit on top of s1's. A probe
        // inside s2's visual must hit only s2, NOT s1. Pre-stacker regression
        // would have BOTH visuals at the same Y and either both would hit or
        // the wrong one would.
        var s1 = new StackedColumnSeries<int> { Values = [10, 10, 10] };
        var s2 = new StackedColumnSeries<int> { Values = [10, 10, 10] };
        var chart = NewCartesianChartMulti(s1, s2);
        var s2Visual = ReadVisual<RoundedRectangleGeometry>(s2, chart, 1);
        var probeX = s2Visual.X + s2Visual.Width * 0.5f;
        var probeY = s2Visual.Y + s2Visual.Height * 0.5f;

        var hits = chart.GetPointsAt(new(probeX, probeY), FindingStrategy.ExactMatch).ToArray();
        Assert.AreEqual(1, hits.Length, "ExactMatch inside s2's rect must hit exactly one layer");
        Assert.AreSame(s2, hits[0].Context.Series, "the hit must be the top layer (s2), not s1 underneath");
    }

    // --- StackedRowSeries -----------------------------------------------
    // Horizontal mirror.

    [TestMethod]
    public void StackedRow_CompareOnlyY_HitsAllLayers()
    {
        var s1 = new StackedRowSeries<int> { Values = [3, 5, 4] };
        var s2 = new StackedRowSeries<int> { Values = [2, 3, 6] };
        var chart = NewCartesianChartMulti(s1, s2);
        var center = HoverAreaCenter(s1, chart, 1);

        var hits = chart.GetPointsAt(new(center.X, center.Y), FindingStrategy.CompareOnlyY).ToArray();
        Assert.AreEqual(2, hits.Length, "CompareOnlyY must report a hit for each stacked layer at the probed Y");
    }

    [TestMethod]
    public void StackedRow_LayerHoverAreasOffsetByStacker()
    {
        // The stacker shifts s2's HoverArea to the right of s1's (cumulative
        // on X). Pin the offset explicitly — pre-stacker regression would
        // collapse both onto the same X.
        //
        // Note: HorizontalBarSeries.MeasureBarLayout records categoryHoverWidth
        // as a NEGATIVE value when stacked (cx = pixel of cumulative END,
        // width = pixel of START - pixel of END < 0). RectangleHoverArea
        // normalizes internally (issue #2165 / PR #2183), so CompareOnlyY-style
        // strategies work; but BarSeries.FindPointsInPosition's ExactMatch
        // override is a straight `pX > v.X && pX < v.X + v.Width` check that
        // returns false for negative-width visuals. Test the HoverArea
        // contract (which is correct) here; the inline ExactMatch case lives
        // as a known gap on BarSeries.
        var s1 = new StackedRowSeries<int> { Values = [10, 10, 10] };
        var s2 = new StackedRowSeries<int> { Values = [10, 10, 10] };
        var chart = NewCartesianChartMulti(s1, s2);
        var a1 = ReadHoverArea(s1, chart, 1);
        var a2 = ReadHoverArea(s2, chart, 1);

        // Use NormalizedXEnd because Width may be negative under the
        // hood — RectangleHoverArea exposes the *effective* extents but a
        // direct (X+Width) read would mislead. We just need the offset proof.
        var s1RightEdge = Math.Max(a1.X, a1.X + a1.Width);
        var s2LeftEdge = Math.Min(a2.X, a2.X + a2.Width);

        Assert.IsTrue(s2LeftEdge >= s1RightEdge - 0.5,
            $"Stacker must offset s2 to the right of s1; got s1.right={s1RightEdge}, s2.left={s2LeftEdge}");
    }

    // --- StackedAreaSeries -----------------------------------------------
    // Inherits LineSeries' FindPointsInPosition. The stacker anchors each
    // layer's per-point HoverArea at the layer's CumulativeStart on Y.
    // Default ctor sets GeometrySize=0; bump it for the test so the
    // marker-sized hover area is non-degenerate (uwx × geometrySize).

    [TestMethod]
    public void StackedArea_CompareOnlyX_HitsAllLayers()
    {
        var s1 = new StackedAreaSeries<int> { Values = [3, 5, 4], GeometrySize = 12 };
        var s2 = new StackedAreaSeries<int> { Values = [2, 3, 6], GeometrySize = 12 };
        var chart = NewCartesianChartMulti(s1, s2);
        var area = ReadHoverArea(s1, chart, 1);
        var midX = area.X + area.Width * 0.5f;
        var midY = area.Y + area.Height * 0.5f;

        var hits = chart.GetPointsAt(new(midX, midY), FindingStrategy.CompareOnlyX).ToArray();
        Assert.AreEqual(2, hits.Length, "Each stacked layer reports a hit at the same X");
    }

    [TestMethod]
    public void StackedArea_LayerHoverAreasOffsetByStacker()
    {
        // The bottom layer's HoverArea sits at the data-value Y; the top
        // layer's sits at (bottom.Value + top.Value)Y. Pre-stacker regression
        // would put both at the same Y. Pin the offset explicitly.
        var s1 = new StackedAreaSeries<int> { Values = [10, 10, 10], GeometrySize = 12 };
        var s2 = new StackedAreaSeries<int> { Values = [10, 10, 10], GeometrySize = 12 };
        var chart = NewCartesianChartMulti(s1, s2);
        var a1 = ReadHoverArea(s1, chart, 1);
        var a2 = ReadHoverArea(s2, chart, 1);

        // On a non-inverted Y axis, larger Y values render at smaller pixel
        // Y. s2 sits ATOP s1 in data space, so s2.Y must be a smaller pixel
        // (higher on screen) than s1.Y.
        Assert.IsTrue(a2.Y < a1.Y,
            $"Stacker must offset s2 above s1; got s1.Y={a1.Y}, s2.Y={a2.Y}");
    }

    // --- StackedStepAreaSeries -------------------------------------------
    // Same shape as StackedArea but inherits CoreStepLineSeries instead.

    [TestMethod]
    public void StackedStepArea_CompareOnlyX_HitsAllLayers()
    {
        var s1 = new StackedStepAreaSeries<int> { Values = [3, 5, 4], GeometrySize = 12 };
        var s2 = new StackedStepAreaSeries<int> { Values = [2, 3, 6], GeometrySize = 12 };
        var chart = NewCartesianChartMulti(s1, s2);
        var area = ReadHoverArea(s1, chart, 1);
        var midX = area.X + area.Width * 0.5f;
        var midY = area.Y + area.Height * 0.5f;

        var hits = chart.GetPointsAt(new(midX, midY), FindingStrategy.CompareOnlyX).ToArray();
        Assert.AreEqual(2, hits.Length);
    }

    // --- HeatSeries ------------------------------------------------------
    // Default = CompareAllTakeClosest (PrefersXYStrategyTooltips falls through).
    // Each cell carries a RectangleHoverArea covering exactly the cell rect.

    [TestMethod]
    public void Heat_CompareAll_HitsCellCenter()
    {
        var series = new HeatSeries<WeightedPoint>
        {
            Values =
            [
                new(0, 0, 0), new(0, 1, 1), new(0, 2, 2),
                new(1, 0, 3), new(1, 1, 4), new(1, 2, 5),
                new(2, 0, 6), new(2, 1, 7), new(2, 2, 8),
            ],
        };
        var chart = NewCartesianChart(series);
        var center = HoverAreaCenter(series, chart, 4); // cell (1,1)

        var hits = chart.GetPointsAt(new(center.X, center.Y), FindingStrategy.CompareAll).ToArray();
        Assert.AreEqual(1, hits.Length);
    }

    [TestMethod]
    public void Heat_OutsideAnyCell_Misses()
    {
        var series = new HeatSeries<WeightedPoint> { Values = [new(0, 0, 0)] };
        var chart = NewCartesianChart(series);
        _ = chart.GetImage();
        var hits = chart.GetPointsAt(new(-100, -100), FindingStrategy.CompareAll).ToArray();
        Assert.AreEqual(0, hits.Length);
    }

    // --- PolarLineSeries -------------------------------------------------
    // Like ScatterSeries: square HoverArea around each polar point.

    [TestMethod]
    public void PolarLine_CompareAll_HitsAtPointCenter()
    {
        var series = new PolarLineSeries<int> { Values = [10, 20, 30], GeometrySize = 20 };
        var chart = new SKPolarChart
        {
            Width = 600,
            Height = 600,
            Series = [series],
        };
        _ = chart.GetImage();

        var area = (RectangleHoverArea)((ISeries)series)
            .Fetch(chart.CoreChart).ElementAt(1).Context.HoverArea!;
        var center = new LvcPoint(area.X + area.Width * 0.5f, area.Y + area.Height * 0.5f);

        var hits = chart.GetPointsAt(new(center.X, center.Y), FindingStrategy.CompareAll).ToArray();
        Assert.AreEqual(1, hits.Length);
    }

    // --- PieSeries -------------------------------------------------------
    // SemicircleHoverArea: angular containment + radial containment in
    // [InnerRadius, Radius/2].

    [TestMethod]
    public void Pie_HitsInsideSlice()
    {
        var seriesCollection = new double[] { 100, 100, 100, 100 }.AsPieSeries();
        var chart = new SKPieChart
        {
            Width = 400,
            Height = 400,
            Series = seriesCollection,
        };
        _ = chart.GetImage();

        // Slice 0 occupies angles [0, 90); probe at ~45° at mid-radius.
        var cx = 200f; var cy = 200f;
        var r = 80f;
        var angle = 45 * Math.PI / 180;
        var px = (float)(cx + r * Math.Cos(angle));
        var py = (float)(cy + r * Math.Sin(angle));

        var hits = chart.GetPointsAt(new(px, py), FindingStrategy.CompareAll).ToArray();
        Assert.IsTrue(hits.Length >= 1, "Probe inside the first slice should hit at least one point");
    }

    [TestMethod]
    public void Pie_OutsideRadius_Misses()
    {
        var seriesCollection = new double[] { 100, 100, 100, 100 }.AsPieSeries();
        var chart = new SKPieChart
        {
            Width = 400,
            Height = 400,
            Series = seriesCollection,
        };
        _ = chart.GetImage();

        var hits = chart.GetPointsAt(new(395, 395), FindingStrategy.CompareAll).ToArray();
        Assert.AreEqual(0, hits.Length);
    }

    // --- RectangleHoverArea strategy contract ----------------------------
    // Pins that Automatic is rejected at the area level (the chart is
    // expected to resolve it via GetFindingStrategy() before calling).
    // Also pins the strict-edge contract — a probe exactly on X or Y edge
    // is a MISS (the ` > ` / ` < ` form in IsPointerOver).

    [TestMethod]
    public void RectangleHoverArea_Automatic_Throws()
    {
        var ha = new RectangleHoverArea().SetDimensions(0, 0, 10, 10);
        _ = Assert.ThrowsExactly<Exception>(
            () => ha.IsPointerOver(new LvcPoint(5, 5), FindingStrategy.Automatic));
    }

    [TestMethod]
    public void RectangleHoverArea_OnEdge_Misses()
    {
        var ha = new RectangleHoverArea().SetDimensions(10, 20, 30, 40);
        Assert.IsFalse(ha.IsPointerOver(new LvcPoint(10, 30), FindingStrategy.CompareAll), "left edge");
        Assert.IsFalse(ha.IsPointerOver(new LvcPoint(40, 30), FindingStrategy.CompareAll), "right edge");
        Assert.IsFalse(ha.IsPointerOver(new LvcPoint(20, 20), FindingStrategy.CompareAll), "top edge");
        Assert.IsFalse(ha.IsPointerOver(new LvcPoint(20, 60), FindingStrategy.CompareAll), "bottom edge");
        Assert.IsTrue(ha.IsPointerOver(new LvcPoint(20, 30), FindingStrategy.CompareAll), "interior");
    }

    // --- SemicircleHoverArea distance contract ---------------------------
    // Pins that DistanceTo measures from the slice midpoint anchored at
    // (CenterX, CenterY) — pre-fix it ignored center and computed against
    // a phantom point around the origin, breaking TakeClosest ordering.

    [TestMethod]
    public void SemicircleHoverArea_DistanceTo_AtSliceMidpoint_IsNearZero()
    {
        // First quadrant slice (0° to 90°), inner=0, outer=Radius/2.
        // Mid-radius = (0 + 100/2) / 2 = 25. Mid-angle = 45°.
        var ha = new SemicircleHoverArea
        {
            CenterX = 500,
            CenterY = 500,
            StartAngle = 0,
            EndAngle = 90,
            InnerRadius = 0,
            Radius = 100,
        };

        var rad = 45 * Math.PI / 180;
        var midX = (float)(500 + 25 * Math.Cos(rad));
        var midY = (float)(500 + 25 * Math.Sin(rad));

        var d = ha.DistanceTo(new LvcPoint(midX, midY), FindingStrategy.CompareAllTakeClosest);

        Assert.IsTrue(d < 0.001,
            $"DistanceTo must be ~0 at the slice midpoint; got {d}. " +
            "If this asserts again, DistanceTo dropped the CenterX/CenterY translation.");
    }

    [TestMethod]
    public void SemicircleHoverArea_DistanceTo_AtPieCenter_EqualsMidRadius()
    {
        // A probe at the pie center must be exactly mid-radius away from any
        // slice's midpoint (regardless of which slice — they all share the
        // same mid-radius for a given InnerRadius/Radius pair).
        var ha = new SemicircleHoverArea
        {
            CenterX = 500,
            CenterY = 500,
            StartAngle = 30,
            EndAngle = 70,
            InnerRadius = 20,
            Radius = 200,
        };

        var d = ha.DistanceTo(new LvcPoint(500, 500), FindingStrategy.CompareAllTakeClosest);

        var midRadius = (20 + 200 * 0.5f) * 0.5; // 60
        Assert.AreEqual(midRadius, d, 0.001);
    }

    // --- helpers ---------------------------------------------------------

    // --- SankeySeries -----------------------------------------------------
    // Vertical layout uses RectangleHoverArea per node (the column rect).
    // BipartiteArc layout uses AnnularSectorHoverArea (polar bounds).
    // Series.Fetch goes through DataFactory which requires a registered
    // mapper for the user node type; sankey synthesizes points internally so
    // tests probe via the public chart.GetPointsAt path instead.

    [TestMethod]
    public void Sankey_Vertical_CompareAll_HitsLeftColumnNode()
    {
        var (chart, _, _, _) = NewSankeyVertical();
        _ = chart.GetImage();

        // Left column = x ∈ [0, 24] (NodeWidth=24). Aim somewhere in the
        // top half of that column — the top source-node will live there.
        var hits = chart.GetPointsAt(new(12, 200), FindingStrategy.CompareAll).ToArray();

        Assert.AreEqual(1, hits.Length, "expected a single source-column node hit");
        Assert.IsInstanceOfType<SankeyNode>(hits[0].Context.DataSource);
    }

    [TestMethod]
    public void Sankey_Vertical_BetweenColumns_Misses()
    {
        var (chart, _, _, _) = NewSankeyVertical();
        _ = chart.GetImage();
        // Dead center is between the two columns (ribbons live there but
        // aren't hit-testable in v1).
        var hits = chart.GetPointsAt(new(500, 500), FindingStrategy.CompareAll).ToArray();
        Assert.AreEqual(0, hits.Length);
    }

    [TestMethod]
    public void Sankey_BipartiteArc_EastEdgeHitsRightArcNode()
    {
        var (chart, _, _, _) = NewSankeyBipartiteArc();
        _ = chart.GetImage();

        // 1000×1000 chart, center=(500,500), outerR=500 (no labels). Probe
        // ~(990, 500): angle 0, r=490 — inside [innerR=476, outerR=500],
        // squarely on the right-arc node ring.
        var hits = chart.GetPointsAt(new(990, 500), FindingStrategy.CompareAll).ToArray();

        Assert.AreEqual(1, hits.Length, "expected a right-arc node hit");
        Assert.IsInstanceOfType<SankeyNode>(hits[0].Context.DataSource);
    }

    [TestMethod]
    public void Sankey_BipartiteArc_InsideInnerRadius_Misses()
    {
        var (chart, _, _, _) = NewSankeyBipartiteArc();
        _ = chart.GetImage();
        // Dead center: inside innerR, the chord ribbons pass through here but
        // ribbons don't get hover areas in v1.
        var hits = chart.GetPointsAt(new(500, 500), FindingStrategy.CompareAll).ToArray();
        Assert.AreEqual(0, hits.Length);
    }

    private static (SKSankeyChart Chart, SankeySeries<SankeyNode> Series, SankeyNode[] Sources, SankeyNode[] Sinks)
        NewSankeyVertical()
    {
        var sources = new[] { new SankeyNode("A"), new SankeyNode("B") };
        var sinks = new[] { new SankeyNode("X"), new SankeyNode("Y") };
        var nodes = sources.Concat(sinks).ToArray();
        var links = new[]
        {
            new SankeyLink<SankeyNode>(sources[0], sinks[0], 4),
            new SankeyLink<SankeyNode>(sources[0], sinks[1], 2),
            new SankeyLink<SankeyNode>(sources[1], sinks[1], 6),
        };
        var series = new SankeySeries<SankeyNode>
        {
            Values = nodes,
            Links = links,
            NodeWidth = 24,
        };
        var chart = new SKSankeyChart
        {
            Width = 1000,
            Height = 1000,
            Series = [series],
        };
        return (chart, series, sources, sinks);
    }

    private static (SKSankeyChart Chart, SankeySeries<SankeyNode> Series, SankeyNode[] Sources, SankeyNode[] Sinks)
        NewSankeyBipartiteArc()
    {
        var sources = new[] { new SankeyNode("A"), new SankeyNode("B") };
        var sinks = new[] { new SankeyNode("X"), new SankeyNode("Y") };
        var nodes = sources.Concat(sinks).ToArray();
        var links = new[]
        {
            new SankeyLink<SankeyNode>(sources[0], sinks[0], 4),
            new SankeyLink<SankeyNode>(sources[0], sinks[1], 2),
            new SankeyLink<SankeyNode>(sources[1], sinks[1], 6),
        };
        var series = new SankeySeries<SankeyNode>
        {
            Values = nodes,
            Links = links,
            Layout = LiveChartsCore.Kernel.Sketches.SankeyLayoutKind.BipartiteArc,
            NodeWidth = 24,
        };
        var chart = new SKSankeyChart
        {
            Width = 1000,
            Height = 1000,
            Series = [series],
        };
        return (chart, series, sources, sinks);
    }

    private static SKCartesianChart NewCartesianChart(ISeries series) =>
        new()
        {
            Width = 1000,
            Height = 1000,
            Series = [series],
            XAxes = [new Axis { IsVisible = false }],
            YAxes = [new Axis { IsVisible = false }],
        };

    private static SKCartesianChart NewCartesianChartMulti(params ISeries[] series) =>
        new()
        {
            Width = 1000,
            Height = 1000,
            Series = series,
            XAxes = [new Axis { IsVisible = false }],
            YAxes = [new Axis { IsVisible = false }],
        };

    private static RectangleHoverArea ReadHoverArea(ISeries series, SKCartesianChart chart, int index)
    {
        _ = chart.GetImage();
        var point = series.Fetch(chart.CoreChart).ElementAt(index);
        return (RectangleHoverArea)(point.Context.HoverArea
            ?? throw new InvalidOperationException("point has no HoverArea after Measure"));
    }

    private static TVisual ReadVisual<TVisual>(ISeries series, SKCartesianChart chart, int index)
        where TVisual : class
    {
        _ = chart.GetImage();
        var point = series.Fetch(chart.CoreChart).ElementAt(index);
        return (TVisual)point.Context.Visual!;
    }

    private static LvcPoint HoverAreaCenter(ISeries series, SKCartesianChart chart, int index)
    {
        var area = ReadHoverArea(series, chart, index);
        return new LvcPoint(area.X + area.Width * 0.5f, area.Y + area.Height * 0.5f);
    }

    private static BoundedDrawnGeometry ReadHighMarker(ISeries series, SKCartesianChart chart, int index)
    {
        _ = chart.GetImage();
        var point = series.Fetch(chart.CoreChart).ElementAt(index);
        var v = (RangeCubicSegmentVisualPoint)point.Context.AdditionalVisuals!;
        return v.HighGeometry;
    }

    private static BoundedDrawnGeometry ReadLowMarker(ISeries series, SKCartesianChart chart, int index)
    {
        _ = chart.GetImage();
        var point = series.Fetch(chart.CoreChart).ElementAt(index);
        var v = (RangeCubicSegmentVisualPoint)point.Context.AdditionalVisuals!;
        return v.LowGeometry;
    }
}
