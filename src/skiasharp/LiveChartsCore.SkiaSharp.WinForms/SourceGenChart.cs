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

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView.WinForms;

namespace LiveChartsGeneratedCode;

// ==============================================================================
// this file is the base class for this UI framework controls, in this file we
// define the UI framework specific code. 
// expanding this file in the solution explorer will show 2 more files:
//    - *.shared.cs:        shared code between all UI frameworks
//    - *.sgp.cs:           the source generated properties
// ==============================================================================

/// <inheritdoc cref="IChartView" />
[DesignerCategory("")]
public abstract partial class SourceGenChart : UserControl, IChartView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SourceGenChart"/> class.
    /// </summary>
    protected SourceGenChart()
    {
        var motionCanvas = new MotionCanvas();
        SuspendLayout();
        motionCanvas.Dock = DockStyle.Fill;
        motionCanvas.Location = new Point(0, 0);
        motionCanvas.Name = "motionCanvas";
        motionCanvas.Size = new Size(150, 150);
        motionCanvas.TabIndex = 0;
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(motionCanvas);
        Name = "Chart";
        ResumeLayout(true);

        InitializeChartControl();
        InitializeObservedProperties();

        motionCanvas.Resize += (s, e) =>
            CoreChart.Update();

        var c = GetDrawnControl();
        c.MouseDown += OnMouseDown;
        c.MouseMove += OnMouseMove;
        c.MouseUp += OnMouseUp;
        c.MouseClick += OnMouseClick;
        c.MouseDoubleClick += OnMouseDoubleClick;
        c.MouseLeave += OnMouseLeave;
    }

    /// <inheritdoc cref="IDrawnView.CoreCanvas"/>"/>
    public CoreMotionCanvas CoreCanvas => ((MotionCanvas)Controls[0]).CanvasCore;

    bool IChartView.DesignerMode => LicenseManager.UsageMode == LicenseUsageMode.Designtime;

    bool IChartView.IsDarkMode => false;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    LvcColor IChartView.BackColor => new(BackColor.R, BackColor.G, BackColor.B, BackColor.A);

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    LvcSize IDrawnView.ControlSize => new() { Width = Width, Height = Height };

    /// <summary>
    /// Gets the drawn control.
    /// </summary>
    /// <returns></returns>
    public Control GetDrawnControl() => Controls[0].Controls[0];

    void IChartView.InvokeOnUIThread(Action action)
    {
        if (!IsHandleCreated) return;
        _ = BeginInvoke(action);
    }

    /// <inheritdoc cref="ContainerControl.OnParentChanged(EventArgs)"/>
    protected override void OnParentChanged(EventArgs e)
    {
        base.OnParentChanged(e);
        StartObserving();
        CoreChart?.Load();
    }

    /// <summary>
    /// Raises the <see cref="E:HandleDestroyed" /> event.
    /// </summary>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    /// <returns></returns>
    protected override void OnHandleDestroyed(EventArgs e)
    {
        base.OnHandleDestroyed(e);

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

    private void OnMouseLeave(object? sender, EventArgs e)
    {
        base.OnMouseLeave(e);
        CoreChart?.InvokePointerLeft();
    }
}
