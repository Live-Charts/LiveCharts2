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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using LiveChartsCore.Kernel.Sketches;

namespace LiveChartsCore.Kernel.Observers;

/// <summary>
/// Observes a collection of source objects and manages a corresponding collection of series objects, providing
/// synchronization between the two collections.
/// </summary>
/// <remarks>This class is designed to map source objects to series objects and track changes in the source
/// collection. It supports collections that implement <see cref="INotifyCollectionChanged"/> for automatic
/// synchronization of additions, removals, and replacements. If the source collection does not implement  <see
/// cref="INotifyCollectionChanged"/>, the mapping is performed once during initialization.</remarks>
/// <param name="chartView">
/// The chart view.
/// </param>
/// <param name="sourceToSeriesMap">
/// A function that maps a source object to an <see cref="ISeries"/> object.
/// </param>
/// <param name="canBuildFromSource">
/// A function that determines whether the observer can build series from the source collection.
/// </param>
public class SeriesSourceObserver(
    IChartView chartView,
    Func<object, ISeries> sourceToSeriesMap,
    Func<bool> canBuildFromSource)
        : IObserver
{
    private IEnumerable<object>? _trackedCollection;
    private ObservableCollection<ISeries> _localSeries = [];
    private Dictionary<object, ISeries> _localSeriesMap = [];

    /// <inheritdoc cref="IObserver.Initialize(object?)"/>
    public void Initialize(object? instance)
    {
        if (_trackedCollection == instance)
            return;

        if (_trackedCollection is not null)
            Dispose();

        if (!canBuildFromSource()) return;
        if (instance is not IEnumerable<object> enumerable) return;

        if (instance is INotifyCollectionChanged incc)
            incc.CollectionChanged += OnSeriesSourceCollectionChanged;

        _localSeries = [];
        _localSeriesMap = [];

        OnItemsAdded(enumerable);

        chartView.Series = _localSeries;
        _trackedCollection = enumerable;
    }

    /// <inheritdoc cref="IObserver.Dispose"/>
    public void Dispose()
    {
        if (_trackedCollection is not INotifyCollectionChanged incc) return;

        incc.CollectionChanged -= OnSeriesSourceCollectionChanged;

        OnItemsRemoved(_trackedCollection);

        _trackedCollection = null;
        _localSeries = [];
        _localSeriesMap = [];

        chartView.Series = [];
    }

    private void OnSeriesSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:

                if (e.NewItems is not null)
                    OnItemsAdded(e.NewItems);

                break;

            case NotifyCollectionChangedAction.Remove:

                if (e.OldItems is not null)
                    OnItemsRemoved(e.OldItems);

                break;

            case NotifyCollectionChangedAction.Replace:

                if (e.OldItems is not null)
                    OnItemsRemoved(e.OldItems);

                if (e.NewItems is not null)
                    OnItemsAdded(e.NewItems);

                break;

            case NotifyCollectionChangedAction.Reset:

                Initialize(sender);

                break;

            case NotifyCollectionChangedAction.Move:
            default:

                break;
        }
    }

    private void OnItemsAdded(IEnumerable newItems)
    {
        foreach (var item in newItems)
        {
            var mapped = sourceToSeriesMap(item);

            _localSeries.Add(mapped);
            _localSeriesMap[item] = mapped;
        }
    }

    private void OnItemsRemoved(IEnumerable oldItems)
    {
        foreach (var item in oldItems)
        {
            if (!_localSeriesMap.TryGetValue(item, out var series))
                continue;

            _ = _localSeries.Remove(series);
            _ = _localSeriesMap.Remove(item);
        }
    }
}
