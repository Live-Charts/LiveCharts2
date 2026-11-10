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
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Rendering;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using LiveChartsCore.Drawing;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView.Avalonia;

namespace LiveChartsGeneratedCode;

/// <inheritdoc cref="SourceGenDrawnView" />
public abstract partial class SourceGenDrawnView : UserControl, ICustomHitTest
{
    private bool _wasInViewport;
    private bool _wasEffectivelyVisible;
    private readonly List<Visual> _visibilitySources = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="SourceGenDrawnView"/> class.
    /// </summary>
    protected SourceGenDrawnView()
    {
        Content = new MotionCanvas();

        SizeChanged += (_, _) => OnDrawnViewSizeChanged();
        AttachedToVisualTree += (_, _) =>
        {
            OnDrawnViewLoaded();
            WatchVisibility();
        };
        DetachedFromVisualTree += (_, _) =>
        {
            UnwatchVisibility();
            OnDrawnViewUnloaded();
            _wasInViewport = false;
        };
        EffectiveViewportChanged += OnEffectiveViewportChanged;
    }

    private MotionCanvas CanvasView => (MotionCanvas)Content!;

    /// <inheritdoc cref="IDrawnView.CoreCanvas" />
    public CoreMotionCanvas CoreCanvas => CanvasView.CanvasCore;

    /// <inheritdoc cref="IDrawnView.ControlSize" />
    public LvcSize ControlSize => new()
    {
        Width = (float)CanvasView.Bounds.Width,
        Height = (float)CanvasView.Bounds.Height
    };

    /// <summary>Whether this control is hosted inside the Avalonia designer.</summary>
    public virtual bool DesignerMode => Design.IsDesignMode;

    /// <summary>Whether Avalonia is in dark mode.</summary>
    public virtual bool IsDarkMode =>
        Application.Current?.ActualThemeVariant == ThemeVariant.Dark;

    /// <summary>Marshals an action onto the Avalonia UI thread.</summary>
    public void InvokeOnUIThread(Action action) => Dispatcher.UIThread.Post(action);

    bool ICustomHitTest.HitTest(Point point) => new Rect(Bounds.Size).Contains(point);

    // Fix for https://github.com/Live-Charts/LiveCharts2/issues/1986
    // EffectiveViewport reports the ancestor scroll viewport in this control's
    // local coordinates, NOT whether the chart is in view. To detect "in view"
    // we check whether the viewport intersects the chart's local bounds.
    // Sitting on the shared base means BOTH cartesian and map get the fix.
    private void OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        var vp = e.EffectiveViewport;
        var nowInViewport =
            vp.Width > 0 && vp.Height > 0 &&
            vp.X < Bounds.Width && vp.X + vp.Width > 0 &&
            vp.Y < Bounds.Height && vp.Y + vp.Height > 0;

        if (nowInViewport && !_wasInViewport)
        {
            // When a chart is off-screen (scrolled out of a ScrollViewer or in an
            // inactive tab) Avalonia stops painting the canvas; Chart.IsRendering()
            // then blocks the measure to avoid wasted work. On transition back into
            // the viewport, mark the canvas visible so IsRendering() allows the next
            // measure, and request an Update.
            CoreCanvas.NotifyPlatformVisible();
            OnDrawnViewReturnedToViewport();
        }
        _wasInViewport = nowInViewport;
    }

    // Fix for https://github.com/Live-Charts/LiveCharts2/issues/2405
    // While the chart or an ancestor has IsVisible=false, MotionCanvas stops asking for frames and no frame is drawn,
    // so Chart.IsRendering() blocks the measure as in #1986. Showing a hidden ancestor does not raise
    // EffectiveViewportChanged, and Visual.IsEffectivelyVisibleChanged is internal, so watch IsVisible on this control
    // and its ancestors (they only change through a detach/attach) and catch up as when returning to the viewport.
    private void WatchVisibility()
    {
        PropertyChanged += OnVisibilitySourceChanged;
        for (var v = this.GetVisualParent(); v is not null; v = v.GetVisualParent())
        {
            v.PropertyChanged += OnVisibilitySourceChanged;
            _visibilitySources.Add(v);
        }
        _wasEffectivelyVisible = IsEffectivelyVisible;
    }

    private void UnwatchVisibility()
    {
        PropertyChanged -= OnVisibilitySourceChanged;
        foreach (var v in _visibilitySources) v.PropertyChanged -= OnVisibilitySourceChanged;
        _visibilitySources.Clear();
    }

    private void OnVisibilitySourceChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != IsVisibleProperty) return;
        var isEffectivelyVisible = IsEffectivelyVisible;
        if (isEffectivelyVisible && !_wasEffectivelyVisible)
        {
            CoreCanvas.NotifyPlatformVisible();
            CanvasView.InvalidateVisual();
            OnDrawnViewReturnedToViewport();
        }
        _wasEffectivelyVisible = isEffectivelyVisible;
    }

    /// <summary>
    /// Hook called when the control becomes visible after being scrolled out of /
    /// switched away from the viewport. Default no-op; subclasses request an update.
    /// </summary>
    protected virtual void OnDrawnViewReturnedToViewport() { }
}
