using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.CoreObjectsTests;

[TestClass]
public class AxisEventsTests
{
    [TestMethod]
    public void AxisMeasureStartedFiresOncePerAxisPerMeasure()
    {
        var xAxis = new Axis();
        var yAxis = new Axis();
        var xCount = 0;
        var yCount = 0;
        xAxis.MeasureStarted += (_, _) => xCount++;
        yAxis.MeasureStarted += (_, _) => yCount++;

        var chart = new SKCartesianChart
        {
            Series = [new LineSeries<int>([1, 2, 3])],
            XAxes = [xAxis],
            YAxes = [yAxis],
        };
        _ = ChangingPaintTasks.DrawChart(chart);

        Assert.AreEqual(1, xCount);
        Assert.AreEqual(1, yCount);
    }

    [TestMethod]
    public void AxisMeasureStartedSenderIsChartAndAxisItself()
    {
        var xAxis = new Axis();
        Chart? receivedChart = null;
        ICartesianAxis? receivedAxis = null;
        xAxis.MeasureStarted += (c, a) =>
        {
            receivedChart = c;
            receivedAxis = a;
        };

        var chart = new SKCartesianChart
        {
            Series = [new LineSeries<int>([1, 2, 3])],
            XAxes = [xAxis],
            YAxes = [new Axis()],
        };
        _ = ChangingPaintTasks.DrawChart(chart);

        Assert.AreSame(chart.CoreChart, receivedChart);
        Assert.AreSame(xAxis, receivedAxis);
    }

    [TestMethod]
    public void AxisMeasureStartedRefiresOnSubsequentMeasure()
    {
        var xAxis = new Axis();
        var count = 0;
        xAxis.MeasureStarted += (_, _) => count++;

        var chart = new SKCartesianChart
        {
            Series = [new LineSeries<int>([1, 2, 3])],
            XAxes = [xAxis],
            YAxes = [new Axis()],
        };
        _ = ChangingPaintTasks.DrawChart(chart);
        _ = ChangingPaintTasks.DrawChart(chart);

        Assert.AreEqual(2, count);
    }

    [TestMethod]
    public void DrawMarginDefinedFiresOncePerMeasure()
    {
        var chart = new SKCartesianChart
        {
            Series = [new LineSeries<int>([1, 2, 3])],
            XAxes = [new Axis()],
            YAxes = [new Axis()],
        };
        var engine = (CartesianChartEngine)chart.CoreChart;
        var count = 0;
        CartesianChartEngine? sender = null;
        engine.DrawMarginDefined += e => { sender = e; count++; };

        _ = ChangingPaintTasks.DrawChart(chart);

        Assert.AreEqual(1, count);
        Assert.AreSame(engine, sender);
    }

    [TestMethod]
    public void PolarAxisInitializedFiresOnMeasure()
    {
        var angleAxis = new PolarAxis();
        var radiusAxis = new PolarAxis();
        var angleCount = 0;
        var radiusCount = 0;
        IPolarAxis? sender = null;
        angleAxis.Initialized += a => { angleCount++; sender = a; };
        radiusAxis.Initialized += _ => radiusCount++;

        var chart = new SKPolarChart
        {
            Series = [new PolarLineSeries<int> { Values = [1, 2, 3] }],
            AngleAxes = [angleAxis],
            RadiusAxes = [radiusAxis],
        };
        _ = ChangingPaintTasks.DrawChart(chart);

        Assert.IsTrue(angleCount >= 1);
        Assert.IsTrue(radiusCount >= 1);
        Assert.AreSame(angleAxis, sender);
    }
}
