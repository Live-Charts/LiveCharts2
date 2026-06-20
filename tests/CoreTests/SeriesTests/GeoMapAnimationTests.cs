using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CoreTests.Helpers;
using LiveChartsCore;
using LiveChartsCore.Geo;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.SeriesTests;

// GeoMap (HeatLandSeries) animation. Unlike cartesian series:
//   - Lands are path-based BaseVectorGeometry instances (no X/Y/W/H rect)
//   - There are no ChartPoints — lands are accessed via the map's
//     ActiveMap.Layers[name].Lands.Values + each land's Data[i].Shape
//
// REAL FINDING: heat lands DON'T animate the fill color. The shapes are
// borrowed from the map's layers; HeatLandSeries.Measure assigns
// shape.Fill = new SolidColorPaint(heat) but never calls Animate() on
// the shape to configure a transition Animation. Without an Animation
// configured, PaintMotionProperty has no time-based interpolation curve,
// so reading shape.Fill returns the latest assigned paint at every
// timestamp. Both first-draw and data-change produce static trajectories
// — the colors jump to their new values instantly.
//
// The JSON baselines pin this no-animation contract. If a future change
// adds Animate() on map shapes (intentionally or by accident, e.g. via
// the refactor planned for the bar template-method work expanding to
// other series), the trajectories will diverge from the baseline and
// surface the change.
[TestClass]
public class GeoMapAnimationTests
{
    private const long AnimationMs = 1000;
    private const long StepMs = 100;
    private const float ColorTolerance = 2f;

    // Sample a handful of lands so trajectories don't balloon. Pick countries
    // that span the gradient (low and high values).
    private static readonly string[] s_sampledLands =
    [
        "usa", "bra", "rus", "chn", "fra",
    ];

    private static HeatLandSeries CreateSeries(double[] values) =>
        new()
        {
            Lands =
            [
                new() { Name = "usa", Value = values[0] },
                new() { Name = "bra", Value = values[1] },
                new() { Name = "rus", Value = values[2] },
                new() { Name = "chn", Value = values[3] },
                new() { Name = "fra", Value = values[4] },
            ],
        };

    private static SKGeoMap BuildChart(HeatLandSeries series) =>
        new()
        {
            Series = [series],
            Width = 600,
            Height = 600,
            AnimationsSpeed = TimeSpan.FromMilliseconds(AnimationMs),
            EasingFunction = EasingFunctions.Lineal,
        };

    private static IEnumerable<LandDefinition> SampledLands(SKGeoMap chart) =>
        s_sampledLands
            .Select(name => chart.ActiveMap.FindLand(name)!)
            .Where(l => l is not null);

    private static SeriesAnimationCapture.MapLandFrame ReadFrame(long t, LandDefinition land)
    {
        var paint = (SolidColorPaint)land.Data[0].Shape!.Fill!;
        var c = paint.Color;
        return new SeriesAnimationCapture.MapLandFrame(t, c.Red, c.Green, c.Blue, c.Alpha);
    }

    private static List<SeriesAnimationCapture.MapLandFrame[]> CaptureMapTrajectory(
        SKGeoMap chart, long startMs, long endMs, long stepMs)
    {
        var lands = SampledLands(chart).ToList();
        var trajectory = new List<SeriesAnimationCapture.MapLandFrame[]>();
        for (var t = startMs; t <= endMs; t += stepMs)
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = t;
            trajectory.Add(lands.Select(l => ReadFrame(t, l)).ToArray());
        }
        return trajectory;
    }

    [TestMethod]
    public void GeoMap_DataChange_LandColorsJumpToNewGradientPositions()
    {
        // Initial values span the gradient so [Min,Max] is non-degenerate. The new
        // values keep the SAME [Min,Max] range so the gradient is identical, but every
        // land is reassigned a different position within it — every color must shift.
        var series = CreateSeries([10d, 30d, 50d, 70d, 90d]);
        var chart = BuildChart(series);
        var core = chart.CoreChart;

        try
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = 0;
            core.IsLoaded = true;
            core._isFirstDraw = true;
            core.Measure();

            // Step past first-draw — every land now has a stable shape.Fill assignment.
            CoreMotionCanvas.DebugElapsedMilliseconds = AnimationMs + 50;
            core.Measure();

            // Capture stable colors per land before mutating.
            var lands = SampledLands(chart).ToList();
            Assert.AreEqual(s_sampledLands.Length, lands.Count,
                "every sampled land must resolve in the world map");

            var stableColors = lands
                .Select(l => ((SolidColorPaint)l.Data[0].Shape!.Fill!).Color)
                .ToArray();

            // Permute values across the same [10,90] range so every land moves to a
            // distinct new position — each color must shift.
            series.Lands = new ObservableCollection<HeatLand>
            {
                new() { Name = "usa", Value = 90 },
                new() { Name = "bra", Value = 50 },
                new() { Name = "rus", Value = 70 },
                new() { Name = "chn", Value = 10 },
                new() { Name = "fra", Value = 30 },
            };
            var measureT = CoreMotionCanvas.DebugElapsedMilliseconds;
            core.Measure();

            var traj = CaptureMapTrajectory(chart, measureT, measureT + AnimationMs, StepMs);

            Assert.AreEqual(11, traj.Count);
            Assert.AreEqual(s_sampledLands.Length, traj[0].Length);

            // No-animation contract: every captured frame after the data-change measure
            // should equal the final frame (no interpolation between stable and new).
            var firstFrame = traj[0];
            var finalFrame = traj[traj.Count - 1];
            for (var fi = 0; fi < traj.Count; fi++)
            {
                for (var li = 0; li < firstFrame.Length; li++)
                {
                    Assert.AreEqual(firstFrame[li].R, traj[fi][li].R, ColorTolerance,
                        $"data-change: land {s_sampledLands[li]} should jump instantly (no animation)");
                }
            }

            // At the final frame, at least one of R/G/B should differ from the stable
            // pre-change color for every land (since every Value changed).
            for (var i = 0; i < finalFrame.Length; i++)
            {
                var stable = stableColors[i];
                var f = finalFrame[i];
                var distinct =
                    Math.Abs(stable.Red - f.R) > ColorTolerance ||
                    Math.Abs(stable.Green - f.G) > ColorTolerance ||
                    Math.Abs(stable.Blue - f.B) > ColorTolerance;
                Assert.IsTrue(distinct,
                    $"land {s_sampledLands[i]} value changed but color did not " +
                    $"(stable={stable.Red},{stable.Green},{stable.Blue}; final={f.R},{f.G},{f.B})");
            }

            SeriesAnimationCapture.AssertTrajectoryMatches(
                traj, "GeoMap_HeatLand_DataChange", tolerance: ColorTolerance);
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }

    [TestMethod]
    public void GeoMap_FirstDraw_LandColorsAppearAtFinalGradient()
    {
        // First draw doesn't animate the heat fill (FromValue=null, s_activePaint is only
        // set inside DrawFrame). Reading shape.Fill outside of a paint pass returns the
        // target paint immediately for every frame. This test pins that contract: every
        // captured frame is byte-identical to the final frame, and the final colors
        // reflect distinct positions in the gradient based on each land's value.
        var series = CreateSeries([1d, 25d, 50d, 75d, 100d]);
        var chart = BuildChart(series);
        var core = chart.CoreChart;

        try
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = 0;
            core.IsLoaded = true;
            core._isFirstDraw = true;
            core.Measure();

            var traj = CaptureMapTrajectory(chart, 0, AnimationMs, StepMs);

            // All 11 frames identical — first draw is non-animated for map lands.
            var firstFrame = traj[0];
            for (var frameIdx = 1; frameIdx < traj.Count; frameIdx++)
            {
                for (var landIdx = 0; landIdx < firstFrame.Length; landIdx++)
                {
                    Assert.AreEqual(firstFrame[landIdx].R, traj[frameIdx][landIdx].R, ColorTolerance);
                    Assert.AreEqual(firstFrame[landIdx].G, traj[frameIdx][landIdx].G, ColorTolerance);
                    Assert.AreEqual(firstFrame[landIdx].B, traj[frameIdx][landIdx].B, ColorTolerance);
                }
            }

            // Lands with distinct values end at distinct gradient colors. Lowest-value
            // (bra=1) and highest-value (fra=100) lands must differ on at least one channel.
            var braIdx = Array.IndexOf(s_sampledLands, "bra");
            var fraIdx = Array.IndexOf(s_sampledLands, "fra");
            var bra = firstFrame[braIdx];
            var fra = firstFrame[fraIdx];
            var distinct =
                Math.Abs(bra.R - fra.R) > ColorTolerance ||
                Math.Abs(bra.G - fra.G) > ColorTolerance ||
                Math.Abs(bra.B - fra.B) > ColorTolerance;
            Assert.IsTrue(distinct,
                $"lowest- and highest-value lands must end at distinct gradient colors");

            SeriesAnimationCapture.AssertTrajectoryMatches(
                traj, "GeoMap_HeatLand_FirstDraw", tolerance: ColorTolerance);
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }
}
