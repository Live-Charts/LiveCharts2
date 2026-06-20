using BenchmarkDotNet.Running;
using LiveChartsCore;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView;

namespace Benchmarks;

public static class Program
{
    public static int Main(string[] args)
    {
        // `compare <baseJsonDir> <headJsonDir> [outMarkdown]` reads BenchmarkDotNet
        // `-report-full.json` exports from two runs and emits a markdown delta table.
        if (args.Length > 0 && args[0] == "compare")
            return ResultsCompare.Run(args.AsSpan(1));

        // Configure LiveCharts once, and disable animations so we measure only the
        // invalidate/draw cost — not time spent waiting for motion to settle.
        LiveCharts.Configure(config => config.UseDefaults());
        CoreMotionCanvas.IsTesting = true;

        _ = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        return 0;
    }
}
