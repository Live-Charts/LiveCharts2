using CommunityToolkit.Mvvm.Input;
using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ViewModelsSamples.Pies.AngularGauge;

public partial class ViewModel : ObservableObject
{
    private readonly Random _random = new();

    [ObservableProperty]
    public partial double Value { get; set; } = 45;

    public Func<double, string> Labeler { get; set; } =
        value => value.ToString("N1");

    [RelayCommand]
    public void DoRandomChange() =>
        Value = _random.Next(0, 100);
}
