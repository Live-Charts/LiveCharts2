using System;
using LiveChartsCore.Kernel;

namespace ViewModelsSamples.Pies.Gauge4;

public class ViewModel
{
    public double Vanesa { get; set; } = 50;
    public double Charles { get; set; } = 80;
    public double Ana { get; set; } = 95;
    public Func<ChartPoint, string> Formatter { get; set; } =
        point => point.Coordinate.PrimaryValue.ToString();
}
