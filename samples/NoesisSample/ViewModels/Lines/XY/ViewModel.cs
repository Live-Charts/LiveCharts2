using LiveChartsCore.Defaults;

namespace ViewModelsSamples.Lines.XY;

public class ViewModel
{
    // use the ObservablePoint class to set both, X and Y coordinates,
    // alternatively you can also use your own object and implement IChartEntity
    // or map the object to a coordinate in the chart, for more info:
    // https://livecharts.dev/docs/{{ platform }}/{{ version }}/Overview.Mappers
    public ObservablePoint[] Values { get; set; } = [
        new(0, 4),
        new(1, 3),
        new(3, 8),
        new(18, 6),
        new(20, 12)
    ];
}
