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
using LiveChartsCore.Drawing;

namespace LiveChartsCore.Geo;

/// <summary>
/// Defines the land data class.
/// </summary>
public class LandData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LandData"/> class.
    /// </summary>
    public LandData(double[][] coordinates)
    {
        var c = new List<LvcPointD>();

        foreach (var point in coordinates)
        {
            var x = point[0];
            var y = point[1];

            if (x > MaxBounds[0]) MaxBounds[0] = x;
            if (x < MinBounds[0]) MinBounds[0] = x;

            if (y > MaxBounds[1]) MaxBounds[1] = y;
            if (y < MinBounds[1]) MinBounds[1] = y;

            c.Add(new LvcPointD(x, y));
        }

        Coordinates = [.. c];
        BoundsHypotenuse = Math.Sqrt(Math.Pow(MaxBounds[0] - MinBounds[0], 2) + Math.Pow(MaxBounds[1] - MinBounds[1], 2));
    }

    /// <summary>
    /// Gets or sets the maximum bounds.
    /// </summary>
    /// <value>
    /// The maximum bounds.
    /// </value>
    public double[] MaxBounds { get; set; } = [double.MinValue, double.MinValue];

    /// <summary>
    /// Gets or sets the minimum bounds.
    /// </summary>
    /// <value>
    /// The minimum bounds.
    /// </value>
    public double[] MinBounds { get; set; } = [double.MaxValue, double.MaxValue];

    /// <summary>
    /// Gets the bounds hypotenuse.
    /// </summary>
    public double BoundsHypotenuse { get; private set; }

    /// <summary>
    /// Gets or sets the land data.
    /// </summary>
    public LvcPointD[] Coordinates { get; }

    /// <summary>
    /// Gets or sets the shape.
    /// </summary>
    public BaseVectorGeometry? Shape { get; set; }

    /// <summary>
    /// Gets or sets the parent land definition.
    /// </summary>
    public LandDefinition? Land { get; set; }
}
