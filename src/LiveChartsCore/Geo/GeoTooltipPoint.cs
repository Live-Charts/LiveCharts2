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

using System.Collections.Generic;
using System.Linq;
using LiveChartsCore.Drawing;

namespace LiveChartsCore.Geo;

/// <summary>
/// Represents a hovered land on a geo map, used for tooltip display.
/// </summary>
public class GeoTooltipPoint
{
    /// <summary>
    /// Gets or sets the land definition.
    /// </summary>
    public LandDefinition Land { get; set; } = null!;

    /// <summary>
    /// Gets or sets the values contributed by each series that has data for
    /// this land. Empty when no series has a value. Series-declaration order
    /// is preserved.
    /// </summary>
    public IReadOnlyList<GeoTooltipValue> Values { get; set; } = [];

    /// <summary>
    /// Gets the heat value of the first contributing series, or 0 when none.
    /// Kept for single-series consumers; prefer iterating <see cref="Values"/>
    /// when multiple series may cover the same land.
    /// </summary>
    public double Value => Values.FirstOrDefault()?.Value ?? 0d;

    /// <summary>
    /// Gets a value indicating whether at least one series has a value for
    /// this land.
    /// </summary>
    public bool HasValue => Values.Count > 0;

    /// <summary>
    /// Gets or sets the heat color of the land.
    /// </summary>
    public LvcColor Color { get; set; }

    /// <summary>
    /// Gets or sets the visual center of the land in screen coordinates.
    /// </summary>
    public LvcPoint LandCenter { get; set; }
}

/// <summary>
/// A single series' contribution to a land tooltip.
/// </summary>
public class GeoTooltipValue
{
    /// <summary>The series the value came from.</summary>
    public IGeoSeries Series { get; set; } = null!;

    /// <summary>The numeric value the series carries for this land.</summary>
    public double Value { get; set; }
}
