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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.Styling;
using Avalonia.Threading;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView.Avalonia;

namespace LiveChartsGeneratedCode;

// ==============================================================================
// this file is the base class for this UI framework controls, in this file we
// define the UI framework specific code. 
// expanding this file in the solution explorer will show 2 more files:
//    - *.shared.cs:        shared code between all UI frameworks
//    - *.sgp.cs:           the source generated properties
// ==============================================================================

/// <inheritdoc cref="ICartesianChartView" />
public abstract partial class SourceGenChart : UserControl, IChartView, ICustomHitTest
{
    private DateTime _lastPressed;
    private LvcPoint _lastPressedPosition;
    // Drop the first ~100ms of moves after a press so the platform's pinch
    // detection has a chance to claim the gesture before any single-pointer
    // move feeds the core deadzone (Chart.cs PanEngageThresholdSq) and
    // accidentally engages pan. Matches the WinUI/Uno-SkiaRenderer pointer
    // controllers — the value must stay aligned across all three SkiaSharp
    // host paths.
    private const int PressDeadzoneMs = 100;
    private bool _wasInViewport;
    private bool _isPointerDown;
    private LvcPoint _lastPointerPosition;

    /// <summary>
    /// Initializes a new instance of the <see cref="SourceGenChart"/> class.
    /// </summary>
    protected SourceGenChart()
    {
        Content = new MotionCanvas();

        AttachedToVisualTree += OnAttachedToVisualTree;
        DetachedFromVisualTree += OnDetachedFromVisualTree;
        EffectiveViewportChanged += OnEffectiveViewportChanged;

        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;
        PointerExited += OnPointerLeave;
        PointerCaptureLost += OnPointerCaptureLost;

        SizeChanged += (s, e) =>
            CoreChart.Update();

        InitializeChartControl();
        InitializeObservedProperties();
    }

    private MotionCanvas CanvasView => (MotionCanvas)Content!;

    /// <inheritdoc cref="IDrawnView.CoreCanvas"/>
    public CoreMotionCanvas CoreCanvas => CanvasView.CanvasCore;

    bool IChartView.DesignerMode => Design.IsDesignMode;
    bool IChartView.IsDarkMode => Application.Current?.ActualThemeVariant == ThemeVariant.Dark;
    LvcColor IChartView.BackColor =>
        Background is not ISolidColorBrush b
            ? CoreCanvas._virtualBackgroundColor
            : LvcColor.FromArgb(b.Color.A, b.Color.R, b.Color.G, b.Color.B);
    LvcSize IDrawnView.ControlSize => new() { Width = (float)CanvasView.Bounds.Width, Height = (float)CanvasView.Bounds.Height };

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        StartObserving();
        CoreChart?.Load();
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        StopObserving();
        CoreChart?.Unload();
        _wasInViewport = false;
    }

    private void OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        // Fix for https://github.com/Live-Charts/LiveCharts2/issues/1986
        // EffectiveViewport reports the ancestor scroll viewport in this control's
        // local coordinates, NOT whether the chart is in view. To detect "in view"
        // we have to check whether the viewport intersects the chart's local bounds
        // (which are 0,0..Bounds.Width,Bounds.Height in local coords).
        var vp = e.EffectiveViewport;
        var nowInViewport =
            vp.Width > 0 && vp.Height > 0 &&
            vp.X < Bounds.Width && vp.X + vp.Width > 0 &&
            vp.Y < Bounds.Height && vp.Y + vp.Height > 0;

        if (nowInViewport && !_wasInViewport && CoreChart is not null)
        {
            // When a chart is off-screen (scrolled out of a ScrollViewer or in an inactive
            // tab) Avalonia stops painting the canvas; Chart.IsRendering() then blocks the
            // measure to avoid wasted work (see Chart.cs:787). On the transition back into
            // the viewport, mark the canvas as visible so IsRendering() will allow the next
            // measure, and request an Update.
            CoreCanvas.NotifyPlatformVisible();
            CoreChart.Update();
        }
        _wasInViewport = nowInViewport;
    }

    void IChartView.InvokeOnUIThread(Action action) =>
        Dispatcher.UIThread.Post(action);

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.KeyModifiers > 0) return;
        var p = e.GetPosition(this);

        // Arm _isPointerDown BEFORE invoking the user's command. If the command
        // synchronously transfers capture (e.g. focuses or captures another
        // element) Avalonia raises PointerCaptureLost immediately, and the
        // recovery handler must observe the flag set so it can synthesize a
        // pointer-up. Otherwise the chart is left in the same stuck-drag state
        // #1576 fixes.
        _isPointerDown = true;
        _lastPointerPosition = new LvcPoint((float)p.X, (float)p.Y);

        if (PointerPressedCommand is not null)
        {
            var args = new PointerCommandArgs(this, new(p.X, p.Y), e);
            if (PointerPressedCommand.CanExecute(args))
                PointerPressedCommand.Execute(args);
        }

        var isSecondary =
            e.GetCurrentPoint(this).Properties.IsRightButtonPressed ||
            GestureHelpers.IsDoubleTap(_lastPointerPosition, _lastPressedPosition, DateTime.Now - _lastPressed);

        CoreChart?.InvokePointerDown(_lastPointerPosition, isSecondary);
        _lastPressed = DateTime.Now;
        _lastPressedPosition = _lastPointerPosition;
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if ((DateTime.Now - _lastPressed).TotalMilliseconds < PressDeadzoneMs) return;
        var p = e.GetPosition(this);

        if (PointerMoveCommand is not null)
        {
            var args = new PointerCommandArgs(this, new(p.X, p.Y), e);
            if (PointerMoveCommand.CanExecute(args))
                PointerMoveCommand.Execute(args);
        }

        _lastPointerPosition = new LvcPoint((float)p.X, (float)p.Y);
        CoreChart?.InvokePointerMove(_lastPointerPosition);
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isPointerDown = false;

        // No time-gate on Release: matches WinUI/Uno-SkiaRenderer pointer
        // controllers. A fast press-release inside the press deadzone window
        // must still flow through to InvokePointerUp so the core chart can
        // clear _isPointerDown / _isPanning — otherwise the pan-engagement
        // deadzone (Chart.cs) would stay armed across gestures.
        var p = e.GetPosition(this);

        if (PointerReleasedCommand is not null)
        {
            var args = new PointerCommandArgs(this, new(p.X, p.Y), e);
            if (PointerReleasedCommand.CanExecute(args))
                PointerReleasedCommand.Execute(args);
        }

        _lastPointerPosition = new LvcPoint((float)p.X, (float)p.Y);
        CoreChart?.InvokePointerUp(_lastPointerPosition, e.GetCurrentPoint(this).Properties.IsRightButtonPressed);
    }

    // When an ancestor (e.g. a button wrapping the chart, see #1576) re-captures
    // the pointer mid-gesture, the chart never receives PointerReleased and pan/drag
    // state stays armed; any subsequent PointerMoved keeps panning. Treat capture
    // loss as a synthetic pointer-up so the drag state always releases.
    private void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (!_isPointerDown) return;
        _isPointerDown = false;
        CoreChart?.InvokePointerUp(_lastPointerPosition, false);
    }

    private void OnPointerLeave(object? sender, PointerEventArgs e) =>
        CoreChart?.InvokePointerLeft();

    private void AddUIElement(object item)
    {
        if (CanvasView is null || item is not ILogical logical) return;
        CanvasView.Children.Add(logical);
    }

    private void RemoveUIElement(object item)
    {
        if (CanvasView is null || item is not ILogical logical) return;
        _ = CanvasView.Children.Remove(logical);
    }

    private ISeries InflateSeriesTemplate(object item)
    {
        var control = SeriesTemplate.Build(item);

        if (control is not ISeries series)
            throw new InvalidOperationException("The template must be a valid series.");

        control.DataContext = item;

        return series;
    }

    bool ICustomHitTest.HitTest(Point point) =>
        new Rect(Bounds.Size).Contains(point);
}
