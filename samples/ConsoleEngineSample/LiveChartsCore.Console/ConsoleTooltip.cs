// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Console.Drawing;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;

namespace LiveChartsCore.Console;

/// <summary>
/// IChartTooltip impl for the console engine. The chart engine calls Show/Hide in response to
/// pointer-move/leave events; we capture the hovered points and render an overlay box on top
/// of the surface during the next <see cref="InMemoryConsoleChart.RenderFrame"/> pass — so the
/// box always reflects the latest hover state and can't get out of sync with the chart pixels.
/// </summary>
public class ConsoleTooltip : IChartTooltip
{
    private List<ChartPoint>? _shownPoints;

    public void Show(IEnumerable<ChartPoint> foundPoints, Chart chart) =>
        // Materialize the enumerable — the chart engine reuses its hover-finder buffers, so
        // a deferred enumeration could read stale state by the time we render.
        _shownPoints = [.. foundPoints];

    public void Hide(Chart chart) =>
        _shownPoints = null;

    /// <summary>
    /// Draws the tooltip box onto the surface if Show has been called and Hide hasn't undone
    /// it. Called by InMemoryConsoleChart.RenderFrame after the chart's main DrawFrame, so it
    /// lands on top of the chart pixels.
    /// </summary>
    internal void Render(ConsoleSurface surface)
    {
        var pts = _shownPoints;
        if (pts is null || pts.Count == 0) return;

        var lines = BuildLines(pts);
        if (lines.Count == 0) return;

        var (charW, charH) = surface.Mode == ConsoleRenderMode.Sixel
            ? (BitmapFont.CellWidth(), BitmapFont.CellHeight())
            : (surface.CellWidth, surface.CellHeight);

        var maxLineChars = lines.Max(l => l.Length);
        const int padCols = 1;
        var boxW = (maxLineChars + 2 * padCols) * charW;
        var boxH = lines.Count * charH + 2; // +2 for a thin border breathing room

        // Anchor near the first hovered point's center. Geometry's X/Y is the top-left of the
        // visual rect; add half-extent to land on the point itself. Visual lives on
        // ChartPoint.Context (the typed accessor on ChartPoint<TModel,TVisual,TLabel> wraps
        // the same field, but we don't have the type parameters here).
        var first = pts[0];
        var visual = first.Context.Visual as BoundedDrawnGeometry;
        var anchorX = (int)((visual?.X ?? 0) + (visual?.Width ?? 0) / 2);
        var anchorY = (int)((visual?.Y ?? 0) + (visual?.Height ?? 0) / 2);

        // Default placement: top-right of the point. Flip if we'd run off the surface.
        var boxX = anchorX + 8;
        var boxY = anchorY - boxH - 4;
        if (boxX + boxW > surface.Width) boxX = anchorX - boxW - 8;
        if (boxY < 0) boxY = anchorY + 8;
        if (boxX < 0) boxX = 0;
        if (boxY + boxH > surface.Height) boxY = surface.Height - boxH - 1;

        var bg = new LvcColor(45, 45, 55);
        var border = new LvcColor(170, 170, 180);
        var fg = new LvcColor(235, 235, 240);

        surface.FillRect(boxX, boxY, boxW, boxH, bg);
        surface.StrokeRect(boxX, boxY, boxW, boxH, border);

        for (var i = 0; i < lines.Count; i++)
        {
            var lineX = boxX + padCols * charW;
            var lineY = boxY + 1 + i * charH;
            if (surface.Mode == ConsoleRenderMode.Sixel)
            {
                // Sixel writes glyph pixels directly into the rasterized image — the box
                // FillRect we already did is fully visible behind them. No per-glyph bg needed.
                BitmapFont.DrawText(surface, lineX, lineY, lines[i], fg);
            }
            else
            {
                // Cell-grid modes write glyph cells that supersede any pixels beneath them
                // (so the FillRect we did doesn't show through where text is). Pass `bg`
                // explicitly so each text cell renders on the tooltip box's background
                // instead of falling back to the surface's terminal-default color.
                surface.DrawText(lineX, lineY, lines[i], fg, bg);
            }
        }
    }

    private static List<string> BuildLines(List<ChartPoint> points)
    {
        var lines = new List<string>();

        // First line: the X-axis context label (e.g. "Mon" for a categorical X, "5" for a
        // numeric one). Pulled from the first point — series share the same secondary axis.
        var first = points[0];
        var sec = first.Context.Series.GetSecondaryToolTipText(first);
        if (!string.IsNullOrEmpty(sec) && sec != LiveCharts.IgnoreToolTipLabel)
            lines.Add(sec!);

        // Following lines: per-series "<name>: <value>".
        foreach (var p in points)
        {
            var val = p.Context.Series.GetPrimaryToolTipText(p);
            if (val is null || val == LiveCharts.IgnoreToolTipLabel) continue;

            var name = p.Context.Series.Name;
            lines.Add(string.IsNullOrEmpty(name) || name == LiveCharts.IgnoreSeriesName
                ? val
                : $"{name}: {val}");
        }

        return lines;
    }
}
