using System;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.SeriesTests;

// Range series default bounds-calc used PrimaryValue alone, so the auto
// value-axis range was [min(High), max(High)] — clipping any bar whose Low
// extended further out (Gantt's first task on Jun 1->Jun 6 vs. min(High)=Jun 6
// elsewhere). Fix merges TertiaryBounds (the Low endpoints) into the value
// axis. These tests pin that contract: GetBounds must report a value-axis
// range that covers both [min(Low), max(High)] regardless of which endpoint
// is the outer extreme.
[TestClass]
public class RangeSeriesAutoBoundsTests
{
    [TestMethod]
    public void RangeRowSeries_AutoBounds_IncludeLowestLowEndpoint()
    {
        // Gantt-shape data: task 0 starts at 0 (the leftmost Low) and ends at
        // 5; task 1 starts at 6 (so min(High) = 5, not 0). Without the fix the
        // X axis would start at 5 and hide the first half of task 0.
        var series = new RangeRowSeries<RangeValue>
        {
            Values =
            [
                new(0, 5),
                new(6, 10),
                new(8, 14),
            ],
        };
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            Series = [series],
            XAxes = [new Axis()],
            YAxes = [new Axis { Labels = ["A", "B", "C"] }],
        };

        var core = (CartesianChartEngine)chart.CoreChart;
        CoreMotionCanvas.DebugElapsedMilliseconds = 0;
        core.IsLoaded = true;
        core._isFirstDraw = true;
        core.Measure();

        try
        {
            var xAxis = core.XAxes[0];
            var yAxis = core.YAxes[0];
            var sb = series.GetBounds(core, yAxis, xAxis);

            Assert.IsFalse(sb.HasData, "expected populated bounds for a non-empty series");

            // HorizontalBar's axis swap means the returned SecondaryBounds is
            // the X-axis (value) range. Without the fix this would be
            // [5, 14] (only High values); with the fix it must include
            // the smallest Low (0) and the largest High (14).
            Assert.IsTrue(sb.Bounds.VisibleSecondaryBounds.Min <= 0,
                $"X auto-min {sb.Bounds.VisibleSecondaryBounds.Min} must include lowest Low (0); " +
                $"missing it means task #0's left edge is clipped off-axis.");
            Assert.IsTrue(sb.Bounds.VisibleSecondaryBounds.Max >= 14,
                $"X auto-max {sb.Bounds.VisibleSecondaryBounds.Max} must include highest High (14).");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }

    [TestMethod]
    public void RangeColumnSeries_AutoBounds_IncludeLowestLowEndpoint()
    {
        // Waterfall-shape data where one step's Low is the global minimum and
        // another step's High is below it — without the fix the Y axis would
        // start at min(High) and clip the low-Low bar.
        var series = new RangeColumnSeries<RangeValue>
        {
            Values =
            [
                new(0,  100),   // Low=0 is the global Y minimum
                new(100, 150),
                new(180, 130),  // Low=180 > min(High)=100
            ],
        };
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            Series = [series],
            XAxes = [new Axis { Labels = ["Start", "Sales", "Costs"] }],
            YAxes = [new Axis()],
        };

        var core = (CartesianChartEngine)chart.CoreChart;
        CoreMotionCanvas.DebugElapsedMilliseconds = 0;
        core.IsLoaded = true;
        core._isFirstDraw = true;
        core.Measure();

        try
        {
            var xAxis = core.XAxes[0];
            var yAxis = core.YAxes[0];
            var sb = series.GetBounds(core, xAxis, yAxis);

            Assert.IsFalse(sb.HasData);

            // VerticalBar keeps the unswapped view: PrimaryBounds = Y axis
            // (value). Auto range must cover [min(Low)=0, max(High)=180].
            Assert.IsTrue(sb.Bounds.VisiblePrimaryBounds.Min <= 0,
                $"Y auto-min {sb.Bounds.VisiblePrimaryBounds.Min} must include lowest Low (0).");
            Assert.IsTrue(sb.Bounds.VisiblePrimaryBounds.Max >= 180,
                $"Y auto-max {sb.Bounds.VisiblePrimaryBounds.Max} must include highest Low (180); " +
                $"missing it means the Costs bar's top edge is clipped above the chart.");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }

    [TestMethod]
    public void RangeLineSeries_AutoBounds_IncludeLowestLowEndpoint()
    {
        // RangeLine inherits the same GetBounds override as the bar variants:
        // PrimaryValue (High) alone seeds the Y-axis bounds, so without folding
        // TertiaryValue (Low) the auto axis would clip the low curve wherever
        // min(Low) < min(High).
        var series = new RangeLineSeries<RangeValue>
        {
            Values =
            [
                new(0,  100),
                new(50, 150),
                new(75, 130),
            ],
        };
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            Series = [series],
            XAxes = [new Axis()],
            YAxes = [new Axis()],
        };

        var core = (CartesianChartEngine)chart.CoreChart;
        CoreMotionCanvas.DebugElapsedMilliseconds = 0;
        core.IsLoaded = true;
        core._isFirstDraw = true;
        core.Measure();

        try
        {
            var xAxis = core.XAxes[0];
            var yAxis = core.YAxes[0];
            var sb = series.GetBounds(core, xAxis, yAxis);

            Assert.IsFalse(sb.HasData);

            Assert.IsTrue(sb.Bounds.VisiblePrimaryBounds.Min <= 0,
                $"Y auto-min {sb.Bounds.VisiblePrimaryBounds.Min} must include lowest Low (0).");
            Assert.IsTrue(sb.Bounds.VisiblePrimaryBounds.Max >= 150,
                $"Y auto-max {sb.Bounds.VisiblePrimaryBounds.Max} must include highest High (150).");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }

    [TestMethod]
    public void RangeRowSeries_DateTimeAxis_AutoMinReachesEarliestStart()
    {
        // The actual Gantt repro: project starts Jun 1, first task spans
        // Jun 1->Jun 6, every other task starts later. Pre-fix the X axis
        // auto-min would have been Jun 6 (min of all High ticks), clipping
        // the first task. This test pins the "first task is fully drawn"
        // contract under a DateTime axis specifically.
        var projectStart = new DateTime(2026, 6, 1);
        var firstTaskStartTicks = projectStart.Ticks;
        var series = new RangeRowSeries<RangeValue>
        {
            Values =
            [
                new(projectStart.AddDays(0).Ticks,  projectStart.AddDays(5).Ticks),
                new(projectStart.AddDays(3).Ticks,  projectStart.AddDays(8).Ticks),
                new(projectStart.AddDays(14).Ticks, projectStart.AddDays(20).Ticks),
            ],
        };
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            Series = [series],
            XAxes = [new DateTimeAxis(TimeSpan.FromDays(2), d => d.ToString("MMM dd"))],
            YAxes = [new Axis { Labels = ["A", "B", "C"] }],
        };

        var core = (CartesianChartEngine)chart.CoreChart;
        CoreMotionCanvas.DebugElapsedMilliseconds = 0;
        core.IsLoaded = true;
        core._isFirstDraw = true;
        core.Measure();

        try
        {
            var xAxis = core.XAxes[0];
            var yAxis = core.YAxes[0];
            var sb = series.GetBounds(core, yAxis, xAxis);

            Assert.IsTrue(sb.Bounds.VisibleSecondaryBounds.Min <= firstTaskStartTicks,
                $"X auto-min {new DateTime((long)sb.Bounds.VisibleSecondaryBounds.Min):MMM dd} " +
                $"must include project start ({projectStart:MMM dd}); without it the first task is clipped.");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }
}
