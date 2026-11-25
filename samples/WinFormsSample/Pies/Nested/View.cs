using System.Linq;
using System.Windows.Forms;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WinForms;
using SkiaSharp;
using ViewModelsSamples.Pies.Nested;

namespace WinFormsSample.Pies.Nested;

public partial class View : UserControl
{
    private readonly PieChart pieChart;

    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

        var viewModel = new ViewModel();

        pieChart = new PieChart
        {
            Series = viewModel.PieDataCollection.Select(x => new PieSeries<double?>
            {
                Name = x.Name,
                Values = x.Values,
                Stroke = new SolidColorPaint(SKColor.Parse("#fff"), 2),
                Fill = new SolidColorPaint(SKColor.Parse(x.Color)),
                HoverPushout = 0,
                DataLabelsFormatter = x.Formatter,
                DataLabelsSize = 20,
                ShowDataLabels = x.IsTotal,
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer,
                IsVisibleAtLegend = x.IsTotal
            }).ToArray(),
            InitialRotation = 120,
            LegendPosition = LiveChartsCore.Measure.LegendPosition.Right,
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(50, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(pieChart);
    }
}
