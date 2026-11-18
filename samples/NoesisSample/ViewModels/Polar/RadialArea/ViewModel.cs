using LiveChartsCore;

namespace ViewModelsSamples.Polar.RadialArea;

public class ViewModel
{
    public int[] Values1 { get; set; } =
        [7, 5, 7, 5, 6];

    public int[] Values2 { get; set; } =
        [2, 7, 5, 9, 7];

    public string[] Labels { get; set; } =
        ["first", "second", "third", "forth", "fifth"];

    public double TangentAngle { get; set; } =
        LiveCharts.TangentAngle;
}
