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

namespace LiveChartsCore.Kernel;

/// <summary>
/// Defines the series properties enumeration.
/// </summary>
[Flags]
public enum SeriesProperties
{
    /// <summary>
    /// includes all series
    /// </summary>
    AllSeries = 0,

    /// <summary>
    /// The Cartesian series
    /// </summary>
    CartesianSeries = 1 << 1,

    /// <summary>
    /// The bar series
    /// </summary>
    /// 
    Bar = 1 << 2,

    /// <summary>
    /// The line series
    /// </summary>
    Line = 1 << 3,

    /// <summary>
    /// The stepline series
    /// </summary>
    StepLine = 1 << 4,

    /// <summary>
    /// The scatter series
    /// </summary>
    Scatter = 1 << 5,

    /// <summary>
    /// The heat series.
    /// </summary>
    Heat = 1 << 6,

    /// <summary>
    /// The financial series.
    /// </summary>
    Financial = 1 << 7,

    /// <summary>
    /// The pie series.
    /// </summary>
    PieSeries = 1 << 8,

    /// <summary>
    /// The stacked series
    /// </summary>
    Stacked = 1 << 9,

    /// <summary>
    /// The vertical orientation
    /// </summary>
    PrimaryAxisVerticalOrientation = 1 << 10,

    /// <summary>
    /// The horizontal orientation
    /// </summary>
    PrimaryAxisHorizontalOrientation = 1 << 11,

    /// <summary>
    /// The gauge.
    /// </summary>
    Gauge = 1 << 12,

    /// <summary>
    /// The gauge fill.
    /// </summary>
    GaugeFill = 1 << 13,

    /// <summary>
    /// The sketch
    /// </summary>
    Sketch = 1 << 14,

    /// <summary>
    /// The solid
    /// </summary>
    Solid = 1 << 15,

    /// <summary>
    /// The prefers x tool tips
    /// </summary>
    PrefersXStrategyTooltips = 1 << 16,

    /// <summary>
    /// The prefers y tool tips
    /// </summary>
    PrefersYStrategyTooltips = 1 << 17,

    /// <summary>
    /// The prefers xy tool tips
    /// </summary>
    PrefersXYStrategyTooltips = 1 << 18,

    /// <summary>
    /// The polar series
    /// </summary>
    Polar = 1 << 19,

    /// <summary>
    /// The polar line series
    /// </summary>
    PolarLine = 1 << 20,

    /// <summary>
    /// Sepcifies that the series visual comes from a svg path.
    /// </summary>
    IsSVGPath = 1 << 21,

    /// <summary>
    /// The box series.
    /// </summary>
    BoxSeries = 1 << 22,

    /// <summary>
    /// The sankey series.
    /// </summary>
    Sankey = 1 << 23
}
