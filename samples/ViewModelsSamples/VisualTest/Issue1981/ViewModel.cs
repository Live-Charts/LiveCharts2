using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;

namespace ViewModelsSamples.VisualTest.Issue1981;

public class ChartContainer
{
    public string Title { get; set; } = string.Empty;
    public ISeries[] Series { get; set; } = [];
    public ICartesianAxis[] XAxes { get; set; } = [];
    public ICartesianAxis[] YAxes { get; set; } = [];
}

public class ViewModel
{
    public ChartContainer[] Items { get; set; } =
    [
        new ChartContainer
        {
            Title = "Sales",
            Series =
            [
                new LineSeries<int> { Name = "Q1", Values = [3, 5, 2, 8, 4] },
                new LineSeries<int> { Name = "Q2", Values = [6, 2, 7, 3, 5] },
            ],
            XAxes = [ new Axis { Name = "Month" } ],
            YAxes = [ new Axis { Name = "Units" } ],
        },
        new ChartContainer
        {
            Title = "Marketing",
            Series =
            [
                new LineSeries<int> { Name = "A", Values = [1, 4, 6, 2, 9] },
                new LineSeries<int> { Name = "B", Values = [5, 3, 4, 7, 2] },
                new LineSeries<int> { Name = "C", Values = [8, 6, 3, 5, 4] },
            ],
            XAxes = [ new Axis { Name = "Month" } ],
            YAxes = [ new Axis { Name = "Leads" } ],
        }
    ];
}
