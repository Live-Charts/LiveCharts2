using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Jobs;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;

namespace Benchmarks;

[MemoryDiagnoser]
[JsonExporterAttribute.Full]
[SimpleJob(RuntimeMoniker.Net80, warmupCount: 3, iterationCount: 8)]
public class PieSeriesBench
{
    // Pies rarely have thousands of slices — 20 is a realistic upper bound for
    // catching regressions in slice layout cost without inflating test runtime.
    private const int SliceCount = 20;

    private PieSeries<ObservableValue>[] _series = null!;
    private ObservableValue[] _values = null!;
    private SKPieChart _chart = null!;

    [GlobalSetup]
    public void Setup()
    {
        _values = new ObservableValue[SliceCount];
        _series = new PieSeries<ObservableValue>[SliceCount];
        for (var i = 0; i < SliceCount; i++)
        {
            _values[i] = new ObservableValue(1 + i);
            _series[i] = new PieSeries<ObservableValue> { Values = new[] { _values[i] } };
        }

        _chart = new SKPieChart
        {
            Width = BenchHarness.Width,
            Height = BenchHarness.Height,
            Series = _series
        };
        BenchHarness.Render(_chart);
    }

    [Benchmark]
    public void Reinvalidate() => BenchHarness.Render(_chart);

    [Benchmark]
    public void UpdateOnePoint()
    {
        _values[SliceCount / 2].Value = _values[SliceCount / 2].Value + 0.1;
        BenchHarness.Render(_chart);
    }
}
