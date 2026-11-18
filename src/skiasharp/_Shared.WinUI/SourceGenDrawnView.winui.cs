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
using System.Runtime.InteropServices;
using LiveChartsCore.Drawing;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace LiveChartsGeneratedCode;

/// <inheritdoc cref="SourceGenDrawnView" />
public abstract partial class SourceGenDrawnView : UserControl
{
    private static readonly bool s_isWebAssembly = RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER"));
    private bool _wasInViewport;

    /// <summary>
    /// Initializes a new instance of the <see cref="SourceGenDrawnView"/> class.
    /// </summary>
    protected SourceGenDrawnView()
    {
        Content = new MotionCanvas();

        SizeChanged += (_, _) => OnDrawnViewSizeChanged();
        Loaded += (_, _) => OnDrawnViewLoaded();
        Unloaded += (_, _) =>
        {
            // When the same element is removed and added back at once (e.g. a ContentControl's
            // content set to null and back), WinUI raises Loaded for the re-add first and then the
            // Unloaded for the removal, while the element is in the tree again and IsLoaded is still
            // true. Unloading then left the chart unloaded and blank. Ignore that stale Unloaded.
            if (IsLoaded) return;

            OnDrawnViewUnloaded();
            _wasInViewport = false;
        };
        EffectiveViewportChanged += OnEffectiveViewportChanged;
    }

    private MotionCanvas MotionCanvas => (MotionCanvas)Content;

    /// <inheritdoc cref="IDrawnView.CoreCanvas" />
    public CoreMotionCanvas CoreCanvas => MotionCanvas.CanvasCore;

    /// <inheritdoc cref="IDrawnView.ControlSize" />
    public LvcSize ControlSize => new() { Width = (float)ActualWidth, Height = (float)ActualHeight };

    /// <summary>WinUI has no built-in designer signal.</summary>
    public virtual bool DesignerMode => false;

    /// <summary>
    /// ActualTheme resolves the chain: this element's RequestedTheme -> any ancestor
    /// FrameworkElement.RequestedTheme -> Application.RequestedTheme -> system theme.
    /// See #2004.
    /// </summary>
    public virtual bool IsDarkMode => ActualTheme == ElementTheme.Dark;

    /// <summary>Marshals an action onto the WinUI dispatcher (or runs inline on WASM).</summary>
    public void InvokeOnUIThread(Action action)
    {
        if (s_isWebAssembly)
        {
            action();
            return;
        }

        _ = DispatcherQueue.TryEnqueue(
            Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () => action());
    }

    // Same fix as the Avalonia view for https://github.com/Live-Charts/LiveCharts2/issues/1986
    // EffectiveViewport reports the ancestor scroll viewport in this control's local
    // coordinates, NOT whether the chart is in view, so we check whether it intersects
    // the chart's local bounds. Sitting on the shared base means BOTH cartesian and map get the fix.
    private void OnEffectiveViewportChanged(FrameworkElement sender, EffectiveViewportChangedEventArgs e)
    {
        var vp = e.EffectiveViewport;
        var nowInViewport =
            vp.Width > 0 && vp.Height > 0 &&
            vp.X < ActualWidth && vp.X + vp.Width > 0 &&
            vp.Y < ActualHeight && vp.Y + vp.Height > 0;

        if (nowInViewport && !_wasInViewport)
        {
            // Uno Platform 7 skips painting elements outside the viewport, so a chart that
            // loads or resizes off-screen renders no frame and Chart.IsRendering() blocks its
            // measure. On transition into the viewport, mark the canvas visible so
            // IsRendering() allows the next measure, and request an Update.
            CoreCanvas.NotifyPlatformVisible();
            OnDrawnViewReturnedToViewport();
        }
        _wasInViewport = nowInViewport;
    }

    /// <summary>
    /// Hook called when the control becomes visible after being scrolled out of /
    /// switched away from the viewport. Default no-op; subclasses request an update.
    /// </summary>
    protected virtual void OnDrawnViewReturnedToViewport() { }
}
