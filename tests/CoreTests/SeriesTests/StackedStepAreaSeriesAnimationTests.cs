using System;
using CoreTests.Helpers;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.SeriesTests;

// StackedStepAreaSeries marker animation — same contract as StackedAreaSeries with a
// step-shaped fill path instead of a curved one. Per-point markers are identical.
[TestClass]
public class StackedStepAreaSeriesAnimationTests
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
    public void StackedStepAreaSeries_FirstDraw_MarkersStackFromPivot()
    {
        var s1 = new StackedStepAreaSeries<double> { Values = [5d, 10d, 15d] };
        var s2 = new StackedStepAreaSeries<double> { Values = [3d, 7d, 11d] };
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

            foreach (var f in traj1[0]) Assert.AreEqual(0f, f.Width, Tolerance, "stacked-step-area s1 entry");
            foreach (var f in traj2[0]) Assert.AreEqual(0f, f.Width, Tolerance, "stacked-step-area s2 entry");

            var s1Final = traj1[traj1.Count - 1];
            var s2Final = traj2[traj2.Count - 1];
            for (var i = 0; i < s1Final.Length; i++)
                Assert.IsTrue(s2Final[i].Y < s1Final[i].Y);

            SeriesAnimationCapture.AssertTrajectoryMatches(traj1, "StackedStepAreaSeries_FirstDraw_Layer1");
            SeriesAnimationCapture.AssertTrajectoryMatches(traj2, "StackedStepAreaSeries_FirstDraw_Layer2");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }
}
