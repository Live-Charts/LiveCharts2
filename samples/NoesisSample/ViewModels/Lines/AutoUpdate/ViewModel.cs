using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.Defaults;

namespace ViewModelsSamples.Lines.AutoUpdate;

public class ChartData(string name, ObservableValue[] points)
{
    public string Name { get; set; } = name;
    public ObservableCollection<ObservableValue> Values { get; set; } = new(points);
}

public partial class ViewModel
{
    private readonly Random _random = new();

    public ObservableCollection<ChartData> Data { get; set; } = [
        new ChartData(
            name: "Ana",
            points: [
                new(2),
                new(5),
                new(4)
            ]),
        new ChartData(
            name: "Mary",
            points: [
                new(5),
                new(4),
                new(1)
            ])
    ];

    [RelayCommand]
    public void AddSeries()
    {
        Data.Add(
            new ChartData(
                name: $"User #{Data.Count}",
                points: FetchVales()));

    }

    [RelayCommand]
    public void RemoveSeries()
    {
        if (Data.Count == 1) return;

        Data.RemoveAt(Data.Count - 1);
    }

    [RelayCommand]
    public void AddItem()
    {
        var newPoint = new ObservableValue
        {
            Value = _random.Next(0, 10)
        };

        Data[0].Values.Add(newPoint);
    }

    [RelayCommand]
    public void RemoveItem()
    {
        if (Data[0].Values.Count < 2) return;

        Data[0].Values.RemoveAt(0);
    }

    [RelayCommand]
    public void ReplaceItem()
    {
        var randomIndex = _random.Next(0, Data[0].Values.Count - 1);

        Data[0].Values[randomIndex] = new(_random.Next(1, 10));
    }

    private ObservableValue[] FetchVales()
    {
        return [
            new(_random.Next(0, 10)),
            new(_random.Next(0, 10)),
            new(_random.Next(0, 10))
        ];
    }
}
