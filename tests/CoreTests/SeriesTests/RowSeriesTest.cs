using System;
using System.Linq;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.SeriesTests;

[TestClass]
public class RowSeriesTest
{
    [TestMethod]
    public void ShouldScaleProperly()
    {
        var sutSeries = new RowSeries<double>
        {
            Values = new double[] { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512 }
        };

        var chart = new SKCartesianChart
        {
            Width = 1000,
            Height = 1000,
            Series = new[] { sutSeries },
            XAxes = new[] { new Axis { MinLimit = 0, MaxLimit = 512 } },
            YAxes = new[] { new Axis { MinLimit = -1, MaxLimit = 10 } }
        };

        _ = chart.GetImage();
        // chart.SaveImage("test.png"); // use this method to see the actual tested image

        var datafactory = sutSeries.DataFactory;
        var points = datafactory.Fetch(sutSeries, chart.CoreChart).ToArray();

        var unit = points.First(x => x.Coordinate.PrimaryValue == 1);
        var typedUnit = sutSeries.ConvertToTypedChartPoint(unit);

        var toCompareGuys = points.Where(x => x != unit).Select(sutSeries.ConvertToTypedChartPoint);

        // ensure the unit has valid dimensions
        Assert.IsTrue(typedUnit.Visual.Width > 1 && typedUnit.Visual.Height > 1);

        var previous = typedUnit;
        float? previousY = null;

        foreach (var sutPoint in toCompareGuys)
        {
            // test width
            Assert.IsTrue(
                // the idea is, the second bar should be 2 times bigger than the first one
                // and the third bar should be 4 times bigger than the first one and so on
                Math.Abs(typedUnit.Visual.Width - sutPoint.Visual.Width / (float)sutPoint.Model) < 0.001);

            // test height
            Assert.IsTrue(
                // and also the width should be the same.
                Math.Abs(typedUnit.Visual.Height - sutPoint.Visual.Height) < 0.001);

            // test y
            var currentDeltaY = previous.Visual.Y - sutPoint.Visual.Y;
            Assert.IsTrue(
                previousY is null
                ||
                Math.Abs(previousY.Value - currentDeltaY) < 0.001);

            // test x
            Assert.IsTrue(
                Math.Abs(sutPoint.Visual.X - chart.CoreChart.DrawMarginLocation.X) < 0.001);

            previousY = previous.Visual.Y - sutPoint.Visual.Y;
            previous = sutPoint;
        }
    }
}
