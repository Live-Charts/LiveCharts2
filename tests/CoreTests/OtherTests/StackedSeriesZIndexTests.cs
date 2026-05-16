using LiveChartsCore.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.OtherTests;

// Regression for issue #1923. Stacked area / step / polar line series
// silently overrode the user's ZIndex with `1000 - stacker.Position`,
// so a non-stacked LineSeries left at default ZIndex always drew BEHIND
// a stacked area, and the ZIndex property reported the user's value
// while the actual paint Z was the workaround value.
[TestClass]
public class StackedSeriesZIndexTests
{
    [TestMethod]
    public void StackedArea_HonorsUserZIndex_OnFillAndStroke()
    {
        var fill = new SolidColorPaint(SKColors.Red);
        var stroke = new SolidColorPaint(SKColors.Black, 4);

        var chart = new SKCartesianChart
        {
            Width = 200,
            Height = 200,
            Series = [
                new StackedAreaSeries<double>
                {
                    Values = [1, 2, 3, 4],
                    Fill = fill,
                    Stroke = stroke,
                    ZIndex = 50,
                }
            ]
        };

        using var image = chart.GetImage();

        Assert.AreEqual(
            50 + PaintConstants.SeriesFillZIndexOffset,
            fill.ZIndex,
            "StackedArea Fill must honor user ZIndex (was overridden by 1000 - stack position).");
        Assert.AreEqual(
            50 + PaintConstants.SeriesStrokeZIndexOffset,
            stroke.ZIndex,
            "StackedArea Stroke must honor user ZIndex (was overridden by 1000 - stack position).");
    }

    [TestMethod]
    public void DefaultLineSeries_DrawsAboveDefaultStackedArea_BySeriesId()
    {
        // With both at default ZIndex, the LineSeries (added second, so
        // SeriesId == 1) should draw on top of the StackedArea (SeriesId
        // == 0). Previously the StackedArea jumped to 1000 unconditionally
        // and won regardless of insertion order.
        var lineFill = new SolidColorPaint(SKColors.Blue);
        var stackedFill = new SolidColorPaint(SKColors.Red);

        var chart = new SKCartesianChart
        {
            Width = 200,
            Height = 200,
            Series = [
                new StackedAreaSeries<double>
                {
                    Values = [1, 2, 3, 4],
                    Fill = stackedFill,
                },
                new LineSeries<double>
                {
                    Values = [1, 2, 3, 4],
                    Fill = lineFill,
                }
            ]
        };

        using var image = chart.GetImage();

        Assert.IsTrue(
            lineFill.ZIndex > stackedFill.ZIndex,
            $"LineSeries Fill (Z={lineFill.ZIndex}) should draw above StackedArea Fill (Z={stackedFill.ZIndex}); see #1923.");
    }

    [TestMethod]
    public void StackedStepArea_HonorsUserZIndex_OnFillAndStroke()
    {
        var fill = new SolidColorPaint(SKColors.Red);
        var stroke = new SolidColorPaint(SKColors.Black, 4);

        var chart = new SKCartesianChart
        {
            Width = 200,
            Height = 200,
            Series = [
                new StackedStepAreaSeries<double>
                {
                    Values = [1, 2, 3, 4],
                    Fill = fill,
                    Stroke = stroke,
                    ZIndex = 50,
                }
            ]
        };

        using var image = chart.GetImage();

        Assert.AreEqual(
            50 + PaintConstants.SeriesFillZIndexOffset,
            fill.ZIndex,
            "StackedStepArea Fill must honor user ZIndex.");
        Assert.AreEqual(
            50 + PaintConstants.SeriesStrokeZIndexOffset,
            stroke.ZIndex,
            "StackedStepArea Stroke must honor user ZIndex.");
    }
}
