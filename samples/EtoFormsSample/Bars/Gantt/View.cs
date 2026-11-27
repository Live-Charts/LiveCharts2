using System;
using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;

namespace EtoFormsSample.Bars.Gantt;

public class View : Panel
{
    private readonly CartesianChart cartesianChart;

    public View()
    {
        var projectStart = new DateTime(2026, 6, 1);

        // each task is a RangeValue from its start date to its end date, in ticks.
        RangeValue MakeTask(int startDay, int endDay) =>
            new(projectStart.AddDays(startDay).Ticks, projectStart.AddDays(endDay).Ticks);

        ISeries[] series =
        [
            new RangeRowSeries<RangeValue>
            {
                Name = "Project",
                Values =
                [
                    MakeTask(0, 5),   // Design
                    MakeTask(3, 8),   // Backend API
                    MakeTask(5, 12),  // Frontend
                    MakeTask(7, 14),  // Integration
                    MakeTask(10, 18), // Testing
                    MakeTask(14, 20)  // Deploy
                ],
                YToolTipLabelFormatter = point =>
                {
                    var start = new DateTime((long)point.Coordinate.TertiaryValue);
                    var end = new DateTime((long)point.Coordinate.PrimaryValue);
                    return $"from {start:MMM dd} to {end:MMM dd}";
                }
            }
        ];

        cartesianChart = new CartesianChart
        {
            Series = series,
            XAxes = [new DateTimeAxis(TimeSpan.FromDays(2), date => date.ToString("MMM dd")) { Name = "Date" }],
            YAxes = [new Axis { Labels = ["Design", "Backend API", "Frontend", "Integration", "Testing", "Deploy"] }],
        };

        Content = cartesianChart;
    }
}
