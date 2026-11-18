using System;
using LiveChartsCore.Kernel;

namespace ViewModelsSamples.StackedBars.Basic;

public class ViewModel
{
    public int[] Values1 { get; set; } =
        [3, 5, -3, 2, 5, -4, -2];

    public int[] Values2 { get; set; } =
        [4, 2, -3, 2, 3, 4, -2];

    public int[] Values3 { get; set; } =
        [-2, 6, 6, 5, 4, 3, -2];

    public Func<ChartPoint, string> Formatter { get; set; } =
        p => $"{p.Coordinate.PrimaryValue:N} ({p.StackedValue!.Share:P})";
}
