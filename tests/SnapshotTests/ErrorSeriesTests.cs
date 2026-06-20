using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;

namespace SnapshotTests;

[TestClass]
public sealed class ErrorSeriesTests
{
    [TestMethod]
    public void ColumnWithError()
    {
        var values1 = new ErrorValue[]
        {
            new(65, 6),
            new(70, 15, 4),
            new(35, 4),
            new(70, 6),
            new(30, 5),
            new(60, 4, 16),
            new(65, 6)
        };
        var values2 = new ErrorPoint[]
        {
            new(0, 50, 0.2, 8),
            new(1, 45, 0.1, 0.3, 15, 4),
            new(2, 25, 0.3, 4),
            new(3, 30, 0.2, 6),
            new(4, 70, 0.2, 8),
            new(5, 30, 0.4, 4),
            new(6, 50, 0.3, 6)
        };

        var chart = new SKCartesianChart
        {
            Series = [
                 new ColumnSeries<ErrorValue>
                {
                    Values = values1,
                    ShowError = true
                },
                new ColumnSeries<ErrorPoint>
                {
                    Values = values2,
                    ShowError = true
                },
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(ErrorSeriesTests)}_{nameof(ColumnWithError)}");
    }

    [TestMethod]
    public void RowWithError()
    {
        var values1 = new ErrorValue[]
        {
            new(65, 6),
            new(70, 15, 4),
            new(35, 4),
            new(70, 6),
            new(30, 5),
            new(60, 4, 16),
            new(65, 6)
        };
        var values2 = new ErrorPoint[]
        {
            new(0, 50, 0.2, 8),
            new(1, 45, 0.1, 0.3, 15, 4),
            new(2, 25, 0.3, 4),
            new(3, 30, 0.2, 6),
            new(4, 70, 0.2, 8),
            new(5, 30, 0.4, 4),
            new(6, 50, 0.3, 6)
        };

        var chart = new SKCartesianChart
        {
            Series = [
                new RowSeries<ErrorValue>
                {
                    Values = values1,
                    ShowError = true
                },
                new RowSeries<ErrorPoint>
                {
                    Values = values2,
                    ShowError = true
                },
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(ErrorSeriesTests)}_{nameof(RowWithError)}");
    }

    [TestMethod]
    public void LineWithError()
    {
        var values1 = new ErrorValue[]
        {
            new(65, 6),
            new(70, 15, 4),
            new(35, 4),
            new(70, 6),
            new(30, 5),
            new(60, 4, 16),
            new(65, 6)
        };

        var chart = new SKCartesianChart
        {
            Series = [
                 new ColumnSeries<ErrorValue>
                {
                    Values = values1,
                    ShowError = true
                },
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(ErrorSeriesTests)}_{nameof(LineWithError)}");
    }

    [TestMethod]
    public void ScatterWithError()
    {
        var values3 = new ErrorDateTimePoint[]
        {
            new(DateTime.Today.AddDays(0), 50, 0.2, 8),
            new(DateTime.Today.AddDays(1), 45, 0.1, 0.3, 15, 4),
            new(DateTime.Today.AddDays(2), 25, 0.3, 4),
            new(DateTime.Today.AddDays(3), 30, 0.2, 6),
            new(DateTime.Today.AddDays(4), 70, 0.2, 8),
            new(DateTime.Today.AddDays(5), 30, 0.4, 4),
            new(DateTime.Today.AddDays(6), 50, 0.3, 6)
        };

        var chart = new SKCartesianChart
        {
            Series = [
                 new ScatterSeries<ErrorDateTimePoint>
                {
                    Values = values3,
                    ShowError = true
                },
            ],
            XAxes = [
                new Axis
                {
                    IsVisible = false
                }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(ErrorSeriesTests)}_{nameof(ScatterWithError)}");
    }
}
