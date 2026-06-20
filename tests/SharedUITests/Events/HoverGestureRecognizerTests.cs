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

#if MAUI_UI_TESTING && MACCATALYST

using System.Reflection;
using Factos;
using LiveChartsCore.Native;
using UIKit;
using Xunit;

namespace SharedUITests.Events;

public class HoverGestureRecognizerTests
{
    public AppController App => AppController.Current;

    // regression for https://github.com/Live-Charts/LiveCharts2/issues/1436
    // on Mac Catalyst the hover recognizer must coexist with the long-press
    // recognizer used for clicks. Without simultaneous recognition, a click
    // forces hover into Cancelled/Failed and tooltips stop showing afterwards.
    [AppTestMethod]
    public Task HoverRecognizer_AllowsSimultaneousRecognitionWithLongPress()
    {
        var controller = new PointerController();
        var hover = GetHoverRecognizer(controller);
        var longPress = GetLongPressRecognizer(controller);

        Assert.NotNull(hover.ShouldRecognizeSimultaneously);
        Assert.True(hover.ShouldRecognizeSimultaneously(hover, longPress));

        return Task.CompletedTask;
    }

    // regression for https://github.com/Live-Charts/LiveCharts2/issues/1436
    // when the hover gesture restarts (e.g. after a click), the first sample
    // arrives in Began state. OnHover must treat Began like Changed and raise
    // Moved, otherwise the tooltip won't reappear until the cursor leaves and
    // re-enters the chart.
    [AppTestMethod]
    public Task OnHover_RaisesMovedOnBeganState()
    {
        var controller = new PointerController();
        var onHover = typeof(PointerController).GetMethod(
            "OnHover", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(onHover);

        var movedFired = false;
        controller.Moved += (_, _) => movedFired = true;

        var fake = new FakeHoverGestureRecognizer { FakeState = UIGestureRecognizerState.Began };
        onHover!.Invoke(controller, [fake]);

        Assert.True(movedFired);
        return Task.CompletedTask;
    }

    private static UIHoverGestureRecognizer GetHoverRecognizer(PointerController controller)
    {
        var field = typeof(PointerController).GetField(
            "_hoverGestureRecognizer", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(field);
        return (UIHoverGestureRecognizer)field!.GetValue(controller)!;
    }

    private static UILongPressGestureRecognizer GetLongPressRecognizer(PointerController controller)
    {
        var field = typeof(PointerController).GetField(
            "_longPressGestureRecognizer", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(field);
        return (UILongPressGestureRecognizer)field!.GetValue(controller)!;
    }

    private sealed class FakeHoverGestureRecognizer : UIHoverGestureRecognizer
    {
        public FakeHoverGestureRecognizer() : base(_ => { }) { }
        public UIGestureRecognizerState FakeState { get; set; }
        public override UIGestureRecognizerState State => FakeState;
    }
}

#endif
