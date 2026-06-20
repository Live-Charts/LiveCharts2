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
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView.WinForms;

namespace LiveChartsGeneratedCode;

/// <inheritdoc cref="SourceGenDrawnView" />
[DesignerCategory("")]
public abstract partial class SourceGenDrawnView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SourceGenDrawnView"/> class.
    /// </summary>
    protected SourceGenDrawnView()
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
        ResumeLayout(true);

        motionCanvas.Resize += (_, _) => OnDrawnViewSizeChanged();
    }

    /// <inheritdoc cref="IDrawnView.CoreCanvas" />
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public CoreMotionCanvas CoreCanvas => ((MotionCanvas)Controls[0]).CanvasCore;

    /// <inheritdoc cref="IDrawnView.ControlSize" />
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public LvcSize ControlSize => new() { Width = Width, Height = Height };

    /// <summary>Whether this control is hosted inside the Visual Studio designer.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public virtual bool DesignerMode => LicenseManager.UsageMode == LicenseUsageMode.Designtime;

    /// <summary>WinForms has no built-in dark-mode signal; subclasses override if they need one.</summary>
    public virtual bool IsDarkMode => false;

    /// <summary>
    /// Returns the actual drawn control (the SKControl/SKGLControl inside MotionCanvas).
    /// Pointer events should be wired on this — see issue #1209.
    /// </summary>
    public Control GetDrawnControl() => Controls[0].Controls[0];

    /// <summary>Marshals an action onto the WinForms UI thread.</summary>
    public void InvokeOnUIThread(Action action)
    {
        if (!IsHandleCreated) return;
        _ = BeginInvoke(action);
    }

    /// <inheritdoc cref="ContainerControl.OnParentChanged(EventArgs)"/>
    protected override void OnParentChanged(EventArgs e)
    {
        base.OnParentChanged(e);
        OnDrawnViewLoaded();
    }

    /// <inheritdoc cref="Control.OnHandleDestroyed(EventArgs)"/>
    protected override void OnHandleDestroyed(EventArgs e)
    {
        base.OnHandleDestroyed(e);
        OnDrawnViewUnloaded();
    }
}
