using System.Linq;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.OtherTests;

[TestClass]
public class LabelVerticalCenteringTests
{
    // Labels used to put the baseline at -Ascent. Fonts reserve very different room above
    // the capitals (Segoe UI ~1.08em, Helvetica ~0.77em), so the same tooltip looked ~1px
    // low on Windows and ~1.4px high on macOS. The cap height must sit in the middle of
    // each line box, whatever the platform default typeface is.
    [TestMethod]
    public void CapHeightIsCenteredInEachLine()
    {
        var paint = new SolidColorPaint(SKColors.Black);
        var label = new LabelGeometry { Text = "Mary\nJohn\nSeries 3", Paint = paint, TextSize = 16 };

        var blobArray = label.BlobArray;
        label.PeekPaintInfo(out _, out var font);
        var metrics = font.Metrics;
        Assert.IsTrue(metrics.CapHeight > 0, "sanity: the default typeface reports a cap height.");

        var lineHeight = metrics.Descent - metrics.Ascent + metrics.Leading;
        var baselines = blobArray.Blobs
            .Where(x => x.Width >= 0) // skip the shared new-line marker
            .Select(x => x.Position.Y)
            .Distinct()
            .OrderBy(x => x)
            .ToArray();

        Assert.AreEqual(3, baselines.Length);
        Assert.AreEqual(3 * lineHeight, blobArray.Size.Height, 0.01);

        for (var i = 0; i < baselines.Length; i++)
        {
            var capCenter = baselines[i] - metrics.CapHeight / 2f;
            var lineCenter = i * lineHeight + lineHeight / 2f;

            Assert.AreEqual(
                lineCenter, capCenter, 0.01,
                $"line {i}: caps are {capCenter - lineCenter:+0.00;-0.00}px off the line center " +
                $"for {font.Typeface.FamilyName}.");
        }
    }
}
