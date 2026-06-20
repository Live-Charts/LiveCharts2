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

// StackedAreaSeries marker animation. Each point has a BoundedDrawnGeometry marker on
// top of a stacked area-fill path. The path itself is a series-level overlay; markers
// pin the visible-line motion at each data point.
[TestClass]
public class StackedAreaSeriesAnimationTests
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
    public void StackedAreaSeries_FirstDraw_MarkersStackFromPivot()
    {
        var s1 = new StackedAreaSeries<double> { Values = [5d, 10d, 15d] };
        var s2 = new StackedAreaSeries<double> { Values = [3d, 7d, 11d] };
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

            // Markers enter with zero size.
            foreach (var f in traj1[0]) Assert.AreEqual(0f, f.Width, Tolerance, "stacked-area s1 entry width");
            foreach (var f in traj2[0]) Assert.AreEqual(0f, f.Width, Tolerance, "stacked-area s2 entry width");

            // Layer 2 markers sit ABOVE layer 1 markers (smaller pixel-Y).
            var s1Final = traj1[traj1.Count - 1];
            var s2Final = traj2[traj2.Count - 1];
            for (var i = 0; i < s1Final.Length; i++)
                Assert.IsTrue(s2Final[i].Y < s1Final[i].Y, $"stacked-area: s2 marker {i} above s1 marker {i}");

            SeriesAnimationCapture.AssertTrajectoryMatches(traj1, "StackedAreaSeries_FirstDraw_Layer1");
            SeriesAnimationCapture.AssertTrajectoryMatches(traj2, "StackedAreaSeries_FirstDraw_Layer2");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }

    [TestMethod]
    public void StackedAreaSeries_DataChange_MarkersAnimateFromPreviousToNew()
    {
        var v1 = new ObservableCollection<double> { 5d, 10d, 15d };
        var v2 = new ObservableCollection<double> { 3d, 7d, 11d };
        var s1 = new StackedAreaSeries<double> { Values = v1 };
        var s2 = new StackedAreaSeries<double> { Values = v2 };
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

            CoreMotionCanvas.DebugElapsedMilliseconds = AnimationMs + 50;
            core.Measure();

            var stable2 = s2.everFetched
                .Select(p => ((BoundedDrawnGeometry)p.Context.Visual!).Y)
                .ToArray();

            v1[0] = 8d; v1[1] = 4d; v1[2] = 10d;
            v2[0] = 1d; v2[1] = 12d; v2[2] = 6d;
            var measureT = CoreMotionCanvas.DebugElapsedMilliseconds;
            core.Measure();

            var traj2 = SeriesAnimationCapture.CaptureTrajectory(
                s2.everFetched, startMs: measureT, endMs: measureT + AnimationMs, stepMs: StepMs);

            for (var i = 0; i < traj2[0].Length; i++)
                Assert.AreEqual(stable2[i], traj2[0][i].Y, Tolerance,
                    $"stacked-area s2 data-change: marker {i} starts at previous stable Y");

            SeriesAnimationCapture.AssertTrajectoryMatches(traj2, "StackedAreaSeries_DataChange_Layer2");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }
}
