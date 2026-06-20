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
/// Defines a range line series in the user interface. Each point contributes a
/// high value and a low value; the series draws two smoothed curves (one through
/// the high values, one through the low values) and a band fill between them.
/// Use <see cref="Defaults.RangeValue"/> for the simplest model, or register a
/// custom <see cref="Series{TModel, TVisual, TLabel}.Mapping"/> that fills
/// <c>PrimaryValue = high</c> and <c>TertiaryValue = low</c>.
/// </summary>
/// <typeparam name="TModel">
/// The type of the points, you can use any type, the library already knows how to handle the most common numeric types,
/// to use a custom type, you must register the type globally
/// (<see cref="LiveChartsSettings.HasMap{TModel}(System.Func{TModel, int, Coordinate})"/>)
/// or at the series level
/// (<see cref="Series{TModel, TVisual, TLabel}.Mapping"/>).
/// </typeparam>
public class RangeLineSeries<TModel>
    : RangeLineSeries<TModel, CircleGeometry, LabelGeometry>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RangeLineSeries{TModel, TVisual, TLabel}"/> class.
    /// </summary>
    public RangeLineSeries() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RangeLineSeries{TModel, TVisual, TLabel}"/> class,
    /// with a given collection of values.
    /// </summary>
    /// <param name="values">The values to plot.</param>
    public RangeLineSeries(IReadOnlyCollection<TModel>? values) : base(values) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RangeLineSeries{TModel, TVisual, TLabel}"/> class,
    /// with a given collection of values.
    /// </summary>
    /// <param name="values">The values to plot.</param>
    public RangeLineSeries(params TModel[] values) : base(values) { }
}

/// <summary>
/// Defines a range line series with a custom marker geometry.
/// </summary>
/// <typeparam name="TModel">The type of the points.</typeparam>
/// <typeparam name="TVisual">The marker geometry type drawn at each high / low endpoint.</typeparam>
public class RangeLineSeries<TModel, TVisual>
    : RangeLineSeries<TModel, TVisual, LabelGeometry>
        where TVisual : BoundedDrawnGeometry, new()
{
    static RangeLineSeries()
    {
        LiveChartsSkiaSharp.EnsureInitialized();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RangeLineSeries{TModel, TVisual, TLabel}"/> class.
    /// </summary>
    public RangeLineSeries() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RangeLineSeries{TModel, TVisual, TLabel}"/> class.
    /// </summary>
    /// <param name="values">The values to plot.</param>
    public RangeLineSeries(IReadOnlyCollection<TModel>? values) : base(values) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RangeLineSeries{TModel, TVisual, TLabel}"/> class.
    /// </summary>
    /// <param name="values">The values to plot.</param>
    public RangeLineSeries(params TModel[] values) : base(values) { }
}

/// <summary>
/// Defines a range line series with a custom marker + label geometry.
/// </summary>
/// <typeparam name="TModel">The type of the points.</typeparam>
/// <typeparam name="TVisual">The marker geometry type.</typeparam>
/// <typeparam name="TLabel">The data-label geometry type.</typeparam>
public class RangeLineSeries<TModel, TVisual, TLabel>
    : CoreRangeLineSeries<TModel, TVisual, TLabel, CubicBezierAreaGeometry, CubicBezierBandGeometry>
        where TVisual : BoundedDrawnGeometry, new()
        where TLabel : BaseLabelGeometry, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RangeLineSeries{TModel, TVisual, TLabel}"/> class.
    /// </summary>
    public RangeLineSeries() : base(null) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RangeLineSeries{TModel, TVisual, TLabel}"/> class.
    /// </summary>
    /// <param name="values">The values to plot.</param>
    public RangeLineSeries(IReadOnlyCollection<TModel>? values) : base(values) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RangeLineSeries{TModel, TVisual, TLabel}"/> class.
    /// </summary>
    /// <param name="values">The values to plot.</param>
    public RangeLineSeries(params TModel[] values) : base(values) { }
}
