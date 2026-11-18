using System.Collections.Generic;
using LiveChartsCore;
using LiveChartsCore.Defaults;

namespace ViewModelsSamples.Axes.MatchScale;

// in this example we are going to learn how to match the // mark
// scale of both axes, this means that both X and Y axes will take // mark
// the same amount of space in the screen per unit of data // mark

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

        return list.ToArray();
    }
}
