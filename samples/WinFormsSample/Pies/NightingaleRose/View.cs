using System.Linq;
using System.Windows.Forms;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;
using ViewModelsSamples.Pies.NightingaleRose;

namespace WinFormsSample.Pies.NightingaleRose;

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
                InnerRadius = 50
            })
            .ToArray();

        pieChart = new PieChart
        {
            Series = seriesCollection,
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(50, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(pieChart);
    }
}
