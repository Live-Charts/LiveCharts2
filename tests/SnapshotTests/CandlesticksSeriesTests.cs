using System.Collections.ObjectModel;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;

namespace SnapshotTests;

[TestClass]
public sealed class CandlesticksSeriesTests
{
    [TestMethod]
    public void Basic()
    {
        var values = new ObservableCollection<FinancialPointI>
        {
            new() { High = 523, Open = 500, Close = 450, Low = 400 },
            new() { High = 500, Open = 450, Close = 425, Low = 400 },
            new() { High = 490, Open = 425, Close = 400, Low = 380 },
            new() { High = 420, Open = 400, Close = 420, Low = 380 },
            new() { High = 520, Open = 420, Close = 490, Low = 400 },
            new() { High = 580, Open = 490, Close = 560, Low = 440 }
        };

        var chart = new SKCartesianChart
        {
            Series = [
                new CandlesticksSeries<FinancialPointI> { Values = values }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(CandlesticksSeriesTests)}_{nameof(Basic)}");
    }

    [TestMethod]
    public void DateTime()
    {
        var values = new ObservableCollection<FinancialPoint>
        {
            new() { Date = new DateTime(2021, 1, 1), High = 523, Open = 500, Close = 450, Low = 400 },
            new() { Date = new DateTime(2021, 1, 2), High = 500, Open = 450, Close = 425, Low = 400 },
            new() { Date = new DateTime(2021, 1, 3), High = 490, Open = 425, Close = 400, Low = 380 },
            new() { Date = new DateTime(2021, 1, 4), High = 420, Open = 400, Close = 420, Low = 380 },
            new() { Date = new DateTime(2021, 1, 5), High = 520, Open = 420, Close = 490, Low = 400 },
            new() { Date = new DateTime(2021, 1, 6), High = 580, Open = 490, Close = 560, Low = 440 }
        };

        var chart = new SKCartesianChart
        {
            Series = [
                new CandlesticksSeries<FinancialPoint> { Values = values }
            ],
            XAxes = [
                new Axis
                {
                    UnitWidth = TimeSpan.FromDays(1).Ticks,
                    Labeler = value => new DateTime((long)value).ToString("MM dd")
                }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(CandlesticksSeriesTests)}_{nameof(DateTime)}");
    }
}
