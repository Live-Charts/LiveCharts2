using System;
using LiveChartsCore.Kernel;

namespace ViewModelsSamples.Pies.OutLabels;

public class PieData(string name, double value)
{
    public string Name { get; set; } = name;
    public double[] Values { get; set; } = [value];

    public Func<ChartPoint, string> LabelFormatter { get; set; } =
        point =>
            $"This slide takes {point.Coordinate.PrimaryValue} " +
            $"out of {point.StackedValue!.Total} parts";

    public Func<ChartPoint, string> TooltipFormatter { get; set; } =
        point =>
            $"{point.StackedValue!.Share:P2}";
}

public class ViewModel
{
    public PieData[] Data { get; set; } = [
        new("Maria",    8),
        new("Susan",    6),
        new("Charles",  5),
        new("Fiona",    3),
        new("George",   3)
    ];
}
