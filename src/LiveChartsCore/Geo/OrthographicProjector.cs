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
/// Projects latitude and longitude coordinates using the Orthographic (globe) projection.
/// Points on the far side of the globe are not visible.
/// </summary>
/// <seealso cref="MapProjector" />
public class OrthographicProjector : MapProjector
{
    private readonly double _centerLon;
    private readonly double _centerLat;
    private readonly double _sinCenterLat;
    private readonly double _cosCenterLat;
    private readonly float _radius;
    private readonly float _screenCenterX;
    private readonly float _screenCenterY;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrthographicProjector"/> class.
    /// </summary>
    /// <param name="mapWidth">Width of the map area.</param>
    /// <param name="mapHeight">Height of the map area.</param>
    /// <param name="offsetX">The offset x.</param>
    /// <param name="offsetY">The offset y.</param>
    /// <param name="centerLon">The center longitude (where the globe is facing).</param>
    /// <param name="centerLat">The center latitude (where the globe is facing).</param>
    public OrthographicProjector(
        float mapWidth, float mapHeight, float offsetX, float offsetY,
        double centerLon, double centerLat)
    {
        _centerLon = centerLon;
        _centerLat = centerLat;
        _sinCenterLat = Math.Sin(centerLat * Math.PI / 180d);
        _cosCenterLat = Math.Cos(centerLat * Math.PI / 180d);
        _radius = Math.Min(mapWidth, mapHeight) / 2f;
        _screenCenterX = mapWidth / 2f + offsetX;
        _screenCenterY = mapHeight / 2f + offsetY;

        XOffset = offsetX;
        YOffset = offsetY;
        MapWidth = mapWidth;
        MapHeight = mapHeight;
    }

    /// <summary>
    /// Gets the center longitude.
    /// </summary>
    public double CenterLongitude => _centerLon;

    /// <summary>
    /// Gets the center latitude.
    /// </summary>
    public double CenterLatitude => _centerLat;

    /// <summary>
    /// Gets the globe radius in screen units.
    /// </summary>
    public float Radius => _radius;

    /// <summary>
    /// Gets the screen X of the globe center.
    /// </summary>
    public float ScreenCenterX => _screenCenterX;

    /// <summary>
    /// Gets the screen Y of the globe center.
    /// </summary>
    public float ScreenCenterY => _screenCenterY;

    /// <summary>
    /// Gets the preferred ratio (1:1 for a circular globe).
    /// </summary>
    public static float[] PreferredRatio => [1f, 1f];

    /// <inheritdoc cref="MapProjector.IsVisible(double, double)"/>
    public override bool IsVisible(double longitude, double latitude)
    {
        var latRad = latitude * Math.PI / 180d;
        var lonDiff = (longitude - _centerLon) * Math.PI / 180d;

        var cosC = _sinCenterLat * Math.Sin(latRad) +
                   _cosCenterLat * Math.Cos(latRad) * Math.Cos(lonDiff);

        return cosC > 0;
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
        var latRad = latitude * Math.PI / 180d;
        var lonDiff = (longitude - _centerLon) * Math.PI / 180d;

        var sinLat = Math.Sin(latRad);
        var cosLat = Math.Cos(latRad);
        var sinLon = Math.Sin(lonDiff);
        var cosLon = Math.Cos(lonDiff);

        var px = _radius * cosLat * sinLon;
        var py = _radius * (_cosCenterLat * sinLat - _sinCenterLat * cosLat * cosLon);

        x = (float)(_screenCenterX + px);
        y = (float)(_screenCenterY - py);
    }

    /// <inheritdoc cref="MapProjector.ToCoordinates(float, float, out double, out double)"/>
    public override bool ToCoordinates(float screenX, float screenY, out double longitude, out double latitude)
    {
        // Standard inverse orthographic (Snyder, USGS Map Projections):
        // ρ = sqrt(px² + py²); reject pixels outside the disc rim. The
        // visible hemisphere is two-to-one with the sphere (front only) so
        // only the visible-side coordinates are recoverable.
        double px = screenX - _screenCenterX;
        double py = _screenCenterY - screenY;
        var rho = Math.Sqrt(px * px + py * py);

        if (rho > _radius)
        {
            longitude = 0;
            latitude = 0;
            return false;
        }

        if (rho < double.Epsilon)
        {
            // Globe center: looks straight at (centerLon, centerLat).
            longitude = _centerLon;
            latitude = _centerLat;
            return true;
        }

        var sinC = rho / _radius;
        var cosC = Math.Sqrt(1d - sinC * sinC);

        var latRad = Math.Asin(cosC * _sinCenterLat + py * sinC * _cosCenterLat / rho);
        var lonRad = Math.Atan2(
            px * sinC,
            rho * _cosCenterLat * cosC - py * _sinCenterLat * sinC);

        latitude = latRad * 180d / Math.PI;
        longitude = _centerLon + lonRad * 180d / Math.PI;
        return true;
    }
}
