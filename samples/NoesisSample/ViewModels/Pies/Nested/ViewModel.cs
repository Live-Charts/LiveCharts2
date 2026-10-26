using System;
using System.Linq;
using LiveChartsCore.Kernel;

namespace ViewModelsSamples.Pies.Nested;

public class SalesRecord(string country, string zone, string name, double value)
{
    public string Country { get; set; } = country;
    public string Zone { get; set; } = zone;
    public string Name { get; set; } = name;
    public double Value { get; set; } = value;
}

public class PieData(string name, double?[] values, string color)
{
    public string Name { get; set; } = name;
    public double?[] Values { get; set; } = values;
    public string Color { get; set; } = color;
    public bool IsTotal => Name is "Brazil" or "Colombia" or "Mexico";
    public Func<ChartPoint, string> Formatter { get; } = point =>
        $"{name}{Environment.NewLine}{point.StackedValue!.Share:P2}";
}

public class ViewModel
{
    public SalesRecord[] Data { get; set; } = [
        new("Brazil",   "North",    "John",     10),
        new("Brazil",   "North",    "Mary",     5),
        new("Brazil",   "South",    "John",     20),
        new("Brazil",   "South",    "Mary",     8),
        new("Colombia", "East",     "Carla",    15),
        new("Colombia", "East",     "Charles",  15),
        new("Colombia", "West",     "Carla",    25),
        new("Colombia", "West",     "Charles",  25),
        new("Mexico",   "Central",  "Sophia",   30),
        new("Mexico",   "Central",  "Petter",   5),
        new("Mexico",   "North",    "Sophia",   30),
        new("Mexico",   "North",    "Petter",   5)
    ];

    public PieData[] PieDataCollection { get; set; }

    public ViewModel()
    {
        PieDataCollection = [
            new("Brazil",   [null,                              null,                       Sum("Brazil")],     "#1976d2"),
            new("North",    [null,                              Sum("Brazil", "North"),     null],              "#1e88e5"),
            new("John",     [Sum("Brazil", "North", "John"),    null,                       null],              "#2196f3"),
            new("Mary",     [Sum("Brazil", "North", "Mary"),    null,                       null],              "#42a5f5"),
            new("South",    [null,                              Sum("Brazil", "South"),     null],              "#64b5f6"),
            new("John",     [Sum("Brazil", "South", "John"),    null,                       null],              "#90caf9"),
            new("Mary",     [Sum("Brazil", "South", "Mary"),    null,                       null],              "#bbdefb"),

            new("Colombia", [null,                              null,                       Sum("Colombia")],   "#d32f2f"),
            new("East",     [null,                              Sum("Colombia", "East"),    null],              "#e53935"),
            new("Carla",    [Sum("Colombia", "East", "Carla"),  null,                       null],              "#f44336"),
            new("Charles",  [Sum("Colombia", "East", "Charles"),null,                       null],              "#ef5350"),
            new("West",     [null,                              Sum("Colombia", "West"),    null],              "#e57373"),
            new("Carla",    [Sum("Colombia", "West", "Carla"),  null,                       null],              "#ef9a9a"),
            new("Charles",  [Sum("Colombia", "West", "Charles"),null,                       null],              "#ffcdd2"),

            new("Mexico",   [null,                              null,                       Sum("Mexico")],     "#ffa000"),
            new("Central",  [null,                              Sum("Mexico", "Central"),   null],              "#ffb300"),
            new("Sophia",   [Sum("Mexico", "Central", "Sophia"),null,                       null],              "#ffc107"),
            new("Petter",   [Sum("Mexico", "Central", "Petter"),null,                       null],              "#ffca28"),
            new("North",    [null,                              Sum("Mexico", "North"),     null],              "#ffd54f"),
            new("Sophia",   [Sum("Mexico", "North", "Sophia"),  null,                       null],              "#ffe082"),
            new("Petter",   [Sum("Mexico", "North", "Petter"),  null,                       null],              "#ffecb3")
        ];
    }

    public double Sum(string country, string? zone = null, string? name = null)
    {
        return Data
            .Where(x =>
                (country is null || x.Country == country) &&
                (zone is null || x.Zone == zone) &&
                (name is null || x.Name == name))
            .Sum(x => x.Value);
    }
}
