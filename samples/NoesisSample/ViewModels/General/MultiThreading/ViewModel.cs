using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace ViewModelsSamples.General.MultiThreading;

public class ViewModel
{
    private readonly Random _r = new();
    private readonly int _delay = 100;
    private int _current;

    public ObservableCollection<int> Values { get; }
    public object Sync { get; } = new object();
    public bool IsReading { get; set; } = true;

    public ViewModel()
    {
        // lets create some initial data. // mark
        var items = new List<int>();
        for (var i = 0; i < 1500; i++)
        {
            _current += _r.Next(-9, 10);
            items.Add(_current);
        }

        Values = [.. items];

        _delay = 1;
        var readTasks = 10;

        // Finally, we need to start the tasks that will add points to the series. // mark
        // we are creating {readTasks} tasks // mark
        // that will add a point every {_delay} milliseconds // mark
        for (var i = 0; i < readTasks; i++)
        {
            _ = Task.Run(ReadData);
        }
    }

    private async Task ReadData()
    {
        await Task.Delay(1000);

        // to keep this sample simple, we run the next infinite loop // mark
        // in a real application you should stop the loop/task when the view is disposed // mark

        while (IsReading)
        {
            await Task.Delay(_delay);

            _current = Interlocked.Add(ref _current, _r.Next(-9, 10));

            lock (Sync)
            {
                Values.Add(_current);
                Values.RemoveAt(0);
            }
        }
    }
}
