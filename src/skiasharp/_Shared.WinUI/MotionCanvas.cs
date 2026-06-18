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

using LiveChartsCore.Motion;
using LiveChartsCore.Native;
using LiveChartsCore.SkiaSharpView.WinUI.Rendering;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

#pragma warning disable IDE0028 // Simplify collection initialization

namespace LiveChartsCore.SkiaSharpView.WinUI;

/// <summary>
/// The motion canvas control for WinUI and Uno Platform.
/// </summary>
public partial class MotionCanvas : Canvas
{
    private readonly MotionCanvasComposer _composer;

    static MotionCanvas()
    {
        _ = LiveChartsSkiaSharp
            .EnsureInitialized()
            .HasRenderingFactory(
                (settings) =>
                {
                    IFrameTicker ticker;

#if !HAS_OS_LVC
                    // SKCanvasElement only works when the app runs Uno's Skia renderer;
                    // under the native renderer (e.g. WASM native) its constructor throws
                    // PlatformNotSupportedException, so fall back to the SkiaSharp.Views
                    // elements in that case (#2020, #2333). CompositionTarget.Rendering
                    // does not tick continuously under the native renderer either, so the
                    // fallback also needs the async-loop ticker or animations freeze at
                    // the first frame.
                    IRenderMode renderMode;

                    if (Uno.WinUI.Graphics2DSK.SKCanvasElement.IsSupportedOnCurrentPlatform())
                    {
                        // At this point, Uno has full control of the frame pacing.
                        // SkiaRenderMode inherits from SKCanvasElement, it integrates with Uno's skia renderer and will draw when and how Uno needs.
                        // NativeFrameTicker is a dummy ticker at this point, instead the frames are paced by Uno's skia renderer.
                        renderMode = new SkiaRenderMode();
                        ticker = new NativeFrameTicker();
                    }
                    else
                    {
                        renderMode = settings.UseGPU
                            ? new GPURenderMode()
                            : new CPURenderMode();

                        ticker = new AsyncLoopTicker();
                    }
#else
                    IRenderMode renderMode = settings.UseGPU
                        ? new GPURenderMode()
                        : new CPURenderMode();

                    ticker = settings.TryUseVSync
                        ? new NativeFrameTicker()
                        : new AsyncLoopTicker();
#endif

                    return new MotionCanvasComposer(renderMode, ticker);
                });
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MotionCanvas"/> class.
    /// </summary>
    public MotionCanvas()
    {
        _composer = LiveChartsSkiaSharp.MotionCanvasRenderingFactory(LiveCharts.RenderingSettings);

        Children.Add((UIElement)_composer.RenderMode);

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;

        SizeChanged += OnSizeChanged;
    }

    /// <inheritdoc cref="CoreMotionCanvas"/>
    public CoreMotionCanvas CanvasCore { get; } = new();

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var fe = (FrameworkElement)_composer.RenderMode;

        fe.Width = e.NewSize.Width;
        fe.Height = e.NewSize.Height;
    }

    private void OnLoaded(object sender, RoutedEventArgs e) =>
        _composer.Initialize(CanvasCore);

    private void OnUnloaded(object sender, RoutedEventArgs e) =>
        _composer.Dispose(CanvasCore);
}
