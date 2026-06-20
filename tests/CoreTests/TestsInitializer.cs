using LiveChartsCore;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests;

[TestClass]
public class TestsInitializer
{
    [AssemblyInitialize()]
    public static void MyTestInitialize(TestContext testContext)
    {
        // The test framework will call this method once -BEFORE- each test run.

        LiveCharts.Configure(config => config.UseDefaults());
        CoreMotionCanvas.IsTesting = true;
    }

    [AssemblyCleanup]
    public static void TearDown()
    {
        // The test framework will call this method once -AFTER- each test run.

    }
}
