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
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView.Avalonia;

namespace LiveChartsGeneratedCode;

// ==============================================================================
// Avalonia-specific base class for cartesian / pie / polar controls. Drawn-view
// scaffolding (MotionCanvas hosting, lifecycle wiring, EffectiveViewport fix
// for #1986, CoreCanvas / ControlSize / theme awareness / ICustomHitTest) lives
// in SourceGenDrawnView.avalonia.cs. Chart-specific plumbing — modifier-aware
// pointer handlers, capture recovery for #1576, double-tap detection, commands,
// series template inflation, observer lifecycle — lives here.
// ==============================================================================

/// <inheritdoc cref="ICartesianChartView" />
public abstract partial class SourceGenChart : SourceGenDrawnView, IChartView
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
    private bool _isPointerDown;
    private LvcPoint _lastPointerPosition;

    /// <summary>
    /// Initializes a new instance of the <see cref="SourceGenChart"/> class.
    /// </summary>
    protected SourceGenChart()
    {
        InitializeChartControl();
        InitializeObservedProperties();

        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;
        PointerExited += OnPointerLeave;
        PointerCaptureLost += OnPointerCaptureLost;
    }

    /// <inheritdoc cref="IChartView.BackColor" />
    LvcColor IChartView.BackColor =>
        Background is not ISolidColorBrush b
            ? CoreCanvas._virtualBackgroundColor
            : LvcColor.FromArgb(b.Color.A, b.Color.R, b.Color.G, b.Color.B);

    /// <inheritdoc />
    protected override void OnDrawnViewSizeChanged() => CoreChart?.Update();

    /// <inheritdoc />
    protected override void OnDrawnViewLoaded()
    {
        StartObserving();
        CoreChart?.Load();
    }

    /// <inheritdoc />
    protected override void OnDrawnViewUnloaded()
    {
        StopObserving();
        CoreChart?.Unload();
    }

    /// <inheritdoc />
    protected override void OnDrawnViewReturnedToViewport() => CoreChart?.Update();

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

    // MotionCanvas is a UserControl; attach to its LogicalChildren (exposed as
    // Children) so the chart's DataContext propagates to XAML series / axes.
    private void AddUIElement(object item)
    {
        if (Content is not MotionCanvas canvas || item is not ILogical logical) return;
        canvas.Children.Add(logical);
    }

    private void RemoveUIElement(object item)
    {
        if (Content is not MotionCanvas canvas || item is not ILogical logical) return;
        _ = canvas.Children.Remove(logical);
    }

    private ISeries InflateSeriesTemplate(object item)
    {
        var control = SeriesTemplate.Build(item);

        if (control is not ISeries series)
            throw new InvalidOperationException("The template must be a valid series.");

        control.DataContext = item;

        return series;
    }
}
