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
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView.WinUI;
using LiveChartsCore.Native;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using LiveChartsCore;
using LiveChartsCore.Motion;

namespace LiveChartsGeneratedCode;

// ==============================================================================
// this file is the base class for this UI framework controls, in this file we
// define the UI framework specific code. 
// expanding this file in the solution explorer will show 2 more files:
//    - *.shared.cs:        shared code between all UI frameworks
//    - *.sgp.cs:           the source generated properties
// ==============================================================================

/// <inheritdoc cref="IChartView"/>
public abstract partial class SourceGenChart : UserControl, IChartView
{
    private DateTime _lastTouch;
    private LvcPoint _lastTouchPosition;
    private readonly PointerController _pointerController;
    private static readonly bool s_isWebAssembly = RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER"));
    private bool _wasInViewport;

    /// <summary>
    /// Initializes a new instance of the <see cref="SourceGenChart"/> class.
    /// </summary>
    public SourceGenChart()
    {
        Content = new MotionCanvas();

        SizeChanged += (s, e) =>
            CoreChart.Update();

        InitializeChartControl();
        InitializeObservedProperties();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        EffectiveViewportChanged += OnEffectiveViewportChanged;

        _pointerController = new PointerController();

        _pointerController.Pressed += OnPressed;
        _pointerController.Moved += OnMoved;
        _pointerController.Released += OnReleased;
        _pointerController.Scrolled += OnScrolled;
        _pointerController.Pinched += OnPinched;
        _pointerController.Exited += OnExited;
    }

    private MotionCanvas MotionCanvas => (MotionCanvas)Content;

    /// <inheritdoc cref="IDrawnView.CoreCanvas"/>
    public CoreMotionCanvas CoreCanvas => MotionCanvas.CanvasCore;

    bool IChartView.DesignerMode => false;
    // ActualTheme resolves the chain: this element's RequestedTheme -> any ancestor
    // FrameworkElement.RequestedTheme -> Application.RequestedTheme -> system theme.
    // Reading Application.RequestedTheme directly (the prior implementation) ignored
    // both element-level overrides (the standard WinUI 3 pattern, e.g. setting
    // RequestedTheme on a Window or page) and runtime theme toggles — see #2004.
    bool IChartView.IsDarkMode => ActualTheme == ElementTheme.Dark;
    LvcColor IChartView.BackColor =>
        Background is not SolidColorBrush b
            ? CoreCanvas._virtualBackgroundColor
            : LvcColor.FromArgb(b.Color.A, b.Color.R, b.Color.G, b.Color.B);
    LvcSize IDrawnView.ControlSize => new() { Width = (float)ActualWidth, Height = (float)ActualHeight };

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ActualThemeChanged += OnActualThemeChanged;
        _pointerController.InitializeController(this);
        StartObserving();
        CoreChart.Load();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        // When the same element is removed and added back at once (e.g. a ContentControl's
        // content set to null and back), WinUI raises Loaded for the re-add first and then the
        // Unloaded for the removal, while the element is in the tree again and IsLoaded is still
        // true. Unloading then left the chart unloaded and blank. Ignore that stale Unloaded.
        if (IsLoaded) return;

        ActualThemeChanged -= OnActualThemeChanged;
        _pointerController.DisposeController(this);
        StopObserving();
        CoreChart.Unload();
        _wasInViewport = false;
    }

    // Same fix as the Avalonia view for https://github.com/Live-Charts/LiveCharts2/issues/1986
    // EffectiveViewport reports the ancestor scroll viewport in this control's local
    // coordinates, NOT whether the chart is in view, so we check whether it intersects
    // the chart's local bounds. SourceGenMapChart has the same handler.
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
            CoreChart?.Update();
        }
        _wasInViewport = nowInViewport;
    }

    private void OnActualThemeChanged(FrameworkElement sender, object args) =>
        CoreChart.ApplyTheme();

    private void AddUIElement(object item)
    {
        if (MotionCanvas is null || item is not UIElement uiElement) return;
        MotionCanvas.Children.Add(uiElement);
    }

    private void RemoveUIElement(object item)
    {
        if (MotionCanvas is null || item is not UIElement uiElement) return;
        _ = MotionCanvas.Children.Remove(uiElement);
    }

    private void OnPressed(object? sender, LiveChartsCore.Native.Events.PressedEventArgs args)
    {
        var cArgs = new PointerCommandArgs(this, new(args.Location.X, args.Location.Y), args);
        if (PointerPressedCommand?.CanExecute(cArgs) == true)
            PointerPressedCommand.Execute(cArgs);

        var isSecondary = GestureHelpers.IsDoubleTap(args.Location, _lastTouchPosition, DateTime.Now - _lastTouch);

        CoreChart?.InvokePointerDown(args.Location, args.IsSecondaryPress || isSecondary);

        if (NativeHelpers.IsTouchDevice())
        {
            _lastTouch = DateTime.Now;
            _lastTouchPosition = args.Location;
        }
    }

    private void OnMoved(object? sender, LiveChartsCore.Native.Events.ScreenEventArgs args)
    {
        var location = args.Location;

        var cArgs = new PointerCommandArgs(this, new(location.X, location.Y), args.OriginalEvent);
        if (PointerMoveCommand?.CanExecute(cArgs) == true)
            PointerMoveCommand.Execute(cArgs);

        CoreChart?.InvokePointerMove(location);
    }

    private void OnReleased(object? sender, LiveChartsCore.Native.Events.PressedEventArgs args)
    {
        // Synthetic releases are raised by the chart itself when an ancestor steals
        // pointer capture mid-drag (see #1576). The user has not actually lifted the
        // pointer, so we must not invoke the public PointerReleasedCommand — only
        // forward to the core chart so internal pan/drag state can be released. WPF
        // and Avalonia follow the same rule from their own capture-loss handlers.
        if (!args.IsSyntheticRelease)
        {
            var cArgs = new PointerCommandArgs(this, new(args.Location.X, args.Location.Y), args);
            if (PointerReleasedCommand?.CanExecute(cArgs) == true)
                PointerReleasedCommand.Execute(cArgs);
        }

        CoreChart?.InvokePointerUp(args.Location, args.IsSecondaryPress);
    }

    private void OnExited(object? sender, LiveChartsCore.Native.Events.EventArgs args) =>
        CoreChart?.InvokePointerLeft();

    internal virtual void OnScrolled(object? sender, LiveChartsCore.Native.Events.ScrollEventArgs args) { }

    internal virtual void OnPinched(object? sender, LiveChartsCore.Native.Events.PinchEventArgs args) { }

    private ISeries InflateSeriesTemplate(object item)
    {
        var content = (FrameworkElement?)SeriesTemplate.LoadContent();

        if (content is not ISeries series)
            throw new InvalidOperationException("The template must be a valid series.");

        content.DataContext = item;

        return series;
    }

    private static object GetSeriesSource(ISeries series) =>
        ((FrameworkElement)series).DataContext!;

    void IChartView.InvokeOnUIThread(Action action)
    {
        if (s_isWebAssembly)
        {
            action();
            return;
        }

        _ = DispatcherQueue.TryEnqueue(
            Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () => action());
    }
}
