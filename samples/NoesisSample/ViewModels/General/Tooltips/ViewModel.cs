using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Measure;

namespace ViewModelsSamples.General.Tooltips;

public partial class ViewModel : ObservableObject
{
    public double[] Values1 { get; set; } =
        [3, 7, 3, 1, 4, 5, 6];

    public double[] Values2 { get; set; } =
        [2, 1, 3, 5, 3, 4, 6];

    [ObservableProperty]
    public partial TooltipPosition SelectedPosition { get; set; } =
        TooltipPosition.Top;

    public TooltipPosition[] Positions { get; } = [
        TooltipPosition.Hidden,
        TooltipPosition.Top,
        TooltipPosition.Bottom,
        TooltipPosition.Right,
        TooltipPosition.Left,
        TooltipPosition.Center
    ];
}
