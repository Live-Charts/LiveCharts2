using System;
using LiveChartsCore.Defaults;

namespace ViewModelsSamples.Axes.DateTimeScaled;

public class ViewModel
{
    public DateTimePoint[] Values { get; set; } = [
        new() { DateTime = new(2021, 1, 1), Value = 3 },
        new() { DateTime = new(2021, 1, 2), Value = 6 },
        new() { DateTime = new(2021, 1, 3), Value = 5 },
        new() { DateTime = new(2021, 1, 4), Value = 3 },
        new() { DateTime = new(2021, 1, 5), Value = 5 },
        new() { DateTime = new(2021, 1, 6), Value = 8 },
        new() { DateTime = new(2021, 1, 7), Value = 6 }
    ];

    public Func<DateTime, string> Formatter { get; set; } =
        date => date.ToString("MMMM dd");
}
