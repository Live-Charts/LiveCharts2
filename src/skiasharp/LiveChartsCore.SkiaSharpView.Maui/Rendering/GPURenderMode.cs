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
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView.Drawing;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace LiveChartsCore.SkiaSharpView.Maui.Rendering;

internal class GPURenderMode : SKGLView, IRenderMode
{
    private CoreMotionCanvas _canvas = null!;

    public event CoreMotionCanvas.FrameRequestHandler? FrameRequest;

    public void InitializeRenderMode(CoreMotionCanvas canvas)
    {
        _canvas = canvas;
        PaintSurface += OnPaintSurface;

        CoreMotionCanvas.s_rendererName = $"{nameof(GPURenderMode)} and {nameof(SKGLView)}";
    }

    public void DisposeRenderMode()
    {
        _canvas = null!;
        PaintSurface -= OnPaintSurface;
    }

    public void InvalidateRenderer() =>
        InvalidateSurface();

    private void OnPaintSurface(object? sender, SKPaintGLSurfaceEventArgs e)
    {
        // Derive scale from actual surface vs logical size so that per-display DPI
        // changes are always reflected correctly, regardless of what system APIs report.
        if (Width > 0 && Height > 0)
        {
            var scaleX = e.BackendRenderTarget.Width / (float)Width;
            var scaleY = e.BackendRenderTarget.Height / (float)Height;
            if (scaleX != 1f || scaleY != 1f)
                e.Surface.Canvas.Scale(scaleX, scaleY);
        }

        FrameRequest?.Invoke(
            new SkiaSharpDrawingContext(_canvas, e.Surface.Canvas, GetBackground()));
    }

    private SKColor GetBackground() =>
        (Parent?.Parent as IChartView)?.BackColor.AsSKColor() ?? SKColor.Empty;
}
