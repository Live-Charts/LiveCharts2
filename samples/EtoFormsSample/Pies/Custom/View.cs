using System;
using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;

namespace EtoFormsSample.Pies.Custom;

public class View : Panel
{
    private readonly PieChart pieChart;

    public View()
    {
        static string Formatter(ChartPoint point)
        {
            var pv = point.Coordinate.PrimaryValue;
            var sv = point.StackedValue!;

            return $"{pv}/{sv.Total}{Environment.NewLine}{sv.Share:P2}";
        }

        var seriesCollection = new ISeries[]
        {
            new PieSeries<double>
            {
                Name = "Mary",
                Values = new double[] { 10 },
                OuterRadiusOffset = 0,
                ShowDataLabels = true,
                ToolTipLabelFormatter = Formatter,
                DataLabelsFormatter = Formatter
            },
            new PieSeries<double>
            {
                Name = "John",
                Values = new double[] { 20 },
                OuterRadiusOffset = 50,
                ShowDataLabels = true,
                ToolTipLabelFormatter = Formatter,
                DataLabelsFormatter = Formatter
            },
            new PieSeries<double>
            {
                Name = "Alice",
                Values = new double[] { 30 },
                OuterRadiusOffset = 100,
                ShowDataLabels = true,
                ToolTipLabelFormatter = Formatter,
                DataLabelsFormatter = Formatter
            },
            new PieSeries<double>
            {
                Name = "Bob",
                Values = new double[] { 40 },
                OuterRadiusOffset = 150,
                ShowDataLabels = true,
                ToolTipLabelFormatter = Formatter,
                DataLabelsFormatter = Formatter
            }
        };

        pieChart = new PieChart
        {
            Series = seriesCollection,
            InitialRotation = -90
        };

        Content = pieChart;
    }
}
