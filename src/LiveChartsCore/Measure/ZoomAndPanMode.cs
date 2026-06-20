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

namespace LiveChartsCore.Measure;

/// <summary>
/// Defines the zooming and panning mode.
/// </summary>
[Flags]
public enum ZoomAndPanMode
{
    /// <summary>
    /// Disables zooming and panning.
    /// </summary>
    None = 0,

    /// <summary>
    /// Enables panning on the X axis.
    /// </summary>
    PanX = 1 << 0,

    /// <summary>
    /// Enables zooming on the X axis (wheel, pinch and zoom by section).
    /// </summary>
    ZoomX = 1 << 1,

    /// <summary>
    /// Enables panning on the Y axis.
    /// </summary>
    PanY = 1 << 2,

    /// <summary>
    /// Enables zooming on the Y axis (wheel, pinch and zoom by section).
    /// </summary>
    ZoomY = 1 << 3,

    /// <summary>
    /// Disables data bounds fitting when zooming or panning, this flag must be used in conjunction with
    /// any of the pan/zoom flags (<see cref="PanX"/>, <see cref="ZoomX"/>, <see cref="PanY"/>, <see cref="ZoomY"/>,
    /// <see cref="X"/>, <see cref="Y"/>, or <see cref="Both"/>) to have an effect.
    /// </summary>
    NoFit = 1 << 4,

    /// <summary>
    /// Disables the "Zoom by section" feature, which allows zooming in on a specific section of the chart.
    /// </summary>
    NoZoomBySection = 1 << 5,

    /// <summary>
    /// When this flag is present the panning will be triggered using the right click on desktop devices and a double tap on touch devices.
    /// The "Zoom by section" feature will be triggered by the left click on desktop devices and a single tap on touch devices,
    /// this flag must be used in conjunction with any of the pan/zoom flags
    /// (<see cref="PanX"/>, <see cref="ZoomX"/>, <see cref="PanY"/>, <see cref="ZoomY"/>,
    /// <see cref="X"/>, <see cref="Y"/>, or <see cref="Both"/>) to have an effect.
    /// </summary>
    InvertPanningPointerTrigger = 1 << 6,

    /// <summary>
    /// Enables zooming and panning on the X axis and enables fitting to bounds.
    /// Equivalent to <see cref="PanX"/> | <see cref="ZoomX"/>. To enable only one
    /// gesture on the X axis use <see cref="PanX"/> or <see cref="ZoomX"/> directly.
    /// </summary>
    X = PanX | ZoomX,

    /// <summary>
    /// Enables zooming and panning on the Y axis and enables fitting to bounds.
    /// Equivalent to <see cref="PanY"/> | <see cref="ZoomY"/>. To enable only one
    /// gesture on the Y axis use <see cref="PanY"/> or <see cref="ZoomY"/> directly.
    /// </summary>
    Y = PanY | ZoomY,

    /// <summary>
    /// Enables zooming and panning on both axes and enables fitting to bounds.
    /// </summary>
    Both = X | Y
}
