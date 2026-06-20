using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.OtherTests;

[TestClass]
public class LabelGeometryCacheTests
{
    // Regression for the PR #2256 review thread: the BlobArray cache-hit path must not
    // call UpdateSkiaPaint, which would overwrite paint properties (IsAntialias / StrokeCap
    // / etc.) set by the font-builder when DrawingTextExtensions.DrawLabel runs
    // PeekPaintInfo earlier in the same frame.
    [TestMethod]
    public void BlobArrayCacheHit_PreservesFontBuilderPaintProperties()
    {
        // LvcSkiaPaint has IsAntialias = false; the default font-builder forces the SKPaint's
        // IsAntialias to true regardless. The bug was that re-entering the BlobArray getter
        // on the cache-hit path called UpdateSkiaPaint which copied IsAntialias=false back
        // onto the SKPaint, breaking subsequent DrawText calls in the same frame.
        var paint = new SolidColorPaint(SKColors.Black) { IsAntialias = false };
        var label = new LabelGeometry { Text = "test", Paint = paint, TextSize = 12 };

        // First access: cache miss → AsBlobArray runs PeekPaintInfo → fontBuilder sets
        // IsAntialias = true on the SKPaint.
        _ = label.BlobArray;
        Assert.IsNotNull(paint._skiaPaint);
        Assert.IsTrue(
            paint._skiaPaint.IsAntialias,
            "sanity: default fontBuilder forces SKPaint.IsAntialias = true on first build.");

        // Second access: must be a cache hit and must NOT invoke UpdateSkiaPaint. If it
        // did, IsAntialias would be reset to the LvcPaint property value (false).
        _ = label.BlobArray;
        Assert.IsTrue(
            paint._skiaPaint.IsAntialias,
            "regression: BlobArray getter on cache-hit must not invoke UpdateSkiaPaint and "
                + "clobber the font-builder's IsAntialias setting.");
    }
}
