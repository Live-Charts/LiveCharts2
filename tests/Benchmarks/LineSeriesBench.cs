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
public class LineSeriesBench
{
    [Params(1_000, 10_000)]
    public int PointCount;

    private ObservableValue[] _values = null!;
    private SKCartesianChart _chart = null!;

    // A chart pre-measured once, so Update/Gap benchmarks isolate the incremental cost
    // rather than first-draw setup.
    private SKCartesianChart _primedChart = null!;
    private ObservableValue[] _primedValues = null!;

    [GlobalSetup]
    public void Setup()
    {
        _values = new ObservableValue[PointCount];
        for (var i = 0; i < PointCount; i++)
            _values[i] = new ObservableValue(Math.Sin(i * 0.05) * 50 + 50);

        _chart = new SKCartesianChart
        {
            Width = BenchHarness.Width,
            Height = BenchHarness.Height,
            Series = [new LineSeries<ObservableValue> { Values = _values }]
        };

        _primedValues = new ObservableValue[PointCount];
        for (var i = 0; i < PointCount; i++)
            _primedValues[i] = new ObservableValue(Math.Sin(i * 0.05) * 50 + 50);

        _primedChart = new SKCartesianChart
        {
            Width = BenchHarness.Width,
            Height = BenchHarness.Height,
            Series = [new LineSeries<ObservableValue> { Values = _primedValues }]
        };
        BenchHarness.Render(_primedChart);
    }

    // Cold-path cost: first draw from a fresh chart.
    [Benchmark]
    public void FirstRender()
    {
        var chart = new SKCartesianChart
        {
            Width = BenchHarness.Width,
            Height = BenchHarness.Height,
            Series = [new LineSeries<ObservableValue> { Values = _values }]
        };
        BenchHarness.Render(chart);
    }

    // Warm-path cost: re-invalidate an unchanged, already-measured chart.
    [Benchmark]
    public void Reinvalidate() => BenchHarness.Render(_chart);

    // Incremental update: one point's value changes, trigger re-invalidate.
    [Benchmark]
    public void UpdateOnePoint()
    {
        _primedValues[PointCount / 2].Value = _primedValues[PointCount / 2].Value + 0.1;
        BenchHarness.Render(_primedChart);
    }

    // #2132 shape: toggle a middle point between null and a value, which changes the
    // sub-segment layout each call and stresses VectorManager's node reconciliation.
    [Benchmark]
    public void ToggleNullGap()
    {
        var idx = PointCount / 2;
        _primedValues[idx].Value = _primedValues[idx].Value is null ? 42.0 : null;
        BenchHarness.Render(_primedChart);
    }
}
