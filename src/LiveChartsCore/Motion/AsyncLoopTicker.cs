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

using System.Threading.Tasks;

namespace LiveChartsCore.Motion;

internal class AsyncLoopTicker : IFrameTicker
{
    private IRenderMode _renderMode = null!;
    private CoreMotionCanvas _canvas = null!;
    private bool _isDrawingLoopRunning = false;

    public void InitializeTicker(CoreMotionCanvas canvas, IRenderMode renderMode)
    {
        _canvas = canvas;
        _renderMode = renderMode;

        _canvas.Invalidated += OnCoreInvalidated;
        CoreMotionCanvas.s_tickerName = nameof(AsyncLoopTicker);
    }

    private void OnCoreInvalidated(CoreMotionCanvas obj) =>
        _ = RunDrawingLoop();

    private async Task RunDrawingLoop()
    {
        if (_isDrawingLoopRunning) return;
        _isDrawingLoopRunning = true;

        while (_canvas is not null && !_canvas.IsValid)
        {
            _renderMode.InvalidateRenderer();
            await Task.Delay(_canvas._nextFrameDelay);
        }

        _isDrawingLoopRunning = false;
    }

    public void DisposeTicker()
    {
        // _canvas can be null when DisposeTicker is called without a prior
        // InitializeTicker, or twice in a row. The WPF sibling
        // CompositionTargetTicker hits this in #2216; same shape applies here.
        if (_canvas is not null) _canvas.Invalidated -= OnCoreInvalidated;

        _canvas = null!;
        _renderMode = null!;
    }
}
