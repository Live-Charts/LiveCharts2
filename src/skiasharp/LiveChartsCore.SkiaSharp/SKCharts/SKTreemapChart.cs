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

using LiveChartsCore.Kernel.Sketches;

namespace LiveChartsCore.SkiaSharpView.SKCharts;

// ==============================================================================
//
// use the LiveChartsGeneratedCode.SourceGenSKTreemapChart class to add Skia (image generation) specific
// code, this class is just to expose the TreemapChart class in this namespace.
//
// ==============================================================================

/// <inheritdoc cref="ITreemapChartView"/>
public class SKTreemapChart : LiveChartsGeneratedCode.SourceGenSKTreemapChart
{
    /// <summary>Initializes a new instance of the <see cref="SKTreemapChart"/> class.</summary>
    public SKTreemapChart() : base(null) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SKTreemapChart"/> class
    /// from an existing chart view (theme + control size are reused).
    /// </summary>
    public SKTreemapChart(IChartView chartView) : base(chartView) { }
}
