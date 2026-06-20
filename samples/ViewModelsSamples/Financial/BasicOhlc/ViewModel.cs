using System;
using System.Collections.ObjectModel;
using LiveChartsCore.Defaults;

namespace ViewModelsSamples.Financial.BasicOhlc;

public class ViewModel
{
    public ObservableCollection<FinancialPoint> Values { get; set; } = [
        new() { Date = new DateTime(2021, 1, 1), High = 523, Open = 500, Close = 450, Low = 400 },
        new() { Date = new DateTime(2021, 1, 2), High = 500, Open = 450, Close = 425, Low = 400 },
        new() { Date = new DateTime(2021, 1, 3), High = 490, Open = 425, Close = 400, Low = 380 },
        new() { Date = new DateTime(2021, 1, 4), High = 420, Open = 400, Close = 420, Low = 380 },
        new() { Date = new DateTime(2021, 1, 5), High = 520, Open = 420, Close = 490, Low = 400 },
        new() { Date = new DateTime(2021, 1, 6), High = 580, Open = 490, Close = 560, Low = 440 }
    ];

    public Func<DateTime, string> DateFormatter { get; set; } = value => value.ToString("yyyy MMM dd");
}
