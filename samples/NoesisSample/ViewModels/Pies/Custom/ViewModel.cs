using System;
using LiveChartsCore.Kernel;

namespace ViewModelsSamples.Pies.Custom;

public class PieData(string name, double value, double offset)
{
    public string Name { get; set; } = name;
    public double[] Values { get; set; } = [value];
    public double Offset { get; set; } = offset;
    public Func<ChartPoint, string> Formatter { get; set; } =
        point =>
        {
            var pv = point.Coordinate.PrimaryValue;
            var sv = point.StackedValue!;

            return $"{pv}/{sv.Total}{Environment.NewLine}{sv.Share:P2}";
        };
}

public class ViewModel
{
    public PieData[] Data { get; set; } = [
        new("Mary",     10, 0),
        new("John",     20, 50),
        new("Alice",    30, 100),
        new("Bob",      40, 150)
    ];
}
