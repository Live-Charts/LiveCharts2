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
public class StackedAreaSeriesBench
{
    private const int PointCount = 1_000;

    private ObservableValue[] _a = null!;
    private ObservableValue[] _b = null!;
    private SKCartesianChart _chart = null!;

    [GlobalSetup]
    public void Setup()
    {
        _a = new ObservableValue[PointCount];
        _b = new ObservableValue[PointCount];
        for (var i = 0; i < PointCount; i++)
        {
            _a[i] = new ObservableValue(Math.Sin(i * 0.05) * 30 + 40);
            _b[i] = new ObservableValue(Math.Cos(i * 0.05) * 30 + 40);
        }

        _chart = new SKCartesianChart
        {
            Width = BenchHarness.Width,
            Height = BenchHarness.Height,
            Series =
            [
                new StackedAreaSeries<ObservableValue> { Values = _a },
                new StackedAreaSeries<ObservableValue> { Values = _b }
            ]
        };
        BenchHarness.Render(_chart);
    }

    [Benchmark]
    public void Reinvalidate() => BenchHarness.Render(_chart);

    [Benchmark]
    public void UpdateOnePoint()
    {
        var idx = PointCount / 2;
        _a[idx].Value = _a[idx].Value + 0.1;
        BenchHarness.Render(_chart);
    }
}
