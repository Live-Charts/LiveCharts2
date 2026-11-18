// WinUI, and MAUI on Windows: the MAUI chart is drawn in a WinUI panel (its handler's platform
// view) that uses the same pointer controller.
#if WINUI_UI_TESTING || (MAUI_UI_TESTING && WINDOWS)
#define WINDOWS_POINTER_TESTING
#endif

using Factos;
using SharedUITests.Helpers;
using Xunit;

#if WINDOWS_POINTER_TESTING
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Input;
using LiveChartsCore.SkiaSharpView;
#endif
#if WINUI_UI_TESTING
using LiveChartsCore.SkiaSharpView.WinUI;
#endif
#if MAUI_UI_TESTING && WINDOWS
using LiveChartsCore.SkiaSharpView.Maui;
#endif

// to run these tests, see the UITests project, specially the program.cs file.
// to enable IDE intellisense for these tests, go to Directory.Build.props and set UITesting to true.

namespace SharedUITests;

public class PointerInputTests
{
    public AppController App => AppController.Current;

#if WINDOWS_POINTER_TESTING
    // https://github.com/Live-Charts/LiveCharts2/issues/2389
    // https://github.com/Live-Charts/LiveCharts2/issues/2403 (MAUI on Windows: the chart's platform
    // view is a WinUI panel that uses the same pointer controller)
    // A mouse drag starts a WinUI manipulation (the chart enables ManipulationModes.Scale for
    // pinch zoom). On a machine with a touch screen, the pointer controller took any started
    // manipulation for a pinch and dropped every pointer move until it completed, so dragging
    // with the mouse neither panned the chart nor raised PointerMoveCommand.
    // WinUI pointer events can't be constructed, so this drags the real mouse (SendInput), and
    // it makes LiveCharts believe the machine has a touch screen, which CI machines don't.
    [AppTestMethod]
    public async Task MouseDragMovesReachTheChartOnTouchCapableMachines()
    {
        var isTouchDevice = typeof(CartesianChart).Assembly
            .GetType("LiveChartsCore.Native.NativeHelpers", throwOnError: true)!
            .GetField("s_isTouchDevice", BindingFlags.NonPublic | BindingFlags.Static)!;
        var previous = isTouchDevice.GetValue(null);
        isTouchDevice.SetValue(null, (bool?)true);

        try
        {
            var moves = 0;
            var sut = new CartesianChart
            {
                Series = [new LineSeries<double> { Values = [2, 5, 3, 6, 4, 7] }],
#if MAUI_UI_TESTING
                MovedCommand = new CountingCommand(() => moves++)
#else
                PointerMoveCommand = new CountingCommand(() => moves++)
#endif
            };

#if MAUI_UI_TESTING
            await App.NavigateToView(new Microsoft.Maui.Controls.ContentPage { Content = sut });
            await sut.WaitUntilChartLoadsAndRenders();
            // the WinUI panel the MAUI chart is drawn in, where the pointer controller listens.
            var element = (Microsoft.UI.Xaml.FrameworkElement)sut.Handler!.PlatformView!;
#else
            await App.NavigateToView(sut);
            await sut.WaitUntilChartLoadsAndRenders();
            var element = sut;
#endif
            var rawMoves = 0;
            var presses = 0;
            element.PointerMoved += (_, _) => rawMoves++;
            // handledEventsToo: count the press even if an element below marked it handled.
            element.AddHandler(Microsoft.UI.Xaml.UIElement.PointerPressedEvent,
                new Microsoft.UI.Xaml.Input.PointerEventHandler((_, _) => presses++), true);

            // The first click on a window that is not active can be spent activating it: the press
            // never reaches the chart, there is nothing to drag, and this test passed for the wrong
            // reason. Bring the window to the front, and press again if the press was lost.
            RealMouse.BringAppWindowToFront();
            await Task.Delay(300);

            var from = RealMouse.ToScreen(element, 0.3, 0.5);
            var to = RealMouse.ToScreen(element, 0.7, 0.5);

            RealMouse.MoveTo(from.X, from.Y);
            await Task.Delay(100);
            for (var attempt = 0; attempt < 3; attempt++)
            {
                RealMouse.LeftDown();
                await Task.Delay(200); // past the 100ms the controller waits after a press
                if (presses > 0) break;
                RealMouse.LeftUp();
                await Task.Delay(300);
            }

            int movesBefore = moves, rawBefore = rawMoves;
            try
            {
                const int steps = 20;
                for (var i = 1; i <= steps; i++)
                {
                    RealMouse.MoveTo(from.X + (to.X - from.X) * i / steps, from.Y);
                    await Task.Delay(20);
                }
            }
            finally
            {
                RealMouse.LeftUp();
            }
            await Task.Delay(100);

            var rawDuringDrag = rawMoves - rawBefore;
            var commandDuringDrag = moves - movesBefore;

            // the OS input reached the chart: the press, and WinUI's own PointerMoved during the drag.
            Assert.True(presses > 0, "The mouse press didn't reach the chart, so nothing was dragged.");
            Assert.True(rawDuringDrag > 10,
                $"WinUI raised PointerMoved {rawDuringDrag} times during the drag, the simulated mouse input didn't reach the chart.");

            // and LiveCharts handled those moves (it dropped all of them before the fix).
            Assert.True(commandDuringDrag >= rawDuringDrag / 2,
                $"The chart's move command ran {commandDuringDrag} times during the drag, WinUI raised PointerMoved {rawDuringDrag} times.");
        }
        finally
        {
            isTouchDevice.SetValue(null, previous);
        }
    }

    private sealed class CountingCommand(Action onExecute) : ICommand
    {
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => onExecute();
    }

    // real OS mouse input through SendInput, in physical screen pixels.
    private static class RealMouse
    {
        private const uint MOVE = 0x0001, LEFTDOWN = 0x0002, LEFTUP = 0x0004, ABSOLUTE = 0x8000;

        public static (int X, int Y) ToScreen(Microsoft.UI.Xaml.FrameworkElement element, double fx, double fy)
        {
            var dip = element.TransformToVisual(null)
                .TransformPoint(new(element.ActualWidth * fx, element.ActualHeight * fy));
            var scale = element.XamlRoot.RasterizationScale;
            var p = new POINT { X = (int)(dip.X * scale), Y = (int)(dip.Y * scale) };
            _ = ClientToScreen(Process.GetCurrentProcess().MainWindowHandle, ref p);
            return (p.X, p.Y);
        }

        public static void MoveTo(int x, int y) => Send(MOVE | ABSOLUTE,
            x * 65535 / (GetSystemMetrics(0) - 1), y * 65535 / (GetSystemMetrics(1) - 1));

        public static void LeftDown() => Send(LEFTDOWN, 0, 0);

        public static void LeftUp() => Send(LEFTUP, 0, 0);

        public static void BringAppWindowToFront() =>
            _ = SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);

        private static void Send(uint flags, int dx, int dy)
        {
            var input = new INPUT { type = 0, mi = new MOUSEINPUT { dx = dx, dy = dy, dwFlags = flags } };
            if (SendInput(1, [input], Marshal.SizeOf<INPUT>()) != 1)
                throw new InvalidOperationException($"SendInput failed, error {Marshal.GetLastWin32Error()}.");
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int X, Y; }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT { public int dx, dy; public uint mouseData, dwFlags, time; public IntPtr dwExtraInfo; }

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT { public uint type; public MOUSEINPUT mi; }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint count, INPUT[] inputs, int size);

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, ref POINT point);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int index);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
    }
#endif
}
