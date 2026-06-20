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

// ScatterSeries marker animation. Each point has X,Y, plus W=H driven by GeometrySize
// (and optionally by a weight value if mapped). For a fixed-size scatter, W/H interpolate
// 0 -> GeometrySize on first draw and stay constant on data-change.
[TestClass]
public class ScatterSeriesAnimationTests
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
    public void ScatterSeries_FirstDraw_PointsAppearAtMappedCoordinates()
    {
        var series = new ScatterSeries<ObservablePoint>
        {
            Values =
            [
                new ObservablePoint(2, 10),
                new ObservablePoint(4, 20),
                new ObservablePoint(6, 30),
            ],
        };
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            AnimationsSpeed = TimeSpan.FromMilliseconds(AnimationMs),
            EasingFunction = EasingFunctions.Lineal,
            Series = [series],
            XAxes = [new Axis { MinLimit = 0, MaxLimit = 10 }],
            YAxes = [new Axis { MinLimit = 0, MaxLimit = 40 }],
        };

        var core = (CartesianChartEngine)chart.CoreChart;

        try
        {
            TriggerFirstMeasure(core);

            var traj = SeriesAnimationCapture.CaptureTrajectory(
                series.everFetched, startMs: 0, endMs: AnimationMs, stepMs: StepMs);

            // Scatter markers enter at zero size.
            foreach (var f in traj[0])
            {
                Assert.AreEqual(0f, f.Width, Tolerance, "scatter entry width");
                Assert.AreEqual(0f, f.Height, Tolerance, "scatter entry height");
            }

            // Final-frame: monotonically increasing X (mapped 2/4/6) and decreasing Y (10/20/30 → larger Y values map to smaller pixel-Y).
            var finalFrame = traj[traj.Count - 1];
            Assert.IsTrue(finalFrame[0].X < finalFrame[1].X);
            Assert.IsTrue(finalFrame[1].X < finalFrame[2].X);
            Assert.IsTrue(finalFrame[0].Y > finalFrame[1].Y);
            Assert.IsTrue(finalFrame[1].Y > finalFrame[2].Y);

            SeriesAnimationCapture.AssertTrajectoryMatches(traj, "ScatterSeries_FirstDraw");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }

    [TestMethod]
    public void ScatterSeries_DataChange_PointsMigrateToNewCoordinates()
    {
        var p0 = new ObservablePoint(2, 10);
        var p1 = new ObservablePoint(4, 20);
        var p2 = new ObservablePoint(6, 30);
        var series = new ScatterSeries<ObservablePoint> { Values = new ObservableCollection<ObservablePoint> { p0, p1, p2 } };
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            AnimationsSpeed = TimeSpan.FromMilliseconds(AnimationMs),
            EasingFunction = EasingFunctions.Lineal,
            Series = [series],
            XAxes = [new Axis { MinLimit = 0, MaxLimit = 10 }],
            YAxes = [new Axis { MinLimit = 0, MaxLimit = 40 }],
        };

        var core = (CartesianChartEngine)chart.CoreChart;

        try
        {
            TriggerFirstMeasure(core);

            CoreMotionCanvas.DebugElapsedMilliseconds = AnimationMs + 50;
            core.Measure();

            var stable = series.everFetched
                .Select(p => (BoundedDrawnGeometry)p.Context.Visual!)
                .Select(v => (X: v.X, Y: v.Y))
                .ToArray();

            // Migrate every point.
            p0.X = 8; p0.Y = 35;
            p1.X = 1; p1.Y = 5;
            p2.X = 5; p2.Y = 15;
            var measureT = CoreMotionCanvas.DebugElapsedMilliseconds;
            core.Measure();

            var traj = SeriesAnimationCapture.CaptureTrajectory(
                series.everFetched, startMs: measureT, endMs: measureT + AnimationMs, stepMs: StepMs);

            for (var i = 0; i < traj[0].Length; i++)
            {
                Assert.AreEqual(stable[i].X, traj[0][i].X, Tolerance, $"point {i} starts at stable X");
                Assert.AreEqual(stable[i].Y, traj[0][i].Y, Tolerance, $"point {i} starts at stable Y");
            }

            SeriesAnimationCapture.AssertTrajectoryMatches(traj, "ScatterSeries_DataChange");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }
}
