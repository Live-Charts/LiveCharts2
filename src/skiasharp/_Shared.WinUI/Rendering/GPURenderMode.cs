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

#if HAS_OS_LVC

using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView.Drawing;
using Microsoft.UI.Xaml;
using SkiaSharp;
using SkiaSharp.Views.Windows;

namespace LiveChartsCore.SkiaSharpView.WinUI.Rendering;

internal partial class GPURenderMode : SKSwapChainPanel, IRenderMode
{
    private CoreMotionCanvas _canvas = null!;

    public event CoreMotionCanvas.FrameRequestHandler? FrameRequest;

    public void InitializeRenderMode(CoreMotionCanvas canvas)
    {
        _canvas = canvas;
        PaintSurface += OnPaintSurface;

        CoreMotionCanvas.s_rendererName = $"{nameof(GPURenderMode)} and {nameof(SKSwapChainPanel)}";
    }

    public void DisposeRenderMode()
    {
        _canvas = null!;
        PaintSurface -= OnPaintSurface;
    }

    private void OnPaintSurface(object? sender, SKPaintGLSurfaceEventArgs e)
    {
        var density = GetPixelDensity();
        if (density.DpiX != 1 || density.DpiY != 1)
            e.Surface.Canvas.Scale(density.DpiX, density.DpiY);

        FrameRequest?.Invoke(new SkiaSharpDrawingContext(
            _canvas, e.Surface.Canvas, GetBackground()));
    }

    public void InvalidateRenderer() =>
        Invalidate();

    private PixelDensity GetPixelDensity()
    {
#if HAS_UNO_WINUI
        var d = Windows.Graphics.Display.DisplayInformation.GetForCurrentView().LogicalDpi / 96.0f;
        return new(d, d);
#else
        var scaleFactor = (float)XamlRoot.RasterizationScale;
        return new(scaleFactor, scaleFactor);
#endif
    }

    private readonly struct PixelDensity(float dpiX, float dpiY)
    {
        public float DpiX { get; } = dpiX;
        public float DpiY { get; } = dpiY;
    }

    private SKColor GetBackground() =>
        ((Parent as FrameworkElement)?.Parent as IChartView)?.BackColor.AsSKColor() ?? SKColor.Empty;
}

#endif
