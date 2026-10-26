using System;
using LiveChartsCore.Kernel;

namespace ViewModelsSamples.Pies.Gauge3;

public class ViewModel
{
    public double Vanesa { get; set; } = 30;
    public double Charles { get; set; } = 50;
    public double Ana { get; set; } = 70;
    public Func<ChartPoint, string> LabelFormatter { get; set; } =
        point => $"{point.Coordinate.PrimaryValue} {point.Context.Series.Name}";
}
