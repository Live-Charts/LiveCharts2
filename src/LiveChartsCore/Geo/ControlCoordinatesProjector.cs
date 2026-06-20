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

namespace LiveChartsCore.Geo;

/// <summary>
/// Projects latitude and longitude coordinates in a control coordinates.
/// </summary>
/// <seealso cref="MapProjector" />
public class ControlCoordinatesProjector : MapProjector
{
    /// <summary>Default minimum latitude (full earth: -90°).</summary>
    public const double DefaultMinLatitudeDegrees = -90d;

    /// <summary>Default maximum latitude (full earth: +90°).</summary>
    public const double DefaultMaxLatitudeDegrees = 90d;

    /// <summary>Default minimum longitude (full earth: -180°).</summary>
    public const double DefaultMinLongitudeDegrees = -180d;

    /// <summary>Default maximum longitude (full earth: +180°).</summary>
    public const double DefaultMaxLongitudeDegrees = 180d;

    private readonly float _w;
    private readonly float _h;
    private readonly float _ox;
    private readonly float _oy;
    private readonly double _minLat;
    private readonly double _maxLat;
    private readonly double _minLon;
    private readonly double _maxLon;

    /// <summary>
    /// Initializes a new instance of the <see cref="ControlCoordinatesProjector"/>
    /// class with the projection's default (full-earth) bounds.
    /// </summary>
    public ControlCoordinatesProjector(float mapWidth, float mapHeight, float offsetX, float offsetY)
        : this(mapWidth, mapHeight, offsetX, offsetY, double.NaN, double.NaN, double.NaN, double.NaN)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ControlCoordinatesProjector"/> class.
    /// Any NaN bound falls back to the projection's default.
    /// </summary>
    /// <param name="mapWidth">Width of the map.</param>
    /// <param name="mapHeight">Height of the map.</param>
    /// <param name="offsetX">The offset x.</param>
    /// <param name="offsetY">The offset y.</param>
    /// <param name="minLatitudeDegrees">Latitude clipped to the bottom edge; NaN for default.</param>
    /// <param name="maxLatitudeDegrees">Latitude clipped to the top edge; NaN for default.</param>
    /// <param name="minLongitudeDegrees">Longitude clipped to the left edge; NaN for default.</param>
    /// <param name="maxLongitudeDegrees">Longitude clipped to the right edge; NaN for default.</param>
    public ControlCoordinatesProjector(
        float mapWidth, float mapHeight, float offsetX, float offsetY,
        double minLatitudeDegrees, double maxLatitudeDegrees,
        double minLongitudeDegrees, double maxLongitudeDegrees)
    {
        _w = mapWidth;
        _h = mapHeight;
        _ox = offsetX;
        _oy = offsetY;
        NormalizeBounds(
            minLatitudeDegrees, maxLatitudeDegrees,
            minLongitudeDegrees, maxLongitudeDegrees,
            out _minLat, out _maxLat, out _minLon, out _maxLon);
        XOffset = _ox;
        YOffset = _oy;
        MapWidth = mapWidth;
        MapHeight = mapHeight;
    }

    // NaN → default; inverted (min > max) → swap; degenerate (min == max) →
    // fall back to the projection defaults for that axis so the ToMap math
    // never divides by zero.
    private static void NormalizeBounds(
        double minLat, double maxLat, double minLon, double maxLon,
        out double normMinLat, out double normMaxLat, out double normMinLon, out double normMaxLon)
    {
        normMinLat = double.IsNaN(minLat) ? DefaultMinLatitudeDegrees : minLat;
        normMaxLat = double.IsNaN(maxLat) ? DefaultMaxLatitudeDegrees : maxLat;
        normMinLon = double.IsNaN(minLon) ? DefaultMinLongitudeDegrees : minLon;
        normMaxLon = double.IsNaN(maxLon) ? DefaultMaxLongitudeDegrees : maxLon;

        if (normMinLat > normMaxLat) (normMinLat, normMaxLat) = (normMaxLat, normMinLat);
        if (normMinLon > normMaxLon) (normMinLon, normMaxLon) = (normMaxLon, normMinLon);

        // Avoid raw `==` on doubles (CodeQL flags it); for the degenerate
        // "min equals max" check we want near-zero range, so an epsilon
        // tolerance is equivalent in intent and satisfies the linter.
        if (Math.Abs(normMinLat - normMaxLat) <= double.Epsilon)
        {
            normMinLat = DefaultMinLatitudeDegrees;
            normMaxLat = DefaultMaxLatitudeDegrees;
        }
        if (Math.Abs(normMinLon - normMaxLon) <= double.Epsilon)
        {
            normMinLon = DefaultMinLongitudeDegrees;
            normMaxLon = DefaultMaxLongitudeDegrees;
        }
    }

    /// <summary>
    /// Gets the preferred ratio for the projection's default (full-earth) bounds.
    /// Use <see cref="GetPreferredRatio(double, double, double, double)"/> for custom bounds.
    /// </summary>
    public static float[] PreferredRatio => GetPreferredRatio(
        DefaultMinLatitudeDegrees, DefaultMaxLatitudeDegrees,
        DefaultMinLongitudeDegrees, DefaultMaxLongitudeDegrees);

    /// <summary>
    /// Returns the natural aspect ratio (width:height) of the equirectangular
    /// rendering for the given lat/lon bounds. Any NaN argument falls back to
    /// the projection's default.
    /// </summary>
    public static float[] GetPreferredRatio(
        double minLatitudeDegrees, double maxLatitudeDegrees,
        double minLongitudeDegrees, double maxLongitudeDegrees)
    {
        NormalizeBounds(
            minLatitudeDegrees, maxLatitudeDegrees,
            minLongitudeDegrees, maxLongitudeDegrees,
            out var minLat, out var maxLat, out var minLon, out var maxLon);
        return new[] { (float)(maxLon - minLon), (float)(maxLat - minLat) };
    }

    /// <inheritdoc cref="MapProjector.ToMap(double[])"/>
    public override float[] ToMap(double[] point)
    {
        ToMap(point[0], point[1], out var x, out var y);
        return [x, y];
    }

    /// <inheritdoc cref="MapProjector.ToMap(double, double, out float, out float)"/>
    public override void ToMap(double longitude, double latitude, out float x, out float y)
    {
        x = (float)(_ox + (longitude - _minLon) / (_maxLon - _minLon) * _w);
        y = (float)(_oy + (_maxLat - latitude) / (_maxLat - _minLat) * _h);
    }

    /// <inheritdoc cref="MapProjector.ToCoordinates(float, float, out double, out double)"/>
    public override bool ToCoordinates(float screenX, float screenY, out double longitude, out double latitude)
    {
        // Linear inverse of ToMap — equirectangular has no horizon, every
        // pixel maps back to a valid coordinate (may be outside the bounds
        // rectangle, but still a real lon/lat).
        longitude = _minLon + (screenX - _ox) / _w * (_maxLon - _minLon);
        latitude = _maxLat - (screenY - _oy) / _h * (_maxLat - _minLat);
        return true;
    }
}
