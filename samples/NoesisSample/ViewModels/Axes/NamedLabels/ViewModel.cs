using System;

namespace ViewModelsSamples.Axes.NamedLabels;

public class ViewModel
{
    public int[] Values1 { get; set; } =
        [200, 558, 458, 249];

    public int[] Values2 { get; set; } =
        [300, 450, 400, 280];

    public string[] Labels { get; set; } =
        ["Anne", "Johnny", "Zac", "Rosa"];

    public Func<double, string> Labeler { get; set; } =
        (value) => value.ToString("C");
}
