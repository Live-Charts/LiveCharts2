using System.Linq;
using System.Windows.Forms;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;
using ViewModelsSamples.Pies.Custom;

namespace WinFormsSample.Pies.Custom;

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
                OuterRadiusOffset = data.Offset,
                ShowDataLabels = true,
                ToolTipLabelFormatter = data.Formatter,
                DataLabelsFormatter = data.Formatter
            })
            .ToArray();

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
