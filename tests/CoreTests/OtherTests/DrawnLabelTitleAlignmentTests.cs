using System;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView.VisualElements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.OtherTests;

// A label is drawn relative to its X and Y by its alignment: Start puts its left/top edge there,
// Middle its center and End its right/bottom edge. BaseLabelGeometry defaults to Align.Middle.
//
// Code-built titles (new DrawnLabelVisual(new LabelGeometry { ... })) were clipped at the top:
// Chart.AddTitleToChart put the title's X/Y at the top of the chart, assuming Align.Start, so a
// Middle label was centered on y = 0. #2211 fixed that by forcing Align.Start in the
// DrawnLabelVisual constructor, which broke every DrawnLabelVisual used outside the title: a label
// placed by its center moved right and down by half its size (#2371). The title placement now
// accounts for the alignment instead, and the constructor leaves the label alone.
[TestClass]
public class DrawnLabelTitleAlignmentTests
{
    private const int Width = 600;
    private const int Height = 400;

    [TestMethod]
    public void DrawnLabelVisual_KeepsTheAlignmentOfTheLabel()
    {
        var label = new LabelGeometry { Text = "label", HorizontalAlign = Align.Middle, VerticalAlign = Align.End };

        _ = new DrawnLabelVisual(label);

        Assert.AreEqual(Align.Middle, label.HorizontalAlign);
        Assert.AreEqual(Align.End, label.VerticalAlign);
    }

    // https://github.com/Live-Charts/LiveCharts2/issues/2371
    [TestMethod]
    public void DrawnLabelVisual_PlacedByItsCenter_IsCenteredOnItsPosition()
    {
        var chart = new SKCartesianChart
        {
            Width = Width,
            Height = Height,
            Background = SKColors.White,
            Series = [],
            VisualElements = [
                new DrawnLabelVisual(new LabelGeometry
                {
                    Text = "subtitle",
                    TextSize = 30,
                    Paint = new SolidColorPaint(SKColors.Red),
                    HorizontalAlign = Align.Middle,
                    VerticalAlign = Align.Middle,
                    X = Width / 2f,
                    Y = 100
                })
            ]
        };

        var box = RedBounds(chart);

        Assert.AreEqual(Width / 2f, box.MidX, 3, $"the label should be centered on x = {Width / 2}, it was drawn at {box}");
        Assert.AreEqual(100, box.MidY, 8, $"the label should be centered on y = 100, it was drawn at {box}");
    }

    // The title is drawn in the same place whatever the alignment of its label: centered at the
    // top of the chart, not clipped. Start is the alignment the placement always handled.
    [TestMethod]
    [DataRow(Align.Middle, Align.Middle, DisplayName = "Middle (LabelGeometry's default)")]
    [DataRow(Align.End, Align.End, DisplayName = "End")]
    [DataRow(Align.Start, Align.Middle, DisplayName = "Start, Middle")]
    public void Title_IsDrawnInTheSamePlaceWhateverTheLabelsAlignment(Align horizontal, Align vertical)
    {
        var expected = RedBounds(TitledChart(Align.Start, Align.Start));
        var actual = RedBounds(TitledChart(horizontal, vertical));

        Assert.IsTrue(expected.Top > 0, $"the reference title should not be clipped, it was drawn at {expected}");
        Assert.AreEqual(expected.Left, actual.Left, 1, $"expected the title at {expected}, it was drawn at {actual}");
        Assert.AreEqual(expected.Top, actual.Top, 1, $"expected the title at {expected}, it was drawn at {actual}");
        Assert.AreEqual(expected.Right, actual.Right, 1, $"expected the title at {expected}, it was drawn at {actual}");
        Assert.AreEqual(expected.Bottom, actual.Bottom, 1, $"expected the title at {expected}, it was drawn at {actual}");
    }

    private static SKCartesianChart TitledChart(Align horizontal, Align vertical) => new()
    {
        Width = Width,
        Height = Height,
        Background = SKColors.White,
        Series = [],
        Title = new DrawnLabelVisual(new LabelGeometry
        {
            Text = "My chart title",
            TextSize = 30,
            Paint = new SolidColorPaint(SKColors.Red),
            Padding = new Padding(10),
            HorizontalAlign = horizontal,
            VerticalAlign = vertical
        })
    };

    // the bounds of the red pixels, the only red thing in these charts is the label.
    private static SKRect RedBounds(SKCartesianChart chart)
    {
        using var image = chart.GetImage();
        using var bitmap = SKBitmap.FromImage(image);

        int left = int.MaxValue, top = int.MaxValue, right = -1, bottom = -1;
        for (var y = 0; y < bitmap.Height; y++)
            for (var x = 0; x < bitmap.Width; x++)
            {
                var c = bitmap.GetPixel(x, y);
                if (c.Red < 180 || c.Green > 90 || c.Blue > 90) continue;
                left = Math.Min(left, x); right = Math.Max(right, x);
                top = Math.Min(top, y); bottom = Math.Max(bottom, y);
            }

        Assert.IsTrue(right >= 0, "the label was not drawn");
        return new SKRect(left, top, right, bottom);
    }
}
