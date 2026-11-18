using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ViewModelsSamples.StepLines.Properties;

public partial class ViewModel : ObservableObject
{
    private readonly Random _random = new();

    [ObservableProperty]
    public partial double[] Values { get; set; } = [-2, -1, 3, 5, 3, 4, 6];

    [ObservableProperty]
    public partial string Stroke { get; set; } = "#000";

    [ObservableProperty]
    public partial string Fill { get; set; } = "#30000000";

    [ObservableProperty]
    public partial string GeometryStroke { get; set; } = "#000";

    [ObservableProperty]
    public partial string GeometryFill { get; set; } = "#30000000";

    [ObservableProperty]
    public partial double GeometrySize { get; set; } = 20;

    [RelayCommand]
    public void ChangeValuesInstance()
    {
        var t = 0;
        var values = new double[10];

        for (var i = 0; i < 10; i++)
        {
            t += _random.Next(-5, 10);
            values[i] = t;
        }

        Values = values;
    }

    [RelayCommand]
    public void NewStroke() => Stroke = GetRandomColor();

    [RelayCommand]
    public void NewFill() => Fill = GetRandomColor();

    [RelayCommand]
    public void NewGeometryFill() => GeometryFill = GetRandomColor();

    [RelayCommand]
    public void NewGeometryStroke() => GeometryStroke = GetRandomColor();

    [RelayCommand]
    public void IncreaseGeometrySize()
    {
        if (GeometrySize == 60) return;
        GeometrySize += 10;
    }

    [RelayCommand]
    public void DecreaseGeometrySize()
    {
        if (GeometrySize == 0) return;
        GeometrySize -= 10;
    }

    private string GetRandomColor()
    {
        var r = _random.Next(0, 255);
        var g = _random.Next(0, 255);
        var b = _random.Next(0, 255);

        return $"#{r:X2}{g:X2}{b:X2}";
    }
}
