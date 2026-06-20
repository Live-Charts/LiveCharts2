using System;
using System.Collections.ObjectModel;
using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;

namespace EtoFormsSample.Bars.AutoUpdate;

public class View : Panel
{
    private readonly CartesianChart cartesianChart;
    private readonly ObservableCollection<ISeries> seriesCollection;
    private readonly ObservableCollection<ObservableValue> juanaValues;
    private readonly Random random = new();

    public View()
    {
        juanaValues = new ObservableCollection<ObservableValue>
        {
            new ObservableValue(2), new ObservableValue(5), new ObservableValue(4)
        };

        seriesCollection = new ObservableCollection<ISeries>
        {
            new ColumnSeries<ObservableValue>
            {
                Name = "Juana",
                Values = juanaValues
            },
            new ColumnSeries<ObservableValue>
            {
                Name = "Mary",
                Values = new ObservableCollection<ObservableValue>
                {
                    new ObservableValue(5), new ObservableValue(4), new ObservableValue(1)
                }
            }
        };

        cartesianChart = new CartesianChart
        {
            Series = seriesCollection,
        };

        var b1 = new Button { Text = "Add item" };
        b1.Click += (sender, e) => { AddItem(); };
        var b2 = new Button { Text = "Replace item" };
        b2.Click += (sender, e) => { ReplaceItem(); };
        var b3 = new Button { Text = "Remove item" };
        b3.Click += (sender, e) => { RemoveItem(); };
        var b4 = new Button { Text = "Add series" };
        b4.Click += (sender, e) => { AddSeries(); };
        var b5 = new Button { Text = "Remove series" };
        b5.Click += (sender, e) => { RemoveSeries(); };

        var buttons = new StackLayout(b1, b2, b3, b4, b5) { Orientation = Orientation.Horizontal, Padding = 2, Spacing = 4 };
        Content = new DynamicLayout(buttons, cartesianChart);
    }

    private void AddSeries()
    {
        seriesCollection.Add(
            new ColumnSeries<ObservableValue>
            {
                Name = $"User #{seriesCollection.Count}",
                Values = FetchValues()
            });
    }

    private void RemoveSeries()
    {
        if (seriesCollection.Count <= 1) return;
        seriesCollection.RemoveAt(seriesCollection.Count - 1);
    }

    private void AddItem()
    {
        var newPoint = new ObservableValue { Value = random.Next(0, 10) };
        juanaValues.Add(newPoint);
    }

    private void RemoveItem()
    {
        if (juanaValues.Count < 2) return;
        juanaValues.RemoveAt(0);
    }

    private void ReplaceItem()
    {
        if (juanaValues.Count < 2) return;
        var randomIndex = random.Next(0, juanaValues.Count - 1);
        juanaValues[randomIndex] = new ObservableValue(random.Next(1, 10));
    }

    private ObservableCollection<ObservableValue> FetchValues()
    {
        return new ObservableCollection<ObservableValue>
        {
            new ObservableValue(random.Next(0, 10)),
            new ObservableValue(random.Next(0, 10)),
            new ObservableValue(random.Next(0, 10))
        };
    }

#if UI_TESTING
    public CartesianChart Chart => cartesianChart;
#endif
}
