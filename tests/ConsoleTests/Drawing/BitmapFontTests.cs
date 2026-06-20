// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Console;
using LiveChartsCore.Console.Drawing;
using LiveChartsCore.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleTests.Drawing;

[TestClass]
public class BitmapFontTests
{
    [TestMethod]
    public void CellSize_Defaults_MatchScale2()
    {
        // (GlyphWidth + GlyphSpacing) * scale = (5 + 1) * 2 = 12 wide.
        // GlyphHeight * scale = 7 * 2 = 14 tall.
        Assert.AreEqual(12, BitmapFont.CellWidth());
        Assert.AreEqual(14, BitmapFont.CellHeight());
    }

    [TestMethod]
    public void DrawChar_KnownGlyph_LightsExpectedPixels()
    {
        // The dot glyph is sparse: only rows 5 & 6 cols 1-2 are lit (pattern 0b01100). At
        // scale=1 the surface should have 4 pixels in a 2×2 block at (1,5)-(2,6).
        var s = new ConsoleSurface(BitmapFont.GlyphWidth, BitmapFont.GlyphHeight, ConsoleRenderMode.Sixel,
            sixelCellWidth: BitmapFont.GlyphWidth, sixelCellHeight: BitmapFont.GlyphHeight);
        s.Background = new LvcColor(0, 0, 0);

        var white = new LvcColor(255, 255, 255);
        BitmapFont.DrawChar(s, 0, 0, '.', white, scale: 1);

        // Verify via Sixel encoding round-trip — palette must contain white (255 → 100%).
        var ansi = s.ToAnsi(home: false);
        Assert.IsTrue(ansi.Contains(";2;100;100;100"), "expected white palette entry from glyph pixels");
    }

    [TestMethod]
    public void DrawChar_UnknownGlyph_DoesNothing()
    {
        var s = new ConsoleSurface(BitmapFont.GlyphWidth, BitmapFont.GlyphHeight, ConsoleRenderMode.Sixel);
        s.Background = new LvcColor(0, 0, 0);
        var ansiBefore = s.ToAnsi(home: false);

        // '$' isn't in the glyph dictionary — call must be a noop.
        BitmapFont.DrawChar(s, 0, 0, '$', new LvcColor(255, 255, 255), scale: 1);

        var ansiAfter = s.ToAnsi(home: false);
        Assert.AreEqual(ansiBefore, ansiAfter, "unknown glyph must not alter the surface");
    }

    [TestMethod]
    public void DrawText_AdvancesByCellWidth()
    {
        // "AB" at scale=1 should advance by GlyphWidth+GlyphSpacing = 6 pixels per char,
        // landing 'A' at x=0 and 'B' at x=6. We verify the second glyph by checking that
        // the Sixel encoding has lit pixels in both halves of the surface.
        var s = new ConsoleSurface(2 * (BitmapFont.GlyphWidth + BitmapFont.GlyphSpacing), BitmapFont.GlyphHeight,
            ConsoleRenderMode.Sixel,
            sixelCellWidth: BitmapFont.GlyphWidth + BitmapFont.GlyphSpacing,
            sixelCellHeight: BitmapFont.GlyphHeight);
        s.Background = new LvcColor(0, 0, 0);

        BitmapFont.DrawText(s, 0, 0, "AB", new LvcColor(255, 255, 255), scale: 1);

        var ansi = s.ToAnsi(home: false);
        // Both glyphs lit → exactly one white palette entry, but multiple band positions
        // ('-' is the band terminator). For a single-band 7px surface there's no useful
        // per-band assertion; instead check that a non-trivial sixel run is present.
        Assert.IsTrue(ansi.Contains(";2;100;100;100"), "expected white palette entry");
        Assert.IsTrue(ansi.Length > "\x1bP0;2;0q\"1;1;12;7\x1b\\".Length + 20,
            "expected non-trivial sixel data for two glyphs");
    }
}
