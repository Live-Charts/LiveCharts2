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

// Per-frame animation contract tests for LineSeries. Captures the per-point marker
// (BoundedDrawnGeometry — the dot at each data point). The bezier segment connecting
// markers is a series-level path overlay; its endpoints coincide with the marker
// positions captured here, so marker trajectories pin the visible-line motion at
// every data point. Internal bezier control-point drift (Xi/Xm/Xj on the
// CubicBezierSegment) is not captured by this helper — snapshot tests at the final
// frame cover that surface.
[TestClass]
public class LineSeriesAnimationTests
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
    public void LineSeries_FirstDraw_MarkersRiseFromPivotToValues()
    {
        var series = new LineSeries<double> { Values = [10d, 20d, 30d] };
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

            // Markers enter at Width=Height=0 (the visual was just created at the pivot).
            foreach (var f in traj[0])
            {
                Assert.AreEqual(0f, f.Width, Tolerance, "first-draw entry width");
                Assert.AreEqual(0f, f.Height, Tolerance, "first-draw entry height");
            }

            // Final-frame: markers reach their GeometrySize and Y values reflect data order.
            var finalFrame = traj[traj.Count - 1];
            Assert.IsTrue(finalFrame[0].Width > 0);
            Assert.IsTrue(finalFrame[0].Height > 0);
            // y(value=10) is higher pixel-Y than y(value=20) is higher than y(value=30).
            Assert.IsTrue(finalFrame[0].Y > finalFrame[1].Y, "marker 0 sits below marker 1");
            Assert.IsTrue(finalFrame[1].Y > finalFrame[2].Y, "marker 1 sits below marker 2");

            SeriesAnimationCapture.AssertTrajectoryMatches(traj, "LineSeries_FirstDraw");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }

    [TestMethod]
    public void LineSeries_DataChange_MarkersAnimateFromPreviousToNew()
    {
        var values = new ObservableCollection<double> { 10d, 20d, 30d };
        var series = new LineSeries<double> { Values = values };
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

            CoreMotionCanvas.DebugElapsedMilliseconds = AnimationMs + 50;
            core.Measure();

            var stableYs = series.everFetched
                .Select(p => ((BoundedDrawnGeometry)p.Context.Visual!).Y)
                .ToArray();

            values[0] = 35d;
            values[1] = 5d;
            values[2] = 15d;
            var measureT = CoreMotionCanvas.DebugElapsedMilliseconds;
            core.Measure();

            var traj = SeriesAnimationCapture.CaptureTrajectory(
                series.everFetched, startMs: measureT, endMs: measureT + AnimationMs, stepMs: StepMs);

            // First captured frame anchors at previous stable Ys.
            for (var i = 0; i < traj[0].Length; i++)
                Assert.AreEqual(stableYs[i], traj[0][i].Y, Tolerance,
                    $"data-change t=start: marker {i} must begin at previous stable Y");

            // 10→35 grows (smaller pixel-Y), 20→5 and 30→15 both shrink (larger pixel-Y).
            var finalFrame = traj[traj.Count - 1];
            Assert.IsTrue(finalFrame[0].Y < stableYs[0], "value 10→35 should raise marker");
            Assert.IsTrue(finalFrame[1].Y > stableYs[1], "value 20→5 should lower marker");
            Assert.IsTrue(finalFrame[2].Y > stableYs[2], "value 30→15 should lower marker");

            SeriesAnimationCapture.AssertTrajectoryMatches(traj, "LineSeries_DataChange");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }
}
