using System;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.ImageFilters;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.OtherTests;

[TestClass]
public class ImageFiltersTests
{
    [TestMethod]
    public void Blur_CreateFilterPopulatesUnderlyingSKImageFilter()
    {
        var blur = new Blur(2f, 3f);

        Assert.IsNull(blur._sKImageFilter);
        blur.CreateFilter();
        Assert.IsNotNull(blur._sKImageFilter);

        blur.Dispose();
        Assert.IsNull(blur._sKImageFilter);
    }

    [TestMethod]
    public void Blur_CloneIsAnIndependentInstanceOfTheSameType()
    {
        var blur = new Blur(2f, 3f);
        var clone = blur.Clone();

        Assert.AreNotSame(blur, clone);
        Assert.IsInstanceOfType<Blur>(clone);
    }

    [TestMethod]
    public void DropShadow_CreateFilterPopulatesUnderlyingSKImageFilter()
    {
        var dropShadow = new DropShadow(1f, 2f, 3f, 4f, SKColors.Black);

        Assert.IsNull(dropShadow._sKImageFilter);
        dropShadow.CreateFilter();
        Assert.IsNotNull(dropShadow._sKImageFilter);

        dropShadow.Dispose();
    }

    [TestMethod]
    public void DropShadow_CloneIsAnIndependentInstanceOfTheSameType()
    {
        var dropShadow = new DropShadow(1f, 2f, 3f, 4f, SKColors.Black);
        var clone = dropShadow.Clone();

        Assert.AreNotSame(dropShadow, clone);
        Assert.IsInstanceOfType<DropShadow>(clone);
    }

    [TestMethod]
    public void Merge_CreateFilterCombinesEachChildFilter()
    {
        var blur = new Blur(2f, 2f);
        var shadow = new DropShadow(1f, 1f, 1f, 1f, SKColors.Black);
        var merged = new ImageFiltersMergeOperation([blur, shadow]);

        merged.CreateFilter();

        Assert.IsNotNull(merged._sKImageFilter);
        // Children get their own SKImageFilter instances populated in the process.
        Assert.IsNotNull(blur._sKImageFilter);
        Assert.IsNotNull(shadow._sKImageFilter);

        merged.Dispose();
    }

    [TestMethod]
    public void Merge_CloneIsAnIndependentInstanceOfTheSameType()
    {
        var merged = new ImageFiltersMergeOperation([new Blur(1f, 1f), new Blur(2f, 2f)]);
        var clone = merged.Clone();

        Assert.AreNotSame(merged, clone);
        Assert.IsInstanceOfType<ImageFiltersMergeOperation>(clone);
    }

    [TestMethod]
    public void Merge_StaticTransitionateBlendsBetweenSameLengthOperations()
    {
        var from = new ImageFiltersMergeOperation([new Blur(0f, 0f), new Blur(0f, 0f)]);
        var to = new ImageFiltersMergeOperation([new Blur(10f, 10f), new Blur(20f, 20f)]);

        // Internal static helper used by the animation pipeline.
        var blended = (ImageFiltersMergeOperation?)ImageFilter.Transitionate(from, to, 0.5f);
        Assert.IsNotNull(blended);
    }

    [TestMethod]
    public void Merge_TransitionateMismatchedLengthThrows()
    {
        var from = new ImageFiltersMergeOperation([new Blur(0f, 0f)]);
        var to = new ImageFiltersMergeOperation([new Blur(1f, 1f), new Blur(2f, 2f)]);

        Assert.ThrowsExactly<Exception>(() => _ = ImageFilter.Transitionate(from, to, 0.5f));
    }

    [TestMethod]
    public void StaticTransitionateNullToNullReturnsNull()
    {
        Assert.IsNull(ImageFilter.Transitionate(null, null, 0.5f));
    }

    [TestMethod]
    public void StaticTransitionateNullSideUsesRegisteredDefault()
    {
        // When one side is null, the static helper falls back to a default registered
        // by the filter's key (e.g. transparent shadow / zero blur).
        var blur = new Blur(5f, 5f);

        var fromNull = ImageFilter.Transitionate(null, blur, 0.25f);
        Assert.IsNotNull(fromNull);

        var toNull = ImageFilter.Transitionate(blur, null, 0.25f);
        Assert.IsNotNull(toNull);
    }

    [TestMethod]
    public void DropShadow_TransitionateBlendsBetweenSameType()
    {
        // Calls the static helper which in turn dispatches to DropShadow.Transitionate.
        var from = new DropShadow(0f, 0f, 0f, 0f, SKColors.Black);
        var to = new DropShadow(10f, 20f, 5f, 5f, SKColors.Red);

        var blended = (DropShadow?)ImageFilter.Transitionate(from, to, 0.5f);

        Assert.IsNotNull(blended);
        Assert.AreNotSame(from, blended);
        Assert.AreNotSame(to, blended);
    }

    [TestMethod]
    public void DropShadow_AppliedThroughChartRendersWithoutError()
    {
        var paint = new SolidColorPaint(SKColors.Red, 6)
        {
            ImageFilter = new DropShadow(2f, 2f, 4f, 4f, SKColors.Black)
        };

        var chart = new SKCartesianChart
        {
            Width = 200,
            Height = 200,
            Series = [
                new LineSeries<double> { Values = [1, 2, 3], Stroke = paint, Fill = null }
            ]
        };

        using var image = chart.GetImage();
        Assert.IsNotNull(image);
    }
}
