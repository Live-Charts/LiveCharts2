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
using System.Collections.Generic;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.Painting;

namespace LiveChartsCore.Geo;

/// <summary>
/// Defines a geographic map view.
/// </summary>
/// <remarks>
/// Inherits from <see cref="IChartView"/> so the map shares the same view
/// contract as cartesian / pie / polar views and is driven through the unified
/// <see cref="Chart"/> base. <see cref="CoreChart"/>, <see cref="Series"/> and
/// <see cref="Tooltip"/> shadow base members with map-specific types; everything
/// else (DesignerMode, IsDarkMode, AutoUpdateEnabled, SyncContext, theme,
/// tooltip-text/background/size, etc.) is inherited unchanged.
/// </remarks>
public interface IGeoMapView : IChartView
{
    /// <summary>
    /// Gets the core chart. Shadows <see cref="IChartView.CoreChart"/> with the
    /// more-derived <see cref="GeoMapChart"/> type.
    /// </summary>
    new GeoMapChart CoreChart { get; }

    /// <summary>
    /// Gets or sets the series. Shadows <see cref="IChartView.Series"/> because
    /// geo series do not implement the chart-series interface.
    /// </summary>
    new IEnumerable<IGeoSeries> Series { get; set; }

    /// <summary>
    /// Gets or sets the tooltip. Shadows <see cref="IChartView.Tooltip"/> because
    /// the map uses a land-based tooltip contract instead of <see cref="IChartTooltip"/>.
    /// </summary>
    new IGeoMapTooltip? Tooltip { get; set; }

    /// <summary>Gets or sets the active map.</summary>
    DrawnMap ActiveMap { get; set; }

    /// <summary>Gets or sets the stroke painted around each land.</summary>
    Paint? Stroke { get; set; }

    /// <summary>Gets or sets the fill painted on lands with no series value.</summary>
    Paint? Fill { get; set; }

    /// <summary>Gets or sets the projection.</summary>
    MapProjection MapProjection { get; set; }

    /// <summary>
    /// Gets or sets the minimum latitude (in degrees) clipped to the bottom
    /// edge of the rendered map. <see cref="double.NaN"/> (the default) means
    /// the projection picks its own default — Mercator uses
    /// <see cref="MercatorProjector.DefaultMinLatitudeDegrees"/> (-65°),
    /// cropping the sub-Antarctic empty band; Default uses
    /// <see cref="ControlCoordinatesProjector.DefaultMinLatitudeDegrees"/>
    /// (-90°). Honored by Mercator and Default; <see cref="MapProjection.Orthographic"/>
    /// ignores it (use <see cref="GeoMapChart.CenterLatitude"/> instead).
    /// </summary>
    double MinLatitude { get; set; }

    /// <summary>
    /// Gets or sets the maximum latitude (in degrees) clipped to the top edge
    /// of the rendered map. <see cref="double.NaN"/> (the default) means the
    /// projection picks its own default — Mercator uses
    /// <see cref="MercatorProjector.DefaultMaxLatitudeDegrees"/> (85°), which
    /// already covers Greenland; Default uses
    /// <see cref="ControlCoordinatesProjector.DefaultMaxLatitudeDegrees"/>
    /// (90°). To render the classic full-earth Mercator including Antarctica,
    /// extend the bottom edge: set <see cref="MinLatitude"/> = -85.
    /// Honored by Mercator and Default; <see cref="MapProjection.Orthographic"/>
    /// ignores it.
    /// </summary>
    double MaxLatitude { get; set; }

    /// <summary>
    /// Gets or sets the minimum longitude (in degrees) clipped to the left
    /// edge of the rendered map. <see cref="double.NaN"/> (the default) means
    /// the projection picks its own default (-180° for both Mercator and
    /// Default). Honored by Mercator and Default;
    /// <see cref="MapProjection.Orthographic"/> ignores it
    /// (use <see cref="GeoMapChart.CenterLongitude"/> instead).
    /// </summary>
    double MinLongitude { get; set; }

    /// <summary>
    /// Gets or sets the maximum longitude (in degrees) clipped to the right
    /// edge of the rendered map. <see cref="double.NaN"/> (the default) means
    /// the projection picks its own default (180° for both Mercator and
    /// Default). Honored by Mercator and Default;
    /// <see cref="MapProjection.Orthographic"/> ignores it.
    /// </summary>
    double MaxLongitude { get; set; }

    /// <summary>
    /// Gets or sets which interactions are enabled (pan / zoom / both / none).
    /// Defaults to <see cref="MapInteractionMode.None"/> — geo maps are most
    /// often embedded as static dashboard tiles, so the default is no
    /// interaction. Set <see cref="MapInteractionMode.Zoom"/> for wheel-zoom,
    /// or <see cref="MapInteractionMode.Both"/> for wheel-zoom + click-drag pan.
    /// </summary>
    MapInteractionMode InteractionMode { get; set; }

    /// <summary>Gets or sets the zooming speed; a value in [0.1, 0.95].</summary>
    double ZoomingSpeed { get; set; }

    /// <summary>Gets or sets the minimum zoom level. Defaults to 1.</summary>
    double MinZoomLevel { get; set; }

    /// <summary>Gets or sets the maximum zoom level. Defaults to 100.</summary>
    double MaxZoomLevel { get; set; }

    /// <summary>
    /// Gets or sets a formatter for the per-series value lines in the default
    /// tooltip. When null, each <see cref="GeoTooltipValue"/> renders as
    /// "{Series.Name}: {Value:N2}" (or just "{Value:N2}" if the series has no
    /// name). Has no effect when a custom <see cref="IGeoMapTooltip"/> is set.
    /// </summary>
    Func<GeoTooltipValue, string>? TooltipFormatter { get; set; }
}
