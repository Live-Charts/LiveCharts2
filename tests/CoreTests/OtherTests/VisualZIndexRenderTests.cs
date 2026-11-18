using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.VisualElements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.OtherTests;

// https://github.com/Live-Charts/LiveCharts2/issues/2364
// The canvas draws zone by zone and a ZIndex only orders the paints within a zone. The series draw
// in the DrawMargin zone and a Visual drew in the NoClip zone, after it, so it covered the series
// whatever its ZIndex. A Visual with a negative ZIndex now draws in the Underlay zone, before the
// series; with the default (0) or a positive ZIndex it stays on top of them.
// These render a red visual over the whole chart and a blue column series, and read a pixel inside
// a column: blue when the series is on top, red when the visual is.
[TestClass]
public class VisualZIndexRenderTests
{
    [TestMethod]
    [DataRow(-1, DisplayName = "ZIndex -1")]
    [DataRow(-1000, DisplayName = "ZIndex -1000")]
    public void Visual_WithANegativeZIndex_IsDrawnBehindTheSeries(int zIndex)
    {
        var chart = ChartWith(new FullChartRectangle { ZIndex = zIndex });

        Assert.AreEqual(SKColors.Blue, PixelInsideAColumn(chart), "the series should be drawn over the visual");
    }

    [TestMethod]
    [DataRow(0, DisplayName = "ZIndex 0 (default)")]
    [DataRow(1000, DisplayName = "ZIndex 1000")]
    public void Visual_WithTheDefaultOrAPositiveZIndex_IsDrawnOnTopOfTheSeries(int zIndex)
    {
        var chart = ChartWith(new FullChartRectangle { ZIndex = zIndex });

        Assert.AreEqual(SKColors.Red, PixelInsideAColumn(chart), "the visual should be drawn over the series");
    }

    [TestMethod]
    public void Visual_MovesBehindTheSeries_WhenItsZIndexBecomesNegative()
    {
        var visual = new FullChartRectangle();
        var chart = ChartWith(visual);

        Assert.AreEqual(SKColors.Red, PixelInsideAColumn(chart), "with the default ZIndex the visual is on top");

        visual.ZIndex = -1;
        Assert.AreEqual(SKColors.Blue, PixelInsideAColumn(chart), "with a negative ZIndex the visual is behind");

        visual.ZIndex = 1;
        Assert.AreEqual(SKColors.Red, PixelInsideAColumn(chart), "back on top with a positive ZIndex");
    }

    // ExplicitDisposing keeps the chart (and its visual's drawn task) loaded between images, like a
    // chart in a window, so a ZIndex changed between images moves the existing task.
    private static SKCartesianChart ChartWith(Visual visual) => new()
    {
        ExplicitDisposing = true,
        Width = 400,
        Height = 300,
        Background = SKColors.White,
        Series = [new ColumnSeries<double>
        {
            Values = [5, 5, 5],
            Fill = new SolidColorPaint(SKColors.Blue),
            Stroke = null,
            MaxBarWidth = double.MaxValue,
            Padding = 0
        }],
        VisualElements = [visual],
        YAxes = [new Axis { MinLimit = 0, MaxLimit = 10 }],
    };

    private static SKColor PixelInsideAColumn(SKCartesianChart chart)
    {
        using var image = chart.GetImage();
        using var bitmap = SKBitmap.FromImage(image);

        // the middle column, low enough to be inside its bar.
        return bitmap.GetPixel(bitmap.Width / 2 + 10, bitmap.Height * 3 / 4);
    }

    // DrawnElement returns IDrawnElement rather than RectangleGeometry: net462 doesn't support
    // covariant return types.
    private class FullChartRectangle : Visual
    {
        private readonly RectangleGeometry _rectangle = new() { Fill = new SolidColorPaint(SKColors.Red) };

        protected internal override IDrawnElement DrawnElement => _rectangle;

        protected override void Measure(Chart chart)
        {
            _rectangle.X = 0;
            _rectangle.Y = 0;
            _rectangle.Width = chart.ControlSize.Width;
            _rectangle.Height = chart.ControlSize.Height;
        }
    }
}
