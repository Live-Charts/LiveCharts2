using System;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;

namespace ViewModelsSamples.Scatter.Custom;

public class ViewModel
{
    private static readonly Random s_r = new();
    public ObservablePoint[] Values1 { get; set; } = Fetch();
    public ObservablePoint[] Values2 { get; set; } = Fetch();
    public ObservablePoint[] Values3 { get; set; } = Fetch();
    public string PinPath { get; } = SVGPoints.Pin;

    private static ObservablePoint[] Fetch()
    {
        var length = 10;
        var values = new ObservablePoint[length];

        for (var i = 0; i < length; i++)
        {
            var x = s_r.Next(0, 20);
            var y = s_r.Next(0, 20);
            values[i] = new ObservablePoint(x, y);
        }

        return values;
    }
}
