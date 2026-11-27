using System;
using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;

namespace EtoFormsSample.Error.Basic;

public class View : Panel
{
    public View()
    {
        var values1 = new ErrorValue[]
        {
            new(65, 6),
            new(70, 15, 4),
            new(35, 4),
            new(70, 6),
            new(30, 5),
            new(60, 4, 16),
            new(65, 6)
        };
        var values2 = new ErrorPoint[]
        {
            new(0, 50, 0.2, 8),
            new(1, 45, 0.1, 0.3, 15, 4),
            new(2, 25, 0.3, 4),
            new(3, 30, 0.2, 6),
            new(4, 70, 0.2, 8),
            new(5, 30, 0.4, 4),
            new(6, 50, 0.3, 6)
        };
        var values3 = new ErrorDateTimePoint[]
        {
            new(DateTime.Today.AddDays(0), 50, 0.2, 8),
            new(DateTime.Today.AddDays(1), 45, 0.1, 0.3, 15, 4),
            new(DateTime.Today.AddDays(2), 25, 0.3, 4),
            new(DateTime.Today.AddDays(3), 30, 0.2, 6),
            new(DateTime.Today.AddDays(4), 70, 0.2, 8),
            new(DateTime.Today.AddDays(5), 30, 0.4, 4),
            new(DateTime.Today.AddDays(6), 50, 0.3, 6)
        };
        static string Formatter(DateTime date) => date.ToString("MMMM dd");

        var chart1 = new CartesianChart
        {
            Series = new ISeries[]
            {
                new ColumnSeries<ErrorValue>
                {
                    Values = values1,
                    ShowError = true,
                    Padding = 0
                },
                new ColumnSeries<ErrorPoint>
                {
                    Values = values2,
                    ShowError = true,
                    Padding = 0
                }
            }
        };

        var chart2 = new CartesianChart
        {
            Series = new ISeries[]
            {
                new LineSeries<ErrorValue>
                {
                    Values = values1,
                    ShowError = true,
                    GeometrySize = 4,
                    Fill = null
                }
            }
        };

        var chart3 = new CartesianChart
        {
            Series = new ISeries[]
            {
                new ScatterSeries<ErrorDateTimePoint>
                {
                    Values = values3,
                    ShowError = true,
                    GeometrySize = 10
                }
            },
            XAxes = new Axis[] { new DateTimeAxis(TimeSpan.FromDays(1), Formatter) }
        };

        Content = new DynamicLayout(
            new DynamicRow(new DynamicControl { Control = chart1, YScale = true }),
            new DynamicRow(new DynamicControl { Control = chart2, YScale = true }),
            new DynamicRow(new DynamicControl { Control = chart3, YScale = true })
        );
    }
}
