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
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;

namespace LiveChartsCore.SkiaSharpView;

/// <summary>
/// Defines an Open/High/Low/Close series in the user interface — the I-bar
/// variant of <see cref="CandlesticksSeries{TModel}"/>. Reuses
/// <see cref="CoreFinancialSeries{TModel, TVisual, TLabel, TMiniatureGeometry}"/>
/// measurement and animation; the only difference is the rendered geometry.
/// </summary>
/// <remarks>
/// The <see cref="CoreFinancialSeries{TModel, TVisual, TLabel, TMiniatureGeometry}.UpFill"/>
/// and <see cref="CoreFinancialSeries{TModel, TVisual, TLabel, TMiniatureGeometry}.DownFill"/>
/// paints have no visible effect on OHLC bars (the geometry has no filled body).
/// Use the <c>UpStroke</c> / <c>DownStroke</c> paints to color the bars.
/// </remarks>
/// <typeparam name="TModel">
/// The type of the points, you can use any type, the library already knows how to handle the most common numeric types,
/// to use a custom type, you must register the type globally
/// (<see cref="LiveChartsSettings.HasMap{TModel}(System.Func{TModel, int, Coordinate})"/>)
/// or at the series level
/// (<see cref="Series{TModel, TVisual, TLabel}.Mapping"/>).
/// </typeparam>
public class OhlcSeries<TModel>
    : OhlcSeries<TModel, OhlcGeometry, LabelGeometry>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OhlcSeries{TModel, TVisual, TLabel}"/> class.
    /// </summary>
    public OhlcSeries()
        : base()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="OhlcSeries{TModel, TVisual, TLabel}"/> class,
    /// with a given collection of values.
    /// </summary>
    /// <param name="values">The values to plot.</param>
    public OhlcSeries(IReadOnlyCollection<TModel>? values)
        : base(values)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="OhlcSeries{TModel, TVisual, TLabel}"/> class,
    /// with a given collection of values.
    /// </summary>
    /// <param name="values">The values to plot.</param>
    public OhlcSeries(params TModel[] values)
        : base(values)
    { }
}

/// <inheritdoc cref="OhlcSeries{TModel}"/>
/// <typeparam name="TModel">The type of the points.</typeparam>
/// <typeparam name="TVisual">
/// The type of the geometry of every point of the series.
/// </typeparam>
public class OhlcSeries<TModel, TVisual>
    : OhlcSeries<TModel, TVisual, LabelGeometry>
        where TVisual : BaseCandlestickGeometry, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OhlcSeries{TModel, TVisual, TLabel}"/> class.
    /// </summary>
    public OhlcSeries()
        : base()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="OhlcSeries{TModel, TVisual, TLabel}"/> class,
    /// with a given collection of values.
    /// </summary>
    /// <param name="values">The values to plot.</param>
    public OhlcSeries(IReadOnlyCollection<TModel>? values)
        : base(values)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="OhlcSeries{TModel, TVisual, TLabel}"/> class,
    /// with a given collection of values.
    /// </summary>
    /// <param name="values">The values to plot.</param>
    public OhlcSeries(params TModel[] values)
        : base(values)
    { }
}

/// <inheritdoc cref="OhlcSeries{TModel}"/>
/// <typeparam name="TModel">The type of the points.</typeparam>
/// <typeparam name="TVisual">
/// The type of the geometry of every point of the series.
/// </typeparam>
/// <typeparam name="TLabel">
/// The type of the data label of every point.
/// </typeparam>
public class OhlcSeries<TModel, TVisual, TLabel>
    : CoreFinancialSeries<TModel, TVisual, TLabel, CircleGeometry>
        where TVisual : BaseCandlestickGeometry, new()
        where TLabel : BaseLabelGeometry, new()
{
    static OhlcSeries()
    {
        LiveChartsSkiaSharp.EnsureInitialized();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OhlcSeries{TModel, TVisual, TLabel}"/> class.
    /// </summary>
    public OhlcSeries()
        : base(null)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="OhlcSeries{TModel, TVisual, TLabel}"/> class,
    /// with a given collection of values.
    /// </summary>
    /// <param name="values">The values to plot.</param>
    public OhlcSeries(IReadOnlyCollection<TModel>? values)
        : base(values)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="OhlcSeries{TModel, TVisual, TLabel}"/> class,
    /// with a given collection of values.
    /// </summary>
    /// <param name="values">The values to plot.</param>
    public OhlcSeries(params TModel[] values)
        : base(values)
    { }
}
