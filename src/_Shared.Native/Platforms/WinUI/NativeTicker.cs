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

#if WINDOWS

// reachable on winui, maui winui, uno winui

using System;
using LiveChartsCore.Motion;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media;

namespace LiveChartsCore.Native;

internal partial class NativeFrameTicker : IFrameTicker
{
    private IRenderMode _renderMode = null!;
    private CoreMotionCanvas _canvas = null!;
    private DispatcherQueue _dispatcher = null!;
    private bool _isSubscribed;

    public void InitializeTicker(CoreMotionCanvas canvas, IRenderMode renderMode)
    {
        _canvas = canvas;
        _renderMode = renderMode;
        _dispatcher = DispatcherQueue.GetForCurrentThread();

        _canvas.Invalidated += OnCoreInvalidated;
        _canvas.Validated += OnCoreValidated;

        CoreMotionCanvas.s_tickerName = "CompositionTarget.Rendering WinUI";

        if (!_canvas.IsValid) OnCoreInvalidated(_canvas);
    }

    private void OnCoreInvalidated(CoreMotionCanvas obj) => RunOnUI(StartLoop);

    private void OnCoreValidated(CoreMotionCanvas obj) => RunOnUI(StopLoop);

    private void StartLoop()
    {
        if (_isSubscribed || _canvas is null) return;
        _isSubscribed = true;
        CompositionTarget.Rendering += OnCompositonTargetRendering;
    }

    private void StopLoop()
    {
        if (!_isSubscribed) return;
        _isSubscribed = false;
        CompositionTarget.Rendering -= OnCompositonTargetRendering;
    }

    private void OnCompositonTargetRendering(object? sender, object e)
    {
        if (_canvas is null || _canvas.IsValid) { StopLoop(); return; }
        _renderMode.InvalidateRenderer();
    }

    private void RunOnUI(Action action)
    {
        if (_dispatcher is null || _dispatcher.HasThreadAccess) action();
        else _ = _dispatcher.TryEnqueue(() => action());
    }

    public void DisposeTicker()
    {
        StopLoop();

        // _canvas can be null when DisposeTicker is called without a prior
        // InitializeTicker, or twice in a row — same #2216 contract violation
        // guarded in the WPF CompositionTargetTicker.
        if (_canvas is not null)
        {
            _canvas.Invalidated -= OnCoreInvalidated;
            _canvas.Validated -= OnCoreValidated;
        }

        _canvas = null!;
        _renderMode = null!;
        _dispatcher = null!;
    }
}

#endif
