using System.Linq;
using System.Windows.Forms;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;
using ViewModelsSamples.Pies.OutLabels;

namespace WinFormsSample.Pies.OutLabels;

public partial class View : UserControl
{
    private readonly PieChart pieChart;

    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

        var viewModel = new ViewModel();

        var seriesCollection = viewModel.Data
            .Select(data => new PieSeries<double>
            {
                Name = data.Name,
                Values = data.Values,
                ShowDataLabels = true,
                DataLabelsPosition = PolarLabelsPosition.Outer,
                DataLabelsSize = 15,
                DataLabelsFormatter = data.LabelFormatter,
                ToolTipLabelFormatter = data.TooltipFormatter
            })
            .ToArray();

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
