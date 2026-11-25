using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Eto;
using SkiaSharp;
using LiveChartsCore.Drawing;
using ViewModelsSamples.Scatter.Custom;

namespace EtoFormsSample.Scatter.Custom;

public class View : Panel
{
    private readonly System.Random _random = new();

    public View()
    {
        var values1 = Fetch();
        var values2 = Fetch();
        var values3 = Fetch();
        var pinPath = SVGPoints.Pin;

        var series = new ISeries[]
        {
            new ScatterSeries<ObservablePoint, HeartGeometry>
            {
                Values = values1,
                Stroke = null,
                GeometrySize = 40
            },
            new ScatterSeries<ObservablePoint, VariableSVGPathGeometry>
            {
                Values = values2,
                GeometrySvg = pinPath,
                GeometrySize = 40
            },
            new ScatterSeries<ObservablePoint, MyGeometry>
            {
                Values = values3,
                GeometrySize = 40
            }
        };

        var cartesianChart = new CartesianChart
        {
            Series = series
        };

        Content = cartesianChart;
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
