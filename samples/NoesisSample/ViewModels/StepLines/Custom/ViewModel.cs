using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Drawing;

namespace ViewModelsSamples.StepLines.Custom;

public partial class ViewModel : ObservableObject
{
    public double[] Values1 { get; set; } =
        [2, 1, 4, 2, 2, -5, -2];

    public double[] Values2 { get; set; } =
        [3, 3, -3, -2, -4, -3, -1];

    public double[] Values3 { get; set; } =
        [-2, 2, 1, 3, -1, 4, 3];

    public double[] Values4 { get; set; } =
        [4, 5, 2, 4, 3, 2, 1];

    public string PinPath { get; set; } =
        SVGPoints.Pin;
}
