using System.Collections.Generic;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.CoreObjectsTests;

[TestClass]
public class ChartLifecycleEventsTests
{
    [TestMethod]
    public void MeasuringFiresOncePerMeasure()
    {
        var chart = NewChart();
        var count = 0;
        chart.Measuring += _ => count++;

        _ = ChangingPaintTasks.DrawChart(chart);

        Assert.AreEqual(1, count);
    }

    [TestMethod]
    public void UpdateStartedFiresOncePerMeasure()
    {
        var chart = NewChart();
        var count = 0;
        chart.UpdateStarted += _ => count++;

        _ = ChangingPaintTasks.DrawChart(chart);

        Assert.AreEqual(1, count);
    }

    [TestMethod]
    public void UpdateFinishedFiresOncePerDrainCycle()
    {
        var chart = NewChart();
        var count = 0;
        chart.UpdateFinished += _ => count++;

        _ = ChangingPaintTasks.DrawChart(chart);

        Assert.AreEqual(1, count);
    }

    [TestMethod]
    public void UpdateFinishedFiresOnceWhenAnimated()
    {
        // animations require multiple DrawFrame calls before the canvas validates;
        // Validated (and therefore UpdateFinished) must still fire exactly once,
        // on the final frame.
        var chart = NewChart(animated: true);
        var count = 0;
        var frames = 0;
        chart.UpdateFinished += _ => count++;

        frames = ChangingPaintTasks.DrawChart(chart, animated: true);

        Assert.IsTrue(frames > 1, "expected the animated path to render multiple frames");
        Assert.AreEqual(1, count);
    }

    [TestMethod]
    public void LifecycleEventsFireInExpectedOrder()
    {
        var chart = NewChart();
        var order = new List<string>();
        chart.Measuring += _ => order.Add(nameof(chart.Measuring));
        chart.UpdateStarted += _ => order.Add(nameof(chart.UpdateStarted));
        chart.UpdateFinished += _ => order.Add(nameof(chart.UpdateFinished));

        _ = ChangingPaintTasks.DrawChart(chart);

        CollectionAssert.AreEqual(
            new[]
            {
                nameof(chart.Measuring),
                nameof(chart.UpdateStarted),
                nameof(chart.UpdateFinished),
            },
            order);
    }

    [TestMethod]
    public void LifecycleEventSenderIsTheChartItself()
    {
        var chart = NewChart();
        IChartView? measuringSender = null;
        IChartView? startedSender = null;
        IChartView? finishedSender = null;
        chart.Measuring += c => measuringSender = c;
        chart.UpdateStarted += c => startedSender = c;
        chart.UpdateFinished += c => finishedSender = c;

        _ = ChangingPaintTasks.DrawChart(chart);

        Assert.AreSame(chart, measuringSender);
        Assert.AreSame(chart, startedSender);
        Assert.AreSame(chart, finishedSender);
    }

    [TestMethod]
    public void LifecycleEventsRefireOnSubsequentMeasure()
    {
        var chart = NewChart();
        var measuring = 0;
        var started = 0;
        var finished = 0;
        chart.Measuring += _ => measuring++;
        chart.UpdateStarted += _ => started++;
        chart.UpdateFinished += _ => finished++;

        _ = ChangingPaintTasks.DrawChart(chart);
        _ = ChangingPaintTasks.DrawChart(chart);

        Assert.AreEqual(2, measuring);
        Assert.AreEqual(2, started);
        Assert.AreEqual(2, finished);
    }

    [TestMethod]
    public void PieChartFiresLifecycleEvents()
    {
        // PieChartEngine has its own InvokeOnMeasuring / InvokeOnUpdateStarted call sites;
        // pin that they also flow through to the SKChart-level events.
        var chart = new SKPieChart
        {
            Series = [new PieSeries<int> { Values = [1] }],
        };
        var measuring = 0;
        var started = 0;
        var finished = 0;
        chart.Measuring += _ => measuring++;
        chart.UpdateStarted += _ => started++;
        chart.UpdateFinished += _ => finished++;

        _ = ChangingPaintTasks.DrawChart(chart);

        Assert.AreEqual(1, measuring);
        Assert.AreEqual(1, started);
        Assert.AreEqual(1, finished);
    }

    [TestMethod]
    public void PolarChartFiresLifecycleEvents()
    {
        // PolarChartEngine has its own InvokeOnMeasuring / InvokeOnUpdateStarted call sites;
        // pin that they also flow through to the SKChart-level events.
        var chart = new SKPolarChart
        {
            Series = [new PolarLineSeries<int> { Values = [1, 2, 3] }],
        };
        var measuring = 0;
        var started = 0;
        var finished = 0;
        chart.Measuring += _ => measuring++;
        chart.UpdateStarted += _ => started++;
        chart.UpdateFinished += _ => finished++;

        _ = ChangingPaintTasks.DrawChart(chart);

        Assert.AreEqual(1, measuring);
        Assert.AreEqual(1, started);
        Assert.AreEqual(1, finished);
    }

    private static SKCartesianChart NewChart(bool animated = false)
    {
        var chart = new SKCartesianChart
        {
            Series = [new LineSeries<int>([1, 2, 3])],
            XAxes = [new Axis()],
            YAxes = [new Axis()],
        };

        if (animated)
        {
            chart.AnimationsSpeed = System.TimeSpan.FromSeconds(1);
            chart.EasingFunction = EasingFunctions.Lineal;
        }

        return chart;
    }
}
