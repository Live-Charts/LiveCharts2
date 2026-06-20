using LiveChartsCore;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView;

namespace SnapshotTests;

[TestClass]
public class TestsInitializer
{
    // SourceGenSKMapChart's ctor (used by SKGeoMap) reaches into
    // LiveCharts.DefaultSettings.GetProvider() before any series is created,
    // so the engine must be configured at assembly init — unlike the cartesian
    // chart whose series static-ctors register it lazily.
    [AssemblyInitialize]
    public static void Initialize(TestContext context)
    {
        LiveCharts.Configure(config => config.UseDefaults());
        CoreMotionCanvas.IsTesting = true;
    }
}
