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
using Eto.Drawing;
using Eto.Forms;
using LiveChartsCore.Drawing;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView.Eto;

namespace LiveChartsGeneratedCode;

/// <inheritdoc cref="SourceGenDrawnView" />
public abstract partial class SourceGenDrawnView : Panel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SourceGenDrawnView"/> class.
    /// </summary>
    protected SourceGenDrawnView()
    {
        Content = new MotionCanvas();
        BackgroundColor = Colors.White;

        Content.SizeChanged += (_, _) => OnDrawnViewSizeChanged();
    }

    /// <inheritdoc cref="IDrawnView.CoreCanvas" />
    public CoreMotionCanvas CoreCanvas => ((MotionCanvas)Content).CanvasCore;

    /// <inheritdoc cref="IDrawnView.ControlSize" />
    public LvcSize ControlSize => new() { Width = Content.Width, Height = Content.Height };

    /// <summary>Eto.Forms has no built-in designer signal.</summary>
    public virtual bool DesignerMode => false;

    /// <summary>Eto.Forms has no built-in dark-mode signal.</summary>
    public virtual bool IsDarkMode => false;

    /// <summary>Marshals an action onto the Eto.Forms application thread.</summary>
    public void InvokeOnUIThread(Action action) =>
        _ = Application.Instance.InvokeAsync(action);

    /// <inheritdoc cref="Control.OnLoadComplete(EventArgs)" />
    protected override void OnLoadComplete(EventArgs e)
    {
        base.OnLoadComplete(e);
        OnDrawnViewLoaded();
    }

    /// <inheritdoc cref="Control.OnUnLoad(EventArgs)" />
    protected override void OnUnLoad(EventArgs e)
    {
        base.OnUnLoad(e);
        OnDrawnViewUnloaded();
    }
}
