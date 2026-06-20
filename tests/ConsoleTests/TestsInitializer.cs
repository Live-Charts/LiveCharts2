// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Motion;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// Chart construction touches static LiveCharts state (default settings, theme rules, the
// motion canvas registry) that isn't safe across concurrent test methods. Parallel runs
// caused intermittent "Collection was modified" exceptions inside ChartElement.RemoveOldPaints
// when two charts measured at the same time — running sequentially eliminates the race.
[assembly: DoNotParallelize]

namespace ConsoleTests;

[TestClass]
public class TestsInitializer
{
    [AssemblyInitialize]
    public static void Initialize(TestContext _)
    {
        // Disables motion animations so Render() returns the fully-settled state in one pass —
        // without this, charts would emit mid-tween frames that vary by clock and break goldens.
        // The console provider/theme/mappers are configured by InMemoryConsoleChart's static
        // ctor on first use, so no explicit LiveCharts.Configure() call is needed here.
        CoreMotionCanvas.IsTesting = true;
    }
}
