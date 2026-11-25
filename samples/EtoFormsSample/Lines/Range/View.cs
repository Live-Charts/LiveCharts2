using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;

namespace EtoFormsSample.Lines.Range;

public class View : Panel
{
    private readonly CartesianChart cartesianChart;

    public View()
    {
        // average monthly temperatures (low / high)
        var temperatures = new RangeValue[]
        {
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
        };

        var series = new ISeries[]
        {
            new RangeLineSeries<RangeValue>
            {
                Name = "Temperature",
                Values = temperatures,
                // TertiaryValue is the low and PrimaryValue the high
                YToolTipLabelFormatter = point =>
                    $"{point.Coordinate.TertiaryValue:0}°C → {point.Coordinate.PrimaryValue:0}°C",
            },
        };

        cartesianChart = new CartesianChart
        {
            Series = series,
            XAxes =
            [
                new Axis
                {
                    Labels = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"]
                }
            ],
            YAxes = [new Axis { Labeler = value => $"{value:0}°C" }],
        };

        Content = cartesianChart;
    }
}
