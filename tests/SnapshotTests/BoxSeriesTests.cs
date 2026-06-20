using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using SkiaSharp;

namespace SnapshotTests;

[TestClass]
public sealed class BoxSeriesTests
{
    [TestMethod]
    public void BasicBox()
    {
        var values1 = new BoxValue[]
        {
            new(100, 80, 60, 20, 70),
            new(90, 70, 50, 30, 60),
            new(80, 60, 40, 10, 50)
        };
        var values2 = new BoxValue[]
        {
            new(90, 70, 50, 30, 60),
            new(80, 60, 40, 10, 50),
            new(70, 50, 30, 20, 40)
        };
        var values3 = new BoxValue[]
        {
            new(80, 60, 40, 10, 50),
            new(70, 50, 30, 20, 40),
            new(60, 40, 20, 10, 30)
        };

        var chart = new SKCartesianChart
        {
            Series = [
                new BoxSeries<BoxValue> { Name = "Year 2023", Values = values1 },
                new BoxSeries<BoxValue> { Name = "Year 2024", Values = values2 },
                new BoxSeries<BoxValue> { Name = "Year 2025", Values = values3 }
            ],
            XAxes = [
                new Axis
                {
                    Labels = ["Apperitizers", "Mains", "Desserts"],
                    LabelsRotation = 0,
                    SeparatorsPaint = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(new SKColor(200, 200, 200)),
                    SeparatorsAtCenter = false,
                    TicksPaint = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(new SKColor(35, 35, 35)),
                    TicksAtCenter = true
                }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(BoxSeriesTests)}_{nameof(BasicBox)}");
    }
}
