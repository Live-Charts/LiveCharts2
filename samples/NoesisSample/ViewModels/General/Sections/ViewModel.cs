using LiveChartsCore.Defaults;

namespace ViewModelsSamples.General.Sections;

public partial class ViewModel
{
    public ObservablePoint[] Values { get; set; } = [
        new ObservablePoint(2.2, 5.4),
        new ObservablePoint(4.5, 2.5),
        new ObservablePoint(4.2, 7.4),
        new ObservablePoint(6.4, 9.9),
        new ObservablePoint(8.9, 3.9),
        new ObservablePoint(9.9, 5.2)
    ];

    public double Xi1 { get; set; } = 3;
    public double Xj1 { get; set; } = 4;

    public double Xi2 { get; set; } = 5;
    public double Xj2 { get; set; } = 6;
    public double Yi2 { get; set; } = 2;
    public double Yj2 { get; set; } = 8;

    public double Xi3 { get; set; } = 8;
    public double Xj3 { get; set; } = double.NaN;
}
