using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsGeneratedCode;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.CoreObjectsTests;

[TestClass]
public class ChangingPaintTasks
{
    [TestMethod]
    public async Task EnsureChartIsMeasured()
    {
        var chart = new SKCartesianChart
        {
            Series = [new LineSeries<int>([1, 2, 3])]
        };

        var measureCount = 0;
        chart.Measuring += c => measureCount++;

        var frames = DrawChart(chart, false);

        await Task.Delay(1000);

        Assert.IsTrue(measureCount == 1 && frames == 1);
    }

    [TestMethod]
    public async Task EnsureChartIsAnimated()
    {
        var chart = new SKCartesianChart
        {
            AnimationsSpeed = TimeSpan.FromSeconds(1),
            EasingFunction = EasingFunctions.Lineal,
            Series = [new LineSeries<int>([1, 2, 3])],
            XAxes = [new Axis()],
            YAxes = [new Axis()],
        };

        var measureCount = 0;
        chart.Measuring += c => measureCount++;

        var frames = DrawChart(chart, true);

        await Task.Delay(1000);

        Assert.IsTrue(measureCount == 1 && frames > 1);
    }

    [TestMethod]
    public void DrawableSeriesFillChanged()
    {
        var series = new LineSeries<int>
        {
            Values = new List<int> { 1, 6, 4, 2 }
        };

        var chart = new SKCartesianChart
        {
            Series = new List<ISeries> { series },
            XAxes = new[] { new Axis() },
            YAxes = new[] { new Axis() },
        };

        var canvas = chart.CoreCanvas;

        _ = DrawChart(chart);

        var drawables = canvas.DrawablesCount;
        var geometries = canvas.CountGeometries();

        // on changing the fill task, the previous instance should be removed.
        series.Fill = new SolidColorPaint();

        _ = DrawChart(chart);

        Assert.IsTrue(
            drawables == canvas.DrawablesCount &&
            geometries == canvas.CountGeometries());
    }

    [TestMethod]
    public void DrawableSeriesStrokeChanged()
    {
        var series = new LineSeries<int>
        {
            Values = new List<int> { 1, 6, 4, 2 }
        };

        var chart = new SKCartesianChart
        {
            Series = new List<ISeries> { series },
            XAxes = new[] { new Axis() },
            YAxes = new[] { new Axis() },
        };

        var canvas = chart.CoreCanvas;

        _ = DrawChart(chart);

        var drawables = canvas.DrawablesCount;
        var geometries = canvas.CountGeometries();

        series.Stroke = new SolidColorPaint();

        _ = DrawChart(chart);

        Assert.IsTrue(
            drawables == canvas.DrawablesCount &&
            geometries == canvas.CountGeometries());
    }

    [TestMethod]
    public void LineSeriesGeometryPaintsChanged()
    {
        var series = new LineSeries<int>
        {
            Values = new List<int> { 1, 6, 4, 2 }
        };

        var chart = new SKCartesianChart
        {
            Series = new List<ISeries> { series },
            XAxes = new[] { new Axis() },
            YAxes = new[] { new Axis() },
        };

        var canvas = chart.CoreCanvas;

        _ = DrawChart(chart);

        var drawables = canvas.DrawablesCount;
        var geometries = canvas.CountGeometries();

        series.GeometryFill = new SolidColorPaint();
        series.GeometryStroke = new SolidColorPaint();

        _ = DrawChart(chart);

        Assert.IsTrue(
            drawables == canvas.DrawablesCount &&
            geometries == canvas.CountGeometries());
    }

    [TestMethod]
    public void SeriesRemoved()
    {
        var series = new LineSeries<int> { Values = new List<int> { 1, 6, 4, 2 } };

        var seriesCollection = new List<ISeries> { series };

        var chart = new SKCartesianChart
        {
            Series = seriesCollection,
            XAxes = new[] { new Axis() },
            YAxes = new[] { new Axis() },
        };

        var canvas = chart.CoreCanvas;

        _ = DrawChart(chart);

        var drawables = canvas.DrawablesCount;
        var geometries = canvas.CountGeometries();
        seriesCollection.Add(new LineSeries<int> { Values = new List<int> { 1, 6, 4, 2 } });

        _ = DrawChart(chart);

        seriesCollection.RemoveAt(0);
        _ = DrawChart(chart);

        Assert.IsTrue(
            drawables == canvas.DrawablesCount &&
            geometries == canvas.CountGeometries());
    }

    [TestMethod]
    public void SeriesCollectionInstanceChanged()
    {
        var series = new LineSeries<int> { Values = new List<int> { 1, 6, 4, 2 } };

        var seriesCollection = new List<ISeries> { series };

        var chart = new SKCartesianChart
        {
            Series = seriesCollection,
            XAxes = new[] { new Axis() },
            YAxes = new[] { new Axis() },
        };

        var canvas = chart.CoreCanvas;

        _ = DrawChart(chart);

        var drawables = canvas.DrawablesCount;
        var geometries = canvas.CountGeometries();

        chart.Series = new List<ISeries>
        {
            new LineSeries<int> { Values = new List<int> { 1, 6, 4, 2 } }
        };

        _ = DrawChart(chart);

        Assert.IsTrue(
            drawables == canvas.DrawablesCount &&
            geometries == canvas.CountGeometries());
    }

    [TestMethod]
    public void AxisTextBrushChanged()
    {
        var axis = new Axis();

        var chart = new SKCartesianChart
        {
            Series = new List<ISeries>
                {
                    new LineSeries<int> { Values = new List<int> { 1, 6, 4, 2 } },
                },
            XAxes = new[] { axis },
            YAxes = new[] { new Axis() },
        };

        var canvas = chart.CoreCanvas;

        _ = DrawChart(chart);

        var drawables = canvas.DrawablesCount;
        var geometries = canvas.CountGeometries();

        axis.LabelsPaint = new SolidColorPaint();

        _ = DrawChart(chart);

        Assert.IsTrue(
            drawables == canvas.DrawablesCount &&
            geometries == canvas.CountGeometries());
    }

    [TestMethod]
    public void AxisSeparatorBrushChanged()
    {
        var axis = new Axis();

        var chart = new SKCartesianChart
        {
            Series = new List<ISeries>
                {
                    new LineSeries<int> { Values = new List<int> { 1, 6, 4, 2 } },
                },
            XAxes = new[] { new Axis() },
            YAxes = new[] { axis },
        };

        var canvas = chart.CoreCanvas;

        _ = DrawChart(chart);

        var drawables = canvas.DrawablesCount;
        var geometries = canvas.CountGeometries();

        axis.SeparatorsPaint = new SolidColorPaint();

        _ = DrawChart(chart);

        Assert.IsTrue(
            drawables == canvas.DrawablesCount &&
            geometries == canvas.CountGeometries());
    }

    [TestMethod]
    public void AxisCollectionInstanceChanged()
    {
        var chart = new SKCartesianChart
        {
            Series = new List<ISeries>
                {
                    new LineSeries<int> { Values = new List<int> { 1, 6, 4, 2 } },
                },
            XAxes = new[] { new Axis() },
            YAxes = new[] { new Axis() },
        };

        var canvas = chart.CoreCanvas;

        _ = DrawChart(chart);

        var drawables = canvas.DrawablesCount;
        var geometries = canvas.CountGeometries();

        chart.XAxes = new[] { new Axis() };

        _ = DrawChart(chart);

        Assert.IsTrue(
            drawables == canvas.DrawablesCount &&
            geometries == canvas.CountGeometries());
    }

    [TestMethod]
    public void SectionBrushChanged()
    {
        var section = new RectangularSection
        {
            Fill = new SolidColorPaint()
        };

        var chart = new SKCartesianChart
        {
            Series = new List<ISeries>
                {
                    new LineSeries<int> { Values = new List<int> { 1, 6, 4, 2 } },
                },
            XAxes = new[] { new Axis() },
            YAxes = new[] { new Axis() },
            Sections = new[] { section }
        };

        var canvas = chart.CoreCanvas;

        _ = DrawChart(chart);

        var drawables = canvas.DrawablesCount;
        var geometries = canvas.CountGeometries();

        section.Fill = new SolidColorPaint();

        _ = DrawChart(chart);

        Assert.IsTrue(
            drawables == canvas.DrawablesCount &&
            geometries == canvas.CountGeometries());
    }

    [TestMethod]
    public void SectionsInstanceChanged()
    {
        var chart = new SKCartesianChart
        {
            Series = new List<ISeries>
                {
                    new LineSeries<int> { Values = new List<int> { 1, 6, 4, 2 } },
                },
            XAxes = new[] { new Axis() },
            YAxes = new[] { new Axis() },
            Sections = new[] { new RectangularSection() }
        };

        var canvas = chart.CoreCanvas;

        _ = DrawChart(chart);

        var drawables = canvas.DrawablesCount;
        var geometries = canvas.CountGeometries();

        chart.Sections = new[] { new RectangularSection() };

        _ = DrawChart(chart);

        Assert.IsTrue(
            drawables == canvas.DrawablesCount &&
            geometries == canvas.CountGeometries());
    }

    public static int DrawChart(SourceGenSKChart chart, bool animated = false)
    {
        var coreChart = chart.CoreChart;

        coreChart.IsLoaded = true;
        coreChart._isFirstDraw = true;
        coreChart.Measure();

        var canvas = chart.CoreCanvas;
        canvas.DisableAnimations = !animated;
        var frames = 0;

        while (!canvas.IsValid)
        {
            frames++;
            canvas.DrawFrame(
                new SkiaSharpDrawingContext(
                    canvas,
                    SKSurface.Create(new SKImageInfo(100, 100)).Canvas,
                    SKColor.Empty));
        }

        return frames;
    }
}
