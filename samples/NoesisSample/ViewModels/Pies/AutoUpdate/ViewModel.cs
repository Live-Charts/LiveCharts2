using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.Defaults;

namespace ViewModelsSamples.Pies.AutoUpdate;

public partial class ChartData(string name, double value)
{
    public string Name { get; set; } = name;

    public ObservableCollection<ObservableValue> Values { get; set; } =
        new([new ObservableValue(value)]);
}

public partial class ViewModel
{
    private readonly Random _random = new();

    public ObservableCollection<ChartData> Data { get; set; } = [
        new("Ana", 2),
        new("Juan", 8)
    ];

    [RelayCommand]
    public void AddSeries()
    {
        if (Data.Count == 15) return;
        Data.Add(new($"Slice #{Data.Count}", _random.Next(2, 9)));
    }

    [RelayCommand]
    public void UpdateAll()
    {
        foreach (var element in Data)
            foreach (var item in element.Values)
                item.Value = _random.Next(1, 10);
    }

    [RelayCommand]
    public void RemoveSeries()
    {
        if (Data.Count == 1) return;
        Data.RemoveAt(Data.Count - 1);
    }
}
