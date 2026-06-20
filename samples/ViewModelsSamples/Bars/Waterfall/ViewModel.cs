using System;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel;

namespace ViewModelsSamples.Bars.Waterfall;

// Waterfall charts visualize a running total through positive and negative
// changes. Each bar's rectangle spans [previousTotal, newTotal] — the exact
// shape RangeColumnSeries renders. The first and last bars are "anchored"
// totals at zero (Start and End); intermediate bars carry the deltas.
//
// MVVM split: this ViewModel exposes raw data + formatters. The platform XAML
// views declare the chart structure using <lvc:XamlRangeColumnSeries> with
// each property bound to a member of this VM.
public class ViewModel
{
    public RangeValue[] Steps { get; set; } =
    [
        new(0,  100),    // Start balance
        new(100, 150),   // Sales (+50)
        new(150, 180),   // Other income (+30)
        new(180, 130),   // Costs (-50)
        new(130, 110),   // Tax (-20)
        new(0,  110),    // End balance
    ];

    public string[] StepNames { get; set; } =
    [
        "Start", "Sales", "Other income", "Costs", "Tax", "End",
    ];

    public Func<ChartPoint, string> StepTooltipFormatter { get; set; } = point =>
    {
        var from = point.Coordinate.TertiaryValue;
        var to = point.Coordinate.PrimaryValue;
        var delta = to - from;
        var sign = delta >= 0 ? "+" : "−";
        return $"{from:N0} → {to:N0} ({sign}{System.Math.Abs(delta):N0})";
    };
}
