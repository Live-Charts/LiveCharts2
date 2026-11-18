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
using LiveChartsCore.Drawing;
using LiveChartsCore.Geo;
using LiveChartsCore.Noesis.Drawing.Geometries;

namespace LiveChartsCore.Noesis;

/// <inheritdoc cref="CoreHeatLandSeries{TModel}"/>
public class HeatLandSeries : HeatLandSeries<HeatLand>
{ }

/// <inheritdoc cref="CoreHeatLandSeries{TModel}"/>
public class HeatLandSeries<TModel>
    : CoreHeatLandSeries<TModel>
        where TModel : IWeigthedMapLand
{
    static HeatLandSeries()
    {
        LiveChartsNoesis.EnsureInitialized();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HeatLandSeries{TModel}"/> class.
    /// </summary>
    public HeatLandSeries()
        : this(null)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="HeatLandSeries{TModel}"/> class.
    /// </summary>
    /// <param name="lands">The lands.</param>
    public HeatLandSeries(ICollection<TModel>? lands)
        : this(lands, [LvcColor.FromArgb(255, 179, 229, 252), LvcColor.FromArgb(255, 2, 136, 209)])
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="HeatLandSeries{TModel}"/> class.
    /// </summary>
    /// <param name="lands">The lands.</param>
    public HeatLandSeries(params TModel[]? lands)
        : this(lands, [LvcColor.FromArgb(255, 179, 229, 252), LvcColor.FromArgb(255, 2, 136, 209)])
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="HeatLandSeries{TModel}"/> class.
    /// </summary>
    /// <param name="lands">The lands.</param>
    /// <param name="heatMap">The heat map.</param>
    public HeatLandSeries(ICollection<TModel>? lands, LvcColor[] heatMap)
        : base(lands)
    {
        HeatMap = heatMap;

        IntitializeSeries(LiveCharts.DefaultSettings.GetProvider().GetSolidColorPaint());
    }
}
