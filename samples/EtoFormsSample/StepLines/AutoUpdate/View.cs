using System;
using System.Collections.ObjectModel;
using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;

namespace EtoFormsSample.StepLines.AutoUpdate;

public class View : Panel
{
    private readonly CartesianChart cartesianChart;
    private readonly ObservableCollection<ISeries> _seriesCollection;
    private readonly ObservableCollection<ObservableValue> _anaValues;
    private readonly Random _random = new();

    public View()
    {
        _anaValues =
        [
            new(2), new(5), new(4)
        ];

        _seriesCollection =
        [
            new StepLineSeries<ObservableValue>
            {
                Name = "Ana",
                Values = _anaValues
            },
            new StepLineSeries<ObservableValue>
            {
                Name = "Mary",
                Values = new ObservableCollection<ObservableValue>
                {
                    new(5), new(4), new(1)
                }
            }
        ];

        cartesianChart = new CartesianChart
        {
            Series = _seriesCollection
        };

        var b1 = new Button { Text = "Add item" };
        b1.Click += (sender, e) => AddItem();

        var b2 = new Button { Text = "Replace item" };
        b2.Click += (sender, e) => ReplaceItem();

        var b3 = new Button { Text = "Remove item" };
        b3.Click += (sender, e) => RemoveItem();

        var b4 = new Button { Text = "Add series" };
        b4.Click += (sender, e) => AddSeries();

        var b5 = new Button { Text = "Remove series" };
        b5.Click += (sender, e) => RemoveSeries();

        var buttons = new StackLayout(b1, b2, b3, b4, b5) { Orientation = Orientation.Horizontal, Padding = 2, Spacing = 4 };

        Content = new DynamicLayout(buttons, cartesianChart);
    }

    private void AddSeries()
    {
        _seriesCollection.Add(
            new StepLineSeries<ObservableValue>
            {
                Name = $"User #{_seriesCollection.Count}",
                Values = new ObservableCollection<ObservableValue>(FetchValues())
            });
    }

    private void RemoveSeries()
    {
        if (_seriesCollection.Count <= 1) return;
        _seriesCollection.RemoveAt(_seriesCollection.Count - 1);
    }

    private void AddItem()
    {
        var newPoint = new ObservableValue { Value = _random.Next(0, 10) };
        _anaValues.Add(newPoint);
    }

    private void RemoveItem()
    {
        if (_anaValues.Count < 2) return;
        _anaValues.RemoveAt(0);
    }

    private void ReplaceItem()
    {
        if (_anaValues.Count < 2) return;
        var randomIndex = _random.Next(0, _anaValues.Count - 1);
        _anaValues[randomIndex] = new ObservableValue(_random.Next(1, 10));
    }

    private ObservableValue[] FetchValues()
    {
        return
        [
            new(_random.Next(0, 10)),
            new(_random.Next(0, 10)),
            new(_random.Next(0, 10))
        ];
    }
}
