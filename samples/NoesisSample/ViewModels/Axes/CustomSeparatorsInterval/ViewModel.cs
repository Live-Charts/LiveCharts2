namespace ViewModelsSamples.Axes.CustomSeparatorsInterval;

public class ViewModel
{
    public int[] Values { get; set; } =
        [10, 55, 45, 68, 60, 70, 75, 78];

    public double[] CustomSeparators { get; set; } =
        [0, 10, 25, 50, 100];
}
