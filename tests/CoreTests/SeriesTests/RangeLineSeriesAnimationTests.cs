using System;
using System.Collections.Generic;
using System.Linq;
using CoreTests.Helpers;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing.Segments;
using LiveChartsCore.Kernel;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.SeriesTests;

// Per-frame animation contract for RangeLineSeries. The series carries two
// markers per point (High at PrimaryValue, Low at TertiaryValue) and the
// trajectory captures BOTH so the baseline locks the symmetric "expand
// outward from the band midpoint" entry pattern.
[TestClass]
public class RangeLineSeriesAnimationTests
{
    private const long AnimationMs = 1000;
    private const long StepMs = 100;
    private const float Tolerance = 0.5f;

    // Each frame records both markers' positions. The High marker's Y is the
    // single most useful assertion target — at t=0 it equals the band midpoint
    // (collapsed entry), at t=AnimationMs it equals the high Y. Symmetric for
    // the Low marker.
    public readonly struct RangeLineFrame
    {
        public RangeLineFrame(long timeMs, float highX, float highY, float lowX, float lowY)
        {
            TimeMs = timeMs;
            HighX = highX; HighY = highY;
            LowX = lowX; LowY = lowY;
        }

        public long TimeMs { get; }
        public float HighX { get; }
        public float HighY { get; }
        public float LowX { get; }
        public float LowY { get; }
    }

    private static List<RangeLineFrame[]> CaptureRangeLine(
        IEnumerable<ChartPoint> points, long startMs, long endMs, long stepMs) =>
        SeriesAnimationCapture.CaptureTrajectory(points, (t, p) =>
        {
            var v = (RangeCubicSegmentVisualPoint)p.Context.AdditionalVisuals!;
            return new RangeLineFrame(
                t,
                highX: v.HighGeometry.X, highY: v.HighGeometry.Y,
                lowX: v.LowGeometry.X, lowY: v.LowGeometry.Y);
        }, startMs, endMs, stepMs);

    private static void TriggerFirstMeasure(CartesianChartEngine core)
    {
        CoreMotionCanvas.DebugElapsedMilliseconds = 0;
        core.IsLoaded = true;
        core._isFirstDraw = true;
        core.Measure();
    }

    [TestMethod]
    public void RangeLineSeries_FirstDraw_MarkersGrowFromMidpointToEndpoints()
    {
        var series = new RangeLineSeries<RangeValue>
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

            var traj = CaptureRangeLine(series.everFetched, 0, AnimationMs, StepMs);

            Assert.AreEqual(11, traj.Count);
            Assert.AreEqual(3, traj[0].Length);

            // At t=0, both markers seeded at the band midpoint — i.e. High.Y == Low.Y.
            // Distinguishes range line from regular line series (which enter at pivot).
            foreach (var f in traj[0])
                Assert.AreEqual(f.HighY, f.LowY, Tolerance,
                    "range line first-draw entry: both markers must seed at band midpoint");

            // At final frame, the High marker sits at a smaller Y than the Low marker
            // (smaller Y = higher up on screen for a non-inverted axis), and they're
            // separated by the band height.
            var finalFrame = traj[traj.Count - 1];
            for (var i = 0; i < finalFrame.Length; i++)
                Assert.IsTrue(finalFrame[i].LowY > finalFrame[i].HighY,
                    $"point {i}: final Low.Y must be below (greater than) High.Y on screen");

            SeriesAnimationCapture.AssertTrajectoryMatches(traj, "RangeLineSeries_FirstDraw");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }

    [TestMethod]
    public void RangeLineSeries_DataChange_MarkersAnimateBetweenEndpoints()
    {
        var values = new System.Collections.ObjectModel.ObservableCollection<RangeValue>
        {
            new(10, 30), new(20, 40), new(5, 25),
        };
        var series = new RangeLineSeries<RangeValue> { Values = values };
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

            var stableHighY = series.everFetched
                .Select(p => ((RangeCubicSegmentVisualPoint)p.Context.AdditionalVisuals!).HighGeometry.Y)
                .ToArray();
            var stableLowY = series.everFetched
                .Select(p => ((RangeCubicSegmentVisualPoint)p.Context.AdditionalVisuals!).LowGeometry.Y)
                .ToArray();

            // Mutate in place — swapping references would dispose+recreate the visuals
            // and the test would observe a fresh midpoint entry instead of a transition
            // between the previous and new endpoints.
            values[0].Low = 15; values[0].High = 25;  // shrink
            values[1].Low = 10; values[1].High = 45;  // grow
            values[2].Low = 20; values[2].High = 30;  // shift up
            var measureT = CoreMotionCanvas.DebugElapsedMilliseconds;
            core.Measure();

            var traj = CaptureRangeLine(
                series.everFetched, measureT, measureT + AnimationMs, StepMs);

            // First captured frame: each marker starts at its previously-stable position.
            for (var i = 0; i < traj[0].Length; i++)
            {
                Assert.AreEqual(stableHighY[i], traj[0][i].HighY, Tolerance,
                    $"data-change t=start: point {i} high marker must begin at previous stable Y");
                Assert.AreEqual(stableLowY[i], traj[0][i].LowY, Tolerance,
                    $"data-change t=start: point {i} low marker must begin at previous stable Y");
            }

            SeriesAnimationCapture.AssertTrajectoryMatches(traj, "RangeLineSeries_DataChange");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }
}
