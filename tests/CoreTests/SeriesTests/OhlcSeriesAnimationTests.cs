using System;
using System.Collections.ObjectModel;
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

// OHLC reuses CoreFinancialSeries — same per-point animation envelope as
// CandlesticksSeries (X / Y(high) / Width / Open / Close / Low). Pinning a
// dedicated baseline catches regressions where a financial-pipeline change
// only manifests for the I-bar geometry (e.g. a future Up/DownStroke-only
// paint attachment for OHLC).
[TestClass]
public class OhlcSeriesAnimationTests
{
    private const long AnimationMs = 1000;
    private const long StepMs = 100;

    private static void TriggerFirstMeasure(CartesianChartEngine core)
    {
        CoreMotionCanvas.DebugElapsedMilliseconds = 0;
        core.IsLoaded = true;
        core._isFirstDraw = true;
        core.Measure();
    }

    private static SeriesAnimationCapture.CandlestickFrame ReadOhlcFrame(long t, ChartPoint p)
    {
        var v = (OhlcGeometry)p.Context.Visual!;
        return new SeriesAnimationCapture.CandlestickFrame(
            t, v.X, v.Y, v.Width,
            v.Open, v.Close, v.Low);
    }

    [TestMethod]
    public void OhlcSeries_FirstDraw_BarsAppearWithOHLC()
    {
        // Index-based X (FinancialPointI) keeps per-bar pixel width stable in the
        // baseline — same reasoning as CandlestickSeries_FirstDraw.
        var series = new OhlcSeries<FinancialPointI>
        {
            Values =
            [
                new FinancialPointI(high: 35, open: 25, close: 20, low: 15),
                new FinancialPointI(high: 30, open: 22, close: 28, low: 18),
                new FinancialPointI(high: 32, open: 27, close: 24, low: 19),
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

            var traj = SeriesAnimationCapture.CaptureTrajectory<SeriesAnimationCapture.CandlestickFrame>(
                series.everFetched, ReadOhlcFrame,
                startMs: 0, endMs: AnimationMs, stepMs: StepMs);

            var finalFrame = traj[traj.Count - 1];
            foreach (var f in finalFrame)
                Assert.IsTrue(f.Low > f.Y, $"OHLC Low pixel-Y must be below High pixel-Y (got Low={f.Low}, Y(high)={f.Y})");

            SeriesAnimationCapture.AssertTrajectoryMatches(traj, "OhlcSeries_FirstDraw");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }

    [TestMethod]
    public void OhlcSeries_DataChange_BarsAnimateOHLCToNewValues()
    {
        var values = new ObservableCollection<FinancialPointI>
        {
            new(high: 35, open: 25, close: 20, low: 15),
            new(high: 30, open: 22, close: 28, low: 18),
            new(high: 32, open: 27, close: 24, low: 19),
        };
        var series = new OhlcSeries<FinancialPointI> { Values = values };
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

            values[0] = new FinancialPointI(high: 38, open: 30, close: 36, low: 28);
            values[1] = new FinancialPointI(high: 25, open: 23, close: 18, low: 15);
            values[2] = new FinancialPointI(high: 33, open: 29, close: 31, low: 25);
            var measureT = CoreMotionCanvas.DebugElapsedMilliseconds;
            core.Measure();

            var traj = SeriesAnimationCapture.CaptureTrajectory<SeriesAnimationCapture.CandlestickFrame>(
                series.everFetched, ReadOhlcFrame,
                startMs: measureT, endMs: measureT + AnimationMs, stepMs: StepMs);

            SeriesAnimationCapture.AssertTrajectoryMatches(traj, "OhlcSeries_DataChange");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }
}
