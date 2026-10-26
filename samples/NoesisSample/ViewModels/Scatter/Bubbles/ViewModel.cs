using System;
using LiveChartsCore.Defaults;

namespace ViewModelsSamples.Scatter.Bubbles;

public class ViewModel
{
    private static readonly Random s_r = new();

    public WeightedPoint[] Values1 { get; set; } = Fetch(1);

    public WeightedPoint[] Values2 { get; set; } = Fetch(10);

    public WeightedPoint[] Values3 { get; set; } = Fetch(10);

    private static WeightedPoint[] Fetch(int scale)
    {
        var length = 10;
        var values = new WeightedPoint[length];

        for (var i = 0; i < length; i++)
        {
            // the WeightedPoint has 3 properties, X, Y and Weight
            // we use the X and Y coordinates to position the point in the chart
            // the weight will be used by the library to define the size of the points

            // where the minimum weight will be the smallest point, and the max weight the biggest

            // for any weight between these limits the library
            // will interpolate lineally to determine the size of each point

            var x = s_r.Next(0, 20);
            var y = s_r.Next(0, 20);
            var w = s_r.Next(0, 100) * scale;

            values[i] = new WeightedPoint(x, y, w);
        }

        return values;
    }
}
