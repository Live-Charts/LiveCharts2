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
using System.IO;
using System.Reflection;

namespace LiveChartsCore.Geo;

/// <summary>
/// Defines the maps
/// </summary>
public static class Maps
{
    /// <summary>
    /// Gets the world map.
    /// </summary>
    /// <returns>The map.</returns>
    /// <exception cref="Exception">Map not found</exception>
    public static DrawnMap GetWorldMap()
    {
        var a = Assembly.GetExecutingAssembly();
        var map = "LiveChartsCore.Geo.world.geojson";
        using var reader = new StreamReader(a.GetManifestResourceStream(map) ?? throw new Exception("file not found"));

        return GetMapFromStreamReader(reader);
    }

    /// <summary>
    /// Gets a map from a specified path.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The map.</returns>
    public static DrawnMap GetMapFromDirectory(string path)
    {
        using var sr = new StreamReader(path);
        return GetMapFromStreamReader(sr);
    }

    /// <summary>
    /// Gets a map from a specified stream.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <returns>The map.</returns>
    public static DrawnMap GetMapFromStreamReader(StreamReader stream) =>
        new(stream, "default");

    /// <summary>
    /// Builds a projector with the given parameters.
    /// </summary>
    /// <param name="projection">The projection.</param>
    /// <param name="mapSize">Size of the map.</param>
    /// <param name="centerLon">The center longitude (used for Orthographic projection).</param>
    /// <param name="centerLat">The center latitude (used for Orthographic projection).</param>
    /// <returns></returns>
    public static MapProjector BuildProjector(
        MapProjection projection, float[] mapSize, double centerLon = 0, double centerLat = 0) =>
        BuildProjector(projection, mapSize, 0f, 0f, centerLon, centerLat);

    /// <summary>
    /// Builds a projector that lays the map out inside a sub-rectangle of the
    /// chart canvas rather than the whole canvas — needed when title / legend
    /// reservations push the map render area away from (0, 0).
    /// </summary>
    /// <param name="projection">The projection.</param>
    /// <param name="mapSize">Size of the map render area (will be normalized by the projector).</param>
    /// <param name="baseOffsetX">Screen-space X of the map render area's top-left.</param>
    /// <param name="baseOffsetY">Screen-space Y of the map render area's top-left.</param>
    /// <param name="centerLon">The center longitude (used for Orthographic projection).</param>
    /// <param name="centerLat">The center latitude (used for Orthographic projection).</param>
    /// <param name="minLatitude">The minimum latitude clip; NaN for the projection's default. Honored by Mercator and Default; Orthographic ignores it.</param>
    /// <param name="maxLatitude">The maximum latitude clip; NaN for the projection's default. Honored by Mercator and Default; Orthographic ignores it.</param>
    /// <param name="minLongitude">The minimum longitude clip; NaN for the projection's default. Honored by Mercator and Default; Orthographic ignores it.</param>
    /// <param name="maxLongitude">The maximum longitude clip; NaN for the projection's default. Honored by Mercator and Default; Orthographic ignores it.</param>
    public static MapProjector BuildProjector(
        MapProjection projection, float[] mapSize, float baseOffsetX, float baseOffsetY,
        double centerLon = 0, double centerLat = 0,
        double minLatitude = double.NaN, double maxLatitude = double.NaN,
        double minLongitude = double.NaN, double maxLongitude = double.NaN)
    {
        var mapRatio =
            projection == MapProjection.Default
            ? ControlCoordinatesProjector.GetPreferredRatio(minLatitude, maxLatitude, minLongitude, maxLongitude)
            : projection == MapProjection.Orthographic
            ? OrthographicProjector.PreferredRatio
            : MercatorProjector.GetPreferredRatio(minLatitude, maxLatitude, minLongitude, maxLongitude);

        var normalizedW = mapSize[0] / mapRatio[0];
        var normalizedH = mapSize[1] / mapRatio[1];
        float ox = baseOffsetX, oy = baseOffsetY;

        if (normalizedW < normalizedH)
        {
            var h = mapSize[0] * mapRatio[1] / mapRatio[0];
            oy += (float)(mapSize[1] - h) * 0.5f;
            mapSize[1] = h;
        }
        else
        {
            var w = mapSize[1] * mapRatio[0] / mapRatio[1];
            ox += (float)(mapSize[0] - w) * 0.5f;
            mapSize[0] = w;
        }

        return projection switch
        {
            MapProjection.Default => new ControlCoordinatesProjector(mapSize[0], mapSize[1], ox, oy, minLatitude, maxLatitude, minLongitude, maxLongitude),
            MapProjection.Orthographic => new OrthographicProjector(mapSize[0], mapSize[1], ox, oy, centerLon, centerLat),
            _ => new MercatorProjector(mapSize[0], mapSize[1], ox, oy, minLatitude, maxLatitude, minLongitude, maxLongitude)
        };
    }
}
