// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Drawing;

namespace LiveChartsCore.Console.Drawing;

/// <summary>
/// Tiny 5×7 monospace bitmap font used to rasterize labels into the pixel grid in
/// <see cref="ConsoleRenderMode.Sixel"/> mode. Cell-grid modes (HalfBlock, Braille) render
/// labels as ANSI text glyphs and don't need this. Glyphs cover digits, sign, decimal,
/// scientific notation, and the full Latin alphabet (a–z, A–Z) so categorical axis labels
/// (month names, person names, etc.) render alongside numeric labels. Unknown characters
/// are silently skipped.
/// </summary>
internal static class BitmapFont
{
    public const int GlyphWidth = 5;
    public const int GlyphHeight = 7;
    public const int GlyphSpacing = 1;

    // Each pattern is 7 rows; per row, the low 5 bits are columns (bit 4 = leftmost col).
    private static readonly Dictionary<char, byte[]> Glyphs = new()
    {
        ['0'] = [0b01110, 0b10001, 0b10011, 0b10101, 0b11001, 0b10001, 0b01110],
        ['1'] = [0b00100, 0b01100, 0b00100, 0b00100, 0b00100, 0b00100, 0b01110],
        ['2'] = [0b01110, 0b10001, 0b00001, 0b00010, 0b00100, 0b01000, 0b11111],
        ['3'] = [0b01110, 0b10001, 0b00001, 0b00110, 0b00001, 0b10001, 0b01110],
        ['4'] = [0b00010, 0b00110, 0b01010, 0b10010, 0b11111, 0b00010, 0b00010],
        ['5'] = [0b11111, 0b10000, 0b11110, 0b00001, 0b00001, 0b10001, 0b01110],
        ['6'] = [0b00110, 0b01000, 0b10000, 0b11110, 0b10001, 0b10001, 0b01110],
        ['7'] = [0b11111, 0b00001, 0b00010, 0b00100, 0b01000, 0b01000, 0b01000],
        ['8'] = [0b01110, 0b10001, 0b10001, 0b01110, 0b10001, 0b10001, 0b01110],
        ['9'] = [0b01110, 0b10001, 0b10001, 0b01111, 0b00001, 0b00010, 0b01100],
        ['-'] = [0, 0, 0, 0b11111, 0, 0, 0],
        ['.'] = [0, 0, 0, 0, 0, 0b01100, 0b01100],
        [','] = [0, 0, 0, 0, 0b01100, 0b01100, 0b01000],
        ['+'] = [0, 0b00100, 0b00100, 0b11111, 0b00100, 0b00100, 0],
        [':'] = [0, 0b01100, 0b01100, 0, 0b01100, 0b01100, 0],
        ['/'] = [0b00001, 0b00010, 0b00010, 0b00100, 0b01000, 0b01000, 0b10000],
        [' '] = [0, 0, 0, 0, 0, 0, 0],

        // Uppercase A–Z. Each row is 5 cols; bit 4 = leftmost col. Patterns sized to fit
        // the canonical 5×7 chart-axis grid Skia's tickers also assume.
        ['A'] = [0b01110, 0b10001, 0b10001, 0b11111, 0b10001, 0b10001, 0b10001],
        ['B'] = [0b11110, 0b10001, 0b10001, 0b11110, 0b10001, 0b10001, 0b11110],
        ['C'] = [0b01110, 0b10001, 0b10000, 0b10000, 0b10000, 0b10001, 0b01110],
        ['D'] = [0b11110, 0b10001, 0b10001, 0b10001, 0b10001, 0b10001, 0b11110],
        ['E'] = [0b11111, 0b10000, 0b10000, 0b11110, 0b10000, 0b10000, 0b11111],
        ['F'] = [0b11111, 0b10000, 0b10000, 0b11110, 0b10000, 0b10000, 0b10000],
        ['G'] = [0b01110, 0b10001, 0b10000, 0b10111, 0b10001, 0b10001, 0b01110],
        ['H'] = [0b10001, 0b10001, 0b10001, 0b11111, 0b10001, 0b10001, 0b10001],
        ['I'] = [0b01110, 0b00100, 0b00100, 0b00100, 0b00100, 0b00100, 0b01110],
        ['J'] = [0b00111, 0b00010, 0b00010, 0b00010, 0b00010, 0b10010, 0b01100],
        ['K'] = [0b10001, 0b10010, 0b10100, 0b11000, 0b10100, 0b10010, 0b10001],
        ['L'] = [0b10000, 0b10000, 0b10000, 0b10000, 0b10000, 0b10000, 0b11111],
        ['M'] = [0b10001, 0b11011, 0b10101, 0b10101, 0b10001, 0b10001, 0b10001],
        ['N'] = [0b10001, 0b10001, 0b11001, 0b10101, 0b10011, 0b10001, 0b10001],
        ['O'] = [0b01110, 0b10001, 0b10001, 0b10001, 0b10001, 0b10001, 0b01110],
        ['P'] = [0b11110, 0b10001, 0b10001, 0b11110, 0b10000, 0b10000, 0b10000],
        ['Q'] = [0b01110, 0b10001, 0b10001, 0b10001, 0b10101, 0b10010, 0b01101],
        ['R'] = [0b11110, 0b10001, 0b10001, 0b11110, 0b10100, 0b10010, 0b10001],
        ['S'] = [0b01110, 0b10001, 0b10000, 0b01110, 0b00001, 0b10001, 0b01110],
        ['T'] = [0b11111, 0b00100, 0b00100, 0b00100, 0b00100, 0b00100, 0b00100],
        ['U'] = [0b10001, 0b10001, 0b10001, 0b10001, 0b10001, 0b10001, 0b01110],
        ['V'] = [0b10001, 0b10001, 0b10001, 0b10001, 0b10001, 0b01010, 0b00100],
        ['W'] = [0b10001, 0b10001, 0b10001, 0b10101, 0b10101, 0b10101, 0b01010],
        ['X'] = [0b10001, 0b10001, 0b01010, 0b00100, 0b01010, 0b10001, 0b10001],
        ['Y'] = [0b10001, 0b10001, 0b10001, 0b01010, 0b00100, 0b00100, 0b00100],
        ['Z'] = [0b11111, 0b00001, 0b00010, 0b00100, 0b01000, 0b10000, 0b11111],

        // Lowercase a–z. Most letters live in rows 2–6 (an x-height of 5) with rows 0–1
        // reserved for ascenders (b, d, f, h, k, l, t) and rows 5–6 for descenders (g, j,
        // p, q, y). Letters with neither (a, c, e, m, n, …) leave the top two rows blank
        // so their visual weight matches uppercase.
        ['a'] = [0, 0, 0b01110, 0b00001, 0b01111, 0b10001, 0b01111],
        ['b'] = [0b10000, 0b10000, 0b11110, 0b10001, 0b10001, 0b10001, 0b11110],
        ['c'] = [0, 0, 0b01110, 0b10001, 0b10000, 0b10001, 0b01110],
        ['d'] = [0b00001, 0b00001, 0b01111, 0b10001, 0b10001, 0b10001, 0b01111],
        ['e'] = [0, 0, 0b01110, 0b10011, 0b11110, 0b10000, 0b01110],
        ['f'] = [0b00110, 0b01001, 0b01000, 0b11100, 0b01000, 0b01000, 0b01000],
        ['g'] = [0, 0, 0b01111, 0b10001, 0b01111, 0b00001, 0b01110],
        ['h'] = [0b10000, 0b10000, 0b11110, 0b10001, 0b10001, 0b10001, 0b10001],
        ['i'] = [0b00100, 0, 0b01100, 0b00100, 0b00100, 0b00100, 0b01110],
        ['j'] = [0b00010, 0, 0b00110, 0b00010, 0b00010, 0b10010, 0b01100],
        ['k'] = [0b10000, 0b10000, 0b10010, 0b10100, 0b11000, 0b10100, 0b10010],
        ['l'] = [0b01100, 0b00100, 0b00100, 0b00100, 0b00100, 0b00100, 0b01110],
        ['m'] = [0, 0, 0b11010, 0b10101, 0b10101, 0b10001, 0b10001],
        ['n'] = [0, 0, 0b11110, 0b10001, 0b10001, 0b10001, 0b10001],
        ['o'] = [0, 0, 0b01110, 0b10001, 0b10001, 0b10001, 0b01110],
        ['p'] = [0, 0, 0b11110, 0b10001, 0b11110, 0b10000, 0b10000],
        ['q'] = [0, 0, 0b01111, 0b10001, 0b01111, 0b00001, 0b00001],
        ['r'] = [0, 0, 0b10110, 0b11001, 0b10000, 0b10000, 0b10000],
        ['s'] = [0, 0, 0b01111, 0b10000, 0b01110, 0b00001, 0b11110],
        ['t'] = [0b00100, 0b00100, 0b11110, 0b00100, 0b00100, 0b00101, 0b00010],
        ['u'] = [0, 0, 0b10001, 0b10001, 0b10001, 0b10001, 0b01110],
        ['v'] = [0, 0, 0b10001, 0b10001, 0b10001, 0b01010, 0b00100],
        ['w'] = [0, 0, 0b10001, 0b10001, 0b10101, 0b10101, 0b01010],
        ['x'] = [0, 0, 0b10001, 0b01010, 0b00100, 0b01010, 0b10001],
        ['y'] = [0, 0, 0b10001, 0b10001, 0b01111, 0b00001, 0b01110],
        ['z'] = [0, 0, 0b11111, 0b00010, 0b00100, 0b01000, 0b11111],
    };

    /// <summary>
    /// Rasterizes a single glyph into the surface pixel grid. Unknown chars are skipped without
    /// advancing — caller manages the X cursor.
    /// </summary>
    public static void DrawChar(ConsoleSurface surface, int x, int y, char ch, LvcColor color, int scale = 2)
    {
        if (!Glyphs.TryGetValue(ch, out var pattern)) return;

        for (var row = 0; row < GlyphHeight; row++)
        {
            var bits = pattern[row];
            if (bits == 0) continue;

            for (var col = 0; col < GlyphWidth; col++)
            {
                if (((bits >> (GlyphWidth - 1 - col)) & 1) == 0) continue;
                for (var dy = 0; dy < scale; dy++)
                    for (var dx = 0; dx < scale; dx++)
                        surface.SetPixel(x + col * scale + dx, y + row * scale + dy, color);
            }
        }
    }

    /// <summary>
    /// Rasterizes a string starting at (<paramref name="x"/>, <paramref name="y"/>). Step width
    /// includes inter-glyph spacing so consecutive chars don't touch.
    /// </summary>
    public static void DrawText(ConsoleSurface surface, int x, int y, string text, LvcColor color, int scale = 2)
    {
        if (string.IsNullOrEmpty(text)) return;
        var step = (GlyphWidth + GlyphSpacing) * scale;
        foreach (var ch in text)
        {
            DrawChar(surface, x, y, ch, color, scale);
            x += step;
        }
    }

    public static int CellWidth(int scale = 2) => (GlyphWidth + GlyphSpacing) * scale;
    public static int CellHeight(int scale = 2) => GlyphHeight * scale;
}
