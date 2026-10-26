using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.Measure;

namespace ViewModelsSamples.Axes.ColorsAndPosition;

public partial class ViewModel : ObservableObject
{
    public double[] Values { get; set; } = [2, 3, 8];

    [ObservableProperty]
    public partial AxisPosition Position { get; set; } = AxisPosition.End;

    [ObservableProperty]
    public partial string Color { get; set; } = "#f00";

    [RelayCommand]
    public void SetNewColor()
    {
        var random = new Random();

        var r = random.Next(0, 255);
        var g = random.Next(0, 255);
        var b = random.Next(0, 255);

        Color = $"#{r:X2}{g:X2}{b:X2}";
    }

    [RelayCommand]
    public void TogglePosition()
    {
        Position = Position == AxisPosition.End
            ? AxisPosition.Start
            : AxisPosition.End;
    }
}
