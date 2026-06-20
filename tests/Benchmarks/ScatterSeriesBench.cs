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
public class ScatterSeriesBench
{
    private const int PointCount = 1_000;

    private ObservablePoint[] _values = null!;
    private SKCartesianChart _chart = null!;

    [GlobalSetup]
    public void Setup()
    {
        _values = new ObservablePoint[PointCount];
        for (var i = 0; i < PointCount; i++)
            _values[i] = new ObservablePoint(i, Math.Sin(i * 0.05) * 50 + 50);

        _chart = new SKCartesianChart
        {
            Width = BenchHarness.Width,
            Height = BenchHarness.Height,
            Series = [new ScatterSeries<ObservablePoint> { Values = _values }]
        };
        BenchHarness.Render(_chart);
    }

    [Benchmark]
    public void Reinvalidate() => BenchHarness.Render(_chart);

    [Benchmark]
    public void UpdateOnePoint()
    {
        var idx = PointCount / 2;
        _values[idx].Y = (_values[idx].Y ?? 0) + 0.1;
        BenchHarness.Render(_chart);
    }
}
