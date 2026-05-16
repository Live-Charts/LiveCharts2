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

namespace LiveChartsCore.Painting;

/// <summary>
/// Contains centralized z-index constants for LiveCharts2 components.
/// </summary>
internal static class PaintConstants
{
    // Axis-related z-index constants
    /// <summary>
    /// Default z-index for axis name paint.
    /// </summary>
    internal const double AxisNamePaintZIndex = -1;

    /// <summary>
    /// Default z-index for axis labels paint.
    /// </summary>
    internal const double AxisLabelsPaintZIndex = -0.9;

    /// <summary>
    /// Default z-index for axis separators paint.
    /// </summary>
    internal const double AxisSeparatorsPaintZIndex = -1;

    /// <summary>
    /// Default z-index for axis sub-separators paint.
    /// </summary>
    internal const double AxisSubseparatorsPaintZIndex = -1;

    /// <summary>
    /// Default z-index for axis ticks paint.
    /// </summary>
    internal const double AxisTicksPaintZIndex = -0.8;

    /// <summary>
    /// Default z-index for axis sub-ticks paint.
    /// </summary>
    internal const double AxisSubticksPaintZIndex = -0.85;

    /// <summary>
    /// Default z-index for axis zero line paint.
    /// </summary>
    internal const double AxisZeroPaintZIndex = -1;

    // Crosshair-related z-index constants
    /// <summary>
    /// Default z-index for crosshair paint.
    /// </summary>
    internal const double CrosshairPaintZIndex = 1050;

    /// <summary>
    /// Default z-index for crosshair labels paint.
    /// </summary>
    internal const double CrosshairLabelsPaintZIndex = 1050;

    // Series-related z-index constants
    /// <summary>
    /// Z-index offset for series fill paint.
    /// </summary>
    internal const double SeriesFillZIndexOffset = 0.1;

    /// <summary>
    /// Z-index offset for series stroke paint.
    /// </summary>
    internal const double SeriesStrokeZIndexOffset = 0.2;

    /// <summary>
    /// Z-index offset for series geometry fill paint.
    /// </summary>
    internal const double SeriesGeometryFillZIndexOffset = 0.3;

    /// <summary>
    /// Z-index offset for series geometry stroke paint.
    /// </summary>
    internal const double SeriesGeometryStrokeZIndexOffset = 0.4;

    /// <summary>
    /// Z-index offset for series data labels paint.
    /// </summary>
    internal const double SeriesDataLabelsZIndexOffset = 0.5;

    // Visual elements z-index constants
    /// <summary>
    /// Default z-index for angular ticks visual stroke.
    /// </summary>
    internal const double AngularTicksStrokeZIndex = 998;

    /// <summary>
    /// Default z-index for angular ticks labels and needle fill.
    /// </summary>
    internal const double AngularTicksLabelsZIndex = 999;

    /// <summary>
    /// Default z-index for needle fill.
    /// </summary>
    internal const double NeedleFillZIndex = 999;

    // Map-related z-index constants
    /// <summary>
    /// Default z-index for geo map stroke.
    /// </summary>
    internal const double GeoMapStrokeZIndex = 2;

    /// <summary>
    /// Base z-index for pie series data labels to ensure they appear on top.
    /// </summary>
    internal const double PieSeriesDataLabelsBaseZIndex = 1000;

    // Frame-related z-index constants
    /// <summary>
    /// Default z-index for draw margin frame fill.
    /// </summary>
    internal const double DrawMarginFrameFillZIndex = -3;

    /// <summary>
    /// Default z-index for draw margin frame stroke.
    /// </summary>
    internal const double DrawMarginFrameStrokeZIndex = -0.9;

    // Section-related z-index constants
    /// <summary>
    /// Default z-index for section fill.
    /// </summary>
    internal const double SectionFillZIndex = -2.5;

    /// <summary>
    /// Default z-index for section stroke.
    /// </summary>
    internal const double SectionStrokeZIndex = 0;

    /// <summary>
    /// Default z-index for section labels.
    /// </summary>
    internal const double SectionLabelsZIndex = 0.01;
}