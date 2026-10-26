using System;

namespace ViewModelsSamples.Axes.LabelsFormat2;

public class ViewModel
{
    public double[] Values1 { get; set; } =
        [426, 583, 104];

    public double[] Values2 { get; set; } =
        [200, 558, 458];

    public string[] Labels { get; set; } =
        ["王", "赵", "张"];

    public Func<double, string> Labeler { get; set; } =
        value => value.ToString("C2");

}
