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
public class HeatSeriesBench
{
    // Heat maps are typically denser per point; a 32x32 grid is 1024, roughly matching
    // the other series' PointCount.
    private const int Side = 32;

    private WeightedPoint[] _values = null!;
    private SKCartesianChart _chart = null!;

    [GlobalSetup]
    public void Setup()
    {
        _values = new WeightedPoint[Side * Side];
        var k = 0;
        for (var x = 0; x < Side; x++)
            for (var y = 0; y < Side; y++)
                _values[k++] = new WeightedPoint(x, y, Math.Sin(x * 0.2) * Math.Cos(y * 0.2) + 1.5);

        _chart = new SKCartesianChart
        {
            Width = BenchHarness.Width,
            Height = BenchHarness.Height,
            Series = [new HeatSeries<WeightedPoint> { Values = _values }]
        };
        BenchHarness.Render(_chart);
    }

    [Benchmark]
    public void Reinvalidate() => BenchHarness.Render(_chart);

    [Benchmark]
    public void UpdateOnePoint()
    {
        var idx = _values.Length / 2;
        _values[idx].Weight = (_values[idx].Weight ?? 0) + 0.1;
        BenchHarness.Render(_chart);
    }
}
