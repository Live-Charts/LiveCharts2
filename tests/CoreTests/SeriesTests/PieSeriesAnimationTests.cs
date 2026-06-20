using System;
using System.Collections.ObjectModel;
using System.Linq;
using CoreTests.Helpers;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.SeriesTests;

// PieSeries slice animation. Each slice's BaseDoughnutGeometry carries angular motion
// (StartAngle/SweepAngle) and radial motion (PushOut/InnerRadius) on top of the
// bounding rect. PieFrame captures all of them so the trajectory pins the actual
// arc geometry — not just the bounding box.
[TestClass]
public class PieSeriesAnimationTests
{
    private const long AnimationMs = 1000;
    private const long StepMs = 100;
    private const float Tolerance = 0.5f;

    private static void TriggerFirstMeasure(PieChartEngine core)
    {
        CoreMotionCanvas.DebugElapsedMilliseconds = 0;
        core.IsLoaded = true;
        core._isFirstDraw = true;
        core.Measure();
    }

    private static SeriesAnimationCapture.PieFrame ReadPieFrame(long t, ChartPoint p)
    {
        var v = (DoughnutGeometry)p.Context.Visual!;
        return new SeriesAnimationCapture.PieFrame(
            t,
            v.X, v.Y, v.Width, v.Height,
            v.CenterX, v.CenterY,
            v.StartAngle, v.SweepAngle,
            v.PushOut, v.InnerRadius);
    }

    [TestMethod]
    public void PieSeries_FirstDraw_SlicesSweepFromZeroAngle()
    {
        var s1 = new PieSeries<double> { Values = [10d] };
        var s2 = new PieSeries<double> { Values = [20d] };
        var s3 = new PieSeries<double> { Values = [30d] };
        var chart = new SKPieChart
        {
            Width = 400,
            Height = 400,
            AnimationsSpeed = TimeSpan.FromMilliseconds(AnimationMs),
            EasingFunction = EasingFunctions.Lineal,
            Series = [s1, s2, s3],
        };

        var core = (PieChartEngine)chart.CoreChart;

        try
        {
            TriggerFirstMeasure(core);

            var traj = SeriesAnimationCapture.CaptureTrajectory<SeriesAnimationCapture.PieFrame>(
                s1.everFetched.Concat(s2.everFetched).Concat(s3.everFetched),
                ReadPieFrame,
                startMs: 0, endMs: AnimationMs, stepMs: StepMs);

            // At t=0 slices have not yet swept — SweepAngle should be 0 for each.
            foreach (var f in traj[0])
                Assert.AreEqual(0f, f.SweepAngle, Tolerance, "pie slice enters at zero sweep");

            // At final frame, sweep angles sum to roughly 360° across the three slices.
            var finalFrame = traj[traj.Count - 1];
            var totalSweep = finalFrame.Sum(f => f.SweepAngle);
            Assert.IsTrue(Math.Abs(totalSweep - 360f) < 1f,
                $"slice sweeps should sum to ~360° (got {totalSweep})");

            SeriesAnimationCapture.AssertTrajectoryMatches(traj, "PieSeries_FirstDraw");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }

    [TestMethod]
    public void PieSeries_DataChange_SlicesResweepToNewAngles()
    {
        var v1 = new ObservableCollection<double> { 10d };
        var v2 = new ObservableCollection<double> { 20d };
        var v3 = new ObservableCollection<double> { 30d };
        var s1 = new PieSeries<double> { Values = v1 };
        var s2 = new PieSeries<double> { Values = v2 };
        var s3 = new PieSeries<double> { Values = v3 };
        var chart = new SKPieChart
        {
            Width = 400,
            Height = 400,
            AnimationsSpeed = TimeSpan.FromMilliseconds(AnimationMs),
            EasingFunction = EasingFunctions.Lineal,
            Series = [s1, s2, s3],
        };

        var core = (PieChartEngine)chart.CoreChart;

        try
        {
            TriggerFirstMeasure(core);

            CoreMotionCanvas.DebugElapsedMilliseconds = AnimationMs + 50;
            core.Measure();

            var allSeries = new[] { s1, s2, s3 };
            var stableSweeps = allSeries.SelectMany(s => s.everFetched)
                .Select(p => ((DoughnutGeometry)p.Context.Visual!).SweepAngle)
                .ToArray();

            // Shift the distribution: slice 1 grows, slice 2 shrinks, slice 3 stays large.
            v1[0] = 40d;
            v2[0] = 5d;
            v3[0] = 15d;
            var measureT = CoreMotionCanvas.DebugElapsedMilliseconds;
            core.Measure();

            var traj = SeriesAnimationCapture.CaptureTrajectory<SeriesAnimationCapture.PieFrame>(
                allSeries.SelectMany(s => s.everFetched),
                ReadPieFrame,
                startMs: measureT, endMs: measureT + AnimationMs, stepMs: StepMs);

            for (var i = 0; i < traj[0].Length; i++)
                Assert.AreEqual(stableSweeps[i], traj[0][i].SweepAngle, Tolerance,
                    $"pie data-change: slice {i} starts at previous stable sweep");

            SeriesAnimationCapture.AssertTrajectoryMatches(traj, "PieSeries_DataChange");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }
}
