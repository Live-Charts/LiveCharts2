// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

namespace LiveChartsCore.Console;

/// <summary>
/// Selects how the off-screen sub-pixel grid is encoded into terminal cells.
/// </summary>
public enum ConsoleRenderMode
{
    /// <summary>
    /// One terminal cell = 1 column × 2 rows of sub-pixels, encoded with the upper/lower
    /// half-block characters (▀ ▄ █). Each cell can hold two distinct colors (top + bottom),
    /// so overlapping series stay visibly separated.
    /// </summary>
    HalfBlock = 0,

    /// <summary>
    /// One terminal cell = 2 columns × 4 rows of sub-pixels, encoded as Braille codepoints
    /// (U+2800–U+28FF). Quadruples the effective resolution at the cost of a single color
    /// per cell (last-write-wins when multiple series cross the same cell).
    /// </summary>
    Braille = 1,

    /// <summary>
    /// True-pixel raster graphics emitted as a DCS Sixel block, with axis labels overlaid as
    /// cell-positioned ANSI text. Highest fidelity, but only renders on Sixel-capable terminals
    /// (Windows Terminal 1.22+, iTerm2, WezTerm, ghostty, modern Konsole, foot, mlterm, recent
    /// VSCode). On unsupported terminals, the Sixel block is dropped as junk text.
    /// </summary>
    Sixel = 2,
}
