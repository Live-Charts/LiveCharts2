using Factos;
using SharedUITests.Helpers;
using Xunit;

#if WINUI_UI_TESTING
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Input;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinUI;
#endif

// to run these tests, see the UITests project, specially the program.cs file.
// to enable IDE intellisense for these tests, go to Directory.Build.props and set UITesting to true.

namespace SharedUITests;

public class PointerInputTests
{
    public AppController App => AppController.Current;

#if WINUI_UI_TESTING
    // https://github.com/Live-Charts/LiveCharts2/issues/2389
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
                PointerMoveCommand = new CountingCommand(() => moves++)
            };
            var rawMoves = 0;
            sut.PointerMoved += (_, _) => rawMoves++;

            await App.NavigateToView(sut);
            await sut.WaitUntilChartLoadsAndRenders();

            var from = RealMouse.ToScreen(sut, 0.3, 0.5);
            var to = RealMouse.ToScreen(sut, 0.7, 0.5);

            RealMouse.MoveTo(from.X, from.Y);
            await Task.Delay(100);
            RealMouse.LeftDown();
            await Task.Delay(200); // past the 100ms the controller waits after a press

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

            // the OS input reached the chart: WinUI raised its own PointerMoved during the drag.
            Assert.True(rawDuringDrag > 10,
                $"WinUI raised PointerMoved {rawDuringDrag} times during the drag, the simulated mouse input didn't reach the chart.");

            // and LiveCharts handled those moves (it dropped all of them before the fix).
            Assert.True(commandDuringDrag >= rawDuringDrag / 2,
                $"PointerMoveCommand ran {commandDuringDrag} times during the drag, WinUI raised PointerMoved {rawDuringDrag} times.");
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
    }
#endif
}
