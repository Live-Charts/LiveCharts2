using LiveChartsCore;

namespace ViewModelsSamples.Polar.Basic;

public class ViewModel
{
    public double[] Values { get; set; } =
        [15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1];

    public double CotangentAngle { get; set; } =
        LiveCharts.CotangentAngle;

    public double TangentAngle { get; set; } =
        LiveCharts.TangentAngle;
}
