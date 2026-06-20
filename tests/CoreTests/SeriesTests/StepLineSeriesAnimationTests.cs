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

// StepLineSeries shares the LineSeries point-marker animation contract — the path
// connecting markers is stepped (right-angle joins) instead of bezier, but the
// per-point visual is the same BoundedDrawnGeometry marker.
[TestClass]
public class StepLineSeriesAnimationTests
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
    public void StepLineSeries_FirstDraw_MarkersRiseFromPivotToValues()
    {
        var series = new StepLineSeries<double> { Values = [10d, 20d, 30d] };
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

            foreach (var f in traj[0])
            {
                Assert.AreEqual(0f, f.Width, Tolerance, "step-line entry width");
                Assert.AreEqual(0f, f.Height, Tolerance, "step-line entry height");
            }

            var finalFrame = traj[traj.Count - 1];
            Assert.IsTrue(finalFrame[0].Y > finalFrame[1].Y);
            Assert.IsTrue(finalFrame[1].Y > finalFrame[2].Y);

            SeriesAnimationCapture.AssertTrajectoryMatches(traj, "StepLineSeries_FirstDraw");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }

    [TestMethod]
    public void StepLineSeries_DataChange_MarkersAnimateFromPreviousToNew()
    {
        var values = new ObservableCollection<double> { 10d, 20d, 30d };
        var series = new StepLineSeries<double> { Values = values };
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

            for (var i = 0; i < traj[0].Length; i++)
                Assert.AreEqual(stableYs[i], traj[0][i].Y, Tolerance,
                    $"step-line data-change: marker {i} starts at previous stable Y");

            var finalFrame = traj[traj.Count - 1];
            Assert.IsTrue(finalFrame[0].Y < stableYs[0]);
            Assert.IsTrue(finalFrame[1].Y > stableYs[1]);
            Assert.IsTrue(finalFrame[2].Y > stableYs[2]);

            SeriesAnimationCapture.AssertTrajectoryMatches(traj, "StepLineSeries_DataChange");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }
}
