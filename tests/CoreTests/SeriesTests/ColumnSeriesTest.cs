using System;
using System.Linq;
using CoreTests.MockedObjects;
using LiveChartsCore.Drawing;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.SeriesTests;

[TestClass]
public class ColumnSeriesTest
{
    [TestMethod]
    public void ShouldScale()
    {
        var sutSeries = new ColumnSeries<double>
        {
            Values = new double[] { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512 }
        };

        var chart = new SKCartesianChart
        {
            Width = 1000,
            Height = 1000,
            Series = new[] { sutSeries },
            XAxes = new[] { new Axis { MinLimit = -1, MaxLimit = 10 } },
            YAxes = new[] { new Axis { MinLimit = 0, MaxLimit = 512 } }
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
        float? previousX = null;

        foreach (var sutPoint in toCompareGuys)
        {
            // test height
            Assert.IsTrue(
                // the idea is, the second bar should be 2 times bigger than the first one
                // and the third bar should be 4 times bigger than the first one and so on
                Math.Abs(typedUnit.Visual.Height - sutPoint.Visual.Height / (float)sutPoint.Model) < 0.001);

            // test width
            Assert.IsTrue(
                // and also the width should be the same.
                Math.Abs(typedUnit.Visual.Width - sutPoint.Visual.Width) < 0.001);

            // test x
            var currentDeltaX = previous.Visual.X - sutPoint.Visual.X;
            Assert.IsTrue(
                previousX is null
                ||
                Math.Abs(previousX.Value - currentDeltaX) < 0.001);

            // test y
            var p = 1f - sutPoint.Coordinate.PrimaryValue / 512f;
            Assert.IsTrue(
                Math.Abs(p * chart.CoreChart.DrawMarginSize.Height - sutPoint.Visual.Y + chart.CoreChart.DrawMarginLocation.Y) < 0.001);

            previousX = previous.Visual.X - sutPoint.Visual.X;
            previous = sutPoint;
        }
    }

    [TestMethod]
    public void ShouldPlaceToolTips()
    {
        var sutSeries = new ColumnSeries<double>
        {
            Values = new double[] { 1, 2, 3, 4, 5 },
            Name = "Series #1",
            DataPadding = new LvcPoint(0, 0)
        };

        var tooltip = new SKDefaultTooltip { Easing = null };

        var chart = new SKCartesianChart
        {
            Width = 300,
            Height = 300,
            Tooltip = tooltip,
            TooltipPosition = TooltipPosition.Top,
            Series = new[] { sutSeries },
            XAxes = new[] { new Axis { IsVisible = false } },
            YAxes = new[] { new Axis { IsVisible = false } },
            ExplicitDisposing = true
        };

        chart.CoreChart._isPointerIn = true;
        chart.CoreChart._isToolTipOpen = true;
        chart.CoreChart._pointerPosition = new(150, 150);

        chart.TooltipPosition = TooltipPosition.Top;
        _ = chart.GetImage();

        LvcRectangle tp;
        void UpdateTooltipRect()
        {
            var g = tooltip;
            tp = new LvcRectangle(new(g.X, g.Y), tooltip.Measure());
        }

        UpdateTooltipRect();
        Assert.IsTrue(
            Math.Abs(tp.X + tp.Width * 0.5f - 150) < 0.1 &&
            Math.Abs(tp.Y - (150 - tp.Height)) < 0.1,
            "Tool tip on top failed");

        chart.TooltipPosition = TooltipPosition.Bottom;
        _ = chart.GetImage();
        UpdateTooltipRect();
        Assert.IsTrue(
            Math.Abs(tp.X + tp.Width * 0.5f - 150) < 0.1 &&
            Math.Abs(tp.Y - 150) < 0.1,
            "Tool tip on bottom failed");

        chart.TooltipPosition = TooltipPosition.Left;
        _ = chart.GetImage();
        UpdateTooltipRect();
        Assert.IsTrue(
            Math.Abs(tp.X - (150 - tp.Width)) < 0.1 &&
            Math.Abs(tp.Y + tp.Height * 0.5f - 150) < 0.1,
            "Tool tip on left failed");

        chart.TooltipPosition = TooltipPosition.Right;
        _ = chart.GetImage();
        UpdateTooltipRect();
        Assert.IsTrue(
            Math.Abs(tp.X - 150) < 0.1 &&
            Math.Abs(tp.Y + tp.Height * 0.5f - 150) < 0.1,
            "Tool tip on right failed");

        chart.TooltipPosition = TooltipPosition.Center;
        _ = chart.GetImage();
        UpdateTooltipRect();
        Assert.IsTrue(
            Math.Abs(tp.X + tp.Width * 0.5f - 150) < 0.1 &&
            Math.Abs(tp.Y + tp.Height * 0.5f - 150) < 0.1,
            "Tool tip on center failed");

        chart.TooltipPosition = TooltipPosition.Auto;
        _ = chart.GetImage();
        UpdateTooltipRect();
        Assert.IsTrue(
            Math.Abs(tp.X + tp.Width * 0.5f - 150) < 0.1 &&
            Math.Abs(tp.Y - (150 - tp.Height)) < 0.1 &&
            chart.CoreChart.AutoToolTipsInfo.ToolTipPlacement == PopUpPlacement.Top,
            "Tool tip on top failed [AUTO]");

        sutSeries.Values = new double[] { -1, -2, -3, -4, -5 };
        _ = chart.GetImage();
        UpdateTooltipRect();
        Assert.IsTrue(
            Math.Abs(tp.X + tp.Width * 0.5f - 150) < 0.1 &&
            Math.Abs(tp.Y - 150) < 0.1 &&
            chart.CoreChart.AutoToolTipsInfo.ToolTipPlacement == PopUpPlacement.Bottom,
            "Tool tip on bottom failed [AUTO]");

        sutSeries.Values = new double[] { 1, 2, 3, 4, 5 };
        chart.CoreChart._pointerPosition = new(299, 150);
        _ = chart.GetImage();
        UpdateTooltipRect();
        Assert.IsTrue(
            Math.Abs(tp.X - (300 - 300 * (1 / 5d) * 0.5 - tp.Width)) < 0.0001 &&
            //Math.Abs(tp.Y - -tp.Height * 0.5f) < 0.1 &&
            chart.CoreChart.AutoToolTipsInfo.ToolTipPlacement == PopUpPlacement.Left,
            "Tool tip on left failed [AUTO]");

        chart.CoreChart._pointerPosition = new(1, 150);
        _ = chart.GetImage();
        UpdateTooltipRect();
        Assert.IsTrue(
            Math.Abs(tp.X - 300 * (1 / 5d) * 0.5) < 0.0001 &&
            //Math.Abs(tp.Y - (300 - tp.Height * 0.5f)) < 0.1 &&
            chart.CoreChart.AutoToolTipsInfo.ToolTipPlacement == PopUpPlacement.Right,
            "Tool tip on right failed [AUTO]");
    }

    [TestMethod]
    public void ShouldPlaceDataLabel()
    {
        var sutSeries = new ColumnSeries<double, RectangleGeometry, TestLabel>
        {
            Values = new double[] { -10, -5, -1, 0, 1, 5, 10 },
            DataPadding = new LvcPoint(0, 0),
        };

        var chart = new SKCartesianChart
        {
            Width = 500,
            Height = 500,
            DrawMargin = new Margin(100),
            DrawMarginFrame = new DrawMarginFrame { Stroke = new SolidColorPaint(SKColors.Yellow, 2) },
            TooltipPosition = TooltipPosition.Top,
            Series = new[] { sutSeries },
            XAxes = new[] { new Axis { IsVisible = false } },
            YAxes = new[] { new Axis { IsVisible = false } }
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
                Math.Abs(v.X + v.Width * 0.5f - l.X) < 0.01 &&    // x is centered
                Math.Abs(v.Y - (l.Y + ls.Height * 0.5)) < 0.01);  // y is top
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
                Math.Abs(v.X + v.Width * 0.5f - l.X) < 0.01 &&              // x is centered
                Math.Abs(v.Y + v.Height - (l.Y - ls.Height * 0.5)) < 0.01); // y is bottom
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
                Math.Abs(v.X + v.Width - (l.X - ls.Width * 0.5)) < 0.01 &&  // x is right
                Math.Abs(v.Y + v.Height * 0.5 - l.Y) < 0.01);               // y is centered
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
                Math.Abs(v.X - (l.X + ls.Width * 0.5f)) < 0.01 &&   // x is left
                Math.Abs(v.Y + v.Height * 0.5f - l.Y) < 0.01);      // y is centered
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
                Math.Abs(v.X + v.Width * 0.5f - l.X) < 0.01 &&      // x is centered
                Math.Abs(v.Y + v.Height * 0.5f - l.Y) < 0.01);      // y is centered
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
                    Math.Abs(v.X + v.Width * 0.5f - l.X) < 0.01 &&    // x is centered
                    Math.Abs(v.Y - (l.Y + ls.Height * 0.5)) < 0.01);  // y is top
            }
            else
            {
                // it should be placed using the bottom position
                Assert.IsTrue(
                    Math.Abs(v.X + v.Width * 0.5f - l.X) < 0.01 &&              // x is centered
                    Math.Abs(v.Y + v.Height - (l.Y - ls.Height * 0.5)) < 0.01); // y is bottom
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
                    Math.Abs(v.X + v.Width * 0.5f - l.X) < 0.01 &&              // x is centered
                    Math.Abs(v.Y + v.Height - (l.Y - ls.Height * 0.5)) < 0.01); // y is bottom
            }
            else
            {
                // it should be placed using the top position
                Assert.IsTrue(
                    Math.Abs(v.X + v.Width * 0.5f - l.X) < 0.01 &&    // x is centered
                    Math.Abs(v.Y - (l.Y + ls.Height * 0.5)) < 0.01);  // y is top
            }
        }

        // FINALLY IF LABELS ARE NULL, IT SHOULD REMOVE THE CURRENT LABELS.
        var previousPaint = sutSeries.DataLabelsPaint;
        sutSeries.DataLabelsPaint = null;
        _ = chart.GetImage();

        Assert.IsTrue(!chart.CoreCanvas.ContainsPaintTask(previousPaint));
    }
}
