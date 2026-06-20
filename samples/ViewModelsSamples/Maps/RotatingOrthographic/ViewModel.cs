using System;
using System.Threading;
using System.Threading.Tasks;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;

namespace ViewModelsSamples.Maps.RotatingOrthographic;

public class ViewModel
{
    private static readonly (double Lon, double Lat)[] s_path =
    [
        (-95, 35),   // North America
        (100, 35)    // Asia
    ];

    private CancellationTokenSource? _cts;
    private SynchronizationContext? _ctx;

    public ViewModel()
    {
        var lands = new HeatLand[]
        {
            new() { Name = "bra", Value = 13 },
            new() { Name = "mex", Value = 10 },
            new() { Name = "usa", Value = 15 },
            new() { Name = "can", Value = 8 },
            new() { Name = "ind", Value = 12 },
            new() { Name = "deu", Value = 13 },
            new() { Name = "jpn", Value = 15 },
            new() { Name = "chn", Value = 14 },
            new() { Name = "rus", Value = 11 },
            new() { Name = "fra", Value = 8 },
            new() { Name = "esp", Value = 7 },
            new() { Name = "kor", Value = 10 },
            new() { Name = "zaf", Value = 12 },
            new() { Name = "are", Value = 13 },
            new() { Name = "aus", Value = 9 },
            new() { Name = "arg", Value = 6 },
            new() { Name = "egy", Value = 7 },
            new() { Name = "nga", Value = 11 },
            new() { Name = "gbr", Value = 9 }
        };

        Series = [new HeatLandSeries { Lands = lands }];
    }

    public HeatLandSeries[] Series { get; set; }

    // Starts the infinite rotation loop. Call from the UI thread so we can
    // capture its SynchronizationContext and dispatch RotateTo back to it —
    // saves every platform view from writing its own Dispatcher.Invoke.
    public void Start(Action<double, double> rotate)
    {
        Stop();
        _ctx = SynchronizationContext.Current;
        _cts = new CancellationTokenSource();
        _ = RunAsync(rotate, _cts.Token);
    }

    public void Stop()
    {
        _cts?.Cancel();
        _cts = null;
    }

    private async Task RunAsync(Action<double, double> rotate, CancellationToken ct)
    {
        var i = 0;
        while (!ct.IsCancellationRequested)
        {
            var (lon, lat) = s_path[i];
            Dispatch(() => rotate(lon, lat));

            try { await Task.Delay(TimeSpan.FromSeconds(5), ct); }
            catch (OperationCanceledException) { return; }

            i = (i + 1) % s_path.Length;
        }
    }

    private void Dispatch(Action action)
    {
        if (_ctx is null) action();
        else _ctx.Post(_ => action(), null);
    }
}
