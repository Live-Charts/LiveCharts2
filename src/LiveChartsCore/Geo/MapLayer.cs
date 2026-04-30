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
using System.Linq;
using System.Text.Json;
using LiveChartsCore.Painting;

namespace LiveChartsCore.Geo;

/// <summary>
/// Defines a map layer.
/// </summary>
/// <remarks>
/// Initializes a new <see cref="MapLayer"/> from the given <see cref="GeoJsonFile"/>.
/// </remarks>
/// <param name="layerName">The layer name.</param>
/// <param name="stroke">The stroke.</param>
/// <param name="fill">The fill.</param>
public class MapLayer(string layerName, Paint stroke, Paint fill)
{
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    public string Name { get; set; } = layerName;

    /// <summary>
    /// Gets or sets the layer process index.
    /// </summary>
    public int ProcessIndex { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this layer is visible.
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Gets or sets the stroke.
    /// </summary>
    public Paint? Stroke { get; set; } = stroke;

    /// <summary>
    /// Gets or sets the fill.
    /// </summary>
    public Paint? Fill { get; set; } = fill;

    /// <summary>
    /// Gets or sets the X bounds.
    /// </summary>
    public double[] Max { get; set; } = [];

    /// <summary>
    /// Gets or sets the Y bounds.
    /// </summary>
    public double[] Min { get; set; } = [];

    /// <summary>
    /// Gets the lands.
    /// </summary>
    public Dictionary<string, LandDefinition> Lands { get; private set; } = [];

    /// <summary>
    /// Gets or sets the land condition, it must return true if the land is required.
    /// </summary>
    public Func<LandDefinition, DrawnMap, bool>? AddLandWhen { get; set; }

    /// <summary>
    /// Adds a GeoJson file to the layer.
    /// </summary>
    /// <param name="file"></param>
    public void AddFile(GeoJsonFile file)
    {
        if (file.Features is null)
            throw new Exception(
                $"The {nameof(GeoJsonFile.Features)} property is required to build a {nameof(DrawnMap)} instance. " +
                $"Ensure the property is not null.");

        var featureIndex = -1;
        foreach (var feature in file.Features)
        {
            featureIndex++;
            if (feature.Geometry?.Coordinates is null) continue;

            var polygons = TryReadPolygons(feature.Geometry.Type, feature.Geometry.Coordinates.Value);
            if (polygons is null) continue;

            var name = GetProperty(feature.Properties, "name") ?? $"feature_{featureIndex}";
            var shortName = GetProperty(feature.Properties, "shortName") ?? name;
            var setOf = GetProperty(feature.Properties, "setOf") ?? "?";

            var definition = new LandDefinition(shortName, name, setOf);
            var dataCollection = new List<LandData>();

            foreach (var polygon in polygons)
            {
                foreach (var ring in polygon)
                {
                    var data = new LandData(ring);

                    if (data.MaxBounds[0] > definition.MaxBounds[0]) definition.MaxBounds[0] = data.MaxBounds[0];
                    if (data.MinBounds[0] < definition.MinBounds[0]) definition.MinBounds[0] = data.MinBounds[0];

                    if (data.MaxBounds[1] > definition.MaxBounds[1]) definition.MaxBounds[1] = data.MaxBounds[1];
                    if (data.MinBounds[1] < definition.MinBounds[1]) definition.MinBounds[1] = data.MinBounds[1];

                    dataCollection.Add(data);
                }
            }

            definition.Data = [.. dataCollection.OrderByDescending(x => x.BoundsHypotenuse)];

            // Two features can resolve to the same shortName (e.g., duplicate "name"
            // properties). Suffix the index instead of throwing so arbitrary GeoJson
            // imports don't abort halfway.
            var key = Lands.ContainsKey(shortName) ? $"{shortName}_{featureIndex}" : shortName;
            Lands.Add(key, definition);
        }
    }

    // Reads coordinates per RFC 7946 Geometry.Type. A Polygon is wrapped into a
    // single-element MultiPolygon so the rendering loop sees a uniform shape. All
    // other geometry types return null and are skipped silently.
    private static double[][][][]? TryReadPolygons(string? type, JsonElement coordinates)
    {
        if (coordinates.ValueKind != JsonValueKind.Array) return null;
        return type switch
        {
            "Polygon" => [ReadPolygon(coordinates)],
            "MultiPolygon" => ReadMultiPolygon(coordinates),
            _ => null
        };
    }

    private static double[][][][] ReadMultiPolygon(JsonElement element)
    {
        var result = new double[element.GetArrayLength()][][][];
        var i = 0;
        foreach (var polygon in element.EnumerateArray()) result[i++] = ReadPolygon(polygon);
        return result;
    }

    private static double[][][] ReadPolygon(JsonElement element)
    {
        var rings = new double[element.GetArrayLength()][][];
        var i = 0;
        foreach (var ring in element.EnumerateArray())
        {
            var points = new double[ring.GetArrayLength()][];
            var j = 0;
            foreach (var point in ring.EnumerateArray())
            {
                var coords = new double[point.GetArrayLength()];
                var k = 0;
                foreach (var c in point.EnumerateArray()) coords[k++] = c.GetDouble();
                points[j++] = coords;
            }
            rings[i++] = points;
        }
        return rings;
    }

    private static string? GetProperty(Dictionary<string, JsonElement>? properties, string key)
    {
        if (properties is null) return null;

        // RFC 7946 doesn't constrain property casing, so accept e.g. "Name" or "SHORTNAME"
        // by falling back to a case-insensitive scan when the exact key isn't present.
        if (!properties.TryGetValue(key, out var value))
        {
            var found = false;
            foreach (var kvp in properties)
            {
                if (!string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase)) continue;
                value = kvp.Value;
                found = true;
                break;
            }
            if (!found) return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString()?.ToLowerInvariant(),
            JsonValueKind.Number => value.ToString().ToLowerInvariant(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => null
        };
    }
}
