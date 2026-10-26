using LiveChartsCore.Defaults;

namespace ViewModelsSamples.General.NullPoints;

public class ViewModel
{
    public double?[] Values1 { get; set; } =
        [5, 4, null, 3, 2, 6, 5, 6, 2];

    public double?[] Values2 { get; set; } =
        [2, 6, 5, 3, null, 5, 2, 4, null];

    public ObservablePoint?[] Values3 { get; set; } =
        [
            new ObservablePoint { X = 0, Y = 1 },
            new ObservablePoint { X = 1, Y = 4 },
            null,
            new ObservablePoint { X = 4, Y = 5 },
            new ObservablePoint { X = 6, Y = 1 },
            new ObservablePoint { X = 8, Y = 6 },
        ];
}
