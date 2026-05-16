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

using LiveChartsCore.Kernel.Sketches;

#if WINFORMS_UI_TESTING
using System.Reflection;
using System.Windows.Forms;
using LiveChartsGeneratedCode;
#endif

namespace SharedUITests.Helpers;

// these helpers raise mouse-input events on a chart's actual input source
// (the inner SKControl on WinForms, the MotionCanvas host on XAML platforms,
// etc.) so contract tests like "the outer chart re-raises MouseClick" cover
// the real wiring rather than the outer UserControl alone.
//
// Factos cannot drive real OS input, so we reflect into the framework's
// protected OnXxx raisers — those are stable inheritance APIs.

public enum InputKind
{
    Move,
    Down,
    Up,
    Click,
    DoubleClick,
    Leave
}

public enum InputButton
{
    Left,
    Right,
    Middle
}

public static class InputTesting
{
    extension(IChartView chartView)
    {
        public Task RaiseInput(
            InputKind kind,
            double x,
            double y,
            InputButton button = InputButton.Left)
        {
#if WINFORMS_UI_TESTING
            return WinFormsInputRaiser.Raise(chartView, kind, x, y, button);
#else
            throw new PlatformNotSupportedException(
                "InputTesting.RaiseInput is not implemented for this UI host yet.");
#endif
        }
    }
}

#if WINFORMS_UI_TESTING
internal static class WinFormsInputRaiser
{
    public static Task Raise(
        IChartView chartView,
        InputKind kind,
        double x,
        double y,
        InputButton button)
    {
        var outer = (Control)chartView;
        var inner = ((SourceGenChart)chartView).GetDrawnControl();

        var methodName = kind switch
        {
            InputKind.Down => "OnMouseDown",
            InputKind.Move => "OnMouseMove",
            InputKind.Up => "OnMouseUp",
            InputKind.Click => "OnMouseClick",
            InputKind.DoubleClick => "OnMouseDoubleClick",
            InputKind.Leave => "OnMouseLeave",
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };

        var method = typeof(Control).GetMethod(
            methodName, BindingFlags.Instance | BindingFlags.NonPublic)!;

        object[] args;
        if (kind == InputKind.Leave)
        {
            args = [EventArgs.Empty];
        }
        else
        {
            var clicks = kind == InputKind.DoubleClick ? 2 : 1;
            args =
            [
                new MouseEventArgs(MapButton(button), clicks, (int)x, (int)y, 0)
            ];
        }

        // Factos test methods don't run on the chart's UI thread, so marshal.
        var tcs = new TaskCompletionSource<object?>();
        _ = outer.BeginInvoke(() =>
        {
            try
            {
                _ = method.Invoke(inner, args);
                tcs.SetResult(null);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        return tcs.Task;
    }

    private static MouseButtons MapButton(InputButton b) => b switch
    {
        InputButton.Left => MouseButtons.Left,
        InputButton.Right => MouseButtons.Right,
        InputButton.Middle => MouseButtons.Middle,
        _ => throw new ArgumentOutOfRangeException(nameof(b))
    };
}
#endif
