using Factos;
using SharedUITests.Helpers;
using Xunit;

// to run these tests, see the UITests project, specially the program.cs file.
// to enable IDE intellisense for these tests, go to Directory.Build.props and set UITesting to true.

namespace SharedUITests;

public class PolarChartTests
{
    public AppController App => AppController.Current;

    [AppTestMethod]
    public async Task ShouldLoad()
    {
        var sut = await App.NavigateTo<Samples.Polar.Basic.View>();
        await sut.Chart.WaitUntilChartRenders();

        Assert.ChartIsLoaded(sut.Chart);
    }

#if (WPF_UI_TESTING && TEST_HA_VIEWS) || MAUI_UI_TESTING || WINUI_UI_TESTING || (UNO_UI_TESTING && HAS_OS_LVC && !LVC_UNO_SKIA && !LVC_UNO_NO_GPU_TESTS)
    // native platforms where gpu is supported.
    //
    // The Uno arm of this gate excludes the heads that render through Uno's
    // Skia pipeline (LVC_UNO_SKIA, defined by the Uno sample for every head
    // but Windows App SDK; Uno Platform 7 has no native renderer anymore):
    // there the chart draws through SKCanvasElement (no hardware-accelerated
    // view), so RendererName never contains "GPU" and this test can't satisfy
    // the assertion. HAS_OS_LVC alone is the wrong signal for "GPU is
    // available": it is set for -android/-ios target frameworks too.

    [AppTestMethod]
    public async Task ShouldLoadHardwareAcceleratedView()
    {
        LiveChartsCore.LiveCharts.Configure(config => config.HasRenderingSettings(builder => builder.UseGPU = true));

        var sut = await App.NavigateTo<Samples.Polar.Basic.View>();
        await sut.Chart.WaitUntilChartRenders();

        Assert.Contains("GPU", sut.Chart.CoreCanvas.RendererName);
        Assert.ChartIsLoaded(sut.Chart);

        // restore default settings for other tests
        LiveChartsCore.LiveCharts.Configure(config => config.HasRenderingSettings(builder => builder.UseGPU = false));
    }
#endif
}
