using LiveChartsCore;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.CoreObjectsTests;

[TestClass]
public class CanvasEventsTests
{
    [TestMethod]
    public void InvalidatedFiresOnMeasure()
    {
        var chart = NewChart();
        var canvas = chart.CoreCanvas;
        var count = 0;
        canvas.Invalidated += _ => count++;

        _ = ChangingPaintTasks.DrawChart(chart);

        Assert.AreEqual(1, count);
    }

    [TestMethod]
    public void ValidatedFiresOncePerDrain()
    {
        var chart = NewChart();
        var canvas = chart.CoreCanvas;
        var count = 0;
        canvas.Validated += _ => count++;

        _ = ChangingPaintTasks.DrawChart(chart);

        Assert.AreEqual(1, count);
    }

    [TestMethod]
    public void ValidatedFiresOncePerDrainWhenAnimated()
    {
        // The Validated event fires only on the final, drained frame — the
        // animated draw loop must not flap the event across in-progress frames.
        var chart = new SKCartesianChart
        {
            AnimationsSpeed = System.TimeSpan.FromSeconds(1),
            EasingFunction = EasingFunctions.Lineal,
            Series = [new LineSeries<int>([1, 2, 3])],
            XAxes = [new Axis()],
            YAxes = [new Axis()],
        };
        var canvas = chart.CoreCanvas;
        var count = 0;
        canvas.Validated += _ => count++;

        var frames = ChangingPaintTasks.DrawChart(chart, animated: true);

        Assert.IsTrue(frames > 1, "expected multiple frames in the animated path");
        Assert.AreEqual(1, count);
    }

    [TestMethod]
    public void InvalidatedSenderIsTheCanvas()
    {
        var chart = NewChart();
        var canvas = chart.CoreCanvas;
        CoreMotionCanvas? sender = null;
        canvas.Invalidated += c => sender = c;

        _ = ChangingPaintTasks.DrawChart(chart);

        Assert.AreSame(canvas, sender);
    }

    [TestMethod]
    public void ValidatedSenderIsTheCanvas()
    {
        var chart = NewChart();
        var canvas = chart.CoreCanvas;
        CoreMotionCanvas? sender = null;
        canvas.Validated += c => sender = c;

        _ = ChangingPaintTasks.DrawChart(chart);

        Assert.AreSame(canvas, sender);
    }

    [TestMethod]
    public void DirectInvalidateCallFiresInvalidatedEvent()
    {
        // Pin the public surface: any caller can Invalidate() the canvas and
        // observe the event — it is not gated on chart Measure participation.
        var chart = NewChart();
        var canvas = chart.CoreCanvas;
        _ = ChangingPaintTasks.DrawChart(chart);

        var count = 0;
        canvas.Invalidated += _ => count++;
        canvas.Invalidate();

        Assert.AreEqual(1, count);
    }

    private static SKCartesianChart NewChart() => new()
    {
        Series = [new LineSeries<int>([1, 2, 3])],
        XAxes = [new Axis()],
        YAxes = [new Axis()],
    };
}
