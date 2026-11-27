using System;
using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;

namespace WinFormsSample.Pies.Custom;

public partial class View : UserControl
{
    private readonly PieChart pieChart;

    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

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
            InitialRotation = -90,
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(50, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(pieChart);
    }
}
