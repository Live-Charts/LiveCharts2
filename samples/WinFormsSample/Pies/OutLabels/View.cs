using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;

namespace WinFormsSample.Pies.OutLabels;

public partial class View : UserControl
{
    private readonly PieChart pieChart;

    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

        static string LabelFormatter(ChartPoint point) =>
            $"This slide takes {point.Coordinate.PrimaryValue} " +
            $"out of {point.StackedValue!.Total} parts";

        static string TooltipFormatter(ChartPoint point) =>
            $"{point.StackedValue!.Share:P2}";

        var seriesCollection = new ISeries[]
        {
            new PieSeries<double>
            {
                Name = "Maria",
                Values = new double[] { 8 },
                ShowDataLabels = true,
                DataLabelsPosition = PolarLabelsPosition.Outer,
                DataLabelsSize = 15,
                DataLabelsFormatter = LabelFormatter,
                ToolTipLabelFormatter = TooltipFormatter
            },
            new PieSeries<double>
            {
                Name = "Susan",
                Values = new double[] { 6 },
                ShowDataLabels = true,
                DataLabelsPosition = PolarLabelsPosition.Outer,
                DataLabelsSize = 15,
                DataLabelsFormatter = LabelFormatter,
                ToolTipLabelFormatter = TooltipFormatter
            },
            new PieSeries<double>
            {
                Name = "Charles",
                Values = new double[] { 5 },
                ShowDataLabels = true,
                DataLabelsPosition = PolarLabelsPosition.Outer,
                DataLabelsSize = 15,
                DataLabelsFormatter = LabelFormatter,
                ToolTipLabelFormatter = TooltipFormatter
            },
            new PieSeries<double>
            {
                Name = "Fiona",
                Values = new double[] { 3 },
                ShowDataLabels = true,
                DataLabelsPosition = PolarLabelsPosition.Outer,
                DataLabelsSize = 15,
                DataLabelsFormatter = LabelFormatter,
                ToolTipLabelFormatter = TooltipFormatter
            },
            new PieSeries<double>
            {
                Name = "George",
                Values = new double[] { 3 },
                ShowDataLabels = true,
                DataLabelsPosition = PolarLabelsPosition.Outer,
                DataLabelsSize = 15,
                DataLabelsFormatter = LabelFormatter,
                ToolTipLabelFormatter = TooltipFormatter
            }
        };

        pieChart = new PieChart
        {
            Series = seriesCollection,
            IsClockwise = false,
            InitialRotation = -90,
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(50, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(pieChart);
    }
}
