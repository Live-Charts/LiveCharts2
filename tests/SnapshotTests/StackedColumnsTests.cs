using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;

namespace SnapshotTests;

[TestClass]
public sealed class StackedColumnsTests
{
    [TestMethod]
    public void Basic()
    {
        var values1 = new int[] { 3, 5, -3, 2, 5, -4, -2 };
        var values2 = new int[] { 4, 2, -3, 2, 3, 4, -2 };
        var values3 = new int[] { -2, 6, 6, 5, 4, 3, -2 };

        static string formatter(ChartPoint p) =>
            $"{p.Coordinate.PrimaryValue:N} ({p.StackedValue!.Share:P})";

        var series = new ISeries[]
        {
            new StackedColumnSeries<int>
            {
                Values = values1,
                ShowDataLabels = true,
                YToolTipLabelFormatter = formatter
            },
            new StackedColumnSeries<int>
            {
                Values = values2,
                ShowDataLabels = true,
                YToolTipLabelFormatter = formatter
            },
            new StackedColumnSeries<int>
            {
                Values = values3,
                ShowDataLabels = true,
                YToolTipLabelFormatter = formatter
            }
        };

        var chart = new SKCartesianChart
        {
            Series = series,
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(StackedColumnsTests)}_{nameof(Basic)}");
    }

    [TestMethod]
    public void StackGroup()
    {
        var values1 = new int[] { 3, 5, 3 };
        var values2 = new int[] { 4, 2, 3 };
        var values3 = new int[] { 4, 6, 6 };
        var values4 = new int[] { 2, 5, 4 };
        var labels = new string[] { "Category 1", "Category 2", "Category 3" };

        var series = new ISeries[]
        {
            new StackedColumnSeries<int> { Values = values1, StackGroup = 0 },
            new StackedColumnSeries<int> { Values = values2, StackGroup = 0 },
            new StackedColumnSeries<int> { Values = values3, StackGroup = 1 },
            new StackedColumnSeries<int> { Values = values4, StackGroup = 1 }
        };

        var xAxes = new Axis[]
        {
            new() {
                LabelsRotation = -15,
                Labels = labels,
                MinStep = 1,
                ForceStepToMin = true
            }
        };

        var chart = new SKCartesianChart
        {
            Series = series,
            XAxes = xAxes,
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(StackedColumnsTests)}_{nameof(StackGroup)}");
    }
}
