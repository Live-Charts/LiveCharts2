using System.Linq;
using System.Windows.Forms;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.SkiaSharpView.WinForms;
using SkiaSharp;
using ViewModelsSamples.Pies.Basic;

namespace WinFormsSample.Pies.Basic;

public partial class View : UserControl
{
    public readonly PieChart Chart;

    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

        var viewModel = new ViewModel();

        Chart = new PieChart
        {
            Series = viewModel.Data
                .Select(data => new PieSeries<double>
                {
                    Name = data.Name,
                    Values = data.Values
                })
                .ToArray(),
            Title = new DrawnLabelVisual(
                new LabelGeometry
                {
                    Text = "My chart title",
                    TextSize = 25,
                    Padding = new LiveChartsCore.Drawing.Padding(15)
                }),
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(50, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(Chart);
    }
}
