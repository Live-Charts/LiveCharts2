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
    private readonly ThemeListener _themeListener;
    private readonly PointerController _pointerController;
    private static readonly bool s_isWebAssembly = RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER"));

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

        _themeListener = new(CoreChart.ApplyTheme, DispatcherQueue);

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
    bool IChartView.IsDarkMode => Application.Current?.RequestedTheme == ApplicationTheme.Dark;
    LvcColor IChartView.BackColor =>
        Background is not SolidColorBrush b
            ? CoreCanvas._virtualBackgroundColor
            : LvcColor.FromArgb(b.Color.A, b.Color.R, b.Color.G, b.Color.B);
    LvcSize IDrawnView.ControlSize => new() { Width = (float)ActualWidth, Height = (float)ActualHeight };

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _themeListener.Listen();
        _pointerController.InitializeController(this);
        StartObserving();
        CoreChart.Load();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _themeListener.Dispose();
        _pointerController.DisposeController(this);
        StopObserving();
        CoreChart.Unload();
    }

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

        var isSecondary = (DateTime.Now - _lastTouch).TotalMilliseconds < 500;

        CoreChart?.InvokePointerDown(args.Location, args.IsSecondaryPress || isSecondary);

        if (NativeHelpers.IsTouchDevice())
            _lastTouch = DateTime.Now;
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
