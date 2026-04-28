using System;
using System.Linq;
using CoreTests.MockedObjects;
using LiveChartsCore.Drawing;
using LiveChartsCore.Drawing.Segments;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.SeriesTests;

[TestClass]
public class StackedAreaSeriesTest
{
    [TestMethod]
    public void ShouldScale()
    {
        var sutSeries = new StackedAreaSeries<double>
        {
            Values = [1, 2, 4, 8, 16, 32, 64, 128, 256],
            GeometrySize = 10
        };

        var sutSeries2 = new StackedAreaSeries<double>
        {
            Values = [1, 2, 4, 8, 16, 32, 64, 128, 256],
            GeometrySize = 10
        };

        var chart = new SKCartesianChart
        {
            Width = 1000,
            Height = 1000,
            Series = [sutSeries, sutSeries2],
            XAxes = [new Axis { MinLimit = -1, MaxLimit = 10 }],
            YAxes = [new Axis { MinLimit = 0, MaxLimit = 512 }]
        };

        _ = chart.GetImage();
        //chart.SaveImage("test.png"); // use this method to see the actual tested image

        var datafactory = sutSeries.DataFactory;
        var points = datafactory.Fetch(sutSeries, chart.CoreChart).ToArray();

        var unit = points.First(x => x.Coordinate.PrimaryValue == 1);
        var typedUnit = sutSeries.ConvertToTypedChartPoint(unit);

        var toCompareGuys = points.Where(x => x != unit).Select(sutSeries.ConvertToTypedChartPoint);

        var datafactory2 = sutSeries2.DataFactory;
        var points2 = datafactory2.Fetch(sutSeries2, chart.CoreChart).ToArray();
        var unit2 = points2.First(x => x.Coordinate.PrimaryValue == 1);
        var typedUnit2 = sutSeries.ConvertToTypedChartPoint(unit2);
        var toCompareGuys2 = points2.Where(x => x != unit2).Select(sutSeries2.ConvertToTypedChartPoint);

        // ensure the unit has valid dimensions
        Assert.IsTrue(typedUnit.Visual.Width == 10 && typedUnit.Visual.Height == 10);

        var previous = typedUnit;
        float? previousX = null;
        float? previousXArea = null;

        foreach (var sutPoint in toCompareGuys)
        {
            var previousBezier = ((CubicSegmentVisualPoint)previous.Context.AdditionalVisuals)?.Segment;
            var sutBezier = ((CubicSegmentVisualPoint)sutPoint.Context.AdditionalVisuals).Segment;

            // test x
            var currentDeltaX = previous.Visual.X - sutPoint.Visual.X;
            var currentDeltaAreaX = previousBezier.Xj - sutBezier.Xj;
            Assert.IsTrue(
                previousX is null
                ||
                Math.Abs(previousX.Value - currentDeltaX) < 0.001);
            Assert.IsTrue(
                previousXArea is null
                ||
                Math.Abs(previousXArea.Value - currentDeltaX) < 0.001);

            // test y
            var p = 1f - (sutPoint.Coordinate.PrimaryValue + sutPoint.StackedValue.Start) / 512f;
            Assert.IsTrue(
                Math.Abs(p * chart.CoreChart.DrawMarginSize.Height - sutPoint.Visual.Y + chart.CoreChart.DrawMarginLocation.Y) < 0.001);
            Assert.IsTrue(
                Math.Abs(p * chart.CoreChart.DrawMarginSize.Height - sutBezier.Yj + chart.CoreChart.DrawMarginLocation.Y) < 0.001);

            previousX = previous.Visual.X - sutPoint.Visual.X;
            previousXArea = previousBezier.Xj - sutBezier.Xj;
            previous = sutPoint;
        }

        previous = typedUnit2;
        previousX = null;
        previousXArea = null;
        foreach (var sutPoint in toCompareGuys2)
        {
            var previousBezier = ((CubicSegmentVisualPoint)previous.Context.AdditionalVisuals).Segment;
            var sutBezier = ((CubicSegmentVisualPoint)sutPoint.Context.AdditionalVisuals).Segment;

            // test x
            var currentDeltaX = previous.Visual.X - sutPoint.Visual.X;
            var currentDeltaAreaX = previousBezier.Xj - sutBezier.Xj;
            Assert.IsTrue(
                previousX is null
                ||
                Math.Abs(previousX.Value - currentDeltaX) < 0.001);
            Assert.IsTrue(
                previousXArea is null
                ||
                Math.Abs(previousXArea.Value - currentDeltaX) < 0.001);

            // test y
            var p = 1f - (sutPoint.Coordinate.PrimaryValue + sutPoint.StackedValue.Start) / 512f;
            Assert.IsTrue(
                Math.Abs(p * chart.CoreChart.DrawMarginSize.Height - sutPoint.Visual.Y + chart.CoreChart.DrawMarginLocation.Y) < 0.001);
            Assert.IsTrue(
                Math.Abs(p * chart.CoreChart.DrawMarginSize.Height - sutBezier.Yj + chart.CoreChart.DrawMarginLocation.Y) < 0.001);

            previousX = previous.Visual.X - sutPoint.Visual.X;
            previousXArea = previousBezier.Xj - sutBezier.Xj;
            previous = sutPoint;
        }
    }

    [TestMethod]
    public void ShouldPlaceDataLabel()
    {
        var gs = 5f;
        var sutSeries = new StackedAreaSeries<double, RectangleGeometry, TestLabel>
        {
            Values = [-10, -5, -1, 0, 1, 5, 10],
            DataPadding = new LvcPoint(0, 0),
            GeometrySize = gs * 2,
        };

        var chart = new SKCartesianChart
        {
            Width = 500,
            Height = 500,
            DrawMargin = new Margin(100),
            DrawMarginFrame = new DrawMarginFrame { Stroke = new SolidColorPaint(SKColors.Yellow, 2) },
            TooltipPosition = TooltipPosition.Top,
            Series = [sutSeries],
            XAxes = [new Axis { IsVisible = false }],
            YAxes = [new Axis { IsVisible = false }]
        };

        var datafactory = sutSeries.DataFactory;

        // TEST HIDDEN ===========================================================
        _ = chart.GetImage();

        var points = datafactory
            .Fetch(sutSeries, chart.CoreChart)
            .Select(sutSeries.ConvertToTypedChartPoint);

        Assert.IsTrue(sutSeries.DataLabelsPosition == DataLabelsPosition.End);
        Assert.IsTrue(points.All(x => x.Label is null));

        sutSeries.DataLabelsPaint = new SolidColorPaint
        {
            Color = SKColors.Black,
            SKTypeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
        };

        // TEST TOP ===============================================================
        sutSeries.DataLabelsPosition = DataLabelsPosition.Top;
        _ = chart.GetImage();

        points = datafactory
            .Fetch(sutSeries, chart.CoreChart)
            .Select(sutSeries.ConvertToTypedChartPoint);

        foreach (var p in points)
        {
            var v = p.Visual;
            var l = p.Label;

            l.Paint = sutSeries.DataLabelsPaint;
            var ls = l.Measure();

            Assert.IsTrue(
                Math.Abs(v.X + v.Width * 0.5f - l.X - gs) < 0.01 &&    // x is centered
                Math.Abs(v.Y - (l.Y + ls.Height * 0.5 + gs)) < 0.01);  // y is top
        }

        // TEST BOTTOM ===========================================================
        sutSeries.DataLabelsPosition = DataLabelsPosition.Bottom;

        _ = chart.GetImage();

        points = datafactory
            .Fetch(sutSeries, chart.CoreChart)
            .Select(sutSeries.ConvertToTypedChartPoint);

        foreach (var p in points)
        {
            var v = p.Visual;
            var l = p.Label;

            l.Paint = sutSeries.DataLabelsPaint;
            var ls = l.Measure();

            Assert.IsTrue(
                Math.Abs(v.X + v.Width * 0.5f - l.X - gs) < 0.01 &&              // x is centered
                Math.Abs(v.Y + v.Height - (l.Y - ls.Height * 0.5 + gs)) < 0.01); // y is bottom
        }

        // TEST RIGHT ============================================================
        sutSeries.DataLabelsPosition = DataLabelsPosition.Right;

        _ = chart.GetImage();

        points = datafactory
            .Fetch(sutSeries, chart.CoreChart)
            .Select(sutSeries.ConvertToTypedChartPoint);

        foreach (var p in points)
        {
            var v = p.Visual;
            var l = p.Label;

            l.Paint = sutSeries.DataLabelsPaint;
            var ls = l.Measure();

            Assert.IsTrue(
                Math.Abs(v.X + v.Width - (l.X - ls.Width * 0.5 + gs)) < 0.01 &&  // x is right
                Math.Abs(v.Y + v.Height * 0.5 - l.Y - gs) < 0.01);               // y is centered
        }

        // TEST LEFT =============================================================
        sutSeries.DataLabelsPosition = DataLabelsPosition.Left;

        _ = chart.GetImage();

        points = datafactory
            .Fetch(sutSeries, chart.CoreChart)
            .Select(sutSeries.ConvertToTypedChartPoint);

        foreach (var p in points)
        {
            var v = p.Visual;
            var l = p.Label;

            l.Paint = sutSeries.DataLabelsPaint;
            var ls = l.Measure();

            Assert.IsTrue(
                Math.Abs(v.X - (l.X + ls.Width * 0.5f + gs)) < 0.01 &&   // x is left
                Math.Abs(v.Y + v.Height * 0.5f - l.Y - gs) < 0.01);      // y is centered
        }

        // TEST MIDDLE ===========================================================
        sutSeries.DataLabelsPosition = DataLabelsPosition.Middle;

        _ = chart.GetImage();

        points = datafactory
            .Fetch(sutSeries, chart.CoreChart)
            .Select(sutSeries.ConvertToTypedChartPoint);

        foreach (var p in points)
        {
            var v = p.Visual;
            var l = p.Label;

            l.Paint = sutSeries.DataLabelsPaint;
            var ls = l.Measure();

            Assert.IsTrue(
                Math.Abs(v.X + v.Width * 0.5f - l.X - gs) < 0.01 &&      // x is centered
                Math.Abs(v.Y + v.Height * 0.5f - l.Y - gs) < 0.01);      // y is centered
        }

        // TEST START ===========================================================
        sutSeries.DataLabelsPosition = DataLabelsPosition.Start;

        _ = chart.GetImage();

        points = datafactory
            .Fetch(sutSeries, chart.CoreChart)
            .Select(sutSeries.ConvertToTypedChartPoint);

        foreach (var p in points)
        {
            var v = p.Visual;
            var l = p.Label;

            l.Paint = sutSeries.DataLabelsPaint;
            var ls = l.Measure();

            if (p.Model <= 0)
            {
                // it should be placed using the top position
                Assert.IsTrue(
                    Math.Abs(v.X + v.Width * 0.5f - l.X - gs) < 0.01 &&    // x is centered
                    Math.Abs(v.Y - (l.Y + ls.Height * 0.5 + gs)) < 0.01);  // y is top
            }
            else
            {
                // it should be placed using the bottom position
                Assert.IsTrue(
                    Math.Abs(v.X + v.Width * 0.5f - l.X - gs) < 0.01 &&              // x is centered
                    Math.Abs(v.Y + v.Height - (l.Y - ls.Height * 0.5 + gs)) < 0.01); // y is bottom
            }
        }

        // TEST END ===========================================================
        sutSeries.DataLabelsPosition = DataLabelsPosition.End;

        _ = chart.GetImage();

        points = datafactory
            .Fetch(sutSeries, chart.CoreChart)
            .Select(sutSeries.ConvertToTypedChartPoint);

        foreach (var p in points)
        {
            var v = p.Visual;
            var l = p.Label;

            l.Paint = sutSeries.DataLabelsPaint;
            var ls = l.Measure();

            if (p.Model <= 0)
            {
                // it should be placed using the bottom position
                Assert.IsTrue(
                    Math.Abs(v.X + v.Width * 0.5f - l.X - gs) < 0.01 &&              // x is centered
                    Math.Abs(v.Y + v.Height - (l.Y - ls.Height * 0.5 + gs)) < 0.01); // y is bottom
            }
            else
            {
                // it should be placed using the top position
                Assert.IsTrue(
                    Math.Abs(v.X + v.Width * 0.5f - l.X - gs) < 0.01 &&    // x is centered
                    Math.Abs(v.Y - (l.Y + ls.Height * 0.5 + gs)) < 0.01);  // y is top
            }
        }

        // FINALLY IF LABELS ARE NULL, IT SHOULD REMOVE THE CURRENT LABELS.
        var previousPaint = sutSeries.DataLabelsPaint;
        sutSeries.DataLabelsPaint = null;
        _ = chart.GetImage();

        Assert.IsTrue(!chart.CoreCanvas.ContainsPaintTask(previousPaint));
    }

    [TestMethod]
    public void ShouldHandleMixedPositiveNegativeValues()
    {
        // Regression test for #2073 and #2152.
        //
        // Stacker exposes three independent tracks per index:
        //   * Start/End            — accumulator that only positives contribute to,
        //                            so stacked column/row positives grow upward from 0.
        //   * NegativeStart/End    — accumulator that only negatives contribute to,
        //                            so stacked column/row negatives grow downward from 0.
        //   * CumulativeStart/End  — running sum of every value regardless of sign,
        //                            used by stacked area/line series for a continuous
        //                            baseline across mixed signs (Excel-like).
        //
        // Series1: positive at index 0, negative at index 1
        // Series2: negative at index 0, positive at index 1
        var series1 = new StackedAreaSeries<double>
        {
            Values = [5, -3],
            GeometrySize = 10
        };

        var series2 = new StackedAreaSeries<double>
        {
            Values = [-2, 4],
            GeometrySize = 10
        };

        var chart = new SKCartesianChart
        {
            Width = 1000,
            Height = 1000,
            Series = [series1, series2],
            XAxes = [new Axis()],
            YAxes = [new Axis()]
        };

        _ = chart.GetImage();

        var points1 = series1.DataFactory.Fetch(series1, chart.CoreChart).ToArray();
        var points2 = series2.DataFactory.Fetch(series2, chart.CoreChart).ToArray();

        var point1_0 = points1.Single(p => p.Coordinate.SecondaryValue == 0);
        var point2_0 = points2.Single(p => p.Coordinate.SecondaryValue == 0);
        var point1_1 = points1.Single(p => p.Coordinate.SecondaryValue == 1);
        var point2_1 = points2.Single(p => p.Coordinate.SecondaryValue == 1);

        // Series1[0] = +5 — only feeds the positive and cumulative tracks.
        Assert.AreEqual(0, point1_0.StackedValue.Start, 0.001);
        Assert.AreEqual(5, point1_0.StackedValue.End, 0.001);
        Assert.AreEqual(0, point1_0.StackedValue.NegativeStart, 0.001);
        Assert.AreEqual(0, point1_0.StackedValue.NegativeEnd, 0.001);
        Assert.AreEqual(0, point1_0.StackedValue.CumulativeStart, 0.001);
        Assert.AreEqual(5, point1_0.StackedValue.CumulativeEnd, 0.001);

        // Series2[0] = -2 stacked on top of Series1[0]:
        //   * positives don't move (Start/End stay at 5 — Series1's positive end)
        //   * negatives extend independently from 0 down to -2
        //   * cumulative = 5 + (-2) = 3 — continuous baseline used by area path
        Assert.AreEqual(5, point2_0.StackedValue.Start, 0.001);
        Assert.AreEqual(5, point2_0.StackedValue.End, 0.001);
        Assert.AreEqual(0, point2_0.StackedValue.NegativeStart, 0.001);
        Assert.AreEqual(-2, point2_0.StackedValue.NegativeEnd, 0.001);
        Assert.AreEqual(5, point2_0.StackedValue.CumulativeStart, 0.001);
        Assert.AreEqual(3, point2_0.StackedValue.CumulativeEnd, 0.001);

        // Series1[1] = -3 — only feeds the negative and cumulative tracks.
        Assert.AreEqual(0, point1_1.StackedValue.Start, 0.001);
        Assert.AreEqual(0, point1_1.StackedValue.End, 0.001);
        Assert.AreEqual(0, point1_1.StackedValue.NegativeStart, 0.001);
        Assert.AreEqual(-3, point1_1.StackedValue.NegativeEnd, 0.001);
        Assert.AreEqual(0, point1_1.StackedValue.CumulativeStart, 0.001);
        Assert.AreEqual(-3, point1_1.StackedValue.CumulativeEnd, 0.001);

        // Series2[1] = +4 stacked on top of Series1[1]:
        //   * positives extend independently from 0 up to +4
        //   * negatives don't move (NegativeStart/End stay at -3 — Series1's neg end)
        //   * cumulative = -3 + 4 = 1
        Assert.AreEqual(0, point2_1.StackedValue.Start, 0.001);
        Assert.AreEqual(4, point2_1.StackedValue.End, 0.001);
        Assert.AreEqual(-3, point2_1.StackedValue.NegativeStart, 0.001);
        Assert.AreEqual(-3, point2_1.StackedValue.NegativeEnd, 0.001);
        Assert.AreEqual(-3, point2_1.StackedValue.CumulativeStart, 0.001);
        Assert.AreEqual(1, point2_1.StackedValue.CumulativeEnd, 0.001);
    }
}
