using System;
using System.Linq;
using CoreTests.MockedObjects;
using LiveChartsCore.Defaults;
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
public class BoxSeriesTest
{
    [TestMethod]
    public void ShouldScale()
    {
        var sutSeries = new BoxSeries<BoxValue>
        {
            Values = new BoxValue[]
            {
                new (1, 0.75, 0.25, 0, 0.5),
                new (64, 32, 64, 32, 40),
                new (512, 511.75, 511.25, 511, 511.5),
            }
        };

        var chart = new SKCartesianChart
        {
            Width = 1000,
            Height = 1000,
            Series = new[] { sutSeries },
            XAxes = new[] { new Axis() },
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
        var unitH = typedUnit.Visual.Min - typedUnit.Visual.Y;
        Assert.IsTrue(typedUnit.Visual.Width > 1 && unitH > 1);

        var previous = typedUnit;
        float? previousX = null;

        foreach (var sutPoint in toCompareGuys)
        {
            // test height
            var h = sutPoint.Visual.Min - sutPoint.Visual.Y;
            var sutH = sutPoint.Visual.Min - sutPoint.Visual.Y;
            var sutDH = sutPoint.Model.Max - sutPoint.Model.Min;
            Assert.IsTrue(Math.Abs(unitH - sutH / (float)sutDH) < 0.001);

            // test width
            Assert.IsTrue(Math.Abs(typedUnit.Visual.Width - sutPoint.Visual.Width) < 0.001);

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
        var sutSeries = new BoxSeries<BoxValue>
        {
            Values = new BoxValue[]
            {
                new(1,0.75,0.25,0, 0.5),
                new(2,1.75,1.25,1, 1.5),
                new(3,2.75,2.25,2, 2.5),
                new(4,3.75,3.25,3, 3.5),
                new(5,4.75,4.25,4, 4.5),
            },
            Name = "A",
            MaxBarWidth = 1000,
            YToolTipLabelFormatter = x => $"{x.Coordinate.PrimaryValue}{Environment.NewLine}{x.Coordinate.PrimaryValue}{Environment.NewLine}{x.Coordinate.PrimaryValue}{Environment.NewLine}",
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
            Math.Abs(tp.Y - (150 - tp.Height - 1 / 5d * 300 * 0.5d)) < 0.1,
            "Tool tip on top failed");

        chart.TooltipPosition = TooltipPosition.Bottom;
        _ = chart.GetImage();
        UpdateTooltipRect();
        Assert.IsTrue(
            Math.Abs(tp.X + tp.Width * 0.5f - 150) < 0.1 &&
            Math.Abs(tp.Y - (150 + 1 / 5d * 300 * 0.5d)) < 0.1,
            "Tool tip on bottom failed");

        chart.TooltipPosition = TooltipPosition.Left;
        _ = chart.GetImage();
        UpdateTooltipRect();
        Assert.IsTrue(
            Math.Abs(tp.X - (150 - tp.Width - 1 / 5d * 300 * 0.5d)) < 0.1 &&
            Math.Abs(tp.Y + tp.Height * 0.5f - 150) < 0.1,
            "Tool tip on left failed");

        chart.TooltipPosition = TooltipPosition.Right;
        _ = chart.GetImage();
        UpdateTooltipRect();
        Assert.IsTrue(
            Math.Abs(tp.X - 150 - 1 / 5d * 300 * 0.5d) < 0.1 &&
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
            Math.Abs(tp.Y - (150 - tp.Height - 1 / 5d * 300 * 0.5d)) < 0.1 &&
            chart.CoreChart.AutoToolTipsInfo.ToolTipPlacement == PopUpPlacement.Top,
            "Tool tip on top failed [AUTO]");

        chart.CoreChart._pointerPosition = new(300 * 4 / 5d - 10, 300 * 1 / 5d + 10);
        _ = chart.GetImage();
        UpdateTooltipRect();
        Assert.IsTrue(
            Math.Abs(tp.X + tp.Width * 0.5f - 1 / 5d * 300 - 150) < 0.1 &&
            Math.Abs(tp.Y - 300 * 1 / 5d) < 0.1 &&
            chart.CoreChart.AutoToolTipsInfo.ToolTipPlacement == PopUpPlacement.Bottom,
            "Tool tip on bottom failed [AUTO]");

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
            //Math.Abs(tp.Y - (300 - tp.Height * 0.5f - 300 * (1 / 5d))) < 0.1 &&
            chart.CoreChart.AutoToolTipsInfo.ToolTipPlacement == PopUpPlacement.Right,
            "Tool tip on left failed [AUTO]");
    }

    [TestMethod]
    public void ShouldPlaceDataLabel()
    {
        var sutSeries = new BoxSeries<BoxValue, BoxGeometry, TestLabel>
        {
            Values = new BoxValue[]
            {
                new(-9, -9.25, -9.75, -10, -9.5),
                new(1, .75, 0.25, 0, 0.5),
                new(10, 9.75, 9.25, 9, 9.5)
            },
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
            var h = Math.Abs(p.Visual.Y - p.Visual.Min);

            l.Paint = sutSeries.DataLabelsPaint;
            var ls = l.Measure();

            Assert.IsTrue(
                Math.Abs(v.X + v.Width * 0.5f - l.X) < 0.01 &&       // x is centered
                Math.Abs(v.Y + h - (l.Y - ls.Height * 0.5)) < 0.01); // y is bottom
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
            var h = Math.Abs(p.Visual.Y - p.Visual.Min);

            l.Paint = sutSeries.DataLabelsPaint;
            var ls = l.Measure();

            Assert.IsTrue(
                Math.Abs(v.X + v.Width - (l.X - ls.Width * 0.5)) < 0.01 &&  // x is right
                Math.Abs(v.Y + h * 0.5 - l.Y) < 0.01);                      // y is centered
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
            var h = Math.Abs(p.Visual.Y - p.Visual.Min);

            l.Paint = sutSeries.DataLabelsPaint;
            var ls = l.Measure();

            Assert.IsTrue(
                Math.Abs(v.X - (l.X + ls.Width * 0.5f)) < 0.01 &&   // x is left
                Math.Abs(v.Y + h * 0.5f - l.Y) < 0.01);             // y is centered
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
            var h = Math.Abs(p.Visual.Y - p.Visual.Min);

            l.Paint = sutSeries.DataLabelsPaint;
            var ls = l.Measure();

            Assert.IsTrue(
                Math.Abs(v.X + v.Width * 0.5f - l.X) < 0.01 &&      // x is centered
                Math.Abs(v.Y + h * 0.5f - l.Y) < 0.01);             // y is centered
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
            var h = Math.Abs(p.Visual.Y - p.Visual.Min);

            l.Paint = sutSeries.DataLabelsPaint;
            var ls = l.Measure();

            if (p.Model.Max <= 0)
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
                    Math.Abs(v.X + v.Width * 0.5f - l.X) < 0.01 &&       // x is centered
                    Math.Abs(v.Y + h - (l.Y - ls.Height * 0.5)) < 0.01); // y is bottom
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
            var h = Math.Abs(p.Visual.Y - p.Visual.Min);

            l.Paint = sutSeries.DataLabelsPaint;
            var ls = l.Measure();

            if (p.Model.Max <= 0)
            {
                // it should be placed using the bottom position
                Assert.IsTrue(
                    Math.Abs(v.X + v.Width * 0.5f - l.X) < 0.01 &&       // x is centered
                    Math.Abs(v.Y + h - (l.Y - ls.Height * 0.5)) < 0.01); // y is bottom
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
