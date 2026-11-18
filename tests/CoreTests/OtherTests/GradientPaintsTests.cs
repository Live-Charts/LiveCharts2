using System;
using LiveChartsCore.Motion;
using LiveChartsCore.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.OtherTests;

[TestClass]
public class GradientPaintsTests
{
    private static readonly SKColor[] s_twoStops =
        [new SKColor(0xFF, 0x00, 0x00), new SKColor(0x00, 0x00, 0xFF)];
    private static readonly SKColor[] s_threeStops =
        [new SKColor(0xFF, 0x00, 0x00), new SKColor(0x00, 0xFF, 0x00), new SKColor(0x00, 0x00, 0xFF)];

    [TestMethod]
    public void LinearGradient_ConstructorOverloadsAllProduceUsablePaint()
    {
        var fromArray = new LinearGradientPaint(s_twoStops);
        var fromArrayWithPoints = new LinearGradientPaint(s_twoStops, new SKPoint(0, 0), new SKPoint(1, 1));
        var fromTwoColorsAndPoints = new LinearGradientPaint(
            s_twoStops[0], s_twoStops[1], new SKPoint(0, 0), new SKPoint(1, 0));
        var fromTwoColors = new LinearGradientPaint(s_twoStops[0], s_twoStops[1]);

        // The defaults exposed by the class are usable as construction inputs.
        Assert.AreEqual(new SKPoint(0, 0.5f), LinearGradientPaint.DefaultStartPoint);
        Assert.AreEqual(new SKPoint(1, 0.5f), LinearGradientPaint.DefaultEndPoint);

        // None of the constructors should throw or yield null.
        foreach (Paint paint in new Paint[] { fromArray, fromArrayWithPoints, fromTwoColorsAndPoints, fromTwoColors })
            Assert.IsNotNull(paint);
    }

    [TestMethod]
    public void LinearGradient_CloneTaskProducesIndependentInstanceOfSameType()
    {
        // Map (the helper SkiaPaint clones use) propagates a fixed subset of fields
        // — IsAntialias, StrokeCap, StrokeJoin, etc. — but interpolates StrokeThickness
        // such that the clone keeps the default at progress=1. Just exercise the path
        // and verify identity / type semantics.
        var original = new LinearGradientPaint(s_twoStops) { IsAntialias = false };

        var clone = (LinearGradientPaint)original.CloneTask();

        Assert.AreNotSame(original, clone);
        Assert.IsInstanceOfType<LinearGradientPaint>(clone);
        Assert.IsFalse(clone.IsAntialias);
    }

    [TestMethod]
    public void LinearGradient_RenderedThroughChartToExerciseShader()
    {
        // Renders a chart that uses a LinearGradientPaint as fill+stroke, which exercises
        // OnPaintStarted/GetShader/DisposeTask on the SkiaSharp drawing path.
        var paint = new LinearGradientPaint(s_threeStops, new SKPoint(0, 0), new SKPoint(1, 1));

        var chart = new SKCartesianChart
        {
            Width = 200,
            Height = 200,
            Series = [
                new ColumnSeries<double> { Values = [1, 2, 3, 4], Fill = paint }
            ]
        };

        using var image = chart.GetImage();
        Assert.IsNotNull(image);

        // DisposeTask is part of the lifecycle. Call explicitly so the SKShader
        // cleanup branch is exercised. (Internal — accessible via InternalsVisibleTo.)
        paint.DisposeTask();
    }

    [TestMethod]
    public void RadialGradient_ConstructorOverloadsAllProduceUsablePaint()
    {
        var fromArray = new RadialGradientPaint(s_twoStops);
        var fromTwoColors = new RadialGradientPaint(s_twoStops[0], s_twoStops[1]);
        var fromArrayWithCenterAndRadius = new RadialGradientPaint(
            s_twoStops, new SKPoint(0.25f, 0.75f), 0.4f, [0f, 1f]);

        foreach (Paint paint in new Paint[] { fromArray, fromTwoColors, fromArrayWithCenterAndRadius })
            Assert.IsNotNull(paint);
    }

    [TestMethod]
    public void RadialGradient_CloneTaskProducesIndependentInstanceOfSameType()
    {
        var original = new RadialGradientPaint(s_twoStops) { IsAntialias = false };
        var clone = (RadialGradientPaint)original.CloneTask();

        Assert.AreNotSame(original, clone);
        Assert.IsInstanceOfType<RadialGradientPaint>(clone);
        Assert.IsFalse(clone.IsAntialias);
    }

    [TestMethod]
    public void RadialGradient_RenderedThroughChartToExerciseShader()
    {
        var paint = new RadialGradientPaint(s_threeStops, new SKPoint(0.5f, 0.5f), 0.5f);

        var chart = new SKPieChart
        {
            Width = 200,
            Height = 200,
            Series = [
                new PieSeries<double> { Values = [3], Fill = paint },
                new PieSeries<double> { Values = [7] }
            ]
        };

        using var image = chart.GetImage();
        Assert.IsNotNull(image);
        paint.DisposeTask();
    }

    // The next three tests cover the opacity-filter cache + disposal contract introduced
    // in this PR. Before the fix, every call to ApplyOpacityMask built a fresh
    // SKColorFilter and RestoreOpacityMask dropped the reference without disposing —
    // native handles leaked to GC finalization. The cache now keys on the opacity value
    // and disposes the cached filter properly in DisposeTask.

    [TestMethod]
    public void LinearGradient_ApplyOpacityMask_ReusesFilterForSameOpacity()
    {
        var (paint, context) = NewLinearGradientWithContext();

        paint.ApplyOpacityMask(context, 0.5f, null);
        var first = paint._opacityFilter;
        Assert.IsNotNull(first);

        paint.ApplyOpacityMask(context, 0.5f, null);
        Assert.AreSame(first, paint._opacityFilter,
            "Same opacity value must reuse the cached SKColorFilter.");
    }

    [TestMethod]
    public void LinearGradient_ApplyOpacityMask_RebuildsForDifferentOpacity()
    {
        var (paint, context) = NewLinearGradientWithContext();

        paint.ApplyOpacityMask(context, 0.5f, null);
        var first = paint._opacityFilter;

        paint.ApplyOpacityMask(context, 0.25f, null);
        Assert.AreNotSame(first, paint._opacityFilter,
            "Different opacity value must rebuild the cached filter.");
        Assert.AreEqual(0.25f, paint._opacityFilterAlpha);
    }

    [TestMethod]
    public void LinearGradient_DisposeTask_ReleasesCachedOpacityFilter()
    {
        var (paint, context) = NewLinearGradientWithContext();

        paint.ApplyOpacityMask(context, 0.5f, null);
        Assert.IsNotNull(paint._opacityFilter);

        paint.DisposeTask();
        Assert.IsNull(paint._opacityFilter,
            "DisposeTask must dispose and null the cached SKColorFilter.");
    }

    [TestMethod]
    public void RadialGradient_ApplyOpacityMask_ReusesFilterForSameOpacity()
    {
        var (paint, context) = NewRadialGradientWithContext();

        paint.ApplyOpacityMask(context, 0.5f, null);
        var first = paint._opacityFilter;
        Assert.IsNotNull(first);

        paint.ApplyOpacityMask(context, 0.5f, null);
        Assert.AreSame(first, paint._opacityFilter,
            "Same opacity value must reuse the cached SKColorFilter.");
    }

    [TestMethod]
    public void RadialGradient_ApplyOpacityMask_RebuildsForDifferentOpacity()
    {
        var (paint, context) = NewRadialGradientWithContext();

        paint.ApplyOpacityMask(context, 0.5f, null);
        var first = paint._opacityFilter;

        paint.ApplyOpacityMask(context, 0.25f, null);
        Assert.AreNotSame(first, paint._opacityFilter,
            "Different opacity value must rebuild the cached filter.");
        Assert.AreEqual(0.25f, paint._opacityFilterAlpha);
    }

    [TestMethod]
    public void RadialGradient_DisposeTask_ReleasesCachedOpacityFilter()
    {
        var (paint, context) = NewRadialGradientWithContext();

        paint.ApplyOpacityMask(context, 0.5f, null);
        Assert.IsNotNull(paint._opacityFilter);

        paint.DisposeTask();
        Assert.IsNull(paint._opacityFilter,
            "DisposeTask must dispose and null the cached SKColorFilter.");
    }

    private static (LinearGradientPaint Paint, SkiaSharpDrawingContext Context) NewLinearGradientWithContext()
    {
        var canvas = new CoreMotionCanvas();
        var surface = SKSurface.Create(new SKImageInfo(100, 100))!;
        var context = new SkiaSharpDrawingContext(canvas, surface.Canvas, SKColor.Empty);
        var paint = new LinearGradientPaint(s_twoStops);
        paint.OnPaintStarted(context, null);
        return (paint, context);
    }

    private static (RadialGradientPaint Paint, SkiaSharpDrawingContext Context) NewRadialGradientWithContext()
    {
        var canvas = new CoreMotionCanvas();
        var surface = SKSurface.Create(new SKImageInfo(100, 100))!;
        var context = new SkiaSharpDrawingContext(canvas, surface.Canvas, SKColor.Empty);
        var paint = new RadialGradientPaint(s_twoStops);
        paint.OnPaintStarted(context, null);
        return (paint, context);
    }

    // https://github.com/Live-Charts/LiveCharts2/issues/2372
    // A gradient is laid out over the area it is drawn in. Its shader used to be built once, on the
    // first frame, so when the chart was resized the gradient kept its old size and the clamp
    // stretched the edge color over the rest.
    [TestMethod]
    public void LinearGradient_FollowsTheChartWhenItIsResized()
    {
        // left to right: black to white, so the middle of the frame is grey.
        var chart = FrameChart(new LinearGradientPaint(SKColors.Black, SKColors.White));

        foreach (var width in new[] { 300, 600 })
        {
            chart.Width = width;
            using var bitmap = Render(chart);
            var y = bitmap.Height / 2;

            AssertGray(bitmap.GetPixel(bitmap.Width / 2, y), 128, $"middle of the {width}px wide chart");
            AssertGray(bitmap.GetPixel(bitmap.Width / 4, y), 64, $"first quarter of the {width}px wide chart");
        }
    }

    [TestMethod]
    public void RadialGradient_FollowsTheChartWhenItIsResized()
    {
        // white in the center, black at the radius (half the frame).
        var chart = FrameChart(new RadialGradientPaint(SKColors.White, SKColors.Black));

        foreach (var width in new[] { 300, 600 })
        {
            chart.Width = width;
            chart.Height = width / 2;
            using var bitmap = Render(chart);

            AssertGray(bitmap.GetPixel(bitmap.Width / 2, bitmap.Height / 2), 255, $"center of the {width}px wide chart");
        }
    }

    // ExplicitDisposing keeps the chart (and its paints) loaded between images, like a chart in a
    // window: otherwise every image unloads it, and the next one rebuilds the paints from scratch.
    private static SKCartesianChart FrameChart(Paint frameFill) => new()
    {
        ExplicitDisposing = true,
        Width = 300,
        Height = 150,
        DrawMargin = new LiveChartsCore.Measure.Margin(0),
        DrawMarginFrame = new DrawMarginFrame { Fill = frameFill, Stroke = null },
        Series = [new LineSeries<double> { Values = [1, 2], Fill = null, Stroke = null, GeometryFill = null, GeometryStroke = null }],
        XAxes = [new Axis { IsVisible = false }],
        YAxes = [new Axis { IsVisible = false }],
    };

    private static SKBitmap Render(SKCartesianChart chart)
    {
        using var image = chart.GetImage();
        return SKBitmap.FromImage(image);
    }

    private static void AssertGray(SKColor color, byte expected, string where) =>
        Assert.IsTrue(Math.Abs(color.Red - expected) <= 24 && Math.Abs(color.Green - expected) <= 24 && Math.Abs(color.Blue - expected) <= 24,
            $"At the {where}: expected a gray of about {expected}, got {color}.");
}
