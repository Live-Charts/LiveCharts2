// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using System.Text;
using LiveChartsCore.Drawing;

namespace LiveChartsCore.Console.Drawing;

/// <summary>
/// A character-grid backbuffer that exposes a pixel coordinate system. The relationship
/// between pixels and terminal cells depends on <see cref="Mode"/>:
///   * <see cref="ConsoleRenderMode.HalfBlock"/>: 1 cell = 1 col × 2 rows of sub-pixels (▀ ▄ █),
///     two colors per cell. Storage is per-cell.
///   * <see cref="ConsoleRenderMode.Braille"/>: 1 cell = 2 cols × 4 rows of sub-pixels, encoded
///     as Braille codepoints. One color per cell. Storage is per-pixel.
///   * <see cref="ConsoleRenderMode.Sixel"/>: 1 cell ≈ <see cref="CellWidth"/> × <see cref="CellHeight"/>
///     real pixels (defaults to 8×16). Output is a DCS Sixel block plus cell-positioned text
///     overlays for axis labels. Storage is per-pixel.
/// Text labels override the derived block/Braille glyph for whatever cell they land in (or
/// land on top of the Sixel image as overlay text in Sixel mode).
/// </summary>
public sealed class ConsoleSurface
{
    private struct HalfBlockCell
    {
        public LvcColor Top;     // top sub-pixel color, A=0 means transparent.
        public LvcColor Bottom;  // bottom sub-pixel color, A=0 means transparent.
    }

    private struct GlyphCell
    {
        public char Glyph;       // '\0' = no override.
        public LvcColor Fg;
        public LvcColor Bg;      // A=0 means inherit Background.
    }

    private readonly HalfBlockCell[,]? _hb;
    private readonly LvcColor[,]? _pixels;
    private readonly GlyphCell[,] _glyphs;

    // Parallel per-cell grid of "bar fill stamp" characters, populated by FillRectStamped
    // (called only from bar/column/row geometries). Used by the plain-mode encoder to give
    // each bar series a distinct texture (█ ▓ ▒ ░ ...) — pies, lines, areas etc. don't
    // call the stamped path, so their cells stay at 0 and read as the original derived
    // glyph. Lazy: stays null until a bar geometry actually fills something.
    private char[,]? _barStamps;
    private readonly Dictionary<int, char> _barStampPalette = new();
    private static readonly char[] s_barStampGlyphs =
        ['█', '▓', '▒', '░', '▚', '▞', '▙', '▟'];

    // Active clip rect in pixel coords. SetPixel rejects writes outside this range, which is
    // how we honor LiveChartsCore's CanvasZone.DrawMargin / XCrosshair / YCrosshair zones —
    // ConsoleDrawingContext.OnBeginZone calls SetClip; OnEndZone calls ResetClip.
    private int _clipX0;
    private int _clipY0;
    private int _clipX1;
    private int _clipY1;

    public ConsoleRenderMode Mode { get; }
    public int Width { get; }
    public int Height { get; }
    public int CellWidth { get; }
    public int CellHeight { get; }
    public int CellCols => Width / CellWidth;
    public int CellRows => Height / CellHeight;

    /// <summary>
    /// Background color used when no pixel is set.
    /// </summary>
    public LvcColor Background { get; set; } = new(0, 0, 0);

    /// <summary>
    /// A high-contrast foreground for text/marks rendered directly on the surface
    /// background — picks dark on light terminals, light on dark. Use for titles, marks,
    /// anything that should pop against the chart bg.
    /// </summary>
    public LvcColor StrongForeground =>
        IsLightBackground
            ? new LvcColor(20, 20, 25)
            : new LvcColor(235, 235, 240);

    /// <summary>
    /// A medium-contrast foreground for de-emphasized text — axis labels, legend names,
    /// etc. Same dark-on-light / light-on-dark logic as <see cref="StrongForeground"/> but
    /// closer to the background, so it reads as a header for the chart rather than another
    /// data layer competing for attention.
    /// </summary>
    public LvcColor SubtleForeground =>
        IsLightBackground
            ? new LvcColor(80, 80, 90)
            : new LvcColor(180, 180, 180);

    /// <summary>
    /// Brightness threshold of 128 splits the LvcColor RGB cube cleanly enough for our
    /// purposes — perceived-luminance weighting (.299/.587/.114) would only matter at
    /// edge tones we never hit (chart backgrounds are almost always near-black or near-
    /// white). A=0 (background unset, e.g. OSC 11 detection failed) treats as dark since
    /// the Sixel encoder also defaults to a dark fallback in that case.
    /// </summary>
    public bool IsLightBackground =>
        Background.A != 0 && (Background.R + Background.G + Background.B) / 3 > 128;

    public ConsoleSurface(int width, int height, ConsoleRenderMode mode = ConsoleRenderMode.HalfBlock,
        int sixelCellWidth = 8, int sixelCellHeight = 16)
    {
        Mode = mode;
        (CellWidth, CellHeight) = mode switch
        {
            ConsoleRenderMode.Braille => (2, 4),
            ConsoleRenderMode.Sixel => (sixelCellWidth, sixelCellHeight),
            _ => (1, 2),
        };

        // Round dimensions down to a multiple of cell size so we never have a partial cell.
        Width = Math.Max(CellWidth, (width / CellWidth) * CellWidth);
        Height = Math.Max(CellHeight, (height / CellHeight) * CellHeight);

        if (mode == ConsoleRenderMode.HalfBlock)
            _hb = new HalfBlockCell[CellRows, CellCols];
        else
            _pixels = new LvcColor[Height, Width];

        _glyphs = new GlyphCell[CellRows, CellCols];

        // Default clip = the whole surface. Drawing operations that don't enter a zone get
        // their existing "draw anywhere" behavior, so unclipped paths (e.g. axis labels with
        // CanvasZone.NoClip) still work without any per-call setup.
        _clipX1 = Width;
        _clipY1 = Height;
    }

    /// <summary>
    /// Restricts subsequent <see cref="SetPixel"/> calls to the given pixel rect (intersected
    /// with the surface bounds). Out-of-clip writes are silently dropped.
    /// </summary>
    public void SetClip(int x, int y, int w, int h)
    {
        _clipX0 = Math.Max(0, x);
        _clipY0 = Math.Max(0, y);
        _clipX1 = Math.Min(Width, x + w);
        _clipY1 = Math.Min(Height, y + h);
        if (_clipX1 < _clipX0) _clipX1 = _clipX0;
        if (_clipY1 < _clipY0) _clipY1 = _clipY0;
    }

    /// <summary>
    /// Clears the clip back to the full surface — undoes a prior <see cref="SetClip"/>.
    /// </summary>
    public void ResetClip()
    {
        _clipX0 = 0;
        _clipY0 = 0;
        _clipX1 = Width;
        _clipY1 = Height;
    }

    public void Clear()
    {
        // Array.Clear hits memset under the hood — much faster than the obvious 2D for-loops
        // when the surface is large (Sixel mode at 1200×660 is ~800k pixels per frame).
        if (_hb is not null) Array.Clear(_hb, 0, _hb.Length);
        if (_pixels is not null) Array.Clear(_pixels, 0, _pixels.Length);
        Array.Clear(_glyphs, 0, _glyphs.Length);

        // Reset bar-stamp state so each frame builds its own color → glyph mapping from
        // scratch — otherwise an animated chart whose colors drift would slowly exhaust the
        // palette and assignments would change between identical-looking frames.
        if (_barStamps is not null) Array.Clear(_barStamps, 0, _barStamps.Length);
        _barStampPalette.Clear();

        // A stale clip from a prior frame would silently drop draws the new frame asks for —
        // OnBeginZone will set it again immediately, but reset here so any draw between Clear
        // and the first zone enters with a sane "anywhere on the surface" clip.
        ResetClip();
    }

    public void SetPixel(int x, int y, LvcColor color)
    {
        // Clip + bounds check in one — _clipX0/Y0/X1/Y1 default to the full surface, so this
        // gives the same behavior as the old explicit (x < 0 || x >= Width) test when no clip
        // is active, and tighter rejection when CanvasZone.DrawMargin is in effect.
        if (x < _clipX0 || y < _clipY0 || x >= _clipX1 || y >= _clipY1) return;

        if (color.A < 255) color = BlendWithBackground(color);

        if (_pixels is not null)
        {
            _pixels[y, x] = color;
            return;
        }

        ref var hb = ref _hb![y >> 1, x];
        if ((y & 1) == 0) hb.Top = color;
        else hb.Bottom = color;
    }

    /// <summary>
    /// Flattens a translucent color against <see cref="Background"/>. We don't carry per-
    /// pixel alpha through the encoders (Sixel / cell-grid both want opaque RGB at write
    /// time), so this is the alpha approximation: blend each translucent draw against the
    /// chart's background once at SetPixel time. Skips compositing against whatever was
    /// previously drawn — overlapping translucent shapes won't accumulate, but for chart
    /// fills at terminal pixel densities the visual difference is invisible.
    /// </summary>
    private LvcColor BlendWithBackground(LvcColor color)
    {
        // If detection failed and the user didn't set a background explicitly, fall back
        // to the same dark default the Sixel encoder uses so the math stays consistent.
        var bg = Background.A != 0 ? Background : new LvcColor(20, 20, 20);
        var a = color.A / 255f;
        var inv = 1f - a;
        return new LvcColor(
            (byte)(bg.R * inv + color.R * a),
            (byte)(bg.G * inv + color.G * a),
            (byte)(bg.B * inv + color.B * a));
    }

    public void DrawLine(int x0, int y0, int x1, int y1, LvcColor color)
    {
        var dx = Math.Abs(x1 - x0);
        var dy = -Math.Abs(y1 - y0);
        var sx = x0 < x1 ? 1 : -1;
        var sy = y0 < y1 ? 1 : -1;
        var err = dx + dy;

        while (true)
        {
            SetPixel(x0, y0, color);
            if (x0 == x1 && y0 == y1) break;
            var e2 = 2 * err;
            if (e2 >= dy) { err += dy; x0 += sx; }
            if (e2 <= dx) { err += dx; y0 += sy; }
        }
    }

    public void FillRect(int x, int y, int w, int h, LvcColor color)
    {
        // Normalize negative dimensions before clipping. Stacked-row bar geometries (and the
        // descending half of any "value crosses zero" stack) come through with negative
        // Width — see StackedRowSeriesTest.cs:54's TODO. Without normalization, x + w < x
        // makes the clipped range empty and the bar draws nothing.
        var ax = Math.Min(x, x + w);
        var ay = Math.Min(y, y + h);
        var bx = Math.Max(x, x + w);
        var by = Math.Max(y, y + h);

        var x0 = Math.Max(0, ax);
        var y0 = Math.Max(0, ay);
        var x1 = Math.Min(Width, bx);
        var y1 = Math.Min(Height, by);

        for (var py = y0; py < y1; py++)
            for (var px = x0; px < x1; px++)
                SetPixel(px, py, color);
    }

    /// <summary>
    /// Bar-aware variant of <see cref="FillRect"/>. Fills the pixel grid identically so the
    /// colored encoder still paints the rect, then additionally writes a per-color stamp
    /// character (cycled through <see cref="s_barStampGlyphs"/>) into every cell the rect
    /// fully covers. Used by <see cref="Geometries.RoundedRectangleGeometry"/> — the only
    /// geometry bar series (column / row / stacked-column / stacked-row) use — so plain
    /// mode can give each bar series a distinct texture. Other geometries (pie, line, area,
    /// candlestick) keep calling <see cref="FillRect"/> and their plain rendering is
    /// unchanged.
    /// </summary>
    public void FillRectStamped(int x, int y, int w, int h, LvcColor color)
    {
        // Pixel fill first so the colored encoder still gets a solid rect.
        FillRect(x, y, w, h, color);

        // Sixel renders pixels directly; cell stamps don't apply.
        if (Mode == ConsoleRenderMode.Sixel) return;

        var stamp = StampForColor(color);
        _barStamps ??= new char[CellRows, CellCols];

        // Normalize negative dims (same shape FillRect handles), then intersect with the
        // active clip rect — otherwise stamps would write into cells that FillRect's pixel
        // path correctly rejected (e.g., a bar rect that extends past DrawMargin would
        // bleed stamp glyphs into axis-label rows where no pixels were ever drawn).
        var ax = Math.Max(_clipX0, Math.Min(x, x + w));
        var ay = Math.Max(_clipY0, Math.Min(y, y + h));
        var bx = Math.Min(_clipX1, Math.Max(x, x + w));
        var by = Math.Min(_clipY1, Math.Max(y, y + h));
        if (bx <= ax || by <= ay) return;

        // Round rect bounds to the nearest cell edge (>50% of a cell covered → include it).
        // Avoids two failure modes: rounding inward (only fully-covered cells) leaves the
        // rect's edge as Braille partials like ⣸/⢸/⡇/⣇ — visually jarring next to a shaded
        // interior like ▓ because Unicode has no shaded half-block variants; rounding
        // outward makes bars look ~1 cell taller than their data, throwing off the
        // proportions you can read off the chart. Half-rounding keeps the bar within
        // ±0.5 cells of its true extent while making the whole bar uniformly textured.
        var c0 = Math.Max(0, (ax + CellWidth / 2) / CellWidth);
        var r0 = Math.Max(0, (ay + CellHeight / 2) / CellHeight);
        var c1 = Math.Min(CellCols, (bx + CellWidth / 2) / CellWidth);
        var r1 = Math.Min(CellRows, (by + CellHeight / 2) / CellHeight);

        for (var r = r0; r < r1; r++)
            for (var c = c0; c < c1; c++)
                _barStamps[r, c] = stamp;
    }

    /// <summary>
    /// Returns the bar-fill stamp character assigned to <paramref name="color"/> in this
    /// frame, or '\0' if there's no stamp to use. Mirrors the encoder's rule: stamps are
    /// only meaningful when 2+ distinct bar colors exist (single-series bar charts keep
    /// their Braille look), so this returns '\0' even if the color is known when the
    /// palette only has one entry. Used by the legend to render a per-series texture
    /// swatch that matches what the chart actually shows.
    /// </summary>
    public char TryGetBarStamp(LvcColor color)
    {
        if (_barStampPalette.Count <= 1) return '\0';
        var key = (color.R << 16) | (color.G << 8) | color.B;
        return _barStampPalette.TryGetValue(key, out var stamp) ? stamp : '\0';
    }

    private char StampForColor(LvcColor c)
    {
        var key = (c.R << 16) | (c.G << 8) | c.B;
        if (!_barStampPalette.TryGetValue(key, out var stamp))
        {
            // Always assign a stamp from index 0 — '█' for the first series, '▓' for the
            // second, etc. Whether to actually USE these in plain encoding is decided at
            // encode time by checking palette count: a single-series chart (count == 1)
            // ignores stamps entirely and falls through to the derived Braille glyph
            // (high-resolution single bar look). A multi-series chart (count > 1) uses
            // stamps for all series so the textures stay visually consistent across the
            // chart — no mixing of Braille edges with shaded interiors.
            var idx = _barStampPalette.Count;
            stamp = s_barStampGlyphs[idx % s_barStampGlyphs.Length];
            _barStampPalette[key] = stamp;
        }
        return stamp;
    }

    public void StrokeRect(int x, int y, int w, int h, LvcColor color)
    {
        // Same negative-dim normalization as FillRect — stacked geometries can hand us
        // (x, y, w<0, h>0) and we need to draw the equivalent positive-axis rectangle.
        var ax = Math.Min(x, x + w);
        var ay = Math.Min(y, y + h);
        var bx = Math.Max(x, x + w);
        var by = Math.Max(y, y + h);
        if (bx <= ax || by <= ay) return;

        DrawLine(ax, ay, bx - 1, ay, color);
        DrawLine(ax, by - 1, bx - 1, by - 1, color);
        DrawLine(ax, ay, ax, by - 1, color);
        DrawLine(bx - 1, ay, bx - 1, by - 1, color);
    }

    /// <summary>
    /// Writes text into the cell row that contains pixel-y. Each character takes one cell.
    /// </summary>
    public void DrawText(int xPx, int yPx, string text, LvcColor fg, LvcColor bg = default)
    {
        if (string.IsNullOrEmpty(text)) return;
        var cellRow = yPx / CellHeight;
        if (cellRow < 0 || cellRow >= CellRows) return;

        var col = xPx / CellWidth;
        foreach (var ch in text)
        {
            if (col < 0) { col++; continue; }
            if (col >= CellCols) break;

            _glyphs[cellRow, col] = new GlyphCell { Glyph = ch, Fg = fg, Bg = bg };
            // Glyph supersedes any pixels in the same cell.
            if (_hb is not null)
            {
                _hb[cellRow, col] = default;
            }
            else if (_pixels is not null)
            {
                var px0 = col * CellWidth;
                var py0 = cellRow * CellHeight;
                for (var py = 0; py < CellHeight; py++)
                    for (var px = 0; px < CellWidth; px++)
                        _pixels[py0 + py, px0 + px] = default;
            }
            // Clear any bar-stamp on this cell so axis labels written over a bar render the
            // label, not the stamp — text supersedes stamp the same way it supersedes pixels.
            if (_barStamps is not null) _barStamps[cellRow, col] = '\0';
            col++;
        }
    }

    /// <summary>
    /// Encodes the surface into an ANSI string. Includes a leading cursor-home (\x1b[H) when
    /// <paramref name="home"/> is true so successive frames overwrite in place.
    /// </summary>
    /// <param name="home">Prepend the cursor-home escape so a follow-up frame overwrites in place.</param>
    /// <param name="color">
    /// When false, emit a Claude / LLM-friendly plain-glyph rendering: just block / Braille
    /// codepoints and text labels separated by newlines, with no SGR escapes, no cursor-home,
    /// and no terminal-specific bytes. Sixel mode has no meaningful plain form and throws —
    /// switch to <see cref="ConsoleRenderMode.HalfBlock"/> or <see cref="ConsoleRenderMode.Braille"/>.
    /// </param>
    public string ToAnsi(bool home = true, bool color = true)
    {
        if (!color)
        {
            if (Mode == ConsoleRenderMode.Sixel)
                throw new NotSupportedException(
                    "Sixel encodes pixels into escape sequences and has no plain-text form. " +
                    "Use ConsoleRenderMode.HalfBlock or Braille with color=false.");
            return EncodePlainCells();
        }

        return Mode switch
        {
            ConsoleRenderMode.Sixel => EncodeSixel(home),
            _ => EncodeCells(home),
        };
    }

    private string EncodeCells(bool home)
    {
        var sb = new StringBuilder(CellRows * CellCols * 8);
        if (home) _ = sb.Append("\x1b[H");

        var lastFg = new LvcColor(255, 255, 255);
        var lastBg = Background;
        _ = sb.Append(Esc(lastFg, true));
        _ = sb.Append(Esc(lastBg, false));

        for (var r = 0; r < CellRows; r++)
        {
            for (var c = 0; c < CellCols; c++)
            {
                ResolveCell(r, c, out var glyph, out var fg, out var bg);

                if (!Equal(fg, lastFg)) { _ = sb.Append(Esc(fg, true)); lastFg = fg; }
                if (!Equal(bg, lastBg)) { _ = sb.Append(Esc(bg, false)); lastBg = bg; }
                _ = sb.Append(glyph);
            }
            if (r < CellRows - 1) _ = sb.Append('\n');
        }

        _ = sb.Append("\x1b[0m");
        return sb.ToString();
    }

    private string EncodeSixel(bool home)
    {
        var sb = new StringBuilder(Width * Height / 3);

        // Reset SGR up-front so anything the terminal paints around the image (cells made
        // hybrid by a non-aligned cell height, rows added by Sixel-induced scroll) inherits
        // the terminal's *default* background, not whatever SGR happened to be active when
        // we got here (e.g. PSReadLine's prompt color).
        if (home) _ = sb.Append("\x1b[0m\x1b[H");

        // Labels are rasterized into the pixel grid by LabelGeometry, so this is a single
        // self-contained DCS block — no cell-text overlay pass needed.
        _ = sb.Append(SixelEncoder.Encode(_pixels!, Background));

        // Reset SGR but DON'T erase below or park the cursor: after the DCS block the
        // terminal places the cursor inside the cell row that contains the image's bottom
        // edge (a partial cell), and any \x1b[J or absolute-row positioning issued from
        // there clobbers that row — which usually contains the X-axis labels. Cell-size
        // alignment (via terminal cell-pixel-size detection in InMemoryConsoleChart) is the
        // right way to avoid the strip-below-image artifact; per-frame erasure isn't.
        _ = sb.Append("\x1b[0m");
        return sb.ToString();
    }

    /// <summary>
    /// Plain-text encoder: emits one glyph per cell with newlines between rows and nothing
    /// else. Designed for consumers that read the output as text (LLM tool results, log
    /// scrapers, golden files) where ANSI escapes would be noise. Two-color half-block
    /// cells collapse to '█' since the color contrast that distinguished them is gone.
    /// </summary>
    private string EncodePlainCells()
    {
        var sb = new StringBuilder(CellRows * (CellCols + 1));

        for (var r = 0; r < CellRows; r++)
        {
            for (var c = 0; c < CellCols; c++)
                _ = sb.Append(GlyphForPlain(r, c));
            if (r < CellRows - 1) _ = sb.Append('\n');
        }

        return sb.ToString();
    }

    private char GlyphForPlain(int r, int c)
    {
        var g = _glyphs[r, c];
        if (g.Glyph != '\0') return g.Glyph;

        // Bar stamps: bar-series geometries (RoundedRectangleGeometry → FillRectStamped)
        // record a per-color glyph in this grid for cells they fully cover. Pies, lines,
        // areas, etc. don't, so their cells stay at '\0' and fall through to the original
        // derived-glyph logic below. Stamps are only USED when there are 2+ distinct bar
        // colors in this frame — a single-series bar chart falls through to Braille for
        // its high-resolution single-bar look.
        if (_barStamps is not null && _barStampPalette.Count > 1)
        {
            var stamp = _barStamps[r, c];
            if (stamp != '\0') return stamp;
        }

        if (_pixels is not null)
        {
            // Braille — same bit packing as ResolveCell, just without picking up the color.
            var bits = 0;
            var baseY = r * CellHeight;
            var baseX = c * CellWidth;
            for (var dy = 0; dy < CellHeight; dy++)
            {
                for (var dx = 0; dx < CellWidth; dx++)
                {
                    if (_pixels[baseY + dy, baseX + dx].A == 0) continue;
                    var bit = dx == 0
                        ? (dy < 3 ? dy : 6)
                        : (dy < 3 ? 3 + dy : 7);
                    bits |= 1 << bit;
                }
            }
            return bits == 0 ? ' ' : (char)(0x2800 | bits);
        }

        var hb = _hb![r, c];
        var hasTop = hb.Top.A != 0;
        var hasBot = hb.Bottom.A != 0;
        if (!hasTop && !hasBot) return ' ';
        if (hasTop && !hasBot) return '▀';
        if (!hasTop) return '▄'; // hasBot is implied — prior branches handled both-clear and top-only.
        // Two colors in one cell normally pick '▀' with bg=bottom-color so the colored
        // encoder can paint both halves. Without color the contrast disappears, so collapse
        // to the both-filled glyph instead — '▀' with no contrast looks half-empty.
        return '█';
    }

    private void ResolveCell(int r, int c, out char glyph, out LvcColor fg, out LvcColor bg)
    {
        var g = _glyphs[r, c];
        if (g.Glyph != '\0')
        {
            glyph = g.Glyph;
            fg = g.Fg;
            bg = g.Bg.A == 0 ? Background : g.Bg;
            return;
        }

        if (_pixels is not null)
        {
            // Braille path — sample 2×4 dots from the pixel grid into a Braille codepoint.
            var bits = 0;
            LvcColor pickedColor = default;
            var baseY = r * CellHeight;
            var baseX = c * CellWidth;
            for (var dy = 0; dy < CellHeight; dy++)
            {
                for (var dx = 0; dx < CellWidth; dx++)
                {
                    var p = _pixels[baseY + dy, baseX + dx];
                    if (p.A == 0) continue;
                    var bit = dx == 0
                        ? (dy < 3 ? dy : 6)
                        : (dy < 3 ? 3 + dy : 7);
                    bits |= 1 << bit;
                    pickedColor = p; // last write wins
                }
            }
            if (bits == 0) { glyph = ' '; fg = Background; bg = Background; }
            else { glyph = (char)(0x2800 | bits); fg = pickedColor; bg = Background; }
            return;
        }

        var hb = _hb![r, c];
        var hasTop = hb.Top.A != 0;
        var hasBot = hb.Bottom.A != 0;
        if (!hasTop && !hasBot) { glyph = ' '; fg = Background; bg = Background; }
        else if (hasTop && !hasBot) { glyph = '▀'; fg = hb.Top; bg = Background; }
        else if (!hasTop) { glyph = '▄'; fg = hb.Bottom; bg = Background; } // hasBot implied here.
        else if (Equal(hb.Top, hb.Bottom)) { glyph = '█'; fg = hb.Top; bg = Background; }
        else { glyph = '▀'; fg = hb.Top; bg = hb.Bottom; }
    }

    private static string Esc(LvcColor c, bool foreground) =>
        c.A == 0
            ? (foreground ? "\x1b[39m" : "\x1b[49m")  // default fg/bg → terminal shows through
            : $"\x1b[{(foreground ? 38 : 48)};2;{c.R};{c.G};{c.B}m";

    private static bool Equal(LvcColor a, LvcColor b) =>
        a.R == b.R && a.G == b.G && a.B == b.B && a.A == b.A;
}
