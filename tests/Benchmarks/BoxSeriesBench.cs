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
public class BoxSeriesBench
{
    private const int PointCount = 1_000;

    private BoxValue[] _values = null!;
    private SKCartesianChart _chart = null!;

    [GlobalSetup]
    public void Setup()
    {
        _values = new BoxValue[PointCount];
        for (var i = 0; i < PointCount; i++)
        {
            var m = Math.Sin(i * 0.05) * 50 + 50;
            // max, Q3, Q1, min, median
            _values[i] = new BoxValue(m + 20, m + 10, m - 10, m - 20, m);
        }

        _chart = new SKCartesianChart
        {
            Width = BenchHarness.Width,
            Height = BenchHarness.Height,
            Series = [new BoxSeries<BoxValue> { Values = _values }]
        };
        BenchHarness.Render(_chart);
    }

    [Benchmark]
    public void Reinvalidate() => BenchHarness.Render(_chart);

    [Benchmark]
    public void UpdateOnePoint()
    {
        var idx = PointCount / 2;
        _values[idx].Median += 0.1;
        BenchHarness.Render(_chart);
    }
}
