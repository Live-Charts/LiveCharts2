using System.Windows.Forms;
using LiveChartsCore.SkiaSharpView.WinForms;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using SkiaSharp;
using LiveChartsCore.Drawing;
using ViewModelsSamples.Scatter.Custom;

namespace WinFormsSample.Scatter.Custom;

public partial class View : UserControl
{
    private readonly System.Random _random = new();

    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

        var values1 = Fetch();
        var values2 = Fetch();
        var values3 = Fetch();
        var pinPath = SVGPoints.Pin;

        var series = new ISeries[]
        {
            // the second type argument is the geometry to draw.
            new ScatterSeries<ObservablePoint, HeartGeometry>
            {
                Values = values1,
                Stroke = null,
                GeometrySize = 40
            },

            // the variable SVG path geometry can change the path at runtime.
            new ScatterSeries<ObservablePoint, VariableSVGPathGeometry>
            {
                Values = values2,
                GeometrySvg = pinPath,
                GeometrySize = 40
            },

            // finally you can use skiasharp to draw your own geometries.
            new ScatterSeries<ObservablePoint, MyGeometry>
            {
                Values = values3,
                GeometrySize = 40
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

    private ObservablePoint[] Fetch()
    {
        var length = 10;
        var values = new ObservablePoint[length];
        for (var i = 0; i < length; i++)
        {
            var x = _random.Next(0, 20);
            var y = _random.Next(0, 20);
            values[i] = new ObservablePoint(x, y);
        }
        return values;
    }
}
