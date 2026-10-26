using System;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Kernel;

namespace ViewModelsSamples.Axes.LabelsRotation;

public partial class ViewModel : ObservableObject
{
    public double[] Values { get; set; } = [200, 558, 458, 249, 457, 339, 587];

    [ObservableProperty]
    public partial double Rotation { get; set; } = 15;

    public Func<ChartPoint, string> TooltipFormat { get; set; } = (point) =>
        $"This is {Environment.NewLine}" +
        $"A multi-line label {Environment.NewLine}" +
        $"With a value of {Environment.NewLine}" + point.Coordinate.PrimaryValue;

    public Func<double, string> Labeler { get; set; } = (value) =>
        $"This is {Environment.NewLine}" +
        $"A multi-line label {Environment.NewLine}" +
        $"With a value of {Environment.NewLine}" + value * 100;
}
