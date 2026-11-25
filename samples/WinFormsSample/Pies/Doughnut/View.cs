using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;

namespace WinFormsSample.Pies.Doughnut;

public partial class View : UserControl
{
    private readonly PieChart pieChart;

    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

        var seriesCollection = new ISeries[]
        {
            new PieSeries<double> { Name = "Mary", Values = new double[] { 10 }, MaxRadialColumnWidth = 60 },
            new PieSeries<double> { Name = "John", Values = new double[] { 20 }, MaxRadialColumnWidth = 60 },
            new PieSeries<double> { Name = "Alice", Values = new double[] { 30 }, MaxRadialColumnWidth = 60 },
            new PieSeries<double> { Name = "Bob", Values = new double[] { 40 }, MaxRadialColumnWidth = 60 },
            new PieSeries<double> { Name = "Charlie", Values = new double[] { 50 }, MaxRadialColumnWidth = 60 }
        };

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
