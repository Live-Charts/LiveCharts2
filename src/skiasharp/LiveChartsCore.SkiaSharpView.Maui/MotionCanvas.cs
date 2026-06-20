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
using LiveChartsCore.Motion;
using LiveChartsCore.Native;
using LiveChartsCore.SkiaSharpView.Maui.Rendering;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;

namespace LiveChartsCore.SkiaSharpView.Maui;

/// <summary>
/// Defines the motion cavnas class for Maui.
/// </summary>
public class MotionCanvas : AbsoluteLayout
{
    private readonly MotionCanvasComposer _composer;

    static MotionCanvas()
    {
        if (!LiveChartsCoreMauiAppBuilderExtensions.AreHandlersRegistered)
        {
            throw new InvalidOperationException(
                "`.UseLiveCharts()` and `.UseSkiaSharp()` must be " +
                "chained to `.UseMauiApp<T>()`, in the MauiProgram.cs file. For more info see: " +
                "https://livecharts.dev/docs/Maui/latest/Overview.Installation");
        }

        _ = LiveChartsSkiaSharp
            .EnsureInitialized()
            .HasRenderingFactory(
                (settings) =>
                {
                    IRenderMode renderMode = settings.UseGPU
                        ? new GPURenderMode()
                        : new CPURenderMode();

                    IFrameTicker ticker = settings.TryUseVSync
                        ? new NativeFrameTicker()
                        : new AsyncLoopTicker();

                    return new MotionCanvasComposer(renderMode, ticker);
                });
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MotionCanvas"/> class.
    /// </summary>
    public MotionCanvas()
    {
        _composer = LiveChartsSkiaSharp.MotionCanvasRenderingFactory(LiveCharts.RenderingSettings);

        var view = (View)_composer.RenderMode;
        AbsoluteLayout.SetLayoutBounds(view, new(0, 0, 1, 1));
        AbsoluteLayout.SetLayoutFlags(view, AbsoluteLayoutFlags.SizeProportional | AbsoluteLayoutFlags.PositionProportional);
        Children.Add(view);

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    /// <inheritdoc cref="CoreMotionCanvas"/>
    public CoreMotionCanvas CanvasCore { get; } = new();

    private void OnLoaded(object? sender, EventArgs e) =>
        _composer.Initialize(CanvasCore);

    private void OnUnloaded(object? sender, EventArgs e) =>
        _composer.Dispose(CanvasCore);
}
