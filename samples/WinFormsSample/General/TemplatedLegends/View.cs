using System.Windows.Forms;
using LiveChartsCore.SkiaSharpView.WinForms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using ViewModelsSamples.General.TemplatedLegends;

namespace WinFormsSample.General.TemplatedLegends;

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

        var series = new ISeries[]
        {
            new ColumnSeries<double> { Values = [2, 1, 3, 5, 3, 4, 6], Name = "Roger" },
            new ColumnSeries<double> { Values = [1, 3, 2, 7, 3, 5, 2], Name = "Susan" }
        };

        var cartesianChart = new CartesianChart
        {
            Series = series,
            LegendPosition = LiveChartsCore.Measure.LegendPosition.Right,
            Legend = new CustomLegend(),
            // out of livecharts properties...
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(50, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(cartesianChart);
    }
}
