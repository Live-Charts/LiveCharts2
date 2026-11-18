using System.Collections.Generic;
using LiveChartsCore;
using LiveChartsCore.Defaults;

namespace ViewModelsSamples.Axes.Style;

public class ViewModel
{
    public ObservablePoint[] Values { get; set; } = Fetch();

    private static ObservablePoint[] Fetch()
    {
        var list = new List<ObservablePoint>();
        var fx = EasingFunctions.BounceInOut;

        for (var x = 0f; x < 1f; x += 0.001f)
        {
            var y = fx(x);

            list.Add(new()
            {
                X = x - 0.5,
                Y = y - 0.5
            });
        }

        return [.. list];
    }
}
