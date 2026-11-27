using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;

namespace EtoFormsSample.Scatter.Bubbles;

public class View : Panel
{
    private readonly System.Random _random = new();

    public View()
    {
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
            Series = series
        };

        Content = cartesianChart;
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
