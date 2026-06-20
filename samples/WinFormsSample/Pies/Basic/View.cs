using System.Windows.Forms;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.SkiaSharpView.WinForms;
using SkiaSharp;

namespace WinFormsSample.Pies.Basic;

public partial class View : UserControl
{
    public readonly PieChart Chart;

    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

        Chart = new PieChart
        {
            Series = new[] { 2, 4, 1, 4, 3 }.AsPieSeries(),
            Title = new DrawnLabelVisual(
                new LabelGeometry
                {
                    Text = "My chart title",
                    Paint = new SolidColorPaint(SKColors.Black),
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
