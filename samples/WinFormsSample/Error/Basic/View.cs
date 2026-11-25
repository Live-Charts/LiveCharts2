using System;
using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;

namespace WinFormsSample.Error.Basic;

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(600, 600);

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
            Series = [
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
            ],
            Location = new System.Drawing.Point(10, 10),
            Size = new System.Drawing.Size(580, 180)
        };

        var chart2 = new CartesianChart
        {
            Series = [
                new LineSeries<ErrorValue>
                {
                    Values = values1,
                    ShowError = true,
                    GeometrySize = 4,
                    Fill = null
                }
            ],
            Location = new System.Drawing.Point(10, 200),
            Size = new System.Drawing.Size(580, 180)
        };

        var chart3 = new CartesianChart
        {
            Series = [
                new ScatterSeries<ErrorDateTimePoint>
                {
                    Values = values3,
                    ShowError = true,
                    GeometrySize = 10
                }
            ],
            XAxes = [
                new DateTimeAxis(TimeSpan.FromDays(1), Formatter)
            ],
            Location = new System.Drawing.Point(10, 390),
            Size = new System.Drawing.Size(580, 180)
        };

        Controls.Add(chart1);
        Controls.Add(chart2);
        Controls.Add(chart3);
    }
}
