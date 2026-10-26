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
using System.Collections.ObjectModel;
using System.Linq;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Sketches;

namespace LiveChartsCore.Noesis.Extensions;

/// <summary>
/// Defines the pie chart esxtensions.
/// </summary>
public static class PieChartExtensions
{
    /// <summary>
    /// Converts an IEnumerable to an ObservableCollection of pie series.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <param name="source">The data source.</param>
    /// <param name="builder">An optional builder.</param>
    /// <param name="gaugeOptions">Defines whether the series are treated as gauge.</param>
    /// <returns></returns>
    public static PieSeries<TModel>[] AsPieSeries<TModel>(
        this IEnumerable<TModel> source,
        Action<TModel, PieSeries<TModel>>? builder = null,
        GaugeOptions gaugeOptions = GaugeOptions.None) =>
            AsPieSeries<TModel, PieSeries<TModel>>(source, builder, gaugeOptions);

    /// <summary>
    /// Converts an IEnumerable to an ObservableCollection of pie series.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <typeparam name="TVisual">The type of the visual.</typeparam>
    /// <param name="source">The data source.</param>
    /// <param name="builder">An optional builder.</param>
    /// <param name="gaugeOptions">Defines whether the series are treated as gauge.</param>
    /// <returns></returns>
    public static PieSeries<TModel, TVisual>[] AsPieSeries<TModel, TVisual>(
        this IEnumerable<TModel> source,
        Action<TModel, PieSeries<TModel, TVisual>>? builder = null,
        GaugeOptions gaugeOptions = GaugeOptions.None)
            where TVisual : BaseDoughnutGeometry, new() =>
                AsPieSeries<TModel, PieSeries<TModel, TVisual>>(source, builder, gaugeOptions);

    /// <summary>
    /// Converts an IEnumerable to an ObservableCollection of pie series.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <typeparam name="TVisual">The type of the visual.</typeparam>
    /// <typeparam name="TLabel">The type of the label.</typeparam>
    /// <param name="source">The data source.</param>
    /// <param name="builder">An optional builder.</param>
    /// <param name="gaugeOptions">Defines whether the series are treated as gauge.</param>
    /// <returns></returns>
    public static PieSeries<TModel, TVisual, TLabel>[] AsPieSeries<TModel, TVisual, TLabel>(
        this IEnumerable<TModel> source,
        Action<TModel, PieSeries<TModel, TVisual, TLabel>>? builder = null,
        GaugeOptions gaugeOptions = GaugeOptions.None)
            where TVisual : BaseDoughnutGeometry, new()
            where TLabel : BaseLabelGeometry, new() =>
                AsPieSeries<TModel, PieSeries<TModel, TVisual, TLabel>>(source, builder, gaugeOptions);

    /// <summary>
    /// Converts an IEnumerable to an ObservableCollection of pie series.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <typeparam name="TSeries">The type of the series.</typeparam>
    /// <param name="source">The data source.</param>
    /// <param name="builder">An optional builder.</param>
    /// <param name="gaugeOptions">Defines whether the series are treated as gauge.</param>
    /// <returns></returns>
    public static TSeries[] AsPieSeries<TModel, TSeries>(
        this IEnumerable<TModel> source,
        Action<TModel, TSeries>? builder = null,
        GaugeOptions gaugeOptions = GaugeOptions.None)
            where TSeries : IPieSeries, new()
    {
        var count = source.Count();
        builder ??= (instance, series) => { };
        var i = 0;

        return [.. source.Select(instance => AsSeries(instance, builder, i++, count, gaugeOptions))];
    }

    internal static TSeries AsSeries<TModel, TSeries>(
        TModel instance,
        Action<TModel, TSeries> builder,
        int i,
        int count,
        GaugeOptions gaugeOptions)
            where TSeries : IPieSeries, new()
    {
        var isGauge = gaugeOptions > 0;
        var series = new TSeries();

        if (isGauge)
        {
            var baseSeries = (IInternalSeries)series;
            baseSeries.SeriesProperties |= Kernel.SeriesProperties.Gauge;
        }

        ObservableCollection<TModel> values;
        if (gaugeOptions == GaugeOptions.Solid)
        {
            values = [];
            while (values.Count < count - 1)
                values.Add(default!);
            values.Insert(i, instance);
        }
        else
        {
            values = [instance];
            if (gaugeOptions == GaugeOptions.Angular)
            {
                series.HoverPushout = 0;
                series.IsHoverable = false;
                series.HoverPushout = 0;
                series.DataLabelsPaint = null;
                series.AnimationsSpeed = TimeSpan.FromSeconds(0);
                series.IsRelativeToMinValue = true;
            }
        }

        series.Values = values;

        if (isGauge) series.HoverPushout = 0;
        builder(instance, series);

        return series;
    }
}
