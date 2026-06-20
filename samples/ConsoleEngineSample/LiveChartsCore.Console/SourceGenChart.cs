// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore;
using LiveChartsCore.Console;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Sketches;

namespace LiveChartsGeneratedCode;

// Bridge: this is the partial half supplied by the console engine. The shared/sgp halves
// (linked from src/skiasharp/_Shared/) supply the IChartView-facing properties.
public abstract partial class SourceGenChart : InMemoryConsoleChart, IDrawnView
{
    protected SourceGenChart()
    {
        // Defaults (AutoUpdateEnabled = true, EasingFunction from theme) are kept so live
        // RenderLoopAsync drives animations naturally. The one-shot Render() opts out by
        // setting Canvas.DisableAnimations = true at the call site.

        InitializeChartControl();
        InitializeObservedProperties();

        StartObserving();
        CoreChart?.Load();

        // Wire click-to-select: the engine fires DataPointerDown with the hit chart points
        // (or an empty enumerable if the click missed all points). Stash them on the base
        // so RenderSelectedPointsMarker can draw the accent on top of the chart pixels.
        DataPointerDown += (_, points) => SetSelectedPoints(points);
    }

    bool IChartView.DesignerMode => false;
    bool IChartView.IsDarkMode => true; // terminals are usually dark.
    LvcColor IChartView.BackColor => Background;
    LvcSize IDrawnView.ControlSize => new() { Width = Width, Height = Height };

    void IChartView.InvokeOnUIThread(Action action) => action();

    protected override Chart GetCoreChart() => CoreChart;
}
