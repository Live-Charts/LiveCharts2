using System;
using LiveChartsCore;

namespace ViewModelsSamples.Axes.Logarithmic;

public class ViewModel
{
    public ViewModel()
    {
        LiveCharts.Configure(config =>
            config.HasMap<LogarithmicPoint>(
                (logPoint, index) => new(logPoint.X, Math.Log(logPoint.Y, LogBase))));
    }

    // base 10 log, change the base if you require it.
    // or use any custom scale the logic is the same.
    public double LogBase { get; set; } = 10;

    public LogarithmicPoint[] Values { get; set; } = [
        new(1, 1),
        new(2, 10),
        new(3, 100),
        new(4, 1000),
        new(5, 10000),
        new(6, 100000),
        new(7, 1000000),
        new(8, 10000000)
    ];
}
