using LiveChartsCore.Defaults;

namespace ViewModelsSamples.VisualTest.DataTemplate;

public class DepartmentInfo
{
    public string DepartmentName { get; set; } = string.Empty;
    public ChartData[] Data { get; set; } = [];
}

public class ChartData(string name, ObservableValue[] points)
{
    public string SeriesName { get; set; } = name;
    public ObservableValue[] Values { get; set; } = points;
}

public partial class ViewModel
{
    public DepartmentInfo[] Departments { get; set; } = [
        new DepartmentInfo
        {
            DepartmentName = "Sales",
            Data = [
                new("Juana",        [ new(2), new(5), new(4) ]),
                new("Pedro",        [ new(5), new(4), new(1) ])
            ]
        },
        new DepartmentInfo
        {
            DepartmentName = "Marketing",
            Data = [
                new("Charles",      [ new(3), new(6), new(2) ]),
                new("Margarita",    [ new(4), new(2), new(5) ]),
                new("Ana",          [ new(5), new(7), new(3) ])
            ]
        }
    ];
}
