using System;
using System.Collections.ObjectModel;
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
public class HeatSeriesTest
{
    [TestMethod]
    public void ShouldScale()
    {
        var sutSeries = new HeatSeries<WeightedPoint>
        {
            Values = new WeightedPoint[]
            {
                new(0, 0, 0),
                new(0, 1, 1),
                new(0, 2, 2),
                new(0, 3, 3),
                new(0, 4, 4),
                new(0, 5, 5),
                new(0, 5, 6),
                new(0, 5, 7),
                new(0, 5, 8),
                new(0, 5, 9),
                new(0, 5, 10),

                new(1, 0, 0),
                new(1, 1, 1),
                new(1, 2, 2),
                new(1, 3, 3),
                new(1, 4, 4),
                new(1, 5, 5),
                new(1, 5, 6),
                new(1, 5, 7),
                new(1, 5, 8),
                new(1, 5, 9),
                new(1, 5, 10)
            },
            HeatMap = new[]
            {
                new LvcColor(0, 0, 0, 0), // the first element is the "coldest"
                new LvcColor(255, 255, 255, 255) // the last element is the "hottest"
            }
        };

        var chart = new SKCartesianChart
        {
            Width = 1000,
            Height = 1000,
            Series = new[] { sutSeries },
            XAxes = new[] { new Axis() },
            YAxes = new[] { new Axis() }
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
                Math.Abs(typedUnit.Visual.Height - sutPoint.Visual.Height) < 0.001);

            // test width
            Assert.IsTrue(
                Math.Abs(typedUnit.Visual.Width - sutPoint.Visual.Width) < 0.001);

            // test gradient
            var p = (byte)(255 * sutPoint.Model.Weight / 10f);
            Assert.IsTrue(
                sutPoint.Visual.Color.R == p &&
                sutPoint.Visual.Color.G == p &&
                sutPoint.Visual.Color.B == p &&
                sutPoint.Visual.Color.A == p);

            previousX = previous.Visual.X - sutPoint.Visual.X;
            previous = sutPoint;
        }
    }

    [TestMethod]
    public void ShouldPlaceToolTips()
    {
        var sutSeries = new HeatSeries<WeightedPoint>
        {
            HeatMap = new[]
            {
                new SKColor(255, 241, 118).AsLvcColor(),
                SKColors.DarkSlateGray.AsLvcColor(),
                SKColors.Blue.AsLvcColor()
            },
            Values = new ObservableCollection<WeightedPoint>
            {
                // Charles
                new(0, 0, 150), // Jan
                new(0, 1, 123), // Feb
                new(0, 2, 310), // Mar
                new(0, 3, 225), // Apr
                new(0, 4, 473), // May

                // Richard
                new(1, 0, 432), // Jan
                new(1, 1, 312), // Feb
                new(1, 2, 135), // Mar
                new(1, 3, 78), // Apr
                new(1, 4, 124), // May

                // Ana
                new(2, 0, 543), // Jan
                new(2, 1, 134), // Feb
                new(2, 2, 524), // Mar
                new(2, 3, 315), // Apr
                new(2, 4, 145), // May

                // Mari
                new(3, 0, 90), // Jan
                new(3, 1, 123), // Feb
                new(3, 2, 70), // Mar
                new(3, 3, 123), // Apr
                new(3, 4, 432), // May

                // Mari
                new(4, 0, 90), // Jan
                new(4, 1, 123), // Feb
                new(4, 2, 70), // Mar
                new(4, 3, 123), // Apr
                new(4, 4, 432), // May
            },
        };

        var tooltip = new SKDefaultTooltip { Easing = null }; ;

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

        chart.CoreChart._pointerPosition = new(150, 10);
        _ = chart.GetImage();
        UpdateTooltipRect();
        Assert.IsTrue(
            Math.Abs(tp.X - (150 - tp.Width * 0.5f)) < 0.1 &&
            Math.Abs(tp.Y - 300 * 1 / 5d * 0.5f) < 0.1 &&
            chart.CoreChart.AutoToolTipsInfo.ToolTipPlacement == PopUpPlacement.Bottom,
            "Tool tip on bottom failed [AUTO]");

        chart.CoreChart._pointerPosition = new(295, 5);
        _ = chart.GetImage();
        UpdateTooltipRect();
        Assert.IsTrue(
            Math.Abs(tp.X - (300 - 300 * 1 / 5d * 0.5 - tp.Width)) < 0.1 &&
            Math.Abs(tp.Y - -(tp.Height * 0.5f - 300 * 1 / 5d * 0.5)) < 0.1 &&
            chart.CoreChart.AutoToolTipsInfo.ToolTipPlacement == PopUpPlacement.Left,
            "Tool tip on left failed [AUTO]");

        chart.CoreChart._pointerPosition = new(5, 295);
        _ = chart.GetImage();
        UpdateTooltipRect();
        Assert.IsTrue(
            Math.Abs(tp.X - 300 * 1 / 5d * 0.5) < 0.1 &&
            Math.Abs(tp.Y - (300 - tp.Height * 0.5f - 300 * 1 / 5d * 0.5)) < 0.1 &&
            chart.CoreChart.AutoToolTipsInfo.ToolTipPlacement == PopUpPlacement.Right,
            "Tool tip on left failed [AUTO]");
    }

    [TestMethod]
    public void ShouldPlaceDataLabel()
    {
        var sutSeries = new HeatSeries<WeightedPoint, ColoredRectangleGeometry, TestLabel>
        {
            Values = new WeightedPoint[]
            {
                new(0, 0, 0), new(1, 0, 1), new(2, 0, 2),
                new(0, 1, 0), new(1, 1, 1), new(2, 1, 2),
                new(0, 2, 0), new(1, 2, 1), new(2, 2, 2),
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

        Assert.IsTrue(sutSeries.DataLabelsPosition == DataLabelsPosition.Middle);
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
        chart.SaveImage("test.png");

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
        // what here?

        // TEST END ===========================================================
        sutSeries.DataLabelsPosition = DataLabelsPosition.End;

        _ = chart.GetImage();
        // what here?

        // FINALLY IF LABELS ARE NULL, IT SHOULD REMOVE THE CURRENT LABELS.
        var previousPaint = sutSeries.DataLabelsPaint;
        sutSeries.DataLabelsPaint = null;
        _ = chart.GetImage();

        Assert.IsTrue(!chart.CoreCanvas.ContainsPaintTask(previousPaint));
    }
}
