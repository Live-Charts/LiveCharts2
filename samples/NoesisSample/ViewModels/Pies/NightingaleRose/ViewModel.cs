namespace ViewModelsSamples.Pies.NightingaleRose;

public class PieData(string name, double value, double offset)
{
    public string Name { get; set; } = name;
    public double[] Values { get; set; } = [value];
    public double Offset { get; set; } = offset;
}

public class ViewModel
{
    public PieData[] Data { get; set; } = [
        new("Mary",     10, 0),
        new("John",     20, 50),
        new("Alice",    30, 100),
        new("Bob",      40, 150)
    ];
}
