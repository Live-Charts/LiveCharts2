using System;
using LiveChartsCore.Drawing;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView.VisualElements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.OtherTests;

[TestClass]
public class DrawMarginAndLegendsTests
{
    [TestMethod]
    public void CartesianBasicCase()
    {
        var legend = new SKDefaultLegend();

        var chart = new SKCartesianChart
        {
            Width = 500,
            Height = 500,
            Series = new[] { new LineSeries<double> { Values = new double[] { 1, 2, 3 } } },
            XAxes = new[] { new Axis { IsVisible = false, } },
            YAxes = new[] { new Axis { IsVisible = false, } },
            DrawMarginFrame = new DrawMarginFrame() { Stroke = new SolidColorPaint(SKColors.Yellow, 5) },
            LegendPosition = LegendPosition.Top,
            Legend = legend,
            ExplicitDisposing = true
        };

        // TEST TOP LEGEND ================================================
        chart.LegendPosition = LegendPosition.Top;
        _ = chart.GetImage();

        var legendSize = legend.Measure(chart.CoreChart);
        var dml = chart.CoreChart.DrawMarginLocation;
        var dms = chart.CoreChart.DrawMarginSize;
        var x = legend.X;
        var y = legend.Y;

        // all the width should be used
        Assert.IsTrue(Math.Abs(500 - dms.Width) < 0.01);

        // all the height should be used
        Assert.IsTrue(Math.Abs(500 - dms.Height - legendSize.Height) < 0.01);

        // x should be centered
        Assert.IsTrue(Math.Abs(x - (500 * 0.5f - legendSize.Width * 0.5f)) < 0.01);

        // y should be 0
        Assert.IsTrue(Math.Abs(y - 0) < 0.01);

        // TEST BOTTOM LEGEND ================================================
        chart.LegendPosition = LegendPosition.Bottom;
        _ = chart.GetImage();

        legendSize = legend.Measure(chart.CoreChart);
        dml = chart.CoreChart.DrawMarginLocation;
        dms = chart.CoreChart.DrawMarginSize;
        x = legend.X;
        y = legend.Y;

        // all the width should be used
        Assert.IsTrue(Math.Abs(500 - dms.Width) < 0.01);

        // all the height should be used
        Assert.IsTrue(Math.Abs(500 - dms.Height - legendSize.Height) < 0.01);

        // x should be centered
        Assert.IsTrue(Math.Abs(x - (500 * 0.5f - legendSize.Width * 0.5f)) < 0.01);

        // y should be 500 - legendsSize.Height
        Assert.IsTrue(Math.Abs(y - (500 - legendSize.Height)) < 0.01);

        // TEST LEFT LEGEND ================================================
        chart.LegendPosition = LegendPosition.Left;
        _ = chart.GetImage();

        legendSize = legend.Measure(chart.CoreChart);
        dml = chart.CoreChart.DrawMarginLocation;
        dms = chart.CoreChart.DrawMarginSize;
        x = legend.X;
        y = legend.Y;

        // all the width should be used
        Assert.IsTrue(Math.Abs(500 - dms.Width - legendSize.Width) < 0.01);

        // all the height should be used
        Assert.IsTrue(Math.Abs(500 - dms.Height) < 0.01);

        // x should be 0
        Assert.IsTrue(Math.Abs(x) < 0.01);

        // y should be centered
        Assert.IsTrue(Math.Abs(y - (500 * 0.5f - legendSize.Height * 0.5f)) < 0.01);

        // TEST RIGHT LEGEND ================================================
        chart.LegendPosition = LegendPosition.Right;
        _ = chart.GetImage();

        legendSize = legend.Measure(chart.CoreChart);
        dml = chart.CoreChart.DrawMarginLocation;
        dms = chart.CoreChart.DrawMarginSize;
        x = legend.X;
        y = legend.Y;

        // all the width should be used
        Assert.IsTrue(Math.Abs(500 - dms.Width - legendSize.Width) < 0.01);

        // all the height should be used
        Assert.IsTrue(Math.Abs(500 - dms.Height) < 0.01);

        // x should be 500 - legendsSize.Width
        Assert.IsTrue(Math.Abs(x - (500 - legendSize.Width)) < 0.01);

        // y should be centered
        Assert.IsTrue(Math.Abs(y - (500 * 0.5f - legendSize.Height * 0.5f)) < 0.01);
    }

    [TestMethod]
    public void CartesianWithTitleCase()
    {
        var legend = new SKDefaultLegend();
        legend.Geometry.Fill = new SolidColorPaint(SKColors.LightGray);

        var title = new LabelVisual
        {
            Text = "My chart title",
            TextSize = 25,
            Padding = new Padding(10),
            Paint = new SolidColorPaint(SKColors.White),
            BackgroundColor = SKColors.Purple.AsLvcColor()
        };

        var chart = new SKCartesianChart
        {
            Width = 500,
            Height = 500,
            Series = new[] { new LineSeries<double> { Values = new double[] { 1, 2, 3 } } },
            XAxes = new[] { new Axis { IsVisible = false, } },
            YAxes = new[] { new Axis { IsVisible = false, } },
            DrawMarginFrame = new DrawMarginFrame() { Stroke = new SolidColorPaint(SKColors.Yellow, 5) },
            LegendPosition = LegendPosition.Top,
            Title = title,
            Legend = legend,
            ExplicitDisposing = true
        };

        // TEST TOP LEGEND ================================================
        chart.LegendPosition = LegendPosition.Top;
        _ = chart.GetImage();

        var legendSize = legend.Measure(chart.CoreChart);
        var titleSize = title.Measure(chart.CoreChart);
        var dml = chart.CoreChart.DrawMarginLocation;
        var dms = chart.CoreChart.DrawMarginSize;
        var x = legend.X;
        var y = legend.Y;

        // all the width should be used
        Assert.IsTrue(Math.Abs(500 - dms.Width) < 0.01);

        // all the height should be used
        Assert.IsTrue(Math.Abs(500 - dms.Height - legendSize.Height - titleSize.Height) < 0.01);

        // x should be centered
        Assert.IsTrue(Math.Abs(x - (500 * 0.5f - legendSize.Width * 0.5f)) < 0.01);

        // y should be titleSize.Height
        Assert.IsTrue(Math.Abs(y - titleSize.Height) < 0.01);

        // TEST BOTTOM LEGEND ================================================
        chart.LegendPosition = LegendPosition.Bottom;
        _ = chart.GetImage();

        legendSize = legend.Measure(chart.CoreChart);
        dml = chart.CoreChart.DrawMarginLocation;
        dms = chart.CoreChart.DrawMarginSize;
        x = legend.X;
        y = legend.Y;

        // all the width should be used
        Assert.IsTrue(Math.Abs(500 - dms.Width) < 0.01);

        // all the height should be used
        Assert.IsTrue(Math.Abs(500 - dms.Height - legendSize.Height - titleSize.Height) < 0.01);

        // x should be centered
        Assert.IsTrue(Math.Abs(x - (500 * 0.5f - legendSize.Width * 0.5f)) < 0.01);

        // y should be 500 - legendsSize.Height
        Assert.IsTrue(Math.Abs(y - (500 - legendSize.Height)) < 0.01);

        // TEST LEFT LEGEND ================================================
        chart.LegendPosition = LegendPosition.Left;
        _ = chart.GetImage();

        legendSize = legend.Measure(chart.CoreChart);
        dml = chart.CoreChart.DrawMarginLocation;
        dms = chart.CoreChart.DrawMarginSize;
        x = legend.X;
        y = legend.Y;

        // all the width should be used
        Assert.IsTrue(Math.Abs(500 - dms.Width - legendSize.Width) < 0.01);

        // all the height should be used
        Assert.IsTrue(Math.Abs(500 - dms.Height - titleSize.Height) < 0.01);

        // x should be 0
        Assert.IsTrue(Math.Abs(x) < 0.01);

        // y should be centered
        Assert.IsTrue(Math.Abs(y - (500 * 0.5f - legendSize.Height * 0.5f)) < 0.01);

        // TEST RIGHT LEGEND ================================================
        chart.LegendPosition = LegendPosition.Right;
        _ = chart.GetImage();

        legendSize = legend.Measure(chart.CoreChart);
        dml = chart.CoreChart.DrawMarginLocation;
        dms = chart.CoreChart.DrawMarginSize;
        x = legend.X;
        y = legend.Y;

        // all the width should be used
        Assert.IsTrue(Math.Abs(500 - dms.Width - legendSize.Width) < 0.01);

        // all the height should be used
        Assert.IsTrue(Math.Abs(500 - dms.Height - titleSize.Height) < 0.01);

        // x should be 500 - legendsSize.Width
        Assert.IsTrue(Math.Abs(x - (500 - legendSize.Width)) < 0.01);

        // y should be centered
        Assert.IsTrue(Math.Abs(y - (500 * 0.5f - legendSize.Height * 0.5f)) < 0.01);
    }

    [TestMethod]
    public void PieBasicCase()
    {
        var legend = new SKDefaultLegend();

        var chart = new SKPieChart
        {
            Width = 500,
            Height = 500,
            Series = new[] { 1, 2, 3 }.AsPieSeries(),
            LegendPosition = LegendPosition.Top,
            Legend = legend,
            ExplicitDisposing = true
        };

        // TEST TOP LEGEND ================================================
        chart.LegendPosition = LegendPosition.Top;
        _ = chart.GetImage();

        var legendSize = legend.Measure(chart.CoreChart);
        var dml = chart.CoreChart.DrawMarginLocation;
        var dms = chart.CoreChart.DrawMarginSize;
        var x = legend.X;
        var y = legend.Y;

        // all the width should be used
        Assert.IsTrue(Math.Abs(500 - dms.Width) < 0.01);

        // all the height should be used
        Assert.IsTrue(Math.Abs(500 - dms.Height - legendSize.Height) < 0.01);

        // x should be centered
        Assert.IsTrue(Math.Abs(x - (500 * 0.5f - legendSize.Width * 0.5f)) < 0.01);

        // y should be 0
        Assert.IsTrue(Math.Abs(y - 0) < 0.01);

        // TEST BOTTOM LEGEND ================================================
        chart.LegendPosition = LegendPosition.Bottom;
        _ = chart.GetImage();

        legendSize = legend.Measure(chart.CoreChart);
        dml = chart.CoreChart.DrawMarginLocation;
        dms = chart.CoreChart.DrawMarginSize;
        x = legend.X;
        y = legend.Y;

        // all the width should be used
        Assert.IsTrue(Math.Abs(500 - dms.Width) < 0.01);

        // all the height should be used
        Assert.IsTrue(Math.Abs(500 - dms.Height - legendSize.Height) < 0.01);

        // x should be centered
        Assert.IsTrue(Math.Abs(x - (500 * 0.5f - legendSize.Width * 0.5f)) < 0.01);

        // y should be 500 - legendsSize.Height
        Assert.IsTrue(Math.Abs(y - (500 - legendSize.Height)) < 0.01);

        // TEST LEFT LEGEND ================================================
        chart.LegendPosition = LegendPosition.Left;
        _ = chart.GetImage();

        legendSize = legend.Measure(chart.CoreChart);
        dml = chart.CoreChart.DrawMarginLocation;
        dms = chart.CoreChart.DrawMarginSize;
        x = legend.X;
        y = legend.Y;

        // all the width should be used
        Assert.IsTrue(Math.Abs(500 - dms.Width - legendSize.Width) < 0.01);

        // all the height should be used
        Assert.IsTrue(Math.Abs(500 - dms.Height) < 0.01);

        // x should be 0
        Assert.IsTrue(Math.Abs(x) < 0.01);

        // y should be centered
        Assert.IsTrue(Math.Abs(y - (500 * 0.5f - legendSize.Height * 0.5f)) < 0.01);

        // TEST RIGHT LEGEND ================================================
        chart.LegendPosition = LegendPosition.Right;
        _ = chart.GetImage();

        legendSize = legend.Measure(chart.CoreChart);
        dml = chart.CoreChart.DrawMarginLocation;
        dms = chart.CoreChart.DrawMarginSize;
        x = legend.X;
        y = legend.Y;

        // all the width should be used
        Assert.IsTrue(Math.Abs(500 - dms.Width - legendSize.Width) < 0.01);

        // all the height should be used
        Assert.IsTrue(Math.Abs(500 - dms.Height) < 0.01);

        // x should be 500 - legendsSize.Width
        Assert.IsTrue(Math.Abs(x - (500 - legendSize.Width)) < 0.01);

        // y should be centered
        Assert.IsTrue(Math.Abs(y - (500 * 0.5f - legendSize.Height * 0.5f)) < 0.01);
    }

    [TestMethod]
    public void PieWithTitleCase()
    {
        var legend = new SKDefaultLegend();
        legend.Geometry.Fill = new SolidColorPaint(SKColors.LightGray);

        var title = new LabelVisual
        {
            Text = "My chart title",
            TextSize = 25,
            Padding = new Padding(10),
            Paint = new SolidColorPaint(SKColors.White),
            BackgroundColor = SKColors.Purple.AsLvcColor()
        };

        var chart = new SKPieChart
        {
            Width = 500,
            Height = 500,
            Series = new[] { 1, 2, 3 }.AsPieSeries(),
            LegendPosition = LegendPosition.Top,
            Title = title,
            Legend = legend,
            ExplicitDisposing = true
        };

        // TEST TOP LEGEND ================================================
        chart.LegendPosition = LegendPosition.Top;
        _ = chart.GetImage();

        var legendSize = legend.Measure(chart.CoreChart);
        var titleSize = title.Measure(chart.CoreChart);
        var dml = chart.CoreChart.DrawMarginLocation;
        var dms = chart.CoreChart.DrawMarginSize;
        var x = legend.X;
        var y = legend.Y;

        // all the width should be used
        Assert.IsTrue(Math.Abs(500 - dms.Width) < 0.01);

        // all the height should be used
        Assert.IsTrue(Math.Abs(500 - dms.Height - legendSize.Height - titleSize.Height) < 0.01);

        // x should be centered
        Assert.IsTrue(Math.Abs(x - (500 * 0.5f - legendSize.Width * 0.5f)) < 0.01);

        // y should be titleSize.Height
        Assert.IsTrue(Math.Abs(y - titleSize.Height) < 0.01);

        // TEST BOTTOM LEGEND ================================================
        chart.LegendPosition = LegendPosition.Bottom;
        _ = chart.GetImage();

        legendSize = legend.Measure(chart.CoreChart);
        dml = chart.CoreChart.DrawMarginLocation;
        dms = chart.CoreChart.DrawMarginSize;
        x = legend.X;
        y = legend.Y;

        // all the width should be used
        Assert.IsTrue(Math.Abs(500 - dms.Width) < 0.01);

        // all the height should be used
        Assert.IsTrue(Math.Abs(500 - dms.Height - legendSize.Height - titleSize.Height) < 0.01);

        // x should be centered
        Assert.IsTrue(Math.Abs(x - (500 * 0.5f - legendSize.Width * 0.5f)) < 0.01);

        // y should be 500 - legendsSize.Height
        Assert.IsTrue(Math.Abs(y - (500 - legendSize.Height)) < 0.01);

        // TEST LEFT LEGEND ================================================
        chart.LegendPosition = LegendPosition.Left;
        _ = chart.GetImage();

        legendSize = legend.Measure(chart.CoreChart);
        dml = chart.CoreChart.DrawMarginLocation;
        dms = chart.CoreChart.DrawMarginSize;
        x = legend.X;
        y = legend.Y;

        // all the width should be used
        Assert.IsTrue(Math.Abs(500 - dms.Width - legendSize.Width) < 0.01);

        // all the height should be used
        Assert.IsTrue(Math.Abs(500 - dms.Height - titleSize.Height) < 0.01);

        // x should be 0
        Assert.IsTrue(Math.Abs(x) < 0.01);

        // y should be centered
        Assert.IsTrue(Math.Abs(y - (500 * 0.5f - legendSize.Height * 0.5f)) < 0.01);

        // TEST RIGHT LEGEND ================================================
        chart.LegendPosition = LegendPosition.Right;
        _ = chart.GetImage();

        legendSize = legend.Measure(chart.CoreChart);
        dml = chart.CoreChart.DrawMarginLocation;
        dms = chart.CoreChart.DrawMarginSize;
        x = legend.X;
        y = legend.Y;

        // all the width should be used
        Assert.IsTrue(Math.Abs(500 - dms.Width - legendSize.Width) < 0.01);

        // all the height should be used
        Assert.IsTrue(Math.Abs(500 - dms.Height - titleSize.Height) < 0.01);

        // x should be 500 - legendsSize.Width
        Assert.IsTrue(Math.Abs(x - (500 - legendSize.Width)) < 0.01);

        // y should be centered
        Assert.IsTrue(Math.Abs(y - (500 * 0.5f - legendSize.Height * 0.5f)) < 0.01);
    }

    [TestMethod]
    public void PolarBasicCase()
    {
        var legend = new SKDefaultLegend();

        var chart = new SKPolarChart
        {
            Width = 500,
            Height = 500,
            Series = new[] { new PolarLineSeries<double> { Values = new double[] { 1, 2, 3 } } },
            LegendPosition = LegendPosition.Top,
            Legend = legend,
            ExplicitDisposing = true
        };

        // TEST TOP LEGEND ================================================
        chart.LegendPosition = LegendPosition.Top;
        _ = chart.GetImage();

        var legendSize = legend.Measure(chart.CoreChart);
        var dml = chart.CoreChart.DrawMarginLocation;
        var dms = chart.CoreChart.DrawMarginSize;
        var x = legend.X;
        var y = legend.Y;

        var m = Math.Min(dml.X, dml.Y) * 2;

        // all the width should be used
        Assert.IsTrue(Math.Abs(500 - dms.Width - m) < 0.01);

        // all the height should be used
        Assert.IsTrue(Math.Abs(500 - dms.Height - m - legendSize.Height) < 0.01);

        // x should be centered
        Assert.IsTrue(Math.Abs(x - (500 * 0.5f - legendSize.Width * 0.5f)) < 0.01);

        // y should be 0
        Assert.IsTrue(Math.Abs(y - 0) < 0.01);

        // TEST BOTTOM LEGEND ================================================
        chart.LegendPosition = LegendPosition.Bottom;
        _ = chart.GetImage();

        legendSize = legend.Measure(chart.CoreChart);
        dml = chart.CoreChart.DrawMarginLocation;
        dms = chart.CoreChart.DrawMarginSize;
        x = legend.X;
        y = legend.Y;

        m = Math.Min(dml.X, dml.Y) * 2;

        // all the width should be used
        Assert.IsTrue(Math.Abs(500 - dms.Width - m) < 0.01);

        // all the height should be used
        Assert.IsTrue(Math.Abs(500 - dms.Height - m - legendSize.Height) < 0.01);

        // x should be centered
        Assert.IsTrue(Math.Abs(x - (500 * 0.5f - legendSize.Width * 0.5f)) < 0.01);

        // y should be 500 - legendsSize.Height
        Assert.IsTrue(Math.Abs(y - (500 - legendSize.Height)) < 0.01);

        // TEST LEFT LEGEND ================================================
        chart.LegendPosition = LegendPosition.Left;
        _ = chart.GetImage();

        legendSize = legend.Measure(chart.CoreChart);
        dml = chart.CoreChart.DrawMarginLocation;
        dms = chart.CoreChart.DrawMarginSize;
        x = legend.X;
        y = legend.Y;

        m = Math.Min(dml.X, dml.Y) * 2;

        // all the width should be used
        Assert.IsTrue(Math.Abs(500 - dms.Width - m - legendSize.Width) < 0.01);

        // all the height should be used
        Assert.IsTrue(Math.Abs(500 - dms.Height - m) < 0.01);

        // x should be 0
        Assert.IsTrue(Math.Abs(x) < 0.01);

        // y should be centered
        Assert.IsTrue(Math.Abs(y - (500 * 0.5f - legendSize.Height * 0.5f)) < 0.01);

        // TEST RIGHT LEGEND ================================================
        chart.LegendPosition = LegendPosition.Right;
        _ = chart.GetImage();

        legendSize = legend.Measure(chart.CoreChart);
        dml = chart.CoreChart.DrawMarginLocation;
        dms = chart.CoreChart.DrawMarginSize;
        x = legend.X;
        y = legend.Y;

        m = Math.Min(dml.X, dml.Y) * 2;

        // all the width should be used
        Assert.IsTrue(Math.Abs(500 - dms.Width - m - legendSize.Width) < 0.01);

        // all the height should be used
        Assert.IsTrue(Math.Abs(500 - dms.Height - m) < 0.01);

        // x should be 500 - legendsSize.Width
        Assert.IsTrue(Math.Abs(x - (500 - legendSize.Width)) < 0.01);

        // y should be centered
        Assert.IsTrue(Math.Abs(y - (500 * 0.5f - legendSize.Height * 0.5f)) < 0.01);
    }

    [TestMethod]
    public void PolarWithTitleCase()
    {
        var legend = new SKDefaultLegend();
        legend.Geometry.Fill = new SolidColorPaint(SKColors.LightGray);

        var title = new LabelVisual
        {
            Text = "My chart title",
            TextSize = 25,
            Padding = new Padding(10),
            Paint = new SolidColorPaint(SKColors.White),
            BackgroundColor = SKColors.Purple.AsLvcColor()
        };

        var chart = new SKPolarChart
        {
            Width = 500,
            Height = 500,
            Series = new[] { new PolarLineSeries<double> { Values = new double[] { 1, 2, 3 } } },
            LegendPosition = LegendPosition.Top,
            Title = title,
            Legend = legend,
            ExplicitDisposing = true
        };

        // TEST TOP LEGEND ================================================
        chart.LegendPosition = LegendPosition.Top;
        _ = chart.GetImage();

        var legendSize = legend.Measure(chart.CoreChart);
        var titleSize = title.Measure(chart.CoreChart);
        var dml = chart.CoreChart.DrawMarginLocation;
        var dms = chart.CoreChart.DrawMarginSize;
        var x = legend.X;
        var y = legend.Y;

        var m = Math.Min(dml.X, dml.Y) * 2;

        // all the width should be used
        Assert.IsTrue(Math.Abs(500 - dms.Width - m) < 0.01);

        // all the height should be used
        Assert.IsTrue(Math.Abs(500 - dms.Height - m - legendSize.Height - titleSize.Height) < 0.01);

        // x should be centered
        Assert.IsTrue(Math.Abs(x - (500 * 0.5f - legendSize.Width * 0.5f)) < 0.01);

        // y should be titleSize.Height
        Assert.IsTrue(Math.Abs(y - titleSize.Height) < 0.01);

        // TEST BOTTOM LEGEND ================================================
        chart.LegendPosition = LegendPosition.Bottom;
        _ = chart.GetImage();

        legendSize = legend.Measure(chart.CoreChart);
        dml = chart.CoreChart.DrawMarginLocation;
        dms = chart.CoreChart.DrawMarginSize;
        x = legend.X;
        y = legend.Y;

        m = Math.Min(dml.X, dml.Y) * 2;

        // all the width should be used
        Assert.IsTrue(Math.Abs(500 - dms.Width - m) < 0.01);

        // all the height should be used
        Assert.IsTrue(Math.Abs(500 - dms.Height - m - legendSize.Height - titleSize.Height) < 0.01);

        // x should be centered
        Assert.IsTrue(Math.Abs(x - (500 * 0.5f - legendSize.Width * 0.5f)) < 0.01);

        // y should be 500 - legendsSize.Height
        Assert.IsTrue(Math.Abs(y - (500 - legendSize.Height)) < 0.01);

        // TEST LEFT LEGEND ================================================
        chart.LegendPosition = LegendPosition.Left;
        _ = chart.GetImage();

        legendSize = legend.Measure(chart.CoreChart);
        dml = chart.CoreChart.DrawMarginLocation;
        dms = chart.CoreChart.DrawMarginSize;
        x = legend.X;
        y = legend.Y;

        m = Math.Min(dml.X, dml.Y) * 2;
        var dx = dml.X - legendSize.Width; // <- why this distance?

        // all the width should be used
        Assert.IsTrue(Math.Abs(500 - dms.Width - dml.X - dx) < 0.01);

        // all the height should be used
        Assert.IsTrue(Math.Abs(500 - dms.Height - titleSize.Height - dx * 2) < 0.01);

        // x should be 0
        Assert.IsTrue(Math.Abs(x) < 0.01);

        // y should be centered
        Assert.IsTrue(Math.Abs(y - (500 * 0.5f - legendSize.Height * 0.5f)) < 0.01);

        // TEST RIGHT LEGEND ================================================
        chart.LegendPosition = LegendPosition.Right;
        _ = chart.GetImage();
        chart.SaveImage("mamá.png");

        legendSize = legend.Measure(chart.CoreChart);
        dml = chart.CoreChart.DrawMarginLocation;
        dms = chart.CoreChart.DrawMarginSize;
        x = legend.X;
        y = legend.Y;

        m = Math.Min(dml.X, dml.Y) * 2;
        dx = dml.X + legendSize.Width; // <- why this distance?

        // all the width should be used
        Assert.IsTrue(Math.Abs(500 - dms.Width - dml.X - dx) < 0.01);

        // all the height should be used
        //Assert.IsTrue(Math.Abs(500 - dms.Height - dml.Y) < 0.01);

        // x should be 500 - legendsSize.Width
        Assert.IsTrue(Math.Abs(x - (500 - legendSize.Width)) < 0.01);

        // y should be centered
        Assert.IsTrue(Math.Abs(y - (500 * 0.5f - legendSize.Height * 0.5f)) < 0.01);
    }
}
