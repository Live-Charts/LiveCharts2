using System.Linq;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Drawing;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.SeriesTests;

// Pins the contract that the tooltip's suggested Y anchor lands on the DRAWN
// rectangle's top (or bottom, for negative values), not on the wider category
// hover strip. Without this, row series with any Padding > 0 float the tooltip
// pointer half-padding above the actual bar top — visually disconnected from
// the rectangle it describes.
[TestClass]
public class BarSeriesTooltipAnchorTests
{
    private static void Measure(SKCartesianChart chart)
    {
        var core = (CartesianChartEngine)chart.CoreChart;
        CoreMotionCanvas.DebugElapsedMilliseconds = 0;
        core.IsLoaded = true;
        core._isFirstDraw = true;
        core.Measure();
    }

    [TestMethod]
    public void RowSeries_TooltipAnchorY_MatchesDrawnBarTop_NotCategoryStripTop()
    {
        // Non-zero Padding makes drawn-bar height < category-strip height, so
        // strip-top and drawn-top diverge — this is the case the fix targets.
        var series = new RowSeries<double>
        {
            Values = [10],
            Padding = 10,
        };
        var chart = new SKCartesianChart
        {
            Width = 300,
            Height = 200,
            Series = [series],
            XAxes = [new Axis { MinLimit = 0, MaxLimit = 20 }],
            YAxes = [new Axis()],
        };

        Measure(chart);

        var point = series.everFetched.Single();
        var ha = (RectangleHoverArea)point.Context.HoverArea!;
        var visual = (RoundedRectangleGeometry)point.Context.Visual!;

        Assert.AreEqual(visual.Y, ha.SuggestedTooltipLocation.Y, 0.001f,
            "tooltip Y must anchor on the drawn-bar top, not on the category-strip top");
        Assert.IsTrue(ha.Height > visual.Height,
            "sanity: the category-strip hover height must exceed the drawn-bar height when Padding > 0 — otherwise the regression cannot reproduce");
    }

    [TestMethod]
    public void RangeRowSeries_TooltipAnchorY_MatchesDrawnBarTop()
    {
        var series = new RangeRowSeries<RangeValue>
        {
            Values = [new(2, 8)],
            Padding = 10,
        };
        var chart = new SKCartesianChart
        {
            Width = 300,
            Height = 200,
            Series = [series],
            XAxes = [new Axis { MinLimit = 0, MaxLimit = 10 }],
            YAxes = [new Axis()],
        };

        Measure(chart);

        var point = series.everFetched.Single();
        var ha = (RectangleHoverArea)point.Context.HoverArea!;
        var visual = (RoundedRectangleGeometry)point.Context.Visual!;

        Assert.AreEqual(visual.Y, ha.SuggestedTooltipLocation.Y, 0.001f);
    }

    [TestMethod]
    public void ColumnSeries_TooltipAnchorY_StillEqualsBarTop_NoRegression()
    {
        // Vertical bars set CategoryHoverY == layout.Y, so the fix is a no-op
        // here — pin that so future BarSeries refactors can't silently shift
        // the column-tooltip anchor.
        var series = new ColumnSeries<double>
        {
            Values = [10],
            Padding = 10,
        };
        var chart = new SKCartesianChart
        {
            Width = 300,
            Height = 200,
            Series = [series],
            YAxes = [new Axis { MinLimit = 0, MaxLimit = 20 }],
            XAxes = [new Axis()],
        };

        Measure(chart);

        var point = series.everFetched.Single();
        var ha = (RectangleHoverArea)point.Context.HoverArea!;
        var visual = (RoundedRectangleGeometry)point.Context.Visual!;

        Assert.AreEqual(visual.Y, ha.SuggestedTooltipLocation.Y, 0.001f);
    }

    [TestMethod]
    public void ColumnSeries_NegativeValue_TooltipAnchorsAtBarBottom()
    {
        // PrimaryValue < pivot → bar grows downward (visually below the axis),
        // anchor should hit the bottom edge of the drawn rect.
        var series = new ColumnSeries<double>
        {
            Values = [-10],
            Padding = 10,
        };
        var chart = new SKCartesianChart
        {
            Width = 300,
            Height = 200,
            Series = [series],
            YAxes = [new Axis { MinLimit = -20, MaxLimit = 20 }],
            XAxes = [new Axis()],
        };

        Measure(chart);

        var point = series.everFetched.Single();
        var ha = (RectangleHoverArea)point.Context.HoverArea!;
        var visual = (RoundedRectangleGeometry)point.Context.Visual!;

        Assert.AreEqual(visual.Y + visual.Height, ha.SuggestedTooltipLocation.Y, 0.001f);
        Assert.IsTrue(ha.LessThanPivot, "LessThanPivot flag must still be set when PrimaryValue < pivot");
    }
}
