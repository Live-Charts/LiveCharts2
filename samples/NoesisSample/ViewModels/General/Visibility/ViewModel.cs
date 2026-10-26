using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ViewModelsSamples.General.Visibility;

public partial class PlotData(double[] values) : ObservableObject
{
    public double[] Values { get; set; } = values;

    [ObservableProperty]
    public partial bool IsVisible { get; set; } = true;

    [RelayCommand]
    public void ToggleVisibility() => IsVisible = !IsVisible;
}

public class ViewModel
{
    public PlotData[] Data { get; set; } = [
        new([2, 5, 4, 3]),
        new([1, 2, 3, 4]),
        new([4, 3, 2, 1])
    ];
}
