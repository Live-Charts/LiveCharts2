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
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Native;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace LiveChartsGeneratedCode;

// ==============================================================================
// WinUI / Uno-WinUI base class for cartesian / pie / polar controls. Drawn-view
// scaffolding (MotionCanvas hosting, Loaded/Unloaded wiring, DispatcherQueue,
// CoreCanvas / ControlSize / ActualTheme-aware IsDarkMode) lives in
// SourceGenDrawnView.winui.cs. Chart-specific plumbing (PointerController with
// double-tap detection, theme-change listener, command-bound pointer handlers,
// series template inflation, observer lifecycle) lives here.
// ==============================================================================

/// <inheritdoc cref="IChartView"/>
public abstract partial class SourceGenChart : SourceGenDrawnView, IChartView
{
    private DateTime _lastTouch;
    private LvcPoint _lastTouchPosition;
    private readonly PointerController _pointerController;

    /// <summary>
    /// Initializes a new instance of the <see cref="SourceGenChart"/> class.
    /// </summary>
    public SourceGenChart()
    {
        InitializeChartControl();
        InitializeObservedProperties();

        _pointerController = new PointerController();

        _pointerController.Pressed += OnPressed;
        _pointerController.Moved += OnMoved;
        _pointerController.Released += OnReleased;
        _pointerController.Scrolled += OnScrolled;
        _pointerController.Pinched += OnPinched;
        _pointerController.Exited += OnExited;
    }

    LvcColor IChartView.BackColor =>
        Background is not SolidColorBrush b
            ? CoreCanvas._virtualBackgroundColor
            : LvcColor.FromArgb(b.Color.A, b.Color.R, b.Color.G, b.Color.B);

    /// <inheritdoc />
    protected override void OnDrawnViewSizeChanged() => CoreChart?.Update();

    /// <inheritdoc />
    protected override void OnDrawnViewLoaded()
    {
        ActualThemeChanged += OnActualThemeChanged;
        _pointerController.InitializeController(this);
        StartObserving();
        CoreChart.Load();
    }

    /// <inheritdoc />
    protected override void OnDrawnViewUnloaded()
    {
        ActualThemeChanged -= OnActualThemeChanged;
        _pointerController.DisposeController(this);
        StopObserving();
        CoreChart.Unload();
    }

    private void OnActualThemeChanged(FrameworkElement sender, object args) =>
        CoreChart.ApplyTheme();

    private void AddUIElement(object item)
    {
        if (Content is not Microsoft.UI.Xaml.Controls.Panel panel || item is not UIElement uiElement) return;
        panel.Children.Add(uiElement);
    }

    private void RemoveUIElement(object item)
    {
        if (Content is not Microsoft.UI.Xaml.Controls.Panel panel || item is not UIElement uiElement) return;
        _ = panel.Children.Remove(uiElement);
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
}
