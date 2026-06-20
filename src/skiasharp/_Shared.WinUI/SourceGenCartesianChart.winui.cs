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

using LiveChartsCore.Native.Events;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore;

namespace LiveChartsGeneratedCode;

// ===============================================
// this file contains the WinUI/Uno specific code
// ===============================================

/// <inheritdoc cref="IChartView" />
public partial class SourceGenCartesianChart : SourceGenChart, ICartesianChartView
{
    internal override void OnScrolled(object? sender, ScrollEventArgs args)
    {
        var c = (CartesianChartEngine)CoreChart;
        c.Zoom(ZoomMode, args.Location, args.ScrollDelta > 0 ? ZoomDirection.ZoomIn : ZoomDirection.ZoomOut);
    }

    internal override void OnPinched(object? sender, PinchEventArgs args)
    {
        var c = (CartesianChartEngine)CoreChart;
        var p = args.PinchStart;
        var pivot = new LvcPoint(p.X, p.Y);
        c.Zoom(ZoomMode, pivot, ZoomDirection.DefinedByScaleFactor, args.Scale);

        // hack:
        // when the pinch started, the isPanning property is set to true,
        // when the pinch is completed, the pointerUp will be called,
        // and within that method panning will occur, lets prevent that
        // by setting isPanning to false here.
        c.ClearPointerDown();
    }
}
