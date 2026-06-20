// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Console.Drawing;
using LiveChartsCore.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleTests.Drawing;

[TestClass]
public class SixelEncoderTests
{
    [TestMethod]
    public void Encode_EmptyGrid_ProducesValidSixelEnvelope()
    {
        // 6×6 of unset pixels → palette[0] = background, single band of all-zero sixels.
        var pixels = new LvcColor[6, 6];
        var s = SixelEncoder.Encode(pixels, new LvcColor(0, 0, 0));

        Assert.IsTrue(s.StartsWith("\x1bP"), "expected DCS introducer");
        Assert.IsTrue(s.EndsWith("\x1b\\"), "expected ST terminator");
        Assert.IsTrue(s.Contains("\"1;1;6;6"), "expected raster attrs '1;1;<w>;<h>'");
        Assert.IsTrue(s.Contains("#0;2;"), "expected palette[0] definition");
    }

    [TestMethod]
    public void Encode_TransparentBackground_PromotesToOpaqueFallback()
    {
        // A=0 background tells the encoder "I have no background" — it must promote to a
        // sensible dark fallback so palette[0] stays defined and Pb=2 has something to paint.
        var pixels = new LvcColor[6, 6];
        var s = SixelEncoder.Encode(pixels, default);

        // Fallback is (20, 20, 20) → 20*100/255 = 7 in Sixel %-units.
        Assert.IsTrue(s.Contains("#0;2;7;7;7") || s.Contains("#0;2;8;8;8"),
            "expected palette[0] to be the (~20,20,20) fallback");
    }

    [TestMethod]
    public void Encode_TwoColors_AllocatesPaletteEntryPerColor()
    {
        var pixels = new LvcColor[6, 6];
        for (var y = 0; y < 6; y++)
        {
            for (var x = 0; x < 6; x++)
            {
                pixels[y, x] = x < 3 ? new LvcColor(255, 0, 0) : new LvcColor(0, 255, 0);
            }
        }

        var s = SixelEncoder.Encode(pixels, new LvcColor(0, 0, 0));

        // Expect palette[0]=bg, plus #1 and #2 for red & green. Order is insertion-order.
        Assert.IsTrue(s.Contains("#0;2;0;0;0"), "expected black background palette entry");
        Assert.IsTrue(s.Contains("#1;2;100;0;0"), "expected red palette entry");
        Assert.IsTrue(s.Contains("#2;2;0;100;0"), "expected green palette entry");
    }

    [TestMethod]
    public void Encode_LongRunOfSameSixel_UsesRleCompression()
    {
        // 30 columns of identical filled pixels → run-length form '!30<char>' instead of
        // 30 separate chars. Threshold in encoder is len >= 4.
        var pixels = new LvcColor[6, 30];
        var red = new LvcColor(255, 0, 0);
        for (var y = 0; y < 6; y++)
            for (var x = 0; x < 30; x++)
                pixels[y, x] = red;

        var s = SixelEncoder.Encode(pixels, new LvcColor(0, 0, 0));

        Assert.IsTrue(s.Contains("!30"), $"expected RLE marker '!30' in output, got: {s}");
    }
}
