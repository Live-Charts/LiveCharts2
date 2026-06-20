using System;
using CoreTests.Helpers;
using LiveChartsCore;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.SeriesTests;

// Per-frame animation contract tests for Cartesian + Polar axes. Mirrors the
// SeriesTests/*AnimationTests pattern: drive coreChart.Measure() at controlled
// timestamps via CoreMotionCanvas.DebugElapsedMilliseconds, capture every active
// separator's separator-line endpoints + label position into AxisFrames, and
// pin the trajectory against a JSON baseline.
//
// These tests are the contract that the upcoming Axis.Invalidate refactor must
// preserve — any change to how separators or labels enter/animate will tripping
// the baseline.
[TestClass]
public class AxisAnimationTests
{
    private const long AnimationMs = 1000;
    private const long StepMs = 100; // 11 frames: 0%, 10%, 20%, ..., 100%
    private const float Tolerance = 0.5f;

    private static void TriggerFirstMeasure(CartesianChartEngine core)
    {
        CoreMotionCanvas.DebugElapsedMilliseconds = 0;
        core.IsLoaded = true;
        core._isFirstDraw = true;
        core.Measure();
    }

    [TestMethod]
    public void XAxis_FirstDraw_SeparatorsAndLabelsAppearAtFinalPositions()
    {
        var xAxis = new Axis { MinLimit = 0, MaxLimit = 40 };
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            AnimationsSpeed = TimeSpan.FromMilliseconds(AnimationMs),
            EasingFunction = EasingFunctions.Lineal,
            Series = [new ColumnSeries<double> { Values = [10d, 20d, 30d] }],
            XAxes = [xAxis],
            YAxes = [new Axis { MinLimit = 0, MaxLimit = 40 }],
        };

        var core = (CartesianChartEngine)chart.CoreChart;

        try
        {
            TriggerFirstMeasure(core);

            var traj = SeriesAnimationCapture.CaptureAxisTrajectory(
                xAxis, core, startMs: 0, endMs: AnimationMs, stepMs: StepMs);

            Assert.AreEqual(11, traj.Count);
            Assert.IsTrue(traj[0].Length > 0, "axis must produce at least one separator");

            // Separators on first draw are seeded UpdateAndComplete, so they sit at their
            // final pixel positions from frame 0 — no entry animation for the line itself
            // (labels follow the same convention).
            var first = traj[0];
            var last = traj[traj.Count - 1];

            for (var i = 0; i < first.Length; i++)
            {
                Assert.AreEqual(first[i].SeparatorX, last[i].SeparatorX, Tolerance,
                    $"separator {i} SeparatorX should be stable across first-draw frames");
                Assert.AreEqual(first[i].LabelX, last[i].LabelX, Tolerance,
                    $"separator {i} LabelX should be stable across first-draw frames");
            }

            SeriesAnimationCapture.AssertTrajectoryMatches(traj, "XAxis_FirstDraw");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }

    [TestMethod]
    public void YAxis_FirstDraw_SeparatorsAndLabelsAppearAtFinalPositions()
    {
        var yAxis = new Axis { MinLimit = 0, MaxLimit = 40 };
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            AnimationsSpeed = TimeSpan.FromMilliseconds(AnimationMs),
            EasingFunction = EasingFunctions.Lineal,
            Series = [new ColumnSeries<double> { Values = [10d, 20d, 30d] }],
            YAxes = [yAxis],
        };

        var core = (CartesianChartEngine)chart.CoreChart;

        try
        {
            TriggerFirstMeasure(core);

            var traj = SeriesAnimationCapture.CaptureAxisTrajectory(
                yAxis, core, startMs: 0, endMs: AnimationMs, stepMs: StepMs);

            Assert.AreEqual(11, traj.Count);
            Assert.IsTrue(traj[0].Length > 0, "axis must produce at least one separator");

            SeriesAnimationCapture.AssertTrajectoryMatches(traj, "YAxis_FirstDraw");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }

    [TestMethod]
    public void YAxis_LimitsChange_SeparatorsAnimateFromOldToNewPositions()
    {
        // Same axis, fixed limits during first draw — then bump the limits and let the
        // separator positions interpolate from the old pixel space into the new one.
        // This is the path the refactor is most likely to regress (scale recompute +
        // separator-position update both fan out from the new limits inside the same
        // Invalidate pass).
        var yAxis = new Axis { MinLimit = 0, MaxLimit = 40 };
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            AnimationsSpeed = TimeSpan.FromMilliseconds(AnimationMs),
            EasingFunction = EasingFunctions.Lineal,
            Series = [new ColumnSeries<double> { Values = [10d, 20d, 30d] }],
            YAxes = [yAxis],
        };

        var core = (CartesianChartEngine)chart.CoreChart;

        try
        {
            TriggerFirstMeasure(core);

            // Settle the first-draw animation.
            CoreMotionCanvas.DebugElapsedMilliseconds = AnimationMs + 50;
            core.Measure();

            // Stretch limits — same separators, new pixel positions.
            yAxis.MinLimit = 0;
            yAxis.MaxLimit = 80;
            var measureT = CoreMotionCanvas.DebugElapsedMilliseconds;
            core.Measure();

            var traj = SeriesAnimationCapture.CaptureAxisTrajectory(
                yAxis, core, startMs: measureT, endMs: measureT + AnimationMs, stepMs: StepMs);

            Assert.AreEqual(11, traj.Count);

            SeriesAnimationCapture.AssertTrajectoryMatches(traj, "YAxis_LimitsChange");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }
}
