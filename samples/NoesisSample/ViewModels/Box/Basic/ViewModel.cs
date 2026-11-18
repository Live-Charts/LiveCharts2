using LiveChartsCore.Defaults;

namespace ViewModelsSamples.Box.Basic;

public class ViewModel
{
    public BoxValue[] Values1 { get; set; } =
        [
            // max, upper quartile, median, lower quartile, min
            new(100, 80, 60, 20, 70),
            new(90, 70, 50, 30, 60),
            new(80, 60, 40, 10, 50)
        ];

    public BoxValue[] Values2 { get; set; } =
        [
            new(90, 70, 50, 30, 60),
            new(80, 60, 40, 10, 50),
            new(70, 50, 30, 20, 40)
        ];

    public BoxValue[] Values3 { get; set; } =
        [
            new(80, 60, 40, 10, 50),
            new(70, 50, 30, 20, 40),
            new(60, 40, 20, 10, 30)
        ];

    public string[] Labels { get; set; } =
        ["Apperitizers", "Mains", "Desserts"];
}
