using System;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel;

namespace ViewModelsSamples.Lines.Range;

// Range line series draws two smoothed curves (one through each set of high /
// low endpoints) and a band between them. The classic use case is a min / max
// envelope: monthly temperature range, daily price high / low, forecast
// confidence intervals, etc. Here we show average monthly temperatures (low /
// high) for a Mediterranean-ish climate over a year — the band visually
// conveys "the temperature lives somewhere in here" at each point in time.
//
// MVVM split: this ViewModel exposes raw data + lambda formatters. The
// platform XAML views declare the chart structure using <lvc:XamlRangeLineSeries>
// with each property bound to a member of this VM.
public class ViewModel
{
    public RangeValue[] Temperatures { get; set; } =
    [
        new(low:  8, high: 16),  // Jan
        new(low:  9, high: 17),  // Feb
        new(low: 11, high: 20),  // Mar
        new(low: 13, high: 23),  // Apr
        new(low: 16, high: 27),  // May
        new(low: 20, high: 31),  // Jun
        new(low: 22, high: 33),  // Jul
        new(low: 22, high: 33),  // Aug
        new(low: 19, high: 29),  // Sep
        new(low: 16, high: 24),  // Oct
        new(low: 12, high: 20),  // Nov
        new(low:  9, high: 17),  // Dec
    ];

    public string[] Months { get; set; } =
    [
        "Jan", "Feb", "Mar", "Apr", "May", "Jun",
        "Jul", "Aug", "Sep", "Oct", "Nov", "Dec",
    ];

    // Y axis labeler — appended °C onto whatever value the axis renders.
    public Func<double, string> TempLabeler { get; set; } =
        value => $"{value:0}°C";

    // The point's Coordinate carries TertiaryValue = low and PrimaryValue = high
    // (see RangeValue.OnCoordinateChanged). Format as a temperature span.
    public Func<ChartPoint, string> TempTooltipFormatter { get; set; } = point =>
        $"{point.Coordinate.TertiaryValue:0}°C → {point.Coordinate.PrimaryValue:0}°C";
}
