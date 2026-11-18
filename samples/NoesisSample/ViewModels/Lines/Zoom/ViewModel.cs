using System;

namespace ViewModelsSamples.Lines.Zoom;

public class ViewModel
{
    public int[] Values { get; set; } = Fetch();

    private static int[] Fetch()
    {
        var values = new int[100];
        var r = new Random();
        var t = 0;

        for (var i = 0; i < 100; i++)
        {
            t += r.Next(-90, 100);
            values[i] = t;
        }

        return values;
    }
}
