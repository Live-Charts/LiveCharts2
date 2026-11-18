using System;

namespace ViewModelsSamples.Axes.LabelsFormat;

public class ViewModel
{
    public double[] Values1 { get; set; } =
        [426, 583, 104];

    public double[] Values2 { get; set; } =
        [200, 558, 458];

    public string[] Labels { get; set; } =
        ["Sergio", "Lando", "Lewis"];

    public Func<double, string> Labeler { get; set; } =
        value => value.ToString("C2");
}
