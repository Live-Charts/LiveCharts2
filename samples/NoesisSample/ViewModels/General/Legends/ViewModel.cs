using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Measure;

namespace ViewModelsSamples.General.Legends;

public partial class ViewModel : ObservableObject
{
    public LegendPosition[] Positions { get; } = [
        LegendPosition.Hidden,
        LegendPosition.Top,
        LegendPosition.Bottom,
        LegendPosition.Right,
        LegendPosition.Left
    ];

    [ObservableProperty]
    public partial LegendPosition SelectedPosition { get; set; } =
        LegendPosition.Left;

    public ChartData[] Data { get; set; } = [
        new ChartData("Peru", [3, 7, 3]),
        new ChartData("Colombia", [4, 6, 2]),
        new ChartData("Mexico", [5, 5, 5]),
        new ChartData("Argentina", [6, 4, 6]),
        new ChartData("Chile", [7, 3, 7]),
        new ChartData("Ecuador", [8, 2, 8]),
        new ChartData("Bolivia", [9, 1, 9]),
        new ChartData("Venezuela", [10, 0, 10]),
        new ChartData("Uruguay", [11, 1, 11]),
        new ChartData("Paraguay", [12, 2, 12]),
        new ChartData("Brazil", [13, 3, 13]),
        new ChartData("Costa Rica", [14, 4, 14]),
        new ChartData("Panama", [15, 5, 15]),
        new ChartData("Honduras", [16, 6, 16])
    ];

}

public class ChartData(string name, double[] values)
{
    public string Name { get; } = name;
    public double[] Values { get; } = values;
}
