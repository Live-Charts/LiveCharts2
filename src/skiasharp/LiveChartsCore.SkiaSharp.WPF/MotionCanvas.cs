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
using System.Windows.Controls;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView.WPF.Rendering;
namespace LiveChartsCore.SkiaSharpView.WPF;

/// <summary>
/// Defines the motion canvas control for WPF, <see cref="CoreMotionCanvas"/>.
/// </summary>
/// <seealso cref="Control" />
public class MotionCanvas : Canvas
{
    private readonly MotionCanvasComposer _composer;

    static MotionCanvas()
    {
        _ = LiveChartsSkiaSharp
            .EnsureInitialized()
            .HasRenderingFactory(
                (settings) =>
                {
#if NET6_0_OR_GREATER
                    IRenderMode renderMode = settings.UseGPU
                        ? new GPURenderMode()
                        : new CPURenderMode();

                    IFrameTicker ticker = settings.TryUseVSync
                        ? new CompositionTargetTicker()
                        : new AsyncLoopTicker();

                    return new MotionCanvasComposer(renderMode, ticker);
#else
                    if (settings.UseGPU)
                    {
                        System.Diagnostics.Trace.WriteLine(
                            "LiveCharts does not support hardware acceleration in WPF .Net Framework. " +
                            "Falling back to CPU rendering. To use GPU rendering, please migrate your project to .Net 6 or later.");
                    }

                    IRenderMode renderMode = new CPURenderMode();

                    IFrameTicker ticker = settings.TryUseVSync
                        ? new CompositionTargetTicker()
                        : new AsyncLoopTicker();

                    return new MotionCanvasComposer(renderMode, ticker);
#endif
                });
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MotionCanvas"/> class.
    /// </summary>
    public MotionCanvas()
    {
        _composer = LiveChartsSkiaSharp.MotionCanvasRenderingFactory(LiveCharts.RenderingSettings);

        _ = Children.Add((UIElement)_composer.RenderMode);

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
