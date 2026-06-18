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

#if !HAS_OS_LVC && UNO_LVC

// reachable on uno skia renderer
// HAS_OS_LVC is true when the target framework contains any of the following:
// -windows, -android, -ios, -maccatalyst, -tizen
// This is a dummy class, when Uno SkiaRenderer is used, we don't need a ticker, the control itself
// can (and should) pace frame requests. This class is needed for compatibility with Native implementations that use
// the ticker to request frames, like CAD display link on iOS, Choreographer on Android or CompositionTarget.Rendering on Windows.

using LiveChartsCore.Motion;

namespace LiveChartsCore.Native;

internal partial class NativeFrameTicker : IFrameTicker
{
    public void InitializeTicker(CoreMotionCanvas canvas, IRenderMode renderMode) =>
        // no real ticker here: Uno's skia renderer paces the frames itself.
        // still report a name so the FPS/debug overlay does not show an empty ticker.
        CoreMotionCanvas.s_tickerName = "self-paced by Uno's skia renderer";

    public void DisposeTicker()
    { }
}

#endif
