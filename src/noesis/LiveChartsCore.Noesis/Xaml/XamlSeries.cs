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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Noesis;

namespace LiveChartsCore.Noesis;

/// <summary>
/// A base series class for XAML-based series in LiveCharts.
/// </summary>
public abstract class XamlSeries : FrameworkElement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="XamlSeries"/> class.
    /// </summary>
    protected XamlSeries()
    {
        var observableCollection = new ObservableCollection<ChartPointState>();
        observableCollection.CollectionChanged += OnAdditionalVisualStatesCollectionChanged;
        SetValue(AdditionalVisualStatesProperty, observableCollection);
    }

    /// <summary>
    /// The AdditionalVisualStates property, which holds additional visual states for the series.
    /// </summary>
    public static readonly DependencyProperty AdditionalVisualStatesProperty =
        DependencyProperty.Register(
            nameof(AdditionalVisualStates), typeof(ObservableCollection<ChartPointState>), typeof(XamlSeries));

    /// <summary>
    /// Gets the additional visual states for the series.
    /// </summary>
    public ObservableCollection<ChartPointState> AdditionalVisualStates =>
        (ObservableCollection<ChartPointState>)GetValue(AdditionalVisualStatesProperty)!;

    /// <summary>
    /// Gets the wrapped series that this XAML series represents.
    /// </summary>
    protected abstract ISeries WrappedSeries { get; }

    /// <summary>
    /// Maps the specified value to the underlying series' values collection.
    /// </summary>
    protected void ValuesMap(object value) => WrappedSeries.Values = (IEnumerable)value;

    private void OnAdditionalVisualStatesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (var item in e.NewItems ?? Array.Empty<object>())
                {
                    var state = (ChartPointState)item;
                    state.Setters.CollectionChanged += OnSetCollectionChanged;
                    WrappedSeries.VisualStates[state.Name] = state.Setters.AsStatesCollection();
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (var item in e.OldItems ?? Array.Empty<object>())
                {
                    var state = (ChartPointState)item;
                    state.Setters.CollectionChanged -= OnSetCollectionChanged;
                    _ = WrappedSeries.VisualStates.Remove(state.Name);
                }
                break;
            case NotifyCollectionChangedAction.Replace:
            case NotifyCollectionChangedAction.Move:
            case NotifyCollectionChangedAction.Reset:
            default:
                break;
        }
    }

    private void OnSetCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (sender is not Generators.BaseChartPointState.SetterCollection setters) return;

        WrappedSeries.VisualStates[setters.Name] = setters.AsStatesCollection();
    }
}
