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

using Eto.Forms;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Sketches;

namespace LiveChartsGeneratedCode;

// ==============================================================================
// Eto-specific base class for cartesian / pie / polar controls. Drawn-view
// scaffolding (MotionCanvas hosting, lifecycle wiring via OnLoadComplete /
// OnUnLoad, CoreCanvas / ControlSize) lives in SourceGenDrawnView.eto.cs.
// Chart-specific plumbing (observer lifecycle, modifier-free pointer handlers
// that distinguish primary vs secondary) lives here.
// ==============================================================================

/// <inheritdoc cref="IChartView" />
public abstract partial class SourceGenChart : SourceGenDrawnView, IChartView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SourceGenChart"/> class.
    /// </summary>
    protected SourceGenChart()
    {
        InitializeChartControl();
        InitializeObservedProperties();

        Content.MouseDown += OnMouseDown;
        Content.MouseMove += OnMouseMove;
        Content.MouseUp += OnMouseUp;
        Content.MouseLeave += OnMouseLeave;
    }

    LvcColor IChartView.BackColor =>
        new((byte)BackgroundColor.Rb, (byte)BackgroundColor.Gb, (byte)BackgroundColor.Bb, (byte)BackgroundColor.Ab);

    /// <inheritdoc />
    protected override void OnDrawnViewSizeChanged() => CoreChart?.Update();

    /// <inheritdoc />
    protected override void OnDrawnViewLoaded()
    {
        StartObserving();
        CoreChart.Load();
    }

    /// <inheritdoc />
    protected override void OnDrawnViewUnloaded()
    {
        StopObserving();
        CoreChart.Unload();
    }

    private void OnMouseMove(object? sender, MouseEventArgs e)
    {
        base.OnMouseMove(e);
        CoreChart?.InvokePointerMove(new LvcPoint(e.Location.X, e.Location.Y));
    }

    private void OnMouseDown(object? sender, MouseEventArgs e) =>
        //if (ModifierKeys > 0) return; // is this supported in Eto.Forms?
        CoreChart?.InvokePointerDown(new LvcPoint(e.Location.X, e.Location.Y), e.Buttons != MouseButtons.Primary);

    private void OnMouseUp(object? sender, MouseEventArgs e) =>
        CoreChart?.InvokePointerUp(new LvcPoint(e.Location.X, e.Location.Y), e.Buttons != MouseButtons.Primary);

    private void OnMouseLeave(object? sender, MouseEventArgs e) =>
        CoreChart?.InvokePointerLeft();
}
