using LiveChartsCore.Drawing;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.VisualElements;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.CoreObjectsTests;

[TestClass]
public class VisualElementMeasurementEventsTests
{
#pragma warning disable CS0618 // VariableGeometryVisual is obsolete but the event contract still ships.
    [TestMethod]
    public void GeometryInitializedFiresOnceOnFirstMeasure()
    {
        var geometry = new RectangleGeometry();
        var visual = NewVisual(geometry);
        var chart = NewChart();
        chart.VisualElements = [visual];
        var count = 0;
        visual.GeometryInitialized += _ => count++;

        _ = ChangingPaintTasks.DrawChart(chart);

        Assert.AreEqual(1, count);
    }

    [TestMethod]
    public void GeometryInitializedDoesNotRefireOnSubsequentMeasure()
    {
        var geometry = new RectangleGeometry();
        var visual = NewVisual(geometry);
        var chart = NewChart();
        chart.VisualElements = [visual];
        var count = 0;
        visual.GeometryInitialized += _ => count++;

        _ = ChangingPaintTasks.DrawChart(chart);
        _ = ChangingPaintTasks.DrawChart(chart);

        Assert.AreEqual(1, count);
    }

    [TestMethod]
    public void GeometryInitializedRefiresWhenGeometryInstanceChanges()
    {
        // Reassigning Geometry flips _isInitialized back to false, so the next
        // measure must re-emit the event against the new instance.
        var firstGeometry = new RectangleGeometry();
        var visual = NewVisual(firstGeometry);
        var chart = NewChart();
        chart.VisualElements = [visual];
        BoundedDrawnGeometry? lastReported = null;
        visual.GeometryInitialized += g => lastReported = g;

        _ = ChangingPaintTasks.DrawChart(chart);
        Assert.AreSame(firstGeometry, lastReported);

        var secondGeometry = new RectangleGeometry();
        visual.Geometry = secondGeometry;
        _ = ChangingPaintTasks.DrawChart(chart);

        Assert.AreSame(secondGeometry, lastReported);
    }

    private static VariableGeometryVisual NewVisual(BoundedDrawnGeometry geometry) =>
        new(geometry)
        {
            X = 200,
            Y = 200,
            Width = 100,
            Height = 100,
            LocationUnit = MeasureUnit.Pixels,
            SizeUnit = MeasureUnit.Pixels,
        };
#pragma warning restore CS0618

    private static SKCartesianChart NewChart() => new()
    {
        Series = [new LineSeries<int>([1, 2, 3])],
        XAxes = [new Axis()],
        YAxes = [new Axis()],
    };
}
