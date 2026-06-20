using System.Windows.Forms;
using LiveChartsCore.SkiaSharpView.WinForms;
using SkiaSharp;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;

namespace WinFormsSample.General.FirstChart;

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(400, 400);

        var cartesianChart = new CartesianChart
        {
            LegendPosition = LegendPosition.Right,
            Series = [
                new LineSeries<int>
                {
                    Values = [5, 10, 8, 4],
                    Name = "Mary"
                },
                new ColumnSeries<int>
                {
                    Values = [4, 7, 3, 8],
                    Name = "Ana"
                }
            ],


            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(400, 400),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(cartesianChart);
        Chart = cartesianChart;
    }

    public CartesianChart Chart;
}
