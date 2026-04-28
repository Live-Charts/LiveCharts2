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

using System.Windows;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView.Drawing;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;

namespace LiveChartsCore.SkiaSharpView.WPF.Rendering;

internal class GPURenderMode : SKGLElement, IRenderMode
{
    private CoreMotionCanvas _canvas = null!;

    public event CoreMotionCanvas.FrameRequestHandler? FrameRequest;

    public void InitializeRenderMode(CoreMotionCanvas canvas)
    {
        _canvas = canvas;
        PaintSurface += OnPaintSurface;

        CoreMotionCanvas.s_rendererName = $"{nameof(GPURenderMode)} and {nameof(SKGLElement)}";
    }

    public void DisposeRenderMode()
    {
        _canvas = null!;
        PaintSurface -= OnPaintSurface;
        Dispose();
    }

    public void InvalidateRenderer() =>
        InvalidateVisual();

    private void OnPaintSurface(object? sender, SKPaintGLSurfaceEventArgs args)
    {
        // Derive scale from actual surface vs logical size so that RDP and per-monitor
        // DPI changes are always reflected correctly, regardless of what system APIs report.
        if (ActualWidth > 0 && ActualHeight > 0)
        {
            var scaleX = args.BackendRenderTarget.Width / (float)ActualWidth;
            var scaleY = args.BackendRenderTarget.Height / (float)ActualHeight;
            if (scaleX != 1f || scaleY != 1f)
                args.Surface.Canvas.Scale(scaleX, scaleY);
        }

        FrameRequest?.Invoke(
            new SkiaSharpDrawingContext(_canvas, args.Surface.Canvas, GetBackground()));
    }

    private SKColor GetBackground() =>
        ((Parent as FrameworkElement)?.Parent as IChartView)?.BackColor.AsSKColor() ?? SKColor.Empty;
}
