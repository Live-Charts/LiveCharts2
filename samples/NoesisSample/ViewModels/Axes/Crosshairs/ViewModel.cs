using System;

namespace ViewModelsSamples.Axes.Crosshairs;

public class ViewModel
{
    public double[] Values { get; set; } =
        [200, 558, 458, 249, 457, 339, 587];

    public Func<double, string> LabelFormatter { get; set; } =
        value => value.ToString("N2");
}
