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
public class CandlesticksSeriesBench
{
    private const int PointCount = 1_000;

    private FinancialPointI[] _values = null!;
    private SKCartesianChart _chart = null!;

    [GlobalSetup]
    public void Setup()
    {
        _values = new FinancialPointI[PointCount];
        for (var i = 0; i < PointCount; i++)
        {
            var m = Math.Sin(i * 0.05) * 50 + 50;
            // high, open, close, low
            _values[i] = new FinancialPointI(m + 10, m + 2, m - 2, m - 10);
        }

        _chart = new SKCartesianChart
        {
            Width = BenchHarness.Width,
            Height = BenchHarness.Height,
            Series = [new CandlesticksSeries<FinancialPointI> { Values = _values }]
        };
        BenchHarness.Render(_chart);
    }

    [Benchmark]
    public void Reinvalidate() => BenchHarness.Render(_chart);

    [Benchmark]
    public void UpdateOnePoint()
    {
        var idx = PointCount / 2;
        _values[idx].Close += 0.1;
        BenchHarness.Render(_chart);
    }
}
