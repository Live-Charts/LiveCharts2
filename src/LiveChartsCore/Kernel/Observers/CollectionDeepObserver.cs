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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace LiveChartsCore.Kernel.Observers;

/// <summary>
/// A Class that tracks the <see cref="INotifyCollectionChanged.CollectionChanged"/> event of a
/// collection and, when item tracking applies (it can be disabled for element types that can
/// never be tracked — see <see cref="MayContainTrackableItems{T}"/> — and never applies to
/// value-type items, whose subscription would attach to a temporary box), the
/// <see cref="INotifyPropertyChanged.PropertyChanged"/> event of each element in the collection.
/// </summary>
public class CollectionDeepObserver : IObserver
{
    private readonly Action _onChange;
    private readonly Action<object>? _onItemAdded;
    private readonly Action<object>? _onItemRemoved;
    private readonly bool _walksItems;
    private readonly bool _tracksItemProperties;
    private readonly HashSet<INotifyPropertyChanged> _itemsListening = [];
    private IEnumerable? _trackedCollection;

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionDeepObserver"/> class.
    /// </summary>
    /// <param name="onChange">
    /// An action that is called when the collection items or a property in an item in the collection change.
    /// </param>
    /// <param name="onItemAdded">
    /// if specified, this action is called for each new item in the collection.
    /// This action is also called for each item when the collection is initialized.
    /// </param>
    /// <param name="onItemRemoved">
    /// if specified, this action is called each time an item is removed in the collection.
    /// This action is also called for each item when the collection is disposed.
    /// </param>
    public CollectionDeepObserver(
        Action onChange,
        Action<object>? onItemAdded = null,
        Action<object>? onItemRemoved = null)
            : this(onChange, onItemAdded, onItemRemoved, trackItemProperties: true)
    { }

    /// <inheritdoc cref="CollectionDeepObserver(Action, Action{object}?, Action{object}?)"/>
    /// <param name="onChange">See the other overload.</param>
    /// <param name="onItemAdded">See the other overload.</param>
    /// <param name="onItemRemoved">See the other overload.</param>
    /// <param name="trackItemProperties">
    /// Whether items could need <see cref="INotifyPropertyChanged"/> tracking — generic callers
    /// decide statically via <see cref="MayContainTrackableItems{T}"/>. When <c>false</c> and no
    /// per-item callbacks are registered, the per-item walk is skipped entirely: it is O(N) and
    /// boxes every struct, which at large-data scale (a multi-million-point <c>double[]</c> or
    /// struct collection assigned to a series) measures in SECONDS of pure overhead per assignment.
    /// </param>
    public CollectionDeepObserver(
        Action onChange,
        Action<object>? onItemAdded,
        Action<object>? onItemRemoved,
        bool trackItemProperties)
    {
        _onChange = onChange;
        _onItemAdded = onItemAdded;
        _onItemRemoved = onItemRemoved;
        // Per-item callbacks need the walk even when property tracking is off; the
        // PropertyChanged subscription itself stays gated on _tracksItemProperties so a
        // callback-driven walk over untrackable items never attaches handlers to boxes.
        _tracksItemProperties = trackItemProperties;
        _walksItems = onItemAdded is not null || onItemRemoved is not null || trackItemProperties;
    }

    /// <summary>
    /// Whether any instance of <typeparamref name="T"/> could ever need
    /// <see cref="INotifyPropertyChanged"/> tracking:
    /// <list type="bullet">
    /// <item>A VALUE TYPE never can — even one implementing INPC: enumerating through the
    /// non-generic <see cref="IEnumerable"/> boxes each item, so the subscription would attach
    /// to a temporary box the collection does not hold (the handler can never fire) while the
    /// observer roots that box. Skipping is both faster and more correct.</item>
    /// <item>A SEALED reference type that does not implement INPC has no INPC instances.</item>
    /// <item>Anything else (an INPC type, an open hierarchy) may contain trackable items.</item>
    /// </list>
    /// Evaluated on the statically-known type only — AOT/trimmer safe, no reflection over
    /// the collection instance.
    /// </summary>
    public static bool MayContainTrackableItems<T>() =>
        !typeof(T).IsValueType &&
        (!typeof(T).IsSealed || typeof(INotifyPropertyChanged).IsAssignableFrom(typeof(T)));

    /// <inheritdoc cref="IObserver.Initialize(object?)"/>
    public void Initialize(object? instance)
    {
        if (_trackedCollection == instance)
            return;

        if (_trackedCollection is not null)
            Dispose();

        if (instance is not IEnumerable enumerable) return;

        if (instance is INotifyCollectionChanged incc)
            incc.CollectionChanged += OnCollectionChanged;

        if (_walksItems)
            OnItemsAdded(enumerable);

        _trackedCollection = enumerable;
    }

    /// <inheritdoc cref="IObserver.Dispose"/>
    public void Dispose()
    {
        if (_trackedCollection is null) return;

        if (_trackedCollection is INotifyCollectionChanged incc)
            incc.CollectionChanged -= OnCollectionChanged;

        if (_walksItems)
            OnItemsRemoved(_trackedCollection);

        _trackedCollection = null;
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (!_walksItems)
        {
            // Untrackable items and no per-item callbacks: any change is just "redraw".
            _onChange();
            return;
        }

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

                // Snapshot: OnItemsRemoved removes from _itemsListening while this iterates
                // it — only .NET Core 3.0+ hash sets tolerate that, and this assembly also
                // runs on older runtimes through the netstandard2.0 asset.
                OnItemsRemoved(_itemsListening.ToArray());
                _itemsListening.Clear();

                if (sender is IEnumerable enumerable)
                    OnItemsAdded(enumerable);

                break;

            case NotifyCollectionChangedAction.Move:
            default:

                break;
        }

        _onChange();
    }

    // The `is not ValueType` guard: enumerating through the non-generic IEnumerable BOXES
    // every struct, so an INPC-struct subscription would attach to that temporary box — a
    // handler that can never fire — and root it in _itemsListening (mutated items then miss
    // the value-equality Remove and leak past Dispose). Untrackable by nature, every ctor.
    private void OnItemsAdded(IEnumerable newItems)
    {
        foreach (var item in newItems)
        {
            if (_tracksItemProperties && item is INotifyPropertyChanged inpcItem && item is not ValueType)
            {
                if (_itemsListening.Add(inpcItem))
                {
                    inpcItem.PropertyChanged += OnItemPropertyChanged;
                }
            }

            _onItemAdded?.Invoke(item);
        }
    }

    private void OnItemsRemoved(IEnumerable oldItems)
    {
        foreach (var item in oldItems)
        {
            if (_tracksItemProperties && item is INotifyPropertyChanged inpcItem && item is not ValueType
                && _itemsListening.Remove(inpcItem))
            {
                inpcItem.PropertyChanged -= OnItemPropertyChanged;
            }

            _onItemRemoved?.Invoke(item);
        }
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e) => _onChange();
}
