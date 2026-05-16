// The MIT License(MIT)
//
// Copyright(c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using System;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;

namespace LiveChartsGeneratedCode;

// ==============================================================================
// 
// this file contains the Skia (image generation) specific code for the SourceGenSKChart class,
// the rest of the code can be found in the _Shared project.
// 
// ==============================================================================

/// <inheritdoc cref="IChartView" />
public abstract partial class SourceGenSKChart : InMemorySkiaSharpChart
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SourceGenSKChart"/> class.
    /// </summary>
    /// <param name="chartView">The chart view to generate the image from.</param>
    public SourceGenSKChart(IChartView? chartView = null)
        : base(chartView)
    {
        EasingFunction = null;
        AutoUpdateEnabled = false;

        InitializeChartControl();
        InitializeObservedProperties();

        StartObserving();
        CoreChart?.Load();
    }

    /// <summary>
    /// Whether this in-memory chart reports itself as running in a designer
    /// host. Always <c>false</c> in production; test fixtures override to
    /// exercise the design-time short-circuit in <see cref="Chart.Load"/>.
    /// </summary>
    protected virtual bool IsDesignerMode => false;

    bool IChartView.DesignerMode => IsDesignerMode;
    bool IChartView.IsDarkMode => false;
    LvcColor IChartView.BackColor => Background.AsLvcColor();
    LvcSize IDrawnView.ControlSize => new() { Width = Width, Height = Height };

    void IChartView.InvokeOnUIThread(Action action) =>
        action();

    /// <inheritdoc cref="InMemorySkiaSharpChart.GetCoreChart"/>
    protected override Chart GetCoreChart() => CoreChart;
}
