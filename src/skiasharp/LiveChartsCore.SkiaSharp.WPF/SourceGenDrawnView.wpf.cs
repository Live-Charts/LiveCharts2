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
using System.Windows.Controls;
using LiveChartsCore.Drawing;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView.WPF;

namespace LiveChartsGeneratedCode;

/// <inheritdoc cref="SourceGenDrawnView" />
public abstract partial class SourceGenDrawnView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SourceGenDrawnView"/> class.
    /// Sets the <see cref="MotionCanvas"/> as content and wires size / load /
    /// unload events into the hooks subclasses override.
    /// </summary>
    protected SourceGenDrawnView()
    {
        Content = new MotionCanvas();

        SizeChanged += (_, _) => OnDrawnViewSizeChanged();
        Loaded += (_, _) => OnDrawnViewLoaded();
        Unloaded += (_, _) => OnDrawnViewUnloaded();
    }

    private MotionCanvas MotionCanvas => (MotionCanvas)Content;

    /// <inheritdoc cref="IDrawnView.CoreCanvas" />
    public CoreMotionCanvas CoreCanvas => MotionCanvas.CanvasCore;

    /// <inheritdoc cref="IDrawnView.ControlSize" />
    public LvcSize ControlSize => new() { Width = (float)ActualWidth, Height = (float)ActualHeight };

    /// <summary>Whether this control is hosted inside the Visual Studio designer.</summary>
    public virtual bool DesignerMode => DesignerProperties.GetIsInDesignMode(this);

    /// <summary>Whether the surrounding app is in dark mode.</summary>
    public virtual bool IsDarkMode => false;

    /// <summary>Marshals an action onto the WPF dispatcher.</summary>
    public void InvokeOnUIThread(Action action) => Dispatcher.Invoke(action);
}
