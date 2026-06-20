using System;
using System.Collections.ObjectModel;
using System.Linq;
using CoreTests.Helpers;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.SeriesTests;

// Per-frame animation contract tests for bar-family series. Drive coreChart.Measure() with
// CoreMotionCanvas.DebugElapsedMilliseconds; assert that each visual interpolates linearly
// between its from/to values over the animation duration.
//
// Charts use EasingFunctions.Lineal and fixed axis limits so the math is deterministic:
//   - SKCartesianChart constructor sets EasingFunction = null (animations off by default
//     in test-mode) — we explicitly set Lineal so transitions interpolate
//   - Fixed MinLimit/MaxLimit prevents axis bounds from shifting between measures, which
//     would invalidate intermediate trajectory frames
//
// These tests are the contract that downstream bar-series refactors must preserve.
[TestClass]
public class BarSeriesAnimationTests
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
    public void ColumnSeries_FirstDraw_BarsGrowFromPivotToFinalHeight()
    {
        var series = new ColumnSeries<double> { Values = [10d, 20d, 30d] };
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            AnimationsSpeed = TimeSpan.FromMilliseconds(AnimationMs),
            EasingFunction = EasingFunctions.Lineal,
            Series = [series],
            YAxes = [new Axis { MinLimit = 0, MaxLimit = 40 }],
        };

        var core = (CartesianChartEngine)chart.CoreChart;

        try
        {
            TriggerFirstMeasure(core);

            var traj = SeriesAnimationCapture.CaptureTrajectory(
                series.everFetched, startMs: 0, endMs: AnimationMs, stepMs: StepMs);

            Assert.AreEqual(11, traj.Count);
            Assert.AreEqual(3, traj[0].Length);

            // bars enter at Height=0 (visual just created at pivot)
            foreach (var f in traj[0])
                Assert.AreEqual(0f, f.Height, Tolerance, "first-draw entry height");

            // final-frame heights are non-zero and monotonically increase with Values
            var finalFrame = traj[traj.Count - 1];
            Assert.IsTrue(finalFrame[0].Height > 0);
            Assert.IsTrue(finalFrame[0].Height < finalFrame[1].Height);
            Assert.IsTrue(finalFrame[1].Height < finalFrame[2].Height);

            SeriesAnimationCapture.AssertTrajectoryMatches(traj, "ColumnSeries_FirstDraw");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }

    [TestMethod]
    public void ColumnSeries_DataChange_BarsAnimateFromPreviousToNewHeight()
    {
        var values = new ObservableCollection<double> { 10d, 20d, 30d };
        var series = new ColumnSeries<double> { Values = values };
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            AnimationsSpeed = TimeSpan.FromMilliseconds(AnimationMs),
            EasingFunction = EasingFunctions.Lineal,
            Series = [series],
            YAxes = [new Axis { MinLimit = 0, MaxLimit = 40 }],
        };

        var core = (CartesianChartEngine)chart.CoreChart;

        try
        {
            TriggerFirstMeasure(core);

            // Step past first-draw animation so visuals settle at their steady state.
            CoreMotionCanvas.DebugElapsedMilliseconds = AnimationMs + 50;
            core.Measure();

            var stableHeights = series.everFetched
                .Select(p => ((BoundedDrawnGeometry)p.Context.Visual!).Height)
                .ToArray();

            // Mutate values; next Measure arms a new transition stable→new.
            values[0] = 35d;
            values[1] = 5d;
            values[2] = 15d;
            var measureT = CoreMotionCanvas.DebugElapsedMilliseconds;
            core.Measure();

            var traj = SeriesAnimationCapture.CaptureTrajectory(
                series.everFetched, startMs: measureT, endMs: measureT + AnimationMs, stepMs: StepMs);

            // First captured frame reflects the start of the new transition — the
            // previous stable height.
            for (var i = 0; i < traj[0].Length; i++)
                Assert.AreEqual(stableHeights[i], traj[0][i].Height, Tolerance,
                    $"data-change t=start: bar {i} must begin at previous stable height");

            // Final frame reflects new values — first bar bigger, second smaller, third bigger.
            var finalFrame = traj[traj.Count - 1];
            Assert.IsTrue(finalFrame[0].Height > stableHeights[0], "value 10→35 should grow bar");
            Assert.IsTrue(finalFrame[1].Height < stableHeights[1], "value 20→5 should shrink bar");
            Assert.IsTrue(finalFrame[2].Height < stableHeights[2], "value 30→15 should shrink bar");

            SeriesAnimationCapture.AssertTrajectoryMatches(traj, "ColumnSeries_DataChange");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }

    [TestMethod]
    public void RowSeries_FirstDraw_BarsGrowFromPivotToFinalWidth()
    {
        var series = new RowSeries<double> { Values = [10d, 20d, 30d] };
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            AnimationsSpeed = TimeSpan.FromMilliseconds(AnimationMs),
            EasingFunction = EasingFunctions.Lineal,
            Series = [series],
            // For row series the X axis is the value axis.
            XAxes = [new Axis { MinLimit = 0, MaxLimit = 40 }],
        };

        var core = (CartesianChartEngine)chart.CoreChart;

        try
        {
            TriggerFirstMeasure(core);

            var traj = SeriesAnimationCapture.CaptureTrajectory(
                series.everFetched, startMs: 0, endMs: AnimationMs, stepMs: StepMs);

            Assert.AreEqual(11, traj.Count);
            Assert.AreEqual(3, traj[0].Length);

            // Row bars enter at Width=0 (the orientation-flipped counterpart of column Height=0).
            foreach (var f in traj[0])
                Assert.AreEqual(0f, f.Width, Tolerance, "row first-draw entry width");

            var finalFrame = traj[traj.Count - 1];
            Assert.IsTrue(finalFrame[0].Width > 0);
            Assert.IsTrue(finalFrame[0].Width < finalFrame[1].Width);
            Assert.IsTrue(finalFrame[1].Width < finalFrame[2].Width);

            SeriesAnimationCapture.AssertTrajectoryMatches(traj, "RowSeries_FirstDraw");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }

    [TestMethod]
    public void RowSeries_DataChange_BarsAnimateFromPreviousToNewWidth()
    {
        var values = new ObservableCollection<double> { 10d, 20d, 30d };
        var series = new RowSeries<double> { Values = values };
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            AnimationsSpeed = TimeSpan.FromMilliseconds(AnimationMs),
            EasingFunction = EasingFunctions.Lineal,
            Series = [series],
            XAxes = [new Axis { MinLimit = 0, MaxLimit = 40 }],
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

            values[0] = 35d;
            values[1] = 5d;
            values[2] = 15d;
            var measureT = CoreMotionCanvas.DebugElapsedMilliseconds;
            core.Measure();

            var traj = SeriesAnimationCapture.CaptureTrajectory(
                series.everFetched, startMs: measureT, endMs: measureT + AnimationMs, stepMs: StepMs);

            for (var i = 0; i < traj[0].Length; i++)
                Assert.AreEqual(stableWidths[i], traj[0][i].Width, Tolerance,
                    $"row data-change t=start: bar {i} must begin at previous stable width");

            var finalFrame = traj[traj.Count - 1];
            Assert.IsTrue(finalFrame[0].Width > stableWidths[0]);
            Assert.IsTrue(finalFrame[1].Width < stableWidths[1]);
            Assert.IsTrue(finalFrame[2].Width < stableWidths[2]);

            SeriesAnimationCapture.AssertTrajectoryMatches(traj, "RowSeries_DataChange");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }

    [TestMethod]
    public void StackedColumnSeries_FirstDraw_StackedBarsGrowFromPivot()
    {
        var s1 = new StackedColumnSeries<double> { Values = [5d, 10d, 15d] };
        var s2 = new StackedColumnSeries<double> { Values = [3d, 7d, 11d] };
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            AnimationsSpeed = TimeSpan.FromMilliseconds(AnimationMs),
            EasingFunction = EasingFunctions.Lineal,
            Series = [s1, s2],
            YAxes = [new Axis { MinLimit = 0, MaxLimit = 40 }],
        };

        var core = (CartesianChartEngine)chart.CoreChart;

        try
        {
            TriggerFirstMeasure(core);

            var traj1 = SeriesAnimationCapture.CaptureTrajectory(
                s1.everFetched, startMs: 0, endMs: AnimationMs, stepMs: StepMs);
            var traj2 = SeriesAnimationCapture.CaptureTrajectory(
                s2.everFetched, startMs: 0, endMs: AnimationMs, stepMs: StepMs);

            foreach (var f in traj1[0]) Assert.AreEqual(0f, f.Height, Tolerance, "stacked col s1 entry");
            foreach (var f in traj2[0]) Assert.AreEqual(0f, f.Height, Tolerance, "stacked col s2 entry");

            // s2 final bars sit ABOVE s1 final bars (smaller pixel-Y for stack layer 2).
            var s1Final = traj1[traj1.Count - 1];
            var s2Final = traj2[traj2.Count - 1];
            for (var i = 0; i < s1Final.Length; i++)
                Assert.IsTrue(s2Final[i].Y < s1Final[i].Y,
                    $"stacked: s2 must sit above s1 at column {i}");

            SeriesAnimationCapture.AssertTrajectoryMatches(traj1, "StackedColumnSeries_FirstDraw_Layer1");
            SeriesAnimationCapture.AssertTrajectoryMatches(traj2, "StackedColumnSeries_FirstDraw_Layer2");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }

    // The stacked-row baseline encodes negative Width values at intermediate and final
    // frames (e.g. w=-123 in StackedRowSeries_FirstDraw_Layer1.json). That's intentional:
    // CoreRowSeries' stacker branch stores the bar's RIGHT edge as X and grows backward
    // via a negative Width — the rendered span is x..(x + width) where width < 0. Issue
    // #2165 fixed the corresponding hover-area normalization (RectangleHoverArea now
    // uses min/max); the geometry layer itself was left as-is. A refactor that
    // "normalizes" to positive Width here will trip this baseline AND must also touch
    // any downstream code that relies on the current convention.
    [TestMethod]
    public void StackedRowSeries_FirstDraw_StackedBarsGrowFromPivot()
    {
        var s1 = new StackedRowSeries<double> { Values = [5d, 10d, 15d] };
        var s2 = new StackedRowSeries<double> { Values = [3d, 7d, 11d] };
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            AnimationsSpeed = TimeSpan.FromMilliseconds(AnimationMs),
            EasingFunction = EasingFunctions.Lineal,
            Series = [s1, s2],
            XAxes = [new Axis { MinLimit = 0, MaxLimit = 40 }],
        };

        var core = (CartesianChartEngine)chart.CoreChart;

        try
        {
            TriggerFirstMeasure(core);

            var traj1 = SeriesAnimationCapture.CaptureTrajectory(
                s1.everFetched, startMs: 0, endMs: AnimationMs, stepMs: StepMs);
            var traj2 = SeriesAnimationCapture.CaptureTrajectory(
                s2.everFetched, startMs: 0, endMs: AnimationMs, stepMs: StepMs);

            foreach (var f in traj1[0]) Assert.AreEqual(0f, f.Width, Tolerance, "stacked row s1 entry");
            foreach (var f in traj2[0]) Assert.AreEqual(0f, f.Width, Tolerance, "stacked row s2 entry");

            // s2 final bars sit to the RIGHT of s1 final bars (larger pixel-X for stack layer 2).
            var s1Final = traj1[traj1.Count - 1];
            var s2Final = traj2[traj2.Count - 1];
            for (var i = 0; i < s1Final.Length; i++)
                Assert.IsTrue(s2Final[i].X > s1Final[i].X,
                    $"stacked: s2 must sit right-of s1 at row {i}");

            SeriesAnimationCapture.AssertTrajectoryMatches(traj1, "StackedRowSeries_FirstDraw_Layer1");
            SeriesAnimationCapture.AssertTrajectoryMatches(traj2, "StackedRowSeries_FirstDraw_Layer2");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }
}
