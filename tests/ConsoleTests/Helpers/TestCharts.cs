// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Console;
using LiveChartsCore.Drawing;

namespace ConsoleTests.Helpers;

/// <summary>
/// Factories for golden-friendly chart configurations. All charts:
///   * use <see cref="ConsoleRenderMode.Braille"/> — fixed 2×4 sub-pixel cells, no terminal-
///     specific cell-pixel-size detection (Sixel) and no half-block ambiguity.
///   * set an explicit dark background — sidesteps the OSC 11 query that
///     <see cref="InMemoryConsoleChart"/> would otherwise issue on first render.
///   * size in pixels directly (not <c>ConfigureFromTerminalCells</c>) so the output is
///     independent of <see cref="System.Console.WindowWidth"/>.
/// </summary>
internal static class TestCharts
{
    // 80 cells × 24 cells in Braille = 160 × 96 sub-pixels. Modest enough to keep goldens
    // readable in PR diffs, large enough for axes/legend to lay out without collapsing.
    public const int Width = 160;
    public const int Height = 96;

    public static readonly LvcColor Background = new(20, 20, 20);

    public static T Configure<T>(T chart) where T : InMemoryConsoleChart
    {
        chart.RenderMode = ConsoleRenderMode.Braille;
        chart.Width = Width;
        chart.Height = Height;
        chart.Background = Background;
        return chart;
    }
}
