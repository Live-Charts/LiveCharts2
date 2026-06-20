// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Console.Drawing;
using LiveChartsCore.Console.Painting;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.Painting;

namespace LiveChartsCore.Console;

/// <summary>
/// IChartLegend impl for the console engine. The chart engine calls Measure to find out
/// how much space to reserve (top / bottom / left / right of the draw margin, picked by
/// <c>chart.LegendPosition</c>), then Draw to render. We capture series + colors in
/// Measure and stash them, then paint a single-row entry list from
/// <see cref="InMemoryConsoleChart"/>'s post-DrawFrame overlay pass — same approach as
/// <see cref="ConsoleTooltip"/>, so the legend always lands on top of chart pixels and
/// can't get out of sync with them.
///
/// Position: the engine has already reserved space at the top, bottom, left or right
/// (per LegendPosition). We render in the matching strip of the surface — the bottom row
/// for Top/Bottom positions, the leftmost or rightmost column for Left/Right.
/// </summary>
public class ConsoleLegend : IChartLegend
{
    private readonly List<(string Name, LvcColor Color)> _entries = [];
    private LegendPosition _position = LegendPosition.Hidden;
    private LvcSize _measuredSize;

    public LvcSize Measure(Chart chart)
    {
        _entries.Clear();
        _position = chart.LegendPosition;
        if (_position == LegendPosition.Hidden) return new LvcSize(0, 0);

        foreach (var series in chart.Series)
        {
            if (!series.IsVisibleAtLegend) continue;
            if (series.Name == LiveCharts.IgnoreSeriesName) continue;
            _entries.Add((series.Name ?? string.Empty, GetSeriesColor(series)));
        }

        if (_entries.Count == 0) return new LvcSize(0, 0);

        // Per-cell dimensions. Sixel uses the bitmap-font glyph cell (so the legend text
        // aligns with the chart's axis labels rendered the same way); cell-grid modes use
        // the surface's cell pixel size (1 ANSI char per cell).
        var (charW, charH) = ResolveCharSize(chart);

        // Each entry = swatch(1 cell) + space(1 cell) + name + space(1 cell). Swatch is
        // drawn at the cell width so it reads as a colored block flush with the text.
        var perEntryChars = _entries.Sum(e => 3 + e.Name.Length);

        if (_position is LegendPosition.Left or LegendPosition.Right)
        {
            // Vertical: one row per entry; width = widest entry.
            var widest = _entries.Max(e => 3 + e.Name.Length);
            _measuredSize = new LvcSize(widest * charW, _entries.Count * charH + 2);
        }
        else
        {
            // Horizontal: one row total; height = 1 cell + small padding.
            _measuredSize = new LvcSize(perEntryChars * charW, charH + 2);
        }
        return _measuredSize;
    }

    public void Draw(Chart chart) { /* state captured in Measure; rendered in overlay pass */ }

    public void Hide(Chart chart)
    {
        _entries.Clear();
        _position = LegendPosition.Hidden;
    }

    internal void Render(ConsoleSurface surface)
    {
        if (_position == LegendPosition.Hidden || _entries.Count == 0) return;

        var (charW, charH) = surface.Mode == ConsoleRenderMode.Sixel
            ? (BitmapFont.CellWidth(), BitmapFont.CellHeight())
            : (surface.CellWidth, surface.CellHeight);

        // Pick the strip to paint into — the engine reserved space here when LegendPosition
        // shifted the draw margin, so chart pixels don't conflict.
        int x, y;
        var horizontal = _position is LegendPosition.Top or LegendPosition.Bottom;
        if (horizontal)
        {
            var totalChars = _entries.Sum(e => 3 + e.Name.Length);
            x = (surface.Width - totalChars * charW) / 2;
            y = _position == LegendPosition.Top ? 1 : surface.Height - charH - 1;
        }
        else
        {
            var widest = _entries.Max(e => 3 + e.Name.Length);
            x = _position == LegendPosition.Left ? 1 : surface.Width - widest * charW - 1;
            y = (surface.Height - _entries.Count * charH) / 2;
        }

        var fg = surface.SubtleForeground;
        var cursorX = x;
        var cursorY = y;

        foreach (var (name, color) in _entries)
        {
            // Swatch: prefer the per-series texture stamp when one was assigned by a bar
            // series during this frame (and the chart has 2+ bar series, so stamps are
            // actually visible in the chart body) — that way "█ NA / ▓ EU / ▒ APAC" reads
            // as a direct map from the legend back to what's in the bars. Falls back to a
            // filled-rect colored swatch for everything else (line series, single-bar
            // charts, pie, etc.) which keeps the existing colored-mode look untouched.
            var stamp = surface.TryGetBarStamp(color);
            if (stamp != '\0')
            {
                if (surface.Mode == ConsoleRenderMode.Sixel)
                    BitmapFont.DrawText(surface, cursorX, cursorY, stamp.ToString(), color);
                else
                    surface.DrawText(cursorX, cursorY, stamp.ToString(), color);
            }
            else
            {
                surface.FillRect(cursorX + 1, cursorY + 1, charW - 2, charH - 2, color);
            }

            var labelX = cursorX + charW * 2; // skip swatch + 1 cell of spacing
            if (surface.Mode == ConsoleRenderMode.Sixel)
                BitmapFont.DrawText(surface, labelX, cursorY, name, fg);
            else
                surface.DrawText(labelX, cursorY, name, fg);

            if (horizontal)
            {
                cursorX += (3 + name.Length) * charW;
            }
            else
            {
                cursorY += charH;
            }
        }
    }

    private static LvcColor GetSeriesColor(ISeries series)
    {
        // Series don't expose a single "Color" through ISeries — colors live on the per-
        // type Stroke / Fill paints. The miniature geometry is the cleanest hand-off:
        // it's the same shape a SkiaSharp legend would render, with the right paint
        // already wired up. We pull the SolidColorPaint's color off it; if the paint
        // isn't a SolidColorPaint (gradient, image, etc.) fall back to a neutral gray.
        var miniature = series.GetMiniatureGeometry(null);
        var paint = miniature.Fill ?? miniature.Stroke;
        return paint is SolidColorPaint solid ? solid.Color : new LvcColor(180, 180, 180);
    }

    private static (int charW, int charH) ResolveCharSize(Chart chart) =>
        // Match what LabelGeometry uses for axis labels in each mode so the legend reads
        // at the same density as everything else.
        (Drawing.Geometries.LabelGeometry.GlyphPixelsW, Drawing.Geometries.LabelGeometry.GlyphPixelsH);
}
