using System.Linq;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.CoreObjectsTests;

[TestClass]
public class SubseparatorsCountTests
{
    // Regression for issue #2287: changing SubseparatorsCount at runtime kept the
    // existing subseparator array length, so old geometries were repositioned via
    // the (j+1)/(count+1) divisor but no new lines were added — leaving gaps.
    [TestMethod]
    public void ChangingSubseparatorsCountRebuildsGeometryArray()
    {
        var subseparatorsPaint = new SolidColorPaint(SKColors.Blue);
        var yAxis = new Axis
        {
            SeparatorsPaint = new SolidColorPaint(SKColors.Red),
            SubseparatorsPaint = subseparatorsPaint,
            SubseparatorsCount = 1,
        };

        var chart = new SKCartesianChart
        {
            Series = [new LineSeries<double>([4, 7, 5, 8, 6, 9, 5, 7])],
            XAxes = [new Axis()],
            YAxes = [yAxis],
        };

        _ = ChangingPaintTasks.DrawChart(chart);
        var initialCount = subseparatorsPaint.GetGeometries(chart.CoreCanvas).Count;
        Assert.IsTrue(initialCount > 0, "Expected subseparators after the first measure.");

        yAxis.SubseparatorsCount = 2;
        _ = ChangingPaintTasks.DrawChart(chart);
        var doubledCount = subseparatorsPaint.GetGeometries(chart.CoreCanvas).Count;

        yAxis.SubseparatorsCount = 3;
        _ = ChangingPaintTasks.DrawChart(chart);
        var tripledCount = subseparatorsPaint.GetGeometries(chart.CoreCanvas).Count;

        // Major-separator count stayed constant across the three measures (same
        // data + same bounds), so the subseparator count must scale linearly with
        // SubseparatorsCount.
        var majorCount = initialCount; // 1 subseparator per major slot at count=1
        Assert.AreEqual(2 * majorCount, doubledCount, "SubseparatorsCount=2 should produce 2x the geometries");
        Assert.AreEqual(3 * majorCount, tripledCount, "SubseparatorsCount=3 should produce 3x the geometries");
    }

    [TestMethod]
    public void ShrinkingSubseparatorsCountDetachesExtraGeometries()
    {
        var subseparatorsPaint = new SolidColorPaint(SKColors.Blue);
        var yAxis = new Axis
        {
            SeparatorsPaint = new SolidColorPaint(SKColors.Red),
            SubseparatorsPaint = subseparatorsPaint,
            SubseparatorsCount = 3,
        };

        var chart = new SKCartesianChart
        {
            Series = [new LineSeries<double>([4, 7, 5, 8, 6, 9, 5, 7])],
            XAxes = [new Axis()],
            YAxes = [yAxis],
        };

        _ = ChangingPaintTasks.DrawChart(chart);
        var initial = subseparatorsPaint.GetGeometries(chart.CoreCanvas).Count;

        yAxis.SubseparatorsCount = 1;
        _ = ChangingPaintTasks.DrawChart(chart);
        var shrunk = subseparatorsPaint.GetGeometries(chart.CoreCanvas).Count;

        Assert.AreEqual(initial / 3, shrunk, "Shrinking SubseparatorsCount must detach the extra geometries");
    }
}
