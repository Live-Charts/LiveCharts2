using System;
using System.Linq;
using CoreTests.Helpers;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.SeriesTests;

// HeatSeries animation. Per-cell rect dimensions are static — set once when the
// visual is created and never animated. The animated quantity is each cell's
// COLOR, which interpolates between two colors in the heat gradient based on the
// point's weight. HeatFrame captures the R/G/B/A byte components (as floats) so
// the JSON baseline pins the color trajectory.
[TestClass]
public class HeatSeriesAnimationTests
{
    private const long AnimationMs = 1000;
    private const long StepMs = 100;
    private const float ColorTolerance = 2f;

    private static void TriggerFirstMeasure(CartesianChartEngine core)
    {
        CoreMotionCanvas.DebugElapsedMilliseconds = 0;
        core.IsLoaded = true;
        core._isFirstDraw = true;
        core.Measure();
    }

    private static SeriesAnimationCapture.HeatFrame ReadHeatFrame(long t, ChartPoint p)
    {
        var v = (ColoredRectangleGeometry)p.Context.Visual!;
        var c = v.Color;
        return new SeriesAnimationCapture.HeatFrame(
            t, v.X, v.Y, v.Width, v.Height,
            c.R, c.G, c.B, c.A);
    }

    [TestMethod]
    public void HeatSeries_FirstDraw_CellColorsInterpolateAcrossGradient()
    {
        var series = new HeatSeries<WeightedPoint>
        {
            Values =
            [
                new WeightedPoint(0, 0, 1),
                new WeightedPoint(1, 0, 5),
                new WeightedPoint(0, 1, 3),
                new WeightedPoint(1, 1, 9),
            ],
        };
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            AnimationsSpeed = TimeSpan.FromMilliseconds(AnimationMs),
            EasingFunction = EasingFunctions.Lineal,
            Series = [series],
            XAxes = [new Axis { MinLimit = -0.5, MaxLimit = 1.5 }],
            YAxes = [new Axis { MinLimit = -0.5, MaxLimit = 1.5 }],
        };

        var core = (CartesianChartEngine)chart.CoreChart;

        try
        {
            TriggerFirstMeasure(core);

            var traj = SeriesAnimationCapture.CaptureTrajectory<SeriesAnimationCapture.HeatFrame>(
                series.everFetched, ReadHeatFrame,
                startMs: 0, endMs: AnimationMs, stepMs: StepMs);

            Assert.AreEqual(11, traj.Count);
            Assert.AreEqual(4, traj[0].Length, "expected 4 heat cells");

            // Rect dimensions are static across all frames (heat doesn't animate position).
            for (var i = 0; i < traj[0].Length; i++)
            {
                Assert.AreEqual(traj[0][i].X, traj[traj.Count - 1][i].X, 0.5f, $"cell {i} X must be static");
                Assert.AreEqual(traj[0][i].Width, traj[traj.Count - 1][i].Width, 0.5f, $"cell {i} W must be static");
            }

            // Color DOES animate — cells with different weights have different final colors.
            // Cell 0 (weight=1, lowest) vs cell 3 (weight=9, highest) must end at distinct
            // points in the gradient — at least one of R/G/B differs by more than tolerance.
            var c0 = traj[traj.Count - 1][0];
            var c3 = traj[traj.Count - 1][3];
            var distinct =
                Math.Abs(c0.R - c3.R) > ColorTolerance ||
                Math.Abs(c0.G - c3.G) > ColorTolerance ||
                Math.Abs(c0.B - c3.B) > ColorTolerance;
            Assert.IsTrue(distinct,
                $"weight=1 and weight=9 cells must end at distinct gradient colors (got {c0.R},{c0.G},{c0.B} vs {c3.R},{c3.G},{c3.B})");

            SeriesAnimationCapture.AssertTrajectoryMatches(traj, "HeatSeries_FirstDraw", tolerance: ColorTolerance);
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }
}
