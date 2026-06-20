using System;
using System.Collections.ObjectModel;
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

// BoxSeries animation. Each box's BaseBoxGeometry carries five values (max=Y, third-Q,
// median, first-Q, min) along with the body envelope X/Y/W/H. BoxFrame captures the
// internal divisions so a refactor that breaks quartile placement is caught.
[TestClass]
public class BoxSeriesAnimationTests
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

    private static SeriesAnimationCapture.BoxFrame ReadBoxFrame(long t, ChartPoint p)
    {
        var v = (BoxGeometry)p.Context.Visual!;
        return new SeriesAnimationCapture.BoxFrame(
            t, v.X, v.Y, v.Width,
            v.Third, v.First, v.Min, v.Median);
    }

    [TestMethod]
    public void BoxSeries_FirstDraw_BoxesAppearWithQuartiles()
    {
        var series = new BoxSeries<BoxValue>
        {
            Values =
            [
                new BoxValue(max: 35, thirdQuartile: 28, median: 22, firstQuartile: 18, min: 12),
                new BoxValue(max: 32, thirdQuartile: 26, median: 20, firstQuartile: 14, min: 8),
                new BoxValue(max: 38, thirdQuartile: 30, median: 24, firstQuartile: 20, min: 15),
            ],
        };
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

            var traj = SeriesAnimationCapture.CaptureTrajectory<SeriesAnimationCapture.BoxFrame>(
                series.everFetched, ReadBoxFrame,
                startMs: 0, endMs: AnimationMs, stepMs: StepMs);

            // Final-frame: pixel-Y values monotonically increase from Max -> Min (Y axis grows down).
            var finalFrame = traj[traj.Count - 1];
            foreach (var f in finalFrame)
            {
                Assert.IsTrue(f.Third > f.Y, $"third-Q pixel-Y > max pixel-Y for box");
                Assert.IsTrue(f.Median > f.Third);
                Assert.IsTrue(f.First > f.Median);
                Assert.IsTrue(f.Min > f.First);
            }

            SeriesAnimationCapture.AssertTrajectoryMatches(traj, "BoxSeries_FirstDraw");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }

    [TestMethod]
    public void BoxSeries_DataChange_BoxesAnimateQuartilesToNewValues()
    {
        var values = new ObservableCollection<BoxValue>
        {
            new(max: 35, thirdQuartile: 28, median: 22, firstQuartile: 18, min: 12),
            new(max: 32, thirdQuartile: 26, median: 20, firstQuartile: 14, min: 8),
            new(max: 38, thirdQuartile: 30, median: 24, firstQuartile: 20, min: 15),
        };
        var series = new BoxSeries<BoxValue> { Values = values };
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

            // Mutate each box's quartiles.
            values[0] = new BoxValue(max: 39, thirdQuartile: 32, median: 28, firstQuartile: 22, min: 18);
            values[1] = new BoxValue(max: 25, thirdQuartile: 20, median: 15, firstQuartile: 10, min: 5);
            values[2] = new BoxValue(max: 33, thirdQuartile: 27, median: 22, firstQuartile: 18, min: 12);
            var measureT = CoreMotionCanvas.DebugElapsedMilliseconds;
            core.Measure();

            var traj = SeriesAnimationCapture.CaptureTrajectory<SeriesAnimationCapture.BoxFrame>(
                series.everFetched, ReadBoxFrame,
                startMs: measureT, endMs: measureT + AnimationMs, stepMs: StepMs);

            // Box quartile motion has its own transition seed semantics; let the JSON
            // baseline pin the actual contract rather than over-specifying here.
            SeriesAnimationCapture.AssertTrajectoryMatches(traj, "BoxSeries_DataChange");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }
}
