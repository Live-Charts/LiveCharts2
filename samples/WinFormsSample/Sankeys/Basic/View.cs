using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WinForms;
using SkiaSharp;

namespace WinFormsSample.Sankeys.Basic;

public partial class View : UserControl
{
    public readonly SankeyChart Chart;

    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

        var bills = new SankeyNode("Bills");
        var groceries = new SankeyNode("Groceries");
        var transport = new SankeyNode("Transport");
        var entertainment = new SankeyNode("Entertainment");
        var rent = new SankeyNode("Rent");
        var fixedExpenses = new SankeyNode("Fixed Expenses");
        var savings = new SankeyNode("Savings");
        var leisure = new SankeyNode("Leisure");

        var nodes = new[]
        {
            bills, groceries, transport, entertainment,
            rent, fixedExpenses, savings, leisure,
        };

        var links = new[]
        {
            new SankeyLink<SankeyNode>(bills, fixedExpenses, 320),
            new SankeyLink<SankeyNode>(bills, savings, 80),
            new SankeyLink<SankeyNode>(groceries, fixedExpenses, 420),
            new SankeyLink<SankeyNode>(groceries, savings, 60),
            new SankeyLink<SankeyNode>(transport, fixedExpenses, 180),
            new SankeyLink<SankeyNode>(transport, leisure, 40),
            new SankeyLink<SankeyNode>(rent, fixedExpenses, 950),
            new SankeyLink<SankeyNode>(entertainment, leisure, 220),
            new SankeyLink<SankeyNode>(entertainment, savings, 30),
        };

        var series = new ISeries[]
        {
            new SankeySeries<SankeyNode>
            {
                Values = nodes,
                Links = links,
                NodeWidth = 14,
                NodePadding = 10,
                Fill = new SolidColorPaint(new SKColor(96, 138, 218)),
                DataLabelsPaint = new SolidColorPaint(new SKColor(69, 69, 77)),
                DataLabelsSize = 13,
            }
        };

        Chart = new SankeyChart
        {
            Series = series,
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(50, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(Chart);
    }
}
