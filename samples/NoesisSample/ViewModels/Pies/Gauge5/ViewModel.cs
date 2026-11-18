using System;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ViewModelsSamples.Pies.Gauge5;

public partial class ViewModel : ObservableObject
{
    private readonly Random _random = new();

    [ObservableProperty]
    public partial double Value1 { get; set; } = 50;

    [ObservableProperty]
    public partial double Value2 { get; set; } = 80;

    [RelayCommand]
    public void DoRandomChange()
    {
        Value1 = _random.Next(0, 100);
        Value2 = _random.Next(0, 100);
    }
}
