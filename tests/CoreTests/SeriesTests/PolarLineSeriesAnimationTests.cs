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

// PolarLineSeries marker animation. Markers live on a polar surface; the engine
// converts polar coordinates to pixel-space (Cartesian) before reading from
// BoundedDrawnGeometry, so the harness captures the polar→pixel mapped position.
[TestClass]
public class PolarLineSeriesAnimationTests
{
    private const long AnimationMs = 1000;
    private const long StepMs = 100;
    private const float Tolerance = 0.5f;

    private static void TriggerFirstMeasure(PolarChartEngine core)
    {
        CoreMotionCanvas.DebugElapsedMilliseconds = 0;
        core.IsLoaded = true;
        core._isFirstDraw = true;
        core.Measure();
    }

    [TestMethod]
    public void PolarLineSeries_FirstDraw_MarkersAppearAtMappedPolarCoords()
    {
        var series = new PolarLineSeries<double> { Values = [10d, 20d, 30d] };
        var chart = new SKPolarChart
        {
            Width = 400,
            Height = 400,
            AnimationsSpeed = TimeSpan.FromMilliseconds(AnimationMs),
            EasingFunction = EasingFunctions.Lineal,
            Series = [series],
            RadiusAxes = [new PolarAxis { MinLimit = 0, MaxLimit = 40 }],
        };

        var core = (PolarChartEngine)chart.CoreChart;

        try
        {
            TriggerFirstMeasure(core);

            var traj = SeriesAnimationCapture.CaptureTrajectory(
                series.everFetched, startMs: 0, endMs: AnimationMs, stepMs: StepMs);

            // Polar markers don't shrink-grow from zero like cartesian ones — they appear
            // at their final size from t=0 (the polar surface uses a different first-draw
            // animation profile). The JSON baseline pins whatever the actual contract is.
            var finalFrame = traj[traj.Count - 1];
            Assert.IsTrue(finalFrame[0].Width > 0);

            SeriesAnimationCapture.AssertTrajectoryMatches(traj, "PolarLineSeries_FirstDraw");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }

    [TestMethod]
    public void PolarLineSeries_DataChange_MarkersMigrateOnPolarSurface()
    {
        var values = new ObservableCollection<double> { 10d, 20d, 30d };
        var series = new PolarLineSeries<double> { Values = values };
        var chart = new SKPolarChart
        {
            Width = 400,
            Height = 400,
            AnimationsSpeed = TimeSpan.FromMilliseconds(AnimationMs),
            EasingFunction = EasingFunctions.Lineal,
            Series = [series],
            RadiusAxes = [new PolarAxis { MinLimit = 0, MaxLimit = 40 }],
        };

        var core = (PolarChartEngine)chart.CoreChart;

        try
        {
            TriggerFirstMeasure(core);

            CoreMotionCanvas.DebugElapsedMilliseconds = AnimationMs + 50;
            core.Measure();

            var stable = series.everFetched
                .Select(p => (BoundedDrawnGeometry)p.Context.Visual!)
                .Select(v => (X: v.X, Y: v.Y))
                .ToArray();

            values[0] = 35d;
            values[1] = 5d;
            values[2] = 15d;
            var measureT = CoreMotionCanvas.DebugElapsedMilliseconds;
            core.Measure();

            var traj = SeriesAnimationCapture.CaptureTrajectory(
                series.everFetched, startMs: measureT, endMs: measureT + AnimationMs, stepMs: StepMs);

            for (var i = 0; i < traj[0].Length; i++)
            {
                Assert.AreEqual(stable[i].X, traj[0][i].X, Tolerance, $"polar point {i} starts at stable X");
                Assert.AreEqual(stable[i].Y, traj[0][i].Y, Tolerance, $"polar point {i} starts at stable Y");
            }

            SeriesAnimationCapture.AssertTrajectoryMatches(traj, "PolarLineSeries_DataChange");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }
}
