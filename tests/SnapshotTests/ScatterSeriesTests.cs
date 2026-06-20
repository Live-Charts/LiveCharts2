using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;

namespace SnapshotTests;

[TestClass]
public sealed class ScatterSeriesTests
{
    [TestMethod]
    public void Basic()
    {
        var values = new ObservablePoint[]
        {
            new(2.2, 5.4),
            new(3.6, 9.6),
            new(9.9, 5.2),
            new(8.1, 4.7),
            new(5.3, 7.1)
        };

        var series = new ISeries[]
        {
            new ScatterSeries<ObservablePoint>
            {
                Values = values
            }
        };

        var chart = new SKCartesianChart
        {
            Series = series,
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(ScatterSeriesTests)}_{nameof(Basic)}");
    }

    [TestMethod]
    public void Bubbles()
    {
        var values1 = new WeightedPoint[]
        {
            new(2.2, 5.4, 0.5),
            new(3.6, 9.6, 0.8),
            new(9.9, 5.2, 0.3),
            new(8.1, 4.7, 0.7),
            new(5.3, 7.1, 0.6)
        };
        var values2 = new WeightedPoint[]
        {
            new(1.2, 4.4, 0.4),
            new(2.6, 8.6, 0.9),
            new(8.9, 4.2, 0.2),
            new(7.1, 3.7, 0.6),
            new(4.3, 6.1, 0.5)
        };
        var values3 = new WeightedPoint[]
        {
            new(0.2, 3.4, 0.3),
            new(1.6, 7.6, 0.7),
            new(7.9, 3.2, 0.1),
            new(6.1, 2.7, 0.5),
            new(3.3, 5.1, 0.4)
        };

        var series = new ISeries[]
        {
            new ScatterSeries<WeightedPoint>
            {
                Values = values1,
                GeometrySize = 100,
                MinGeometrySize = 5
            },
            new ScatterSeries<WeightedPoint>
            {
                Values = values2,
                GeometrySize = 100,
                MinGeometrySize = 5,
                StackGroup = 1
            },
            new ScatterSeries<WeightedPoint>
            {
                Values = values3,
                GeometrySize = 100,
                MinGeometrySize = 5,
                StackGroup = 1
            }
        };

        var chart = new SKCartesianChart
        {
            Series = series,
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(ScatterSeriesTests)}_{nameof(Bubbles)}");
    }
}
