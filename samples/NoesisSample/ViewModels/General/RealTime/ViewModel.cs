using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Defaults;

namespace ViewModelsSamples.General.RealTime;

public partial class ViewModel : ObservableObject
{
    public ViewModel()
    {
        _ = ReadData();
    }

    [ObservableProperty]
    public partial double[] Separators { get; set; } = [];

    public ObservableCollection<DateTimePoint> Values { get; set; } = [];

    public Func<DateTime, string> LabelsFormatter { get; } = Formatter;

    public object Sync { get; } = new object();

    public bool IsReading { get; set; } = true;

    private async Task ReadData()
    {
        var random = new Random();

        // to keep this sample simple, we run the next infinite loop // mark
        // in a real application you should stop the loop/task when the view is disposed // mark

        while (IsReading)
        {
            await Task.Delay(100);

            // Because we are updating the chart from a different thread // mark
            // we need to use a lock to access the chart data. // mark
            // this is not necessary if your changes are made on the UI thread. // mark
            lock (Sync)
            {
                Values.Add(new DateTimePoint(DateTime.Now, random.Next(0, 10)));
                if (Values.Count > 250) Values.RemoveAt(0);

                // we need to update the separators every time we add a new point // mark
                Separators = GetSeparators();
            }
        }
    }

    private static double[] GetSeparators()
    {
        var now = DateTime.Now;

        return
        [
            now.AddSeconds(-25).Ticks,
            now.AddSeconds(-20).Ticks,
            now.AddSeconds(-15).Ticks,
            now.AddSeconds(-10).Ticks,
            now.AddSeconds(-5).Ticks,
            now.Ticks
        ];
    }

    private static string Formatter(DateTime date)
    {
        var secsAgo = (DateTime.Now - date).TotalSeconds;

        return secsAgo < 1
            ? "now"
            : $"{secsAgo:N0}s ago";
    }
}
