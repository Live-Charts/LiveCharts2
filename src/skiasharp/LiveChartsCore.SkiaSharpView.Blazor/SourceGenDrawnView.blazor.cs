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
using LiveChartsCore.Drawing;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView.Blazor;
using Microsoft.AspNetCore.Components;

namespace LiveChartsGeneratedCode;

/// <inheritdoc cref="SourceGenDrawnView" />
public abstract partial class SourceGenDrawnView : ComponentBase, IDisposable
{
#pragma warning disable IDE0032 // Use auto property, blazor ref
    /// <summary>
    /// The MotionCanvas component reference captured during <see cref="BuildRenderTree"/>.
    /// Subclasses pass <c>r =&gt; _motionCanvas = (MotionCanvas)r</c> to
    /// <c>builder.AddComponentReferenceCapture</c>.
    /// </summary>
    protected MotionCanvas _motionCanvas = null!;
#pragma warning restore IDE0032 // Use auto property

    /// <inheritdoc cref="IDrawnView.CoreCanvas" />
    public CoreMotionCanvas CoreCanvas => _motionCanvas.CanvasCore;

    /// <inheritdoc cref="IDrawnView.ControlSize" />
    public LvcSize ControlSize => new()
    {
        Width = _motionCanvas.Width,
        Height = _motionCanvas.Height
    };

    /// <summary>Blazor has no built-in designer signal.</summary>
    public virtual bool DesignerMode => false;

    /// <summary>Blazor has no portable dark-mode signal.</summary>
    public virtual bool IsDarkMode => false;

    /// <summary>Marshals an action onto the Blazor renderer thread.</summary>
    public void InvokeOnUIThread(Action action) => _ = InvokeAsync(action);

    /// <inheritdoc />
    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender) return;

        _motionCanvas.SizeChanged += OnDrawnViewSizeChanged;
        OnDrawnViewLoaded();
    }

    /// <inheritdoc cref="IDisposable.Dispose" />
    void IDisposable.Dispose() => OnDrawnViewUnloaded();
}
