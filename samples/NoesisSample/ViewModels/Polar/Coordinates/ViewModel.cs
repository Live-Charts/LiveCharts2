using System;
using LiveChartsCore.Defaults;

namespace ViewModelsSamples.Polar.Coordinates;

public class ViewModel
{
    public ObservablePolarPoint[] Values { get;set; } = [
        new(0, 10),
        new(45, 15),
        new(90, 20),
        new(135, 25),
        new(180, 30),
        new(225, 35),
        new(270, 40),
        new(315, 45),
        new(360, 50)
    ];

    public Func<double, string> Labeler { get; set; } =
        angle => $"{angle}°";
}
