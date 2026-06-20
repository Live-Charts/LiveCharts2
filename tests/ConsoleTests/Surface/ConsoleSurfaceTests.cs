// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Console;
using LiveChartsCore.Console.Drawing;
using LiveChartsCore.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleTests.Surface;

[TestClass]
public class ConsoleSurfaceTests
{
    [TestMethod]
    public void HalfBlock_RoundsDimensions_ToCellMultiples()
    {
        var s = new ConsoleSurface(7, 5, ConsoleRenderMode.HalfBlock);

        // HalfBlock cell = 1×2. Width 7 → 7 cells = 7px wide. Height 5 → 5/2*2 = 4px tall.
        Assert.AreEqual(7, s.Width);
        Assert.AreEqual(4, s.Height);
        Assert.AreEqual(7, s.CellCols);
        Assert.AreEqual(2, s.CellRows);
    }

    [TestMethod]
    public void Braille_RoundsDimensions_ToCellMultiples()
    {
        var s = new ConsoleSurface(11, 9, ConsoleRenderMode.Braille);

        // Braille cell = 2×4. 11/2*2 = 10. 9/4*4 = 8.
        Assert.AreEqual(10, s.Width);
        Assert.AreEqual(8, s.Height);
        Assert.AreEqual(5, s.CellCols);
        Assert.AreEqual(2, s.CellRows);
    }

    [TestMethod]
    public void Sixel_HonorsCustomCellSize()
    {
        var s = new ConsoleSurface(100, 100, ConsoleRenderMode.Sixel, sixelCellWidth: 10, sixelCellHeight: 20);

        Assert.AreEqual(10, s.CellWidth);
        Assert.AreEqual(20, s.CellHeight);
        Assert.AreEqual(100, s.Width);
        Assert.AreEqual(100, s.Height);
    }

    [TestMethod]
    public void SetClip_DropsWritesOutsideRect()
    {
        var s = new ConsoleSurface(20, 20, ConsoleRenderMode.Braille);
        s.SetClip(4, 4, 8, 8); // covers x∈[4..12), y∈[4..12)

        var red = new LvcColor(255, 0, 0);
        s.SetPixel(2, 6, red);   // outside (x < 4) → dropped
        s.SetPixel(6, 2, red);   // outside (y < 4) → dropped
        s.SetPixel(11, 11, red); // inside → kept
        s.SetPixel(12, 12, red); // outside (x >= 12) → dropped

        // ResetClip must restore full surface bounds — write at (0,0) should land afterwards.
        s.ResetClip();
        s.SetPixel(0, 0, red);

        var ansi = s.ToAnsi(home: false);

        // Two lit pixels: one inside the clipped region, one after reset. Braille codepoints
        // start at U+2800; their presence (vs ' ') is what we're asserting.
        var brailleCount = ansi.Count(c => c >= '⠀' && c <= '⣿');
        Assert.AreEqual(2, brailleCount, "expected exactly 2 lit Braille cells");
    }

    [TestMethod]
    public void SetPixel_BlendsTranslucent_AgainstBackground()
    {
        // Pure-red @ alpha=128 over a black background should land ~half-red. The blend is
        // (bg * (1-a) + color * a), with no per-pixel accumulation: a single SetPixel = a
        // single composite against Background, regardless of what was previously drawn.
        var s = new ConsoleSurface(2, 4, ConsoleRenderMode.Braille);
        s.Background = new LvcColor(0, 0, 0);

        var translucentRed = new LvcColor(255, 0, 0, 128);
        s.SetPixel(0, 0, translucentRed);

        var ansi = s.ToAnsi(home: false);

        // Expected R component: 0 * (127/255) + 255 * (128/255) ≈ 128. Sample-blended 24-bit
        // SGR escape should encode roughly 38;2;128;0;0 — allow ±2 for byte-rounding wobble.
        Assert.IsTrue(ansi.Contains("38;2;12") || ansi.Contains("38;2;13"),
            $"expected blended red ~128, got: {ansi.Replace("\x1b", "ESC")}");
    }

    [TestMethod]
    public void HalfBlock_DistinctTopBottom_EmitsUpperHalfWithFgAndBg()
    {
        var s = new ConsoleSurface(2, 2, ConsoleRenderMode.HalfBlock);
        s.Background = new LvcColor(0, 0, 0);

        s.SetPixel(0, 0, new LvcColor(255, 0, 0));   // top sub-pixel of cell (0,0) → red
        s.SetPixel(0, 1, new LvcColor(0, 255, 0));   // bottom sub-pixel → green

        var ansi = s.ToAnsi(home: false);

        // Half-block convention (see ResolveCell): both sub-pixels filled with different colors
        // → emit '▀' with top color as fg and bottom color as bg.
        Assert.IsTrue(ansi.Contains('▀'), "expected upper-half-block glyph");
        Assert.IsTrue(ansi.Contains("38;2;255;0;0"), "expected red foreground SGR");
        Assert.IsTrue(ansi.Contains("48;2;0;255;0"), "expected green background SGR");
    }

    [TestMethod]
    public void DrawText_OverridesPixelsInTargetCell()
    {
        var s = new ConsoleSurface(20, 8, ConsoleRenderMode.Braille);
        s.Background = new LvcColor(0, 0, 0);

        var red = new LvcColor(255, 0, 0);
        // Fill the cell that text will land on with red pixels first — text must wipe them.
        for (var x = 0; x < 2; x++)
            for (var y = 0; y < 4; y++)
                s.SetPixel(x, y, red);

        s.DrawText(0, 0, "X", new LvcColor(255, 255, 255));

        var ansi = s.ToAnsi(home: false);
        Assert.IsTrue(ansi.Contains('X'), "expected text glyph in output");
        // The Braille codepoint for the previously-set pixels must NOT appear at row 0,
        // because DrawText cleared the underlying cell.
        Assert.IsFalse(ansi.StartsWith('⠀'), "expected text to override leading Braille glyph");
    }

    [TestMethod]
    public void ToAnsi_WithHome_PrependsCursorReset()
    {
        var s = new ConsoleSurface(2, 2, ConsoleRenderMode.HalfBlock);
        var ansi = s.ToAnsi(home: true);
        Assert.IsTrue(ansi.StartsWith("\x1b[H"), "expected ESC[H prefix when home=true");
    }

    [TestMethod]
    public void ToAnsi_NoColor_HalfBlock_EmitsPlainGlyphsWithoutEscapes()
    {
        // Targets the AI-consumer path: stdout is read as text by something that doesn't
        // paint terminal control sequences. A plain rendering must be readable on its own
        // (✓ block glyphs present, ✓ newlines between rows) and must contain zero ESC bytes.
        var s = new ConsoleSurface(4, 4, ConsoleRenderMode.HalfBlock);
        s.Background = new LvcColor(0, 0, 0);

        // Fill a diagonal so the output isn't all spaces — gives the test something to assert
        // about *content* in addition to the absence of escapes.
        s.SetPixel(0, 0, new LvcColor(255, 0, 0));
        s.SetPixel(1, 1, new LvcColor(0, 255, 0));
        s.SetPixel(2, 2, new LvcColor(0, 0, 255));
        s.SetPixel(3, 3, new LvcColor(255, 255, 0));

        var plain = s.ToAnsi(home: false, color: false);

        Assert.IsFalse(plain.Contains('\x1b'),
            $"expected no ESC bytes in plain output, got: {plain.Replace("\x1b", "ESC")}");
        Assert.IsTrue(plain.Contains('▀') || plain.Contains('▄') || plain.Contains('█'),
            $"expected block glyphs in plain output, got: {plain}");
        Assert.IsTrue(plain.Contains('\n'), "expected newlines between rows");
    }

    [TestMethod]
    public void ToAnsi_NoColor_HalfBlock_TwoColorCell_CollapsesToFullBlock()
    {
        // The colored encoder paints '▀' fg=top bg=bottom for two-color cells. Without color
        // that glyph reads as half-empty — '█' is the right substitute. Locks the choice so a
        // future refactor doesn't quietly regress to '▀'.
        var s = new ConsoleSurface(1, 2, ConsoleRenderMode.HalfBlock);
        s.SetPixel(0, 0, new LvcColor(255, 0, 0));
        s.SetPixel(0, 1, new LvcColor(0, 255, 0));

        var plain = s.ToAnsi(home: false, color: false);

        Assert.IsTrue(plain.Contains('█'),
            $"expected '█' when both sub-pixels are filled in plain mode, got: {plain}");
        Assert.IsFalse(plain.Contains('▀'), "'▀' would imply only the top is filled");
    }

    [TestMethod]
    public void ToAnsi_NoColor_Braille_EmitsCodepointsWithoutEscapes()
    {
        var s = new ConsoleSurface(2, 4, ConsoleRenderMode.Braille);
        s.SetPixel(0, 0, new LvcColor(255, 0, 0));
        s.SetPixel(1, 3, new LvcColor(0, 255, 0));

        var plain = s.ToAnsi(home: false, color: false);

        Assert.IsFalse(plain.Contains('\x1b'), "expected no ESC bytes in plain Braille output");
        // Codepoint U+2800..U+28FF — one of these should appear for the lit sub-pixels.
        Assert.IsTrue(plain.Any(ch => ch >= '⠀' && ch <= '⣿'),
            $"expected a Braille codepoint in plain output, got: {plain}");
    }

    [TestMethod]
    public void ToAnsi_NoColor_PreservesTextLabels()
    {
        // Axis labels reach the surface via DrawText — they must survive into the plain
        // rendering unchanged so the chart still reads as a chart, not just a blob of dots.
        var s = new ConsoleSurface(20, 8, ConsoleRenderMode.Braille);
        s.DrawText(0, 0, "Y:42", new LvcColor(255, 255, 255));

        var plain = s.ToAnsi(home: false, color: false);

        Assert.IsTrue(plain.Contains("Y:42"), $"expected label text in plain output, got: {plain}");
        Assert.IsFalse(plain.Contains('\x1b'), "expected no ESC bytes around the label");
    }

    [TestMethod]
    public void ToAnsi_NoColor_Sixel_Throws()
    {
        // Sixel encodes pixels into a DCS escape sequence. There's no meaningful plain form,
        // and silently downgrading to half-block would hide a configuration mistake — caller
        // gets a clear exception so the CLI can surface it as an error message.
        var s = new ConsoleSurface(64, 64, ConsoleRenderMode.Sixel);
        Assert.ThrowsExactly<NotSupportedException>(() => s.ToAnsi(home: false, color: false));
    }
}
