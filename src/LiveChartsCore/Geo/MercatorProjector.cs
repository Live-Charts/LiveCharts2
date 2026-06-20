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
/// Projects latitude and longitude coordinates using the Mercator projection.
/// </summary>
/// <seealso cref="MapProjector" />
public class MercatorProjector : MapProjector
{
    /// <summary>
    /// Default minimum latitude (in degrees) when none is specified on the
    /// view. -65° crops the sub-Antarctic empty band beneath the populated
    /// continents — there are mostly penguins below this line.
    /// </summary>
    public const double DefaultMinLatitudeDegrees = -65d;

    /// <summary>
    /// Default maximum latitude (in degrees) when none is specified on the
    /// view. +85° is the standard Mercator northern limit and keeps Greenland
    /// fully visible.
    /// </summary>
    public const double DefaultMaxLatitudeDegrees = 85d;

    /// <summary>
    /// Default minimum longitude (in degrees) when none is specified on the
    /// view. Standard antimeridian.
    /// </summary>
    public const double DefaultMinLongitudeDegrees = -180d;

    /// <summary>
    /// Default maximum longitude (in degrees) when none is specified on the
    /// view. Standard antimeridian.
    /// </summary>
    public const double DefaultMaxLongitudeDegrees = 180d;

    private readonly float _w;
    private readonly float _h;
    private readonly float _ox;
    private readonly float _oy;
    private readonly double _minLat;
    private readonly double _maxLat;
    private readonly double _minLon;
    private readonly double _maxLon;
    private readonly double _minLatMercN;
    private readonly double _mercNRange;

    /// <summary>
    /// Initializes a new instance of the <see cref="MercatorProjector"/> class
    /// with the projection's default bounds.
    /// </summary>
    public MercatorProjector(float mapWidth, float mapHeight, float offsetX, float offsetY)
        : this(mapWidth, mapHeight, offsetX, offsetY, double.NaN, double.NaN, double.NaN, double.NaN)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MercatorProjector"/> class.
    /// Any NaN bound falls back to the projection's default (<see cref="DefaultMinLatitudeDegrees"/> etc.).
    /// </summary>
    /// <param name="mapWidth">Width of the map.</param>
    /// <param name="mapHeight">Height of the map.</param>
    /// <param name="offsetX">The offset x.</param>
    /// <param name="offsetY">The offset y.</param>
    /// <param name="minLatitudeDegrees">Latitude clipped to the bottom edge; NaN for default.</param>
    /// <param name="maxLatitudeDegrees">Latitude clipped to the top edge; NaN for default.</param>
    /// <param name="minLongitudeDegrees">Longitude clipped to the left edge; NaN for default.</param>
    /// <param name="maxLongitudeDegrees">Longitude clipped to the right edge; NaN for default.</param>
    public MercatorProjector(
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
        _minLatMercN = MercN(_minLat);
        _mercNRange = MercN(_maxLat) - _minLatMercN;
        XOffset = _ox;
        YOffset = _oy;
        MapWidth = mapWidth;
        MapHeight = mapHeight;
    }

    // NaN → default; inverted (min > max) → swap; degenerate (min == max) →
    // fall back to the projection defaults for that axis so the ToMap math
    // never divides by zero / mercN-range of zero.
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
    /// Gets the preferred ratio for the projection's default bounds.
    /// Use <see cref="GetPreferredRatio(double, double, double, double)"/> for custom bounds.
    /// </summary>
    public static float[] PreferredRatio => GetPreferredRatio(
        DefaultMinLatitudeDegrees, DefaultMaxLatitudeDegrees,
        DefaultMinLongitudeDegrees, DefaultMaxLongitudeDegrees);

    /// <summary>
    /// Returns the natural conformal aspect ratio (width:height) of a Mercator
    /// rendering of the given lat/lon bounds. Any NaN argument falls back to
    /// the projection's default. Wider bounds give a wider ratio.
    /// </summary>
    public static float[] GetPreferredRatio(
        double minLatitudeDegrees, double maxLatitudeDegrees,
        double minLongitudeDegrees, double maxLongitudeDegrees)
    {
        NormalizeBounds(
            minLatitudeDegrees, maxLatitudeDegrees,
            minLongitudeDegrees, maxLongitudeDegrees,
            out var minLat, out var maxLat, out var minLon, out var maxLon);

        var mercNRange = MercN(maxLat) - MercN(minLat);
        var lonRangeRadians = (maxLon - minLon) * Math.PI / 180d;
        return new[] { (float)(lonRangeRadians / mercNRange), 1f };
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
        // Scale so [_minLon, _maxLon] spans 0.._w and [mercN(_minLat),
        // mercN(_maxLat)] spans 0.._h; lands outside these ranges are
        // extrapolated past the projection's rendering rectangle and rely
        // on the chart's canvas clip to crop them.
        var mercN = MercN(latitude);
        var py = _h - _h * (mercN - _minLatMercN) / _mercNRange;
        x = (float)((longitude - _minLon) * (_w / (_maxLon - _minLon)) + _ox);
        y = (float)py + _oy;
    }

    /// <inheritdoc cref="MapProjector.ToCoordinates(float, float, out double, out double)"/>
    public override bool ToCoordinates(float screenX, float screenY, out double longitude, out double latitude)
    {
        // Inverse of ToMap. Closed-form: invert the linear longitude step,
        // recover mercN from py, then invert the Gudermannian:
        //   lat = 2 * atan(e^mercN) − π/2.
        longitude = _minLon + (screenX - _ox) / _w * (_maxLon - _minLon);
        var py = screenY - _oy;
        var mercN = (_h - py) / _h * _mercNRange + _minLatMercN;
        var latRad = 2d * Math.Atan(Math.Exp(mercN)) - Math.PI / 2d;
        latitude = latRad * 180d / Math.PI;
        return true;
    }

    private static double MercN(double latitudeDegrees)
    {
        var latRad = latitudeDegrees * Math.PI / 180d;
        return Math.Log(Math.Tan(Math.PI / 4d + latRad / 2d), Math.E);
    }
}
