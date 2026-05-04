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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView.WPF;

namespace LiveChartsGeneratedCode;

// ==============================================================================
// this file is the base class for this UI framework controls, in this file we
// define the UI framework specific code. 
// expanding this file in the solution explorer will show 2 more files:
//    - *.shared.cs:        shared code between all UI frameworks
//    - *.sgp.cs:           the source generated properties
// ==============================================================================

/// <inheritdoc cref="IChartView" />
public abstract partial class SourceGenChart : UserControl, IChartView
{
    private bool _isPointerDown;

    /// <summary>
    /// Initializes a new instance of the <see cref="Chart"/> class.
    /// </summary>
    /// <exception cref="Exception">Default colors are not valid</exception>
    protected SourceGenChart()
    {
        Content = new MotionCanvas();

        SizeChanged += (s, e) =>
            CoreChart.Update();

        InitializeChartControl();
        InitializeObservedProperties();

        MouseDown += Chart_MouseDown;
        MouseMove += OnMouseMove;
        MouseUp += Chart_MouseUp;
        MouseLeave += OnMouseLeave;
        LostMouseCapture += OnLostMouseCapture;

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private MotionCanvas MotionCanvas => (MotionCanvas)Content;

    /// <inheritdoc cref="IDrawnView.CoreCanvas" />
    public CoreMotionCanvas CoreCanvas => MotionCanvas.CanvasCore;

    bool IChartView.DesignerMode => DesignerProperties.GetIsInDesignMode(this);
    bool IChartView.IsDarkMode => false;
    LvcColor IChartView.BackColor =>
        Background is not SolidColorBrush b
            ? CoreCanvas._virtualBackgroundColor
            : LvcColor.FromArgb(b.Color.A, b.Color.R, b.Color.G, b.Color.B);
    LvcSize IDrawnView.ControlSize => new() { Width = (float)ActualWidth, Height = (float)ActualHeight };

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        StartObserving();
        CoreChart.Load();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        StopObserving();
        CoreChart?.Unload();
    }

    private void AddUIElement(object item)
    {
        if (MotionCanvas is null || item is not FrameworkElement view) return;
        _ = MotionCanvas.Children.Add(view);
    }

    private void RemoveUIElement(object item)
    {
        if (MotionCanvas is null || item is not FrameworkElement view) return;
        MotionCanvas.Children.Remove(view);
    }

    private void Chart_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (Keyboard.Modifiers > 0) return;
        _ = CaptureMouse();

        // Arm _isPointerDown BEFORE invoking the user's command. If the command
        // synchronously transfers capture (e.g. focuses or captures another
        // element) WPF raises LostMouseCapture immediately, and the recovery
        // handler must observe the flag set so it can synthesize a pointer-up.
        // Otherwise the chart is left in the same stuck-drag state #1576 fixes.
        _isPointerDown = true;

        var p = e.GetPosition(this);
        var cArgs = new PointerCommandArgs(this, new(p.X, p.Y), e);
        if (PointerPressedCommand?.CanExecute(cArgs) == true)
            PointerPressedCommand.Execute(cArgs);

        CoreChart?.InvokePointerDown(new(p.X, p.Y), e.ChangedButton == MouseButton.Right);
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        var p = e.GetPosition(this);

        if (PointerMoveCommand is not null)
        {
            var args = new PointerCommandArgs(this, new(p.X, p.Y), e);
            if (PointerMoveCommand.CanExecute(args))
                PointerMoveCommand.Execute(args);
        }

        CoreChart?.InvokePointerMove(new LvcPoint((float)p.X, (float)p.Y));
    }

    private void Chart_MouseUp(object sender, MouseButtonEventArgs e)
    {
        var p = e.GetPosition(this);

        // Clear _isPointerDown BEFORE invoking the user's command. If the command
        // synchronously changes capture (e.g. focuses or captures another element)
        // WPF raises LostMouseCapture immediately; with the flag still set the
        // recovery handler would then synthesize a redundant pointer-up after the
        // real release. Clearing first keeps the release path single-shot.
        _isPointerDown = false;

        var cArgs = new PointerCommandArgs(this, new(p.X, p.Y), e);
        if (PointerReleasedCommand?.CanExecute(cArgs) == true)
            PointerReleasedCommand.Execute(cArgs);

        CoreChart?.InvokePointerUp(new(p.X, p.Y), e.ChangedButton == MouseButton.Right);
        ReleaseMouseCapture();
    }

    // When an ancestor (e.g. a ToggleButton wrapping the chart, see #1576) calls
    // CaptureMouse() during the same mouse-down burst, capture is transferred away
    // from the chart and the chart never receives MouseUp; pan/drag state then stays
    // armed and any subsequent MouseMove keeps panning. Treat capture loss as a
    // synthetic pointer-up so the drag state always releases.
    private void OnLostMouseCapture(object sender, MouseEventArgs e)
    {
        if (!_isPointerDown) return;
        _isPointerDown = false;

        var p = Mouse.GetPosition(this);
        CoreChart?.InvokePointerUp(new(p.X, p.Y), false);
    }

    private void OnMouseLeave(object sender, MouseEventArgs e) =>
        CoreChart?.InvokePointerLeft();

    private ISeries InflateSeriesTemplate(object item)
    {
        var content = (FrameworkElement)SeriesTemplate.LoadContent();

        if (content is not ISeries series)
            throw new InvalidOperationException("The template must be a valid series.");

        content.DataContext = item;

        return series;
    }

    private static object GetSeriesSource(ISeries series) =>
        ((FrameworkElement)series).DataContext!;

    void IChartView.InvokeOnUIThread(Action action) =>
        Dispatcher.Invoke(action);
}
