using System;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel;

namespace ViewModelsSamples.Bars.Gantt;

// Gantt charts present each task as a horizontal bar spanning [start, end] on
// a date / time axis, with the Y axis listing task names. RangeRowSeries fits
// this exactly: each RangeValue carries Low = start.Ticks and High = end.Ticks;
// XamlDateTimeAxis on X converts the ticks back to dates for the tick labels,
// and a custom tooltip formatter prints the date range on hover.
//
// MVVM split: this ViewModel exposes raw data + formatters. The platform XAML
// views declare the chart structure using <lvc:XamlRangeRowSeries> /
// <lvc:XamlDateTimeAxis> with each property bound to a member of this VM.
public class ViewModel
{
    private static readonly DateTime s_projectStart = new(2026, 6, 1);

    public RangeValue[] Tasks { get; set; } =
    [
        MakeTask(0,  5),   // Design
        MakeTask(3,  8),   // Backend API
        MakeTask(5, 12),   // Frontend
        MakeTask(7, 14),   // Integration
        MakeTask(10, 18),  // Testing
        MakeTask(14, 20),  // Deploy
    ];

    public string[] TaskNames { get; set; } =
    [
        "Design",
        "Backend API",
        "Frontend",
        "Integration",
        "Testing",
        "Deploy",
    ];

    public Func<DateTime, string> DateFormatter { get; set; } =
        date => date.ToString("MMM dd");

    // The bar's Coordinate carries TertiaryValue = start.Ticks and
    // PrimaryValue = end.Ticks (see RangeValue.OnCoordinateChanged). Convert
    // each back to a DateTime for a human-friendly tooltip.
    public Func<ChartPoint, string> TaskTooltipFormatter { get; set; } = point =>
    {
        var start = new DateTime((long)point.Coordinate.TertiaryValue);
        var end = new DateTime((long)point.Coordinate.PrimaryValue);
        return $"from {start:MMM dd} to {end:MMM dd}";
    };

    private static RangeValue MakeTask(int startDayOffset, int endDayOffset) =>
        new(s_projectStart.AddDays(startDayOffset).Ticks,
            s_projectStart.AddDays(endDayOffset).Ticks);
}
