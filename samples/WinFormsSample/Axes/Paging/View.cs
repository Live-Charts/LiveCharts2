using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;
using SkiaSharp;
using ViewModelsSamples.Axes.Paging;

#pragma warning disable IDE1006 // Naming Styles

namespace WinFormsSample.Axes.Paging;

public partial class View : UserControl
{
    private readonly CartesianChart cartesianChart;
    private readonly Axis xAxis;
    private readonly int[] values;

    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(100, 100);

        values = new ViewModel().Values;
        xAxis = new Axis();

        var series = new ISeries[]
        {
            new ColumnSeries<int> { Values = values }
        };

        cartesianChart = new CartesianChart
        {
            Series = series,
            XAxes = [xAxis],
            ZoomMode = LiveChartsCore.Measure.ZoomAndPanMode.X,
            Location = new System.Drawing.Point(0, 50),
            Size = new System.Drawing.Size(100, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(cartesianChart);

        var panel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 50 };
        var btn1 = new Button { Text = "Page 1" };
        var btn2 = new Button { Text = "Page 2" };
        var btn3 = new Button { Text = "Page 3" };
        var btnClear = new Button { Text = "Clear" };
        btn1.Click += (s, e) => GoToPage(0);
        btn2.Click += (s, e) => GoToPage(1);
        btn3.Click += (s, e) => GoToPage(2);
        btnClear.Click += (s, e) => SeeAll();
        panel.Controls.AddRange([btn1, btn2, btn3, btnClear]);
        Controls.Add(panel);
    }

    private void GoToPage(int page)
    {
        if (page == 0) { xAxis.MinLimit = -0.5; xAxis.MaxLimit = 10.5; }
        else if (page == 1) { xAxis.MinLimit = 9.5; xAxis.MaxLimit = 20.5; }
        else if (page == 2) { xAxis.MinLimit = 19.5; xAxis.MaxLimit = 30.5; }
    }

    private void SeeAll()
    {
        xAxis.MinLimit = null;
        xAxis.MaxLimit = null;
    }
}
