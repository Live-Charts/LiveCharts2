using System;
using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;

namespace EtoFormsSample.Bars.Waterfall;

public class View : Panel
{
    private readonly CartesianChart cartesianChart;

    public View()
    {
        var series = new ISeries[]
        {
            new RangeColumnSeries<RangeValue>
            {
                Name = "Cash flow",
                Values =
                [
                    new(0, 100),    // Start balance
                    new(100, 150),  // Sales (+50)
                    new(150, 180),  // Other income (+30)
                    new(180, 130),  // Costs (-50)
                    new(130, 110),  // Tax (-20)
                    new(0, 110)     // End balance
                ],
                YToolTipLabelFormatter = point =>
                {
                    var from = point.Coordinate.TertiaryValue;
                    var to = point.Coordinate.PrimaryValue;
                    var delta = to - from;
                    var sign = delta >= 0 ? "+" : "−";
                    return $"{from:N0} → {to:N0} ({sign}{Math.Abs(delta):N0})";
                }
            }
        };

        cartesianChart = new CartesianChart
        {
            Series = series,
            XAxes = [new Axis { Labels = ["Start", "Sales", "Other income", "Costs", "Tax", "End"] }],
            YAxes = [new Axis { Name = "Balance", MinLimit = 0 }],
        };

        Content = cartesianChart;
    }
}
