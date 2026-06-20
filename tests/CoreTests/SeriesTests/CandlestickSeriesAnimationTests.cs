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

// CandlestickSeries OHLC animation. Each candle has X/Y/W/H for the body envelope
// plus Open/Close/Low for the wick + body endpoints. CandlestickFrame captures all
// of them so a refactor that breaks wick positioning is caught at intermediate
// frames, not just the final frame snapshot tests.
[TestClass]
public class CandlestickSeriesAnimationTests
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

    private static SeriesAnimationCapture.CandlestickFrame ReadCandlestickFrame(long t, ChartPoint p)
    {
        var v = (CandlestickGeometry)p.Context.Visual!;
        return new SeriesAnimationCapture.CandlestickFrame(
            t, v.X, v.Y, v.Width,
            v.Open, v.Close, v.Low);
    }

    [TestMethod]
    public void CandlestickSeries_FirstDraw_CandlesAppearWithOHLC()
    {
        // FinancialPointI uses the index as X (0, 1, 2). FinancialPoint uses
        // DateTime.Ticks, which makes secondaryAxis.UnitWidth = 1 tick in chart
        // coordinates — at ~600 chart-units across the data range, the per-candle
        // pixel width rounds to 0.00 in the baseline and loses width-regression
        // coverage. Index-based X keeps Width meaningful and stable.
        var series = new CandlesticksSeries<FinancialPointI>
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
                series.everFetched, ReadCandlestickFrame,
                startMs: 0, endMs: AnimationMs, stepMs: StepMs);

            // Final-frame: pixel-Y for Low > pixel-Y for Open/Close > pixel-Y for High (since Y axis grows downward).
            var finalFrame = traj[traj.Count - 1];
            foreach (var f in finalFrame)
                Assert.IsTrue(f.Low > f.Y, $"candle Low pixel-Y must be below High pixel-Y (got Low={f.Low}, Y(high)={f.Y})");

            SeriesAnimationCapture.AssertTrajectoryMatches(traj, "CandlestickSeries_FirstDraw");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }

    [TestMethod]
    public void CandlestickSeries_DataChange_CandlesAnimateOHLCToNewValues()
    {
        var values = new ObservableCollection<FinancialPointI>
        {
            new(high: 35, open: 25, close: 20, low: 15),
            new(high: 30, open: 22, close: 28, low: 18),
            new(high: 32, open: 27, close: 24, low: 19),
        };
        var series = new CandlesticksSeries<FinancialPointI> { Values = values };
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

            // Mutate every candle's OHLC.
            values[0] = new FinancialPointI(high: 38, open: 30, close: 36, low: 28);
            values[1] = new FinancialPointI(high: 25, open: 23, close: 18, low: 15);
            values[2] = new FinancialPointI(high: 33, open: 29, close: 31, low: 25);
            var measureT = CoreMotionCanvas.DebugElapsedMilliseconds;
            core.Measure();

            var traj = SeriesAnimationCapture.CaptureTrajectory<SeriesAnimationCapture.CandlestickFrame>(
                series.everFetched, ReadCandlestickFrame,
                startMs: measureT, endMs: measureT + AnimationMs, stepMs: StepMs);

            // Candlestick OHLC motion has its own transition seed semantics; let the JSON
            // baseline pin the actual contract rather than over-specifying here.
            SeriesAnimationCapture.AssertTrajectoryMatches(traj, "CandlestickSeries_DataChange");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }
}
