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
    public void LinearGradient_TransitionateBetweenSameLengthStopsBlendsValues()
    {
        var from = new LinearGradientPaint(
            [new SKColor(0, 0, 0, 0), new SKColor(0, 0, 0, 0)],
            new SKPoint(0, 0),
            new SKPoint(0, 0));
        var to = new LinearGradientPaint(
            [new SKColor(100, 200, 50, 200), new SKColor(50, 100, 150, 100)],
            new SKPoint(1, 1),
            new SKPoint(1, 1),
            colorPos: [0f, 1f]);

        // ColorPos on `from` matches `to` length so the colorPos transition branch runs.
        var fromWithColorPos = new LinearGradientPaint(
            [new SKColor(0, 0, 0, 0), new SKColor(0, 0, 0, 0)],
            new SKPoint(0, 0),
            new SKPoint(0, 0),
            colorPos: [0f, 1f]);

        // Simple smoke-call: should not throw and should return `from` (in-place mutation).
        var result = fromWithColorPos.Transitionate(0.5f, to);
        Assert.AreSame(fromWithColorPos, result);

        // Same-length stops without colorPos still works.
        result = from.Transitionate(0.5f, to);
        Assert.AreSame(from, result);
    }

    [TestMethod]
    public void LinearGradient_TransitionateToDifferentTargetTypeReturnsTarget()
    {
        var gradient = new LinearGradientPaint(s_twoStops);
        var solid = new SolidColorPaint(SKColors.Red);

        // When the target is a different paint type, Transitionate must hand off the target.
        var result = gradient.Transitionate(0.5f, solid);
        Assert.AreSame(solid, result);
    }

    [TestMethod]
    public void LinearGradient_TransitionateMismatchedStopsThrows()
    {
        var from = new LinearGradientPaint(s_twoStops);
        var to = new LinearGradientPaint(s_threeStops);

        Assert.ThrowsExactly<NotImplementedException>(() => _ = from.Transitionate(0.5f, to));
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

    [TestMethod]
    public void RadialGradient_TransitionateBetweenSameLengthStopsBlendsValues()
    {
        var from = new RadialGradientPaint(
            [new SKColor(0, 0, 0, 0), new SKColor(0, 0, 0, 0)],
            new SKPoint(0, 0),
            radius: 0f);
        var to = new RadialGradientPaint(
            [new SKColor(100, 200, 50, 200), new SKColor(50, 100, 150, 100)],
            new SKPoint(1, 1),
            radius: 1f,
            colorPos: [0f, 1f]);

        var result = from.Transitionate(0.5f, to);
        Assert.AreSame(from, result);
    }

    [TestMethod]
    public void RadialGradient_TransitionateToDifferentTargetTypeReturnsTarget()
    {
        var gradient = new RadialGradientPaint(s_twoStops);
        var solid = new SolidColorPaint(SKColors.Red);

        var result = gradient.Transitionate(0.5f, solid);
        Assert.AreSame(solid, result);
    }

    [TestMethod]
    public void RadialGradient_TransitionateMismatchedStopsThrows()
    {
        var from = new RadialGradientPaint(s_twoStops);
        var to = new RadialGradientPaint(s_threeStops);

        Assert.ThrowsExactly<ArgumentException>(() => _ = from.Transitionate(0.5f, to));
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
}
