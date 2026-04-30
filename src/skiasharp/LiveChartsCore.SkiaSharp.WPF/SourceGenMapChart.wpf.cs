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
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using LiveChartsCore.Drawing;
using LiveChartsCore.Geo;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView.WPF;

namespace LiveChartsGeneratedCode;

// ===============================================
// this file contains the WPF specific code
// ===============================================

/// <inheritdoc cref="IGeoMapView" />
public abstract partial class SourceGenMapChart : UserControl, IGeoMapView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SourceGenMapChart"/> class.
    /// </summary>
    /// <exception cref="Exception">Default colors are not valid</exception>
    protected SourceGenMapChart()
    {
        Content = new MotionCanvas();

        SizeChanged += (s, e) =>
            CoreChart.Update();

        InitializeChartControl();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private MotionCanvas MotionCanvas => (MotionCanvas)Content;

    /// <inheritdoc cref="IDrawnView.CoreCanvas" />
    public CoreMotionCanvas CoreCanvas => MotionCanvas.CanvasCore;

    bool IGeoMapView.DesignerMode => DesignerProperties.GetIsInDesignMode(this);
    LvcSize IDrawnView.ControlSize => new() { Width = (float)ActualWidth, Height = (float)ActualHeight };

    private void OnLoaded(object sender, RoutedEventArgs e) =>
        CoreChart?.Load();

    private void OnUnloaded(object sender, RoutedEventArgs e) =>
        CoreChart?.Unload();

    void IGeoMapView.InvokeOnUIThread(Action action) =>
        Dispatcher.Invoke(action);
}
