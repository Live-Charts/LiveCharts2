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

#if (WPF_UI_TESTING && TEST_HA_VIEWS) || MAUI_UI_TESTING || WINUI_UI_TESTING || (UNO_UI_TESTING && HAS_OS_LVC && !LVC_UNO_SKIA)
    // native platforms where gpu is supported.
    //
    // The Uno arm of this gate excludes builds with the Uno SkiaRenderer
    // feature enabled: under Uno-Skia the chart renders through
    // Uno.WinUI.Graphics2DSK's SKCanvasElement (no native XAML
    // hardware-accelerated element), so RendererName never contains "GPU"
    // and this test can't satisfy the assertion. HAS_OS_LVC alone is the
    // wrong signal for "GPU is available" now that Uno-Skia is the common
    // setup on mobile.

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
