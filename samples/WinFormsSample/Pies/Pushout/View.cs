using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;

namespace WinFormsSample.Pies.Pushout;

public partial class View : UserControl
{
    private readonly PieChart pieChart;

    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

        pieChart = new PieChart
        {
            Series = new ISeries[]
            {
                // pushes out Maria's slice 30 pixels.
                new PieSeries<double> { Name = "Maria", Values = new double[] { 8 }, Pushout = 30 },
                new PieSeries<double> { Name = "Susan", Values = new double[] { 6 } },
                new PieSeries<double> { Name = "Charles", Values = new double[] { 5 } },
                new PieSeries<double> { Name = "Fiona", Values = new double[] { 3 } },
                new PieSeries<double> { Name = "George", Values = new double[] { 3 } }
            },
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(50, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(pieChart);
    }
}
