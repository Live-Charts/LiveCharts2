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
public class PolarLineSeriesBench
{
    private const int PointCount = 1_000;

    private ObservableValue[] _values = null!;
    private SKPolarChart _chart = null!;

    [GlobalSetup]
    public void Setup()
    {
        _values = new ObservableValue[PointCount];
        for (var i = 0; i < PointCount; i++)
            _values[i] = new ObservableValue(Math.Sin(i * 0.05) * 50 + 50);

        _chart = new SKPolarChart
        {
            Width = BenchHarness.Width,
            Height = BenchHarness.Height,
            Series = [new PolarLineSeries<ObservableValue> { Values = _values }]
        };
        BenchHarness.Render(_chart);
    }

    [Benchmark]
    public void Reinvalidate() => BenchHarness.Render(_chart);

    [Benchmark]
    public void UpdateOnePoint()
    {
        _values[PointCount / 2].Value = _values[PointCount / 2].Value + 0.1;
        BenchHarness.Render(_chart);
    }
}
