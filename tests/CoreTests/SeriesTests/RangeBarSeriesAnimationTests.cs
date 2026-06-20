using System;
using System.Collections.ObjectModel;
using System.Linq;
using CoreTests.Helpers;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.SeriesTests;

// Per-frame animation contract tests for the new range bar series. Pinned by JSON
// baselines under tests/CoreTests/AnimationBaselines/. The key behaviors:
//
//   - First draw: bars enter at midpoint of [Low, High] with zero height/width,
//     then expand symmetrically outward. This differs from regular column/row
//     series which enter at pivot=0.
//   - Data change: bars animate from previous [low, high] rect to new endpoints.
[TestClass]
public class RangeBarSeriesAnimationTests
{
    private const long AnimationMs = 1000;
    private const long StepMs = 100;
    private const float Tolerance = 0.5f;

    private static void TriggerFirstMeasure(CartesianChartEngine core)
    {
        CoreMotionCanvas.DebugElapsedMilliseconds = 0;
        core.IsLoaded = true;
        core._isFirstDraw = true;
        core.Measure();
    }

    [TestMethod]
    public void RangeColumnSeries_FirstDraw_BarsGrowFromMidpointToFullHeight()
    {
        var series = new RangeColumnSeries<RangeValue>
        {
            Values = [new(10, 30), new(20, 40), new(5, 25)],
        };
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            AnimationsSpeed = TimeSpan.FromMilliseconds(AnimationMs),
            EasingFunction = EasingFunctions.Lineal,
            Series = [series],
            YAxes = [new Axis { MinLimit = 0, MaxLimit = 50 }],
        };

        var core = (CartesianChartEngine)chart.CoreChart;

        try
        {
            TriggerFirstMeasure(core);

            var traj = SeriesAnimationCapture.CaptureTrajectory(
                series.everFetched, startMs: 0, endMs: AnimationMs, stepMs: StepMs);

            Assert.AreEqual(11, traj.Count);
            Assert.AreEqual(3, traj[0].Length);

            // Range bars enter at midpoint with Height=0 — distinguishes them from
            // regular column series which enter at pivot=0 with Height=0.
            foreach (var f in traj[0])
                Assert.AreEqual(0f, f.Height, Tolerance, "range column first-draw entry height");

            // Final frame: heights non-zero and ordered by range width.
            var finalFrame = traj[traj.Count - 1];
            // Ranges are [10,30]=20, [20,40]=20, [5,25]=20 — all equal. Use as identity check.
            for (var i = 0; i < finalFrame.Length; i++)
                Assert.IsTrue(finalFrame[i].Height > 0, $"bar {i} should have non-zero final height");

            SeriesAnimationCapture.AssertTrajectoryMatches(traj, "RangeColumnSeries_FirstDraw");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }

    [TestMethod]
    public void RangeRowSeries_FirstDraw_BarsGrowFromMidpointToFullWidth()
    {
        var series = new RangeRowSeries<RangeValue>
        {
            Values = [new(10, 30), new(20, 40), new(5, 25)],
        };
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            AnimationsSpeed = TimeSpan.FromMilliseconds(AnimationMs),
            EasingFunction = EasingFunctions.Lineal,
            Series = [series],
            XAxes = [new Axis { MinLimit = 0, MaxLimit = 50 }],
        };

        var core = (CartesianChartEngine)chart.CoreChart;

        try
        {
            TriggerFirstMeasure(core);

            var traj = SeriesAnimationCapture.CaptureTrajectory(
                series.everFetched, startMs: 0, endMs: AnimationMs, stepMs: StepMs);

            Assert.AreEqual(11, traj.Count);
            Assert.AreEqual(3, traj[0].Length);

            foreach (var f in traj[0])
                Assert.AreEqual(0f, f.Width, Tolerance, "range row first-draw entry width");

            var finalFrame = traj[traj.Count - 1];
            for (var i = 0; i < finalFrame.Length; i++)
                Assert.IsTrue(finalFrame[i].Width > 0, $"bar {i} should have non-zero final width");

            SeriesAnimationCapture.AssertTrajectoryMatches(traj, "RangeRowSeries_FirstDraw");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }

    [TestMethod]
    public void RangeColumnSeries_DataChange_BarsAnimateBetweenRanges()
    {
        var values = new ObservableCollection<RangeValue>
        {
            new(10, 30), new(20, 40), new(5, 25),
        };
        var series = new RangeColumnSeries<RangeValue> { Values = values };
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            AnimationsSpeed = TimeSpan.FromMilliseconds(AnimationMs),
            EasingFunction = EasingFunctions.Lineal,
            Series = [series],
            YAxes = [new Axis { MinLimit = 0, MaxLimit = 50 }],
        };

        var core = (CartesianChartEngine)chart.CoreChart;

        try
        {
            TriggerFirstMeasure(core);

            CoreMotionCanvas.DebugElapsedMilliseconds = AnimationMs + 50;
            core.Measure();

            var stableHeights = series.everFetched
                .Select(p => ((BoundedDrawnGeometry)p.Context.Visual!).Height)
                .ToArray();

            // Mutate the existing instances in-place so INotifyPropertyChanged drives
            // the coordinate update path. Swapping references would dispose+recreate
            // the visuals and the test would observe a fresh midpoint entry instead.
            values[0].Low = 15; values[0].High = 25;  // shrink
            values[1].Low = 10; values[1].High = 45;  // grow
            values[2].Low = 20; values[2].High = 30;  // shift
            var measureT = CoreMotionCanvas.DebugElapsedMilliseconds;
            core.Measure();

            var traj = SeriesAnimationCapture.CaptureTrajectory(
                series.everFetched, startMs: measureT, endMs: measureT + AnimationMs, stepMs: StepMs);

            // First captured frame: each bar starts at its previous stable height.
            for (var i = 0; i < traj[0].Length; i++)
                Assert.AreEqual(stableHeights[i], traj[0][i].Height, Tolerance,
                    $"data-change t=start: bar {i} must begin at previous stable height");

            // Final frame: bar 0 shrunk, bar 1 grew, bar 2 grew (10 → 10 stayed).
            var finalFrame = traj[traj.Count - 1];
            Assert.IsTrue(finalFrame[0].Height < stableHeights[0], "range [10,30]→[15,25] should shrink");
            Assert.IsTrue(finalFrame[1].Height > stableHeights[1], "range [20,40]→[10,45] should grow");

            SeriesAnimationCapture.AssertTrajectoryMatches(traj, "RangeColumnSeries_DataChange");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }

    [TestMethod]
    public void RangeRowSeries_DataChange_BarsAnimateBetweenRanges()
    {
        // Horizontal mirror of RangeColumnSeries_DataChange — pins that the
        // row visual starts the new transition at its previous stable width
        // (NOT a snap to midpoint, which would only happen on a fresh entry).
        var values = new ObservableCollection<RangeValue>
        {
            new(10, 30), new(20, 40), new(5, 25),
        };
        var series = new RangeRowSeries<RangeValue> { Values = values };
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            AnimationsSpeed = TimeSpan.FromMilliseconds(AnimationMs),
            EasingFunction = EasingFunctions.Lineal,
            Series = [series],
            XAxes = [new Axis { MinLimit = 0, MaxLimit = 50 }],
        };

        var core = (CartesianChartEngine)chart.CoreChart;

        try
        {
            TriggerFirstMeasure(core);

            CoreMotionCanvas.DebugElapsedMilliseconds = AnimationMs + 50;
            core.Measure();

            var stableWidths = series.everFetched
                .Select(p => ((BoundedDrawnGeometry)p.Context.Visual!).Width)
                .ToArray();

            // Mutate in-place via INotifyPropertyChanged so the existing
            // visuals transition. Swapping references would dispose+recreate
            // and the test would observe a fresh midpoint entry instead.
            values[0].Low = 15; values[0].High = 25;  // shrink
            values[1].Low = 10; values[1].High = 45;  // grow
            values[2].Low = 20; values[2].High = 30;  // shift
            var measureT = CoreMotionCanvas.DebugElapsedMilliseconds;
            core.Measure();

            var traj = SeriesAnimationCapture.CaptureTrajectory(
                series.everFetched, startMs: measureT, endMs: measureT + AnimationMs, stepMs: StepMs);

            for (var i = 0; i < traj[0].Length; i++)
                Assert.AreEqual(stableWidths[i], traj[0][i].Width, Tolerance,
                    $"data-change t=start: bar {i} must begin at previous stable width");

            var finalFrame = traj[traj.Count - 1];
            Assert.IsTrue(finalFrame[0].Width < stableWidths[0], "range [10,30]→[15,25] should shrink");
            Assert.IsTrue(finalFrame[1].Width > stableWidths[1], "range [20,40]→[10,45] should grow");

            SeriesAnimationCapture.AssertTrajectoryMatches(traj, "RangeRowSeries_DataChange");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }
}
