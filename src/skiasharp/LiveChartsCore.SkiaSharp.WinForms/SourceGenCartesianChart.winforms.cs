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

using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;

namespace LiveChartsGeneratedCode;

// ===============================================
// this file contains the Winforms specific code
// ===============================================

/// <inheritdoc cref="ICartesianChartView" />
public partial class SourceGenCartesianChart : SourceGenChart, ICartesianChartView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SourceGenCartesianChart"/> class.
    /// </summary>
    public SourceGenCartesianChart()
    {
        GetDrawnControl().MouseWheel += OnMouseWheel;
    }

    private void OnMouseWheel(object? sender, MouseEventArgs e)
    {
        var c = (CartesianChartEngine)CoreChart;
        var p = e.Location;
        c.Zoom(ZoomMode, new LvcPoint(p.X, p.Y), e.Delta > 0 ? ZoomDirection.ZoomIn : ZoomDirection.ZoomOut);
    }
}
