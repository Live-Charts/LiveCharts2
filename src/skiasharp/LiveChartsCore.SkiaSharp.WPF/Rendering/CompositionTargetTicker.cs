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
using System.Windows.Media;
using LiveChartsCore.Motion;

namespace LiveChartsCore.SkiaSharpView.WPF.Rendering;

internal class CompositionTargetTicker : IFrameTicker
{
    private IRenderMode _renderMode = null!;
    private CoreMotionCanvas _canvas = null!;

    public void InitializeTicker(CoreMotionCanvas canvas, IRenderMode renderMode)
    {
        _canvas = canvas;
        _renderMode = renderMode;

        _canvas.Invalidated += OnCoreInvalidated;
        CompositionTarget.Rendering += OnCompositonTargetRendering;

        CoreMotionCanvas.s_tickerName = $"{nameof(CompositionTarget)}";
    }

    private void OnCoreInvalidated(CoreMotionCanvas obj) =>
        _renderMode.InvalidateRenderer();

    private void OnCompositonTargetRendering(object? sender, EventArgs e)
    {
        if (_canvas is null || _canvas.IsValid) return;
        _renderMode.InvalidateRenderer();
    }

    public void DisposeTicker()
    {
        CompositionTarget.Rendering -= OnCompositonTargetRendering;

        // _canvas can be null if Unloaded fires on a MotionCanvas whose Loaded
        // never ran (Prism-style ContentControl swaps where a TabControl
        // hosting the chart is removed while a different tab is active) or
        // if Dispose runs twice — both seen in #2216.
        if (_canvas is not null) _canvas.Invalidated -= OnCoreInvalidated;

        _canvas = null!;
        _renderMode = null!;
    }
}
