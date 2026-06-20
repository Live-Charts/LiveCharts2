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

using System.ComponentModel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.WinForms.Rendering;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace LiveChartsCore.SkiaSharpView.WinForms;

/// <summary>
/// The motion canvas control for windows forms, <see cref="CoreMotionCanvas"/>.
/// </summary>
/// <seealso cref="UserControl" />
[DesignerCategory("")]
public partial class MotionCanvas : UserControl, IRenderMode
{
    private IFrameTicker _ticker = null!;

    static MotionCanvas()
    {
        _ = LiveChartsSkiaSharp.EnsureInitialized();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MotionCanvas"/> class.
    /// </summary>
    public MotionCanvas()
    {
        InitializeComponent();
    }

    event CoreMotionCanvas.FrameRequestHandler IRenderMode.FrameRequest
    {
        add => throw new NotImplementedException();
        remove => throw new NotImplementedException();
    }

    /// <inheritdoc cref="CoreMotionCanvas"/>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public CoreMotionCanvas CanvasCore { get; } = new();

    /// <inheritdoc cref="Control.CreateHandle()"/>
    protected override void CreateHandle()
    {
        base.CreateHandle();
        _ticker = LiveCharts.RenderingSettings.TryUseVSync
            ? new ApplicationIdleTicker()
            : new AsyncLoopTicker();
        _ticker.InitializeTicker(CanvasCore, this);
        _skControl?.Invalidate();
        _skglControl?.Invalidate();
    }

    /// <inheritdoc cref="Control.OnHandleDestroyed(EventArgs)"/>
    protected override void OnHandleDestroyed(EventArgs e)
    {
        base.OnHandleDestroyed(e);
        _ticker.DisposeTicker();
        CanvasCore.Dispose();
    }

    private void SkControl_PaintSurface(object sender, SKPaintSurfaceEventArgs e) =>
        CanvasCore.DrawFrame(
            new SkiaSharpDrawingContext(CanvasCore, e.Surface.Canvas, GetBackground()));

    private void SkglControl_PaintSurface(object sender, SKPaintGLSurfaceEventArgs e) =>
        CanvasCore.DrawFrame(
            new SkiaSharpDrawingContext(CanvasCore, e.Surface.Canvas, GetBackground()));

    private SKColor GetBackground() =>
        (Parent as IChartView)?.BackColor.AsSKColor() ?? SKColor.Empty;

    void IRenderMode.InitializeRenderMode(CoreMotionCanvas canvas) =>
        throw new NotImplementedException();

    void IRenderMode.InvalidateRenderer()
    {
        _skControl?.Invalidate();
        _skglControl?.Invalidate();
    }

    void IRenderMode.DisposeRenderMode() =>
        throw new NotImplementedException();
}
