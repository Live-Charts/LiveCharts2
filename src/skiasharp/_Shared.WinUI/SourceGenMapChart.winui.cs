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
using System.Runtime.InteropServices;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using LiveChartsCore.Motion;
using LiveChartsCore.Geo;

namespace LiveChartsGeneratedCode;

// ===============================================
// this file contains the WinUI/Uno specific code
// ===============================================

/// <inheritdoc cref="IChartView"/>
public abstract partial class SourceGenMapChart : UserControl, IGeoMapView
{
    private static readonly bool s_isWebAssembly = RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER"));
    private bool _wasInViewport;

    /// <summary>
    /// Initializes a new instance of the <see cref="SourceGenMapChart"/> class.
    /// </summary>
    public SourceGenMapChart()
    {
        Content = new MotionCanvas();

        InitializeChartControl();

        SizeChanged += (s, e) =>
            CoreChart.Update();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        EffectiveViewportChanged += OnEffectiveViewportChanged;
    }

    private MotionCanvas MotionCanvas => (MotionCanvas)Content;

    /// <inheritdoc cref="IDrawnView.CoreCanvas"/>
    public CoreMotionCanvas CoreCanvas => MotionCanvas.CanvasCore;

    bool IGeoMapView.DesignerMode => false;
    LvcSize IDrawnView.ControlSize => new() { Width = (float)ActualWidth, Height = (float)ActualHeight };

    private void OnLoaded(object sender, RoutedEventArgs e) =>
        CoreChart?.Load();

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        CoreChart?.Unload();
        _wasInViewport = false;
    }

    // Same as SourceGenChart.OnEffectiveViewportChanged, see
    // https://github.com/Live-Charts/LiveCharts2/issues/1986
    private void OnEffectiveViewportChanged(FrameworkElement sender, EffectiveViewportChangedEventArgs e)
    {
        var vp = e.EffectiveViewport;
        var nowInViewport =
            vp.Width > 0 && vp.Height > 0 &&
            vp.X < ActualWidth && vp.X + vp.Width > 0 &&
            vp.Y < ActualHeight && vp.Y + vp.Height > 0;

        if (nowInViewport && !_wasInViewport)
        {
            CoreCanvas.NotifyPlatformVisible();
            CoreChart?.Update();
        }
        _wasInViewport = nowInViewport;
    }

    void IGeoMapView.InvokeOnUIThread(Action action)
    {
        if (s_isWebAssembly)
        {
            action();
            return;
        }

        _ = DispatcherQueue.TryEnqueue(
            Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () => action());
    }
}
