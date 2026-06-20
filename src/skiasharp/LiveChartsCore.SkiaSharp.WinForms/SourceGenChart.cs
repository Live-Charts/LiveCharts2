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

using System.ComponentModel;
using System.Windows.Forms;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Sketches;

namespace LiveChartsGeneratedCode;

// ==============================================================================
// WinForms-specific base class for cartesian / pie / polar controls. Drawn-view
// scaffolding (MotionCanvas hosting, lifecycle wiring via OnParentChanged /
// OnHandleDestroyed, CoreCanvas / ControlSize / DesignerMode / GetDrawnControl)
// lives in SourceGenDrawnView.winforms.cs. Chart-specific plumbing (modifier-aware
// pointer handlers, observer lifecycle, series template inflation) lives here.
// ==============================================================================

/// <inheritdoc cref="IChartView" />
[DesignerCategory("")]
public abstract partial class SourceGenChart : SourceGenDrawnView, IChartView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SourceGenChart"/> class.
    /// </summary>
    protected SourceGenChart()
    {
        Name = "Chart";

        InitializeChartControl();
        InitializeObservedProperties();

        var c = GetDrawnControl();
        c.MouseDown += OnMouseDown;
        c.MouseMove += OnMouseMove;
        c.MouseUp += OnMouseUp;
        c.MouseClick += OnMouseClick;
        c.MouseDoubleClick += OnMouseDoubleClick;
        c.MouseLeave += OnMouseLeave;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    LvcColor IChartView.BackColor => new(BackColor.R, BackColor.G, BackColor.B, BackColor.A);

    /// <inheritdoc />
    protected override void OnDrawnViewSizeChanged() => CoreChart?.Update();

    /// <inheritdoc />
    protected override void OnDrawnViewLoaded()
    {
        StartObserving();
        CoreChart?.Load();
    }

    /// <inheritdoc />
    protected override void OnDrawnViewUnloaded()
    {
        StopObserving();
        CoreChart?.Unload();
    }

    private void OnMouseMove(object? sender, MouseEventArgs e)
    {
        base.OnMouseMove(e);
        CoreChart?.InvokePointerMove(new LvcPoint(e.Location.X, e.Location.Y));
    }

    private void OnMouseDown(object? sender, MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (ModifierKeys > 0) return;
        CoreChart?.InvokePointerDown(new LvcPoint(e.Location.X, e.Location.Y), e.Button == MouseButtons.Right);
    }

    private void OnMouseUp(object? sender, MouseEventArgs e)
    {
        base.OnMouseUp(e);
        CoreChart?.InvokePointerUp(new LvcPoint(e.Location.X, e.Location.Y), e.Button == MouseButtons.Right);
    }

    private void OnMouseClick(object? sender, MouseEventArgs e) =>
        base.OnMouseClick(e);

    private void OnMouseDoubleClick(object? sender, MouseEventArgs e) =>
        base.OnMouseDoubleClick(e);

    private void OnMouseLeave(object? sender, System.EventArgs e)
    {
        base.OnMouseLeave(e);
        CoreChart?.InvokePointerLeft();
    }
}
