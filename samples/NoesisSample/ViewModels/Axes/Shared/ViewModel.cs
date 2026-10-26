using System;

namespace ViewModelsSamples.Axes.Shared;

public class ViewModel
{
    public int[] Values1 { get; set; } =
        Fetch(100);

    public int[] Values2 { get; set; } =
        Fetch(50);

    public Func<double, string> Labeler { get; set; } =
        value => value.ToString("N2");

    // when the data in both charts have different ranges, we can set an initial
    // limit so both axes are aligned, also see the DrawMargin property, it also helps
    // to align the axes
    public int Max => Math.Max(Values1.Length, Values2.Length);

    private static int[] Fetch(int length = 50)
    {
        var values = new int[length];
        var r = new Random();
        var t = 0;

        for (var i = 0; i < length; i++)
        {
            t += r.Next(-90, 100);
            values[i] = t;
        }

        return values;
    }
}
