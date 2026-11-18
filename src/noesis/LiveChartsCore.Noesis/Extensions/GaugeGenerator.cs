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
using LiveChartsCore.Defaults;

namespace LiveChartsCore.Noesis.Extensions;

/// <summary>
/// The gauge generator class.
/// </summary>
public static class GaugeGenerator
{
    /// <summary>
    /// Builds a solid Gauge, it generates a series collectio of
    /// <see cref="PieSeries{ObservableValue, DoughnutGeometry, LabelGeometry}"/>, these series
    /// are ready to be plotted in a pie chart, and will render the gauge, this reuses all the power and
    /// functionality of the <see cref="PieChartEngine"/> class.
    /// </summary>
    /// <param name="items">The items.</param>
    /// <returns>A series collection of pie seires.</returns>
    public static PieSeries<ObservableValue>[] BuildSolidGauge(
        params GaugeItem[] items)
    {
        if (!items.Any(x => x.IsFillSeriesBuilder))
            items = [.. items, new GaugeItem(GaugeItem.Background)];

        return Build(GaugeOptions.Solid, items);
    }

    /// <summary>
    /// Builds an angular Gauge, it generates a series collectio of
    /// <see cref="PieSeries{ObservableValue, DoughnutGeometry, LabelGeometry}"/>, these series
    /// are ready to be plotted in a pie chart, and will render the gauge, this reuses all the power and
    /// functionality of the <see cref="PieChartEngine"/> class.
    /// </summary>
    /// <param name="items">The items.</param>
    /// <returns>A series collection of pie seires.</returns>
    public static PieSeries<ObservableValue>[] BuildAngularGaugeSections(
        params GaugeItem[] items)
    {
        if (!items.Any(x => x.IsFillSeriesBuilder))
            items = [.. items, new GaugeItem(GaugeItem.Background)];

        return Build(GaugeOptions.Angular, items);
    }

    private static PieSeries<ObservableValue>[] Build(
        GaugeOptions options, params GaugeItem[] items)
    {
        List<GaugeItem> seriesRules = [];
        List<GaugeItem> backgroundRules = [];

        foreach (var item in items)
        {
            if (item.IsFillSeriesBuilder)
                backgroundRules.Add(item);
            else
                seriesRules.Add(item);
        }

        var count = seriesRules.Count;
        var i = 0;

        var series = seriesRules.Select(item =>
        {
            Action<ObservableValue, PieSeries<ObservableValue>>? l =
                item.Builder is null ? null : (m, s) => { item.Builder?.Invoke(s); };

            return PieChartExtensions.AsSeries(
                item.Value,
                l ?? ((x, x1) => { }),
                i++,
                count,
                options);
        }).ToArray();

        var fillSeriesValues = new List<ObservableValue>();
        while (fillSeriesValues.Count < items.Length - 1) fillSeriesValues.Add(new ObservableValue(0));

        var backgroundSeries = new PieSeries<ObservableValue>(true, true)
        {
            ZIndex = -1,
            IsFillSeries = true,
            IsVisibleAtLegend = false,
            Values = fillSeriesValues
        };

        if (options == GaugeOptions.Angular)
        {
            backgroundSeries.HoverPushout = 0;
            backgroundSeries.IsHoverable = false;
            backgroundSeries.HoverPushout = 0;
            backgroundSeries.DataLabelsPaint = null;
        }

        foreach (var rule in backgroundRules)
            rule.Builder?.Invoke(backgroundSeries);

        return [.. series, backgroundSeries];
    }
}
