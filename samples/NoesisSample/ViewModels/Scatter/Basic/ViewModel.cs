using LiveChartsCore.Defaults;

namespace ViewModelsSamples.Scatter.Basic;

public class ViewModel
{
    public ObservablePoint[] Values { get; set; } = [
        new(2.2, 5.4),
        new(3.6, 9.6),
        new(9.9, 5.2),
        new(8.1, 4.7),
        new(5.3, 7.1)
    ];
}
