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

using Factos;
using LiveChartsCore.Kernel.Sketches;
using SharedUITests.Helpers;
using Xunit;

#if WPF_UI_TESTING
using System.Reflection;
using System.Windows;
using System.Windows.Input;
#endif

#if AVALONIA_UI_TESTING
using System;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
#endif

#if WINUI_UI_TESTING || UNO_UI_TESTING
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using LiveChartsCore.Native.Events;
using Microsoft.UI.Xaml;
#endif

namespace SharedUITests.Events;

public class PointerCaptureLostTests
{
    public AppController App => AppController.Current;

#if WPF_UI_TESTING
    // regression sister to Wpf_lost_mouse_capture_releases_drag_state.
    // Pre-fix: _isPointerDown was set AFTER PointerPressedCommand executed. If user
    // code inside the command synchronously transferred capture (e.g. CaptureMouse
    // on another element, Focus, etc.), WPF raised LostMouseCapture on the chart
    // immediately and the recovery handler bailed because the flag was still false
    // — the chart stayed armed and a later MouseMove would pan with no button held.
    // The flag must be set before the command runs so the recovery handler always
    // observes it.
    [AppTestMethod]
    public async Task Wpf_pointer_pressed_command_observes_pointer_down_armed()
    {
        var sut = await App.NavigateTo<Samples.General.FirstChart.View>();
        await sut.Chart.WaitUntilChartRenders();

        var chart = (FrameworkElement)sut.Chart;

        var sourceGenChartType = WalkBaseTypes(chart.GetType(), "SourceGenChart");
        Assert.NotNull(sourceGenChartType);
        var isPointerDownField = sourceGenChartType!.GetField(
            "_isPointerDown", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(isPointerDownField);
        var mouseDown = sourceGenChartType.GetMethod(
            "Chart_MouseDown", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(mouseDown);

        var commandProperty = chart.GetType().GetProperty("PointerPressedCommand");
        Assert.NotNull(commandProperty);

        bool? observedFlag = null;
        var probe = new ProbeCommand(() => observedFlag = (bool)isPointerDownField!.GetValue(chart)!);

        await chart.Dispatcher.InvokeAsync(() =>
        {
            commandProperty!.SetValue(chart, probe);

            // Drive Chart_MouseDown directly. The probe records the value of
            // _isPointerDown at the moment the user's command fires, which is
            // exactly the ordering the recovery handler relies on.
            var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = Mouse.MouseDownEvent,
                Source = chart,
            };
            _ = mouseDown!.Invoke(chart, [chart, args]);
        });

        Assert.True(
            observedFlag,
            "_isPointerDown must be armed before PointerPressedCommand fires; otherwise a capture transfer triggered from inside the command would leave the LostMouseCapture recovery handler unable to detect the in-flight gesture (#1576).");
    }

    // regression sister to Wpf_lost_mouse_capture_releases_drag_state.
    // Pre-fix: _isPointerDown was cleared AFTER PointerReleasedCommand executed.
    // If user code inside the command synchronously changed capture (e.g.
    // focused or captured another element), WPF raised LostMouseCapture on the
    // chart immediately and the recovery handler synthesized a redundant
    // pointer-up on top of the real release. Clearing the flag first keeps the
    // release path single-shot.
    [AppTestMethod]
    public async Task Wpf_pointer_released_command_observes_pointer_down_cleared()
    {
        var sut = await App.NavigateTo<Samples.General.FirstChart.View>();
        await sut.Chart.WaitUntilChartRenders();

        var chart = (FrameworkElement)sut.Chart;

        var sourceGenChartType = WalkBaseTypes(chart.GetType(), "SourceGenChart");
        Assert.NotNull(sourceGenChartType);
        var isPointerDownField = sourceGenChartType!.GetField(
            "_isPointerDown", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(isPointerDownField);
        var mouseUp = sourceGenChartType.GetMethod(
            "Chart_MouseUp", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(mouseUp);

        var commandProperty = chart.GetType().GetProperty("PointerReleasedCommand");
        Assert.NotNull(commandProperty);

        bool? observedFlag = null;
        var probe = new ProbeCommand(() => observedFlag = (bool)isPointerDownField!.GetValue(chart)!);

        await chart.Dispatcher.InvokeAsync(() =>
        {
            commandProperty!.SetValue(chart, probe);

            // Pre-arm the flag (as if a real press had just occurred) and drive
            // Chart_MouseUp directly. The probe records the value of
            // _isPointerDown at the moment the user's command fires.
            isPointerDownField!.SetValue(chart, true);

            var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = Mouse.MouseUpEvent,
                Source = chart,
            };
            _ = mouseUp!.Invoke(chart, [chart, args]);
        });

        Assert.False(
            observedFlag,
            "_isPointerDown must be cleared before PointerReleasedCommand fires; otherwise a capture change triggered from inside the command would cause LostMouseCapture to synthesize a redundant pointer-up on top of the real release (#1576).");
    }

    // regression for https://github.com/Live-Charts/LiveCharts2/issues/1576
    // when an ancestor (e.g. a ToggleButton wrapping the chart) calls CaptureMouse
    // during the same mouse-down burst, capture is stolen from the chart and the
    // chart never sees MouseUp; pan/drag state stays armed and any subsequent
    // MouseMove keeps panning. The chart must treat capture loss as a synthetic
    // pointer-up so the drag state always releases.
    [AppTestMethod]
    public async Task Wpf_lost_mouse_capture_releases_drag_state()
    {
        var sut = await App.NavigateTo<Samples.General.FirstChart.View>();
        await sut.Chart.WaitUntilChartRenders();

        var chart = (FrameworkElement)sut.Chart;
        var coreChart = ((IChartView)sut.Chart).CoreChart;

        var sourceGenChartType = WalkBaseTypes(chart.GetType(), "SourceGenChart");
        Assert.NotNull(sourceGenChartType);
        var isPointerDownField = sourceGenChartType!.GetField(
            "_isPointerDown", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(isPointerDownField);

        var chartType = WalkBaseTypes(coreChart.GetType(), "Chart");
        Assert.NotNull(chartType);
        var isPanningField = chartType!.GetField(
            "_isPanning", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(isPanningField);

        await chart.Dispatcher.InvokeAsync(() =>
        {
            // Put the chart into the same state it would have right after a real
            // MouseDown: the WPF-side flag tracking "user is pressing" is set, and
            // the core chart's pan/drag state is armed.
            isPointerDownField!.SetValue(chart, true);
            isPanningField!.SetValue(coreChart, true);

            // Drive a real LostMouseCapture event through WPF's routed-event system.
            // The chart's handler (subscribed via +=) is what we're testing — we don't
            // reflect on it, so we cannot get a name-clash with UIElement's protected
            // virtual of the same name.
            chart.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0)
            {
                RoutedEvent = Mouse.LostMouseCaptureEvent,
                Source = chart
            });

            Assert.False(
                (bool)isPointerDownField.GetValue(chart)!,
                "_isPointerDown should be reset after LostMouseCapture");
            Assert.False(
                (bool)isPanningField.GetValue(coreChart)!,
                "Chart._isPanning should be reset after LostMouseCapture");
        });
    }
#endif

#if AVALONIA_UI_TESTING
    // regression mirror for https://github.com/Live-Charts/LiveCharts2/issues/1576
    // Avalonia has the same class of issue when an ancestor re-captures the pointer
    // mid-gesture (reported by MajesticBevans in the same thread). The chart must
    // treat capture loss as a synthetic pointer-up.
    [AppTestMethod]
    public async Task Avalonia_pointer_capture_lost_releases_drag_state()
    {
        var sut = await App.NavigateTo<Samples.General.FirstChart.View>();
        await sut.Chart.WaitUntilChartRenders();

        var chart = (Control)sut.Chart;
        var coreChart = ((IChartView)sut.Chart).CoreChart;

        var sourceGenChartType = WalkBaseTypes(chart.GetType(), "SourceGenChart");
        Assert.NotNull(sourceGenChartType);
        var isPointerDownField = sourceGenChartType!.GetField(
            "_isPointerDown", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(isPointerDownField);

        // OnPointerCaptureLost shadows InputElement's protected virtual of the same
        // name (single-arg signature), so we must specify the parameter types to
        // disambiguate; the simple overload throws AmbiguousMatchException.
        var onCaptureLost = sourceGenChartType.GetMethod(
            "OnPointerCaptureLost",
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            types: [typeof(object), typeof(PointerCaptureLostEventArgs)],
            modifiers: null);
        Assert.NotNull(onCaptureLost);

        var chartType = WalkBaseTypes(coreChart.GetType(), "Chart");
        Assert.NotNull(chartType);
        var isPanningField = chartType!.GetField(
            "_isPanning", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(isPanningField);

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            isPointerDownField!.SetValue(chart, true);
            isPanningField!.SetValue(coreChart, true);

            // PointerCaptureLostEventArgs has no public constructor and Avalonia's
            // RaiseEvent path therefore can't be exercised from a test. Invoke the
            // handler directly; it doesn't read its argument, so null is safe.
            _ = onCaptureLost!.Invoke(chart, [chart, null]);

            Assert.False(
                (bool)isPointerDownField.GetValue(chart)!,
                "_isPointerDown should be reset after PointerCaptureLost");
            Assert.False(
                (bool)isPanningField.GetValue(coreChart)!,
                "Chart._isPanning should be reset after PointerCaptureLost");
        });
    }
#endif

#if AVALONIA_UI_TESTING
    // Sister regression for the Avalonia OnPointerReleased tolerance early-return.
    // Pre-fix: _isPointerDown was assigned AFTER the < _tolearance bail-out, so a
    // fast press-release left the flag stuck true and a later unrelated
    // PointerCaptureLost would synthesize a phantom pointer-up. The hoisted
    // assignment must clear the flag before the early-return is taken.
    [AppTestMethod]
    public async Task Avalonia_fast_press_release_clears_pointer_down_flag()
    {
        var sut = await App.NavigateTo<Samples.General.FirstChart.View>();
        await sut.Chart.WaitUntilChartRenders();

        var chart = (Control)sut.Chart;

        var sourceGenChartType = WalkBaseTypes(chart.GetType(), "SourceGenChart");
        Assert.NotNull(sourceGenChartType);
        var isPointerDownField = sourceGenChartType!.GetField(
            "_isPointerDown", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(isPointerDownField);
        var lastPresedField = sourceGenChartType.GetField(
            "_lastPresed", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(lastPresedField);

        // OnPointerReleased shadows InputElement's protected virtual; specify
        // parameter types to avoid AmbiguousMatchException.
        var onReleased = sourceGenChartType.GetMethod(
            "OnPointerReleased",
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            types: [typeof(object), typeof(PointerReleasedEventArgs)],
            modifiers: null);
        Assert.NotNull(onReleased);

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            // Simulate the moment right after a real press: pointer-down armed and
            // _lastPresed pinned to "now" so the next OnPointerReleased call hits the
            // tolerance early-return — the very branch that used to leak the flag.
            isPointerDownField!.SetValue(chart, true);
            lastPresedField!.SetValue(chart, DateTime.Now);

            // The handler does not dereference its args before the early-return.
            _ = onReleased!.Invoke(chart, [chart, null]);

            Assert.False(
                (bool)isPointerDownField.GetValue(chart)!,
                "_isPointerDown must be cleared even when OnPointerReleased takes the tolerance early-return; otherwise a later unrelated PointerCaptureLost would fire a phantom synthetic pointer-up.");
        });
    }
#endif

#if WINUI_UI_TESTING || UNO_UI_TESTING
    // regression for https://github.com/Live-Charts/LiveCharts2/issues/1576 on the
    // WinUI/Uno-Skia native pointer controller: when an ancestor steals capture
    // mid-gesture the controller raises a synthetic Released so the chart can
    // release its pan/drag state. That synthetic release used to hard-code
    // IsSecondaryPress=false, which mis-reported right-click drags as primary
    // releases. The controller now snapshots the original press button at press
    // time and replays it on PointerCaptureLost.
    [AppTestMethod]
    public async Task WinUI_pointer_capture_lost_preserves_secondary_press_flag()
    {
        var sut = await App.NavigateTo<Samples.General.FirstChart.View>();
        await sut.Chart.WaitUntilChartRenders();

        var chart = (FrameworkElement)sut.Chart;

        var (pc, captureLost) = ResolvePointerControllerAndCaptureLostMethod(chart);

        var pcType = pc.GetType();
        var isDownField = pcType.GetField("_isPointerDown", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(isDownField);
        var wasSecondaryField = pcType.GetField("_wasSecondaryPress", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(wasSecondaryField);

        var releasedEvent = pcType.GetEvent("Released");
        Assert.NotNull(releasedEvent);

        PressedEventArgs? captured = null;
        PressedHandler observer = (_, args) => captured = args;
        releasedEvent!.AddEventHandler(pc, observer);
        try
        {
            await RunOnDispatcherAsync(chart, () =>
            {
                // simulate the state right after a real right-button press: the
                // controller is armed and remembers the press was secondary. The
                // capture-lost replay must surface that secondary flag back out.
                isDownField!.SetValue(pc, true);
                wasSecondaryField!.SetValue(pc, true);

                // The handler does not dereference its args (we asserted this in
                // the source), so null sender / null PointerRoutedEventArgs is safe.
                _ = captureLost.Invoke(pc, [null!, null!]);
            });
        }
        finally
        {
            releasedEvent.RemoveEventHandler(pc, observer);
        }

        Assert.NotNull(captured);
        Assert.True(
            captured!.IsSecondaryPress,
            "synthetic Released raised on PointerCaptureLost must replay the original press's secondary-button flag; otherwise right-click drags interrupted by capture-loss are reported as primary releases.");
    }

    // regression for https://github.com/Live-Charts/LiveCharts2/issues/1576 on the
    // WinUI/Uno-Skia chart: the synthetic Released that the controller raises on
    // PointerCaptureLost flows through the shared OnReleased handler, which used
    // to fire PointerReleasedCommand for any release. The user has not actually
    // lifted the pointer when capture is stolen, so the public command must NOT
    // fire — only the core chart's internal pan/drag state should release.
    [AppTestMethod]
    public async Task WinUI_pointer_capture_lost_does_not_fire_pointer_released_command()
    {
        var sut = await App.NavigateTo<Samples.General.FirstChart.View>();
        await sut.Chart.WaitUntilChartRenders();

        var chart = (FrameworkElement)sut.Chart;

        var (pc, captureLost) = ResolvePointerControllerAndCaptureLostMethod(chart);

        var pcType = pc.GetType();
        var isDownField = pcType.GetField("_isPointerDown", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(isDownField);

        var commandProperty = chart.GetType().GetProperty("PointerReleasedCommand");
        Assert.NotNull(commandProperty);

        var commandFired = false;
        var probe = new ProbeCommand(() => commandFired = true);

        await RunOnDispatcherAsync(chart, () =>
        {
            commandProperty!.SetValue(chart, probe);
            isDownField!.SetValue(pc, true);

            _ = captureLost.Invoke(pc, [null!, null!]);
        });

        Assert.False(
            commandFired,
            "PointerReleasedCommand must not be invoked when capture-loss synthesizes a release; the user has not actually lifted the pointer and the command's contract is 'real user release'.");
    }

    private static (object Controller, MethodInfo CaptureLost) ResolvePointerControllerAndCaptureLostMethod(FrameworkElement chart)
    {
        var sourceGenChartType = WalkBaseTypes(chart.GetType(), "SourceGenChart");
        Assert.NotNull(sourceGenChartType);

        var pcField = sourceGenChartType!.GetField("_pointerController", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(pcField);
        var pc = pcField!.GetValue(chart);
        Assert.NotNull(pc);

        var pcType = pc!.GetType();
        // Each platform partial owns its own handler name; only one of the two
        // exists in the assembly being tested. Probe both so a single test method
        // covers the WinUI sample and the Uno-Skia sample.
        var captureLost =
            pcType.GetMethod("OnWindowsPointerCaptureLost", BindingFlags.Instance | BindingFlags.NonPublic) ??
            pcType.GetMethod("OnUnoSkiaPointerCaptureLost", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(captureLost);
        return (pc, captureLost!);
    }

    private static Task RunOnDispatcherAsync(FrameworkElement chart, Action work)
    {
        var tcs = new TaskCompletionSource<object?>();
        if (!chart.DispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                work();
                tcs.SetResult(null);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        }))
        {
            tcs.SetException(new InvalidOperationException("Failed to enqueue work on the chart's DispatcherQueue."));
        }
        return tcs.Task;
    }

#endif

#if MAUI_UI_TESTING
    // regression for https://github.com/Live-Charts/LiveCharts2/issues/1576 on
    // MAUI: MAUI on Windows uses the same shared PointerController as WinUI/Uno
    // and the controller raises a synthetic Released on PointerCaptureLost.
    // Pre-fix, MAUI's OnReleased unconditionally invoked ReleasedCommand, so
    // MAUI/Windows apps still received a public release callback when capture
    // was stolen mid-drag. The guard in OnReleased must skip the command when
    // args.IsSyntheticRelease is set.
    [AppTestMethod]
    public async Task Maui_synthetic_release_does_not_fire_released_command()
    {
        var sut = await App.NavigateTo<Samples.General.FirstChart.View>();
        await sut.Chart.WaitUntilChartRenders();

        var chart = (Microsoft.Maui.Controls.View)sut.Chart;

        var sourceGenChartType = WalkBaseTypes(chart.GetType(), "SourceGenChart");
        Assert.NotNull(sourceGenChartType);

        // OnReleased is internal override; reflect into it so we can drive a
        // synthetic-release directly without going through the platform
        // PointerController.
        var onReleased = sourceGenChartType!.GetMethod(
            "OnReleased",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
            binder: null,
            types: [typeof(object), typeof(LiveChartsCore.Native.Events.PressedEventArgs)],
            modifiers: null);
        Assert.NotNull(onReleased);

        var commandProperty = chart.GetType().GetProperty("ReleasedCommand");
        Assert.NotNull(commandProperty);

        var commandFired = false;
        var probe = new ProbeCommand(() => commandFired = true);

        await Microsoft.Maui.ApplicationModel.MainThread.InvokeOnMainThreadAsync(() =>
        {
            commandProperty!.SetValue(chart, probe);

            var args = new LiveChartsCore.Native.Events.PressedEventArgs(
                new LiveChartsCore.Drawing.LvcPoint(0, 0),
                isSecondaryPress: false,
                isSyntheticRelease: true,
                originalEvent: chart);

            _ = onReleased!.Invoke(chart, [chart, args]);
        });

        Assert.False(
            commandFired,
            "ReleasedCommand must not be invoked when args.IsSyntheticRelease is set; capture-loss is not a real user release and the public command's contract is 'real user release' (#1576).");
    }
#endif

#if WPF_UI_TESTING || AVALONIA_UI_TESTING || WINUI_UI_TESTING || UNO_UI_TESTING || MAUI_UI_TESTING
    private static Type? WalkBaseTypes(Type? start, string name)
    {
        for (var t = start; t is not null; t = t.BaseType)
            if (t.Name == name) return t;
        return null;
    }
#endif

#if WPF_UI_TESTING || WINUI_UI_TESTING || UNO_UI_TESTING || MAUI_UI_TESTING
    private sealed class ProbeCommand(System.Action onExecute) : System.Windows.Input.ICommand
    {
        public event System.EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => onExecute();
    }
#endif
}
