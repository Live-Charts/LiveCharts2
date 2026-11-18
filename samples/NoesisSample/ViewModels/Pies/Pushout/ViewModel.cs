namespace ViewModelsSamples.Pies.Pushout;

public class PieData(string name, double value, double pushout)
{
    public string Name { get; set; } = name;
    public double[] Values { get; set; } = [value];
    public double Pushout { get; set; } = pushout;
}

public class ViewModel
{
    public PieData[] Data { get; set; } = [
        new("Maria",    8, 30),
        new("Susan",    6, 0),
        new("Charles",  5, 0),
        new("Fiona",    3, 0),
        new("George",   3, 0)
    ];
}
