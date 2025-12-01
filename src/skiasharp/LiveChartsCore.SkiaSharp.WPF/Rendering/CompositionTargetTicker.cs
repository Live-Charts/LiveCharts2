using System;
using System.Windows.Media;
using LiveChartsCore.Motion;

namespace LiveChartsCore.SkiaSharpView.WPF.Rendering;

internal class CompositionTargetTicker : IFrameTicker
{
    private IRenderMode? _renderMode;
    private CoreMotionCanvas? _canvas;
    private bool _disposed;

    public void InitializeTicker(CoreMotionCanvas canvas, IRenderMode renderMode)
    {
        _canvas = canvas;
        _renderMode = renderMode;
        _disposed = false;

        _canvas.Invalidated += OnCoreInvalidated;
        CompositionTarget.Rendering += OnCompositonTargetRendering;

        CoreMotionCanvas.s_tickerName = $"{nameof(CompositionTarget)}";
    }

    private void OnCoreInvalidated(CoreMotionCanvas obj)
    {
        if (_disposed) return;
        _renderMode?.InvalidateRenderer();
    }

    private void OnCompositonTargetRendering(object? sender, EventArgs e)
    {
        if (_disposed) return;

        // 如果Canvas 或 Renderer 已经释放，停止渲染事件（避免崩溃）
        if (_canvas == null || _renderMode == null)
        {
            DisposeTicker();
            return;
        }

        // 画布如果不再有效（需要重新渲染）
        if (!_canvas.IsValid)
        {
            _renderMode.InvalidateRenderer();
        }
    }

    public void DisposeTicker()
    {
        if (_disposed) return;
        _disposed = true;

        CompositionTarget.Rendering -= OnCompositonTargetRendering;

        if (_canvas != null)
            _canvas.Invalidated -= OnCoreInvalidated;

        _canvas = null;
        _renderMode = null;
    }
}
