using LiveChartsCore.Drawing;

namespace ViewModelsSamples.Bars.Custom;

public class ViewModel
{
    public double[] Values1 { get; } = [2, 1, 4];
    public double[] Values2 { get; } = [4, 3, 6];
    public double[] Values3 { get; } = [-2, 2, 1];
    public double[] Values4 { get; } = [1, 2, 3];
    public string StarPath { get; } = SVGPoints.Star;
}
