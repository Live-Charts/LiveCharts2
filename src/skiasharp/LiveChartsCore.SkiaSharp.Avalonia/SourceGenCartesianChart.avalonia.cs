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
using Avalonia.Input;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;

namespace LiveChartsGeneratedCode;

// ===============================================
// this file contains the Avalonia specific code
// ===============================================

/// <inheritdoc cref="ICartesianChartView" />
public partial class SourceGenCartesianChart : SourceGenChart, ICartesianChartView
{
    private float _previousPinchScale = 1;

    /// <summary>
    /// Initializes a new instance of the <see cref="SourceGenCartesianChart"/> class.
    /// </summary>
    /// <exception cref="Exception">Default colors are not valid</exception>
    public SourceGenCartesianChart()
    {
        var pinchGesture = new PinchGestureRecognizer();
        GestureRecognizers.Add(pinchGesture);
        AddHandler(PinchEvent, OnPinched);

        PointerWheelChanged += OnPointerWheelChanged;
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        e.Handled = true;

        var c = (CartesianChartEngine)CoreChart;
        var p = e.GetPosition(this);

        c.Zoom(ZoomMode, new((float)p.X, (float)p.Y), e.Delta.Y > 0 ? ZoomDirection.ZoomIn : ZoomDirection.ZoomOut);
    }

    private void OnPinched(object? sender, PinchEventArgs e)
    {
        var c = (CartesianChartEngine)CoreChart;
        var p = e.ScaleOrigin;
        var s = c.ControlSize;
        var pivot = new LvcPoint((float)(p.X * s.Width), (float)(p.Y * s.Height));

        var scale = (float)e.Scale;
        var delta = _previousPinchScale - scale;
        if (Math.Abs(delta) > 0.05) delta = 0; // ignore the first call.
        _previousPinchScale = scale;

        c.Zoom(ZoomMode, pivot, ZoomDirection.DefinedByScaleFactor, 1 - delta);

        // hack:
        // when the pinch started, the isPanning property is set to true,
        // when the pinch is completed, the pointerUp will be called,
        // and within that method panning will occur, lets prevent that
        // by setting isPanning to false here.
        c.ClearPointerDown();
    }
}
