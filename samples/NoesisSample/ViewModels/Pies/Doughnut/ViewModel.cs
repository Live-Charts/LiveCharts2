namespace ViewModelsSamples.Pies.Doughnut;

public class PieData(string name, double value)
{
    public string Name { get; set; } = name;
    public double[] Values { get; set; } = [value];
}

public class ViewModel
{
    public PieData[] Data { get; set; } = [
            new("Mary", 10),
            new("John", 20),
            new("Alice", 30),
            new("Bob", 40),
            new("Charlie", 50)
        ];
}
