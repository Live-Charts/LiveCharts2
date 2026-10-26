using System;
using LiveChartsCore.Defaults;

namespace ViewModelsSamples.Axes.TimeSpanScaled;

public class ViewModel
{
    public TimeSpanPoint[] Values { get; set; } = [
        new () { TimeSpan = TimeSpan.FromMilliseconds(1), Value = 10 },
        new () { TimeSpan = TimeSpan.FromMilliseconds(2), Value = 6 },
        new () { TimeSpan = TimeSpan.FromMilliseconds(3), Value = 3 },
        new () { TimeSpan = TimeSpan.FromMilliseconds(4), Value = 12 },
        new () { TimeSpan = TimeSpan.FromMilliseconds(5), Value = 8 }
    ];

    public Func<TimeSpan, string> Formatter { get; set; } =
        value => $"{value:fff}ms";
}
