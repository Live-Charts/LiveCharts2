using System;
using LiveChartsCore.Defaults;

namespace ViewModelsSamples.Error.Basic;

public class ViewModel
{
    public ErrorValue[] Values1 { get; set; } =
        [
            // (Y, Y+- error) // mark
            new(65, 6),
            // (Y, Y+ error, Y- error) // mark
            new(70, 15, 4),
            new(35, 4),
            new(70, 6),
            new(30, 5),
            new(60, 4, 16),
            new(65, 6)
        ];

    public ErrorPoint[] Values2 { get; set; } =
        [
            // (X, Y, Y+- error, Y+- error) // mark
            new(0, 50, 0.2, 8),
            // (X, Y, X- error, X+ erorr, Y+ error, Y- error) // mark
            new(1, 45, 0.1, 0.3, 15, 4),
            new(2, 25, 0.3, 4),
            new(3, 30, 0.2, 6),
            new(4, 70, 0.2, 8),
            new(5, 30, 0.4, 4),
            new(6, 50, 0.3, 6)
        ];

    public ErrorDateTimePoint[] Values3 { get; set; } =
        [
            // (X, Y, Y+- error, Y+- error) // mark
            new(DateTime.Today.AddDays(0), 50, 0.2, 8),
            // (X, Y, X- error, X+ erorr, Y+ error, Y- error) // mark
            new(DateTime.Today.AddDays(1), 45, 0.1, 0.3, 15, 4),
            new(DateTime.Today.AddDays(2), 25, 0.3, 4),
            new(DateTime.Today.AddDays(3), 30, 0.2, 6),
            new(DateTime.Today.AddDays(4), 70, 0.2, 8),
            new(DateTime.Today.AddDays(5), 30, 0.4, 4),
            new(DateTime.Today.AddDays(6), 50, 0.3, 6)
        ];

    public Func<DateTime, string> Formatter { get; set; } =
        date => date.ToString("MMMM dd");
}
