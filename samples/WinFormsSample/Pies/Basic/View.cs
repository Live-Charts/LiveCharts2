using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
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
            Series = new ISeries[]
            {
                new PieSeries<double> { Name = "Mary", Values = new double[] { 10 } },
                new PieSeries<double> { Name = "John", Values = new double[] { 20 } },
                new PieSeries<double> { Name = "Alice", Values = new double[] { 30 } },
                new PieSeries<double> { Name = "Bob", Values = new double[] { 40 } },
                new PieSeries<double> { Name = "Charlie", Values = new double[] { 50 } }
            },
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
