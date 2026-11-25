using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;

namespace WinFormsSample.General.NullPoints;

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(600, 400);

        var values1 = new double?[] { 5, 4, null, 3, 2, 6, 5, 6, 2 };
        var values2 = new double?[] { 2, 6, 5, 3, null, 5, 2, 4, null };
        var values3 = new ObservablePoint[]
        {
            new() { X = 0, Y = 1 },
            new() { X = 1, Y = 4 },
            null,
            new() { X = 4, Y = 5 },
            new() { X = 6, Y = 1 },
            new() { X = 8, Y = 6 },
        };

        var series = new ISeries[]
        {
            new LineSeries<double?> { Values = values1 },
            new LineSeries<double?> { Values = values2 },
            new LineSeries<ObservablePoint> { Values = values3 }
        };

        var cartesianChart = new CartesianChart
        {
            Series = series,
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(600, 400),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(cartesianChart);
    }
}
