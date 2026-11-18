using System;
using LiveChartsCore.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.OtherTests;

[TestClass]
public class GradientResizeTests
{
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
