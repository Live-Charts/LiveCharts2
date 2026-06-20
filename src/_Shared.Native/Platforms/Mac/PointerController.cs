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

#if MACCATALYST || IOS

// reachable on maui ios/catalys or uno ios/catalyst (without skia renderer)

using System;
using CoreGraphics;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using UIKit;

namespace LiveChartsCore.Native;

internal partial class PointerController : INativePointerController
{
    private DateTime _previousPress = DateTime.MinValue;
    private LvcPoint _previousPressPosition;
    private float _previousScale = 1;
    private UILongPressGestureRecognizer _longPressGestureRecognizer;
    private UIPinchGestureRecognizer _pinchGestureRecognizer;
    private UIPanGestureRecognizer _panGestureRecognizer;
#if MACCATALYST
    private UIHoverGestureRecognizer _hoverGestureRecognizer;
#endif

    public void InitializeController(object view)
    {
        var macView = (UIView)view;

        macView.UserInteractionEnabled = true;

        macView.AddGestureRecognizer(_longPressGestureRecognizer);
        macView.AddGestureRecognizer(_pinchGestureRecognizer);
        macView.AddGestureRecognizer(_panGestureRecognizer);
#if MACCATALYST
        macView.AddGestureRecognizer(_hoverGestureRecognizer);
#endif
    }

    public void DisposeController(object view)
    {
        var macView = (UIView)view;

        macView.RemoveGestureRecognizer(_longPressGestureRecognizer);
        macView.RemoveGestureRecognizer(_pinchGestureRecognizer);
        macView.RemoveGestureRecognizer(_panGestureRecognizer);

        _longPressGestureRecognizer.Dispose();
        _pinchGestureRecognizer.Dispose();
        _panGestureRecognizer.Dispose();

        _longPressGestureRecognizer = null!;
        _pinchGestureRecognizer = null!;
        _panGestureRecognizer = null!;

#if MACCATALYST
        macView.RemoveGestureRecognizer(_hoverGestureRecognizer);
        _hoverGestureRecognizer.Dispose();
        _hoverGestureRecognizer = null!;
#endif
    }

    public PointerController()
    {
#if MACCATALYST
        _hoverGestureRecognizer = new UIHoverGestureRecognizer(OnHover)
        {
            // hover must coexist with the long-press recognizer used for clicks;
            // otherwise a click forces hover into Cancelled/Failed and tooltips
            // stop appearing afterwards. See issue #1436.
            ShouldRecognizeSimultaneously = (g1, g2) => true
        };
#endif
        _longPressGestureRecognizer = new UILongPressGestureRecognizer(OnLongPress)
        {
            MinimumPressDuration = 0,
            ShouldRecognizeSimultaneously = (g1, g2) => true
        };
        _pinchGestureRecognizer = new UIPinchGestureRecognizer(OnPinch)
        {
            ShouldRecognizeSimultaneously = (g1, g2) => true
        };
        _panGestureRecognizer = new UIPanGestureRecognizer(OnPan)
        {
#if MACCATALYST
            AllowedScrollTypesMask = UIScrollTypeMask.Discrete | UIScrollTypeMask.Continuous,
#endif
            MinimumNumberOfTouches = 0,
            ShouldRecognizeSimultaneously = (g1, g2) => true
        };
    }

#if MACCATALYST
    private void OnHover(UIHoverGestureRecognizer e)
    {
        var view = e.View;
        switch (e.State)
        {
            case UIGestureRecognizerState.Began:
            case UIGestureRecognizerState.Changed:
                var p = e.LocationInView(view);
                Moved?.Invoke(view, new(new(p.X, p.Y), e));
                break;
            case UIGestureRecognizerState.Cancelled:
            case UIGestureRecognizerState.Failed:
            case UIGestureRecognizerState.Ended:
                Exited?.Invoke(view, new(e));
                break;
            case UIGestureRecognizerState.Possible:
            default:
                break;
        }
    }
#endif

    private void OnLongPress(UILongPressGestureRecognizer e)
    {
        var view = e.View;
        var location = e.LocationInView(view);
        var p = new LvcPoint((float)location.X, (float)location.Y);
        var isRightClick = GestureHelpers.IsDoubleTap(p, _previousPressPosition, DateTime.Now - _previousPress);
        var isPinch = e.NumberOfTouches > 1;

        switch (e.State)
        {
            case UIGestureRecognizerState.Began:
                Pressed?.Invoke(view, new(p, isRightClick, e));
                _previousPress = DateTime.Now;
                _previousPressPosition = p;
                break;
            case UIGestureRecognizerState.Changed:
                Moved?.Invoke(view, new(p, e));
                break;
            case UIGestureRecognizerState.Cancelled:
            case UIGestureRecognizerState.Ended:
                Released?.Invoke(view, new(p, isRightClick, e));
                break;
            case UIGestureRecognizerState.Possible:
            case UIGestureRecognizerState.Failed:
            default:
                break;
        }
    }

    private void OnPinch(UIPinchGestureRecognizer e)
    {
        var view = e.View;
        var p = e.LocationInView(view);

        switch (e.State)
        {
            case UIGestureRecognizerState.Began:
                _previousScale = 1;
                break;
            case UIGestureRecognizerState.Changed:
                var s = (float)e.Scale;
                var delta = _previousScale - s;
                Pinched?.Invoke(view, new(1 - delta, new(p.X, p.Y), e));
                _previousScale = s;
                break;
            case UIGestureRecognizerState.Ended:
            case UIGestureRecognizerState.Cancelled:
                break;
            case UIGestureRecognizerState.Possible:
            case UIGestureRecognizerState.Failed:
            default:
                break;
        }
    }

    private CGPoint? _last;
    private void OnPan(UIPanGestureRecognizer e)
    {
        var view = e.View;
        var l = e.LocationInView(view);
        _last ??= l;
        var delta = _last.Value.Y - l.Y;
        var isZoom = e.NumberOfTouches == 0;
        var tolerance = 5; // just a factor to avoid multiple calls.

        if (e.State == UIGestureRecognizerState.Ended || !isZoom || Math.Abs(delta) < tolerance) return;
        Scrolled?.Invoke(view, new(new(l.X, l.Y), delta, e));
        _last = l;
    }
}

#endif
