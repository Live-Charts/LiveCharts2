using System;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Drawing;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.SeriesTests;

// Regression for the Gantt tooltip-positioning bug discovered while wiring up
// Bars/Gantt: HorizontalBarSeries used to pass the VALUE axis to BarMeasureHelper,
// but the helper computes the row strip height as
//   scaler.MeasureInPixels(axis.UnitWidth)
// where `scaler` is the category-axis scaler. The (scaler, axis) pair must be
// the SAME axis — passing the value axis only worked because the value axis
// usually ships with UnitWidth = 1.
//
// A DateTimeAxis on the value side carries UnitWidth = unit.Ticks (e.g. 1.7e15
// for a 2-day stepping), which used to send the helper's row-strip height into
// the trillions — wrecking the hover rectangle and dumping the tooltip at the
// chart's top-left corner.
[TestClass]
public class RangeRowDateTimeAxisTests
{
    [TestMethod]
    public void RangeRow_WithDateTimeXAxis_HoverAreaStaysInPixelRange()
    {
        var projectStart = new DateTime(2026, 6, 1);
        var series = new RangeRowSeries<RangeValue>
        {
            Values =
            [
                new(projectStart.AddDays(0).Ticks, projectStart.AddDays(5).Ticks),
                new(projectStart.AddDays(3).Ticks, projectStart.AddDays(8).Ticks),
                new(projectStart.AddDays(5).Ticks, projectStart.AddDays(12).Ticks),
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
            foreach (var point in series.everFetched)
            {
                var ha = (RectangleHoverArea?)point.Context.HoverArea;
                Assert.IsNotNull(ha, "every range-row point must produce a RectangleHoverArea");

                // Sanity bounds: a 400x400 chart's row strip can't exceed ~400 px tall.
                // Pre-fix this was 1.9e14 (Ticks-scaled), which dumped the tooltip at
                // the chart's top-left corner via clipped placement math.
                Assert.IsTrue(
                    ha!.Height < 1000,
                    $"hover-area height {ha.Height} is way out of pixel range — " +
                    $"BarMeasureHelper got the wrong (axis, scaler) pair.");
                Assert.IsTrue(
                    Math.Abs(ha.Y) < 1000,
                    $"hover-area Y {ha.Y} is way out of pixel range.");

                // Suggested tooltip location must land somewhere inside the chart
                // pixel space (allowing some slack for bars at the edges).
                var ttl = ha.SuggestedTooltipLocation;
                Assert.IsTrue(
                    ttl.X > -200 && ttl.X < 600,
                    $"suggested tooltip X {ttl.X} is outside any plausible chart extent.");
                Assert.IsTrue(
                    ttl.Y > -200 && ttl.Y < 600,
                    $"suggested tooltip Y {ttl.Y} is outside any plausible chart extent.");
            }
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }
}
