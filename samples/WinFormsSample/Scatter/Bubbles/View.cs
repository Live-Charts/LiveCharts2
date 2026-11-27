using System.Windows.Forms;
using LiveChartsCore.SkiaSharpView.WinForms;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using LiveChartsCore.Defaults;

namespace WinFormsSample.Scatter.Bubbles;

public partial class View : UserControl
{
    private readonly System.Random _random = new();

    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

        var values1 = Fetch(1);
        var values2 = Fetch(10);
        var values3 = Fetch(10);

        var series = new ISeries[]
        {
            new ScatterSeries<WeightedPoint>
            {
                Values = values1,
                GeometrySize = 100,
                MinGeometrySize = 5
            },
            new ScatterSeries<WeightedPoint>
            {
                Values = values2,
                GeometrySize = 100,
                MinGeometrySize = 5,
                StackGroup = 1
            },
            new ScatterSeries<WeightedPoint>
            {
                Values = values3,
                GeometrySize = 100,
                MinGeometrySize = 5,
                StackGroup = 1
            }
        };

        var cartesianChart = new CartesianChart
        {
            Series = series,
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(50, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(cartesianChart);
    }

    private WeightedPoint[] Fetch(int scale)
    {
        var length = 10;
        var values = new WeightedPoint[length];
        for (var i = 0; i < length; i++)
        {
            var x = _random.Next(0, 20);
            var y = _random.Next(0, 20);
            var w = _random.Next(0, 100) * scale;
            values[i] = new WeightedPoint(x, y, w);
        }
        return values;
    }
}
