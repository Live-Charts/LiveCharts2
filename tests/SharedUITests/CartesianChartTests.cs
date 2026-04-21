using Factos;
using SharedUITests.Helpers;
using Xunit;

// to run these tests, see the UITests project, specially the program.cs file.
// to enable IDE intellisense for these tests, go to Directory.Build.props and set UITesting to true.

namespace SharedUITests;

public class CartesianChartTests
{
    public AppController App => AppController.Current;

    [AppTestMethod]
    public async Task ShouldLoad()
    {
        var sut = await App.NavigateTo<Samples.General.FirstChart.View>();
        await sut.Chart.WaitUntilChartRenders();

        Assert.ChartIsLoaded(sut.Chart);
    }

#if XAML_UI_TESTING
    // xaml platforms tests.

    [AppTestMethod]
    public async Task ShouldLoadTemplatedChart()
    {
        var sut = await App.NavigateTo<Samples.VisualTest.DataTemplate.View>();

        // to make it simple, wait for some time for the template to load
        await Task.Delay(2000);

        // now lets find the templated charts
        foreach (var chart in sut.FindCharts())
        {
            await chart.WaitUntilChartRenders();
            Assert.ChartIsLoaded(chart);
        }
    }
#endif

#if !BLAZOR_UI_TESTING
    // this test makes no sense in blazor.

    [AppTestMethod]
    public async Task ShouldUnloadAndReload()
    {
        var sut = new Samples.Bars.AutoUpdate.View();

        await App.NavigateToView(sut);
        await sut.Chart.WaitUntilChartRenders();
        Assert.ChartIsLoaded(sut.Chart);

#if MAUI_UI_TESTING
        // in maui App.NavigateToView(); uses the Shell navigation,
        // but in this test we need to unload and reload the control in the same view

        sut.UnloadChart();
        await Task.Delay(1000);

        sut.ReloadChart();
        await Task.Delay(1000);

        Assert.ChartIsLoaded(sut.Chart);
#else
        await App.PopNavigation();
        await App.NavigateToView(sut);

        // ToDo: improve this method? as a workaround for now we just wait for some time
        // await sut.Chart.WaitUntilChartRenders();
        await Task.Delay(2000);

        Assert.ChartIsLoaded(sut.Chart);
#endif
    }
#endif

#if AVALONIA_UI_TESTING
    // based on:
    // https://github.com/Live-Charts/LiveCharts2/issues/1986
    // ensure charts load when avalonia virtualization is on.

    [AppTestMethod]
    public async Task TabControlScrollViewerRendersAfterTabSwitch()
    {
        var sut = await App.NavigateTo<Samples.VisualTest.VirtualizationTest.View>();

        // open the second tab, scroll to end and ensure the chart is loaded.
        sut.OpenTab2();
        await Task.Delay(1000);
        sut.ScrollToChart();
        await Task.Delay(1000);
        Assert.ChartIsLoaded(sut.Chart2);

        // now open the first tab, scroll to end and ensure the chart is loaded.
        sut.OpenTab1();
        await Task.Delay(1000);
        sut.ScrollToChart();
        await Task.Delay(1000);
        Assert.ChartIsLoaded(sut.Chart1);
    }

    // based on:
    // https://github.com/Live-Charts/LiveCharts2/issues/1986
    // ensure charts render when series data is assigned asynchronously while the chart is
    // off-screen (in a ScrollViewer below the viewport). This is the scenario @busitech
    // described: measure is suppressed because the canvas is not rendering when data arrives.

    [AppTestMethod]
    public async Task ScrollViewerVirtualizationRendersAsyncData()
    {
        var sut = await App.NavigateTo<Samples.VisualTest.ScrollViewerVirtualization.View>();

        // wait for the 1 second async data load to complete, then give the chart time to render.
        await Task.Delay(2500);

        // scroll to reveal chart1 and verify it rendered with the async data.
        sut.ScrollToChart();
        await Task.Delay(1000);
        Assert.ChartIsLoaded(sut.Chart1);

        // switch to tab 2, scroll to chart2, and verify it also loaded.
        sut.OpenTab2();
        await Task.Delay(500);
        sut.ScrollToChart();
        await Task.Delay(1000);
        Assert.ChartIsLoaded(sut.Chart2);
    }
#endif

#if (WPF_UI_TESTING && TEST_HA_VIEWS) || MAUI_UI_TESTING || WINUI_UI_TESTING || (UNO_UI_TESTING && HAS_OS_LVC)
    // native platforms where gpu is supported

    [AppTestMethod]
    public async Task ShouldLoadHardwareAcceleratedView()
    {
        LiveChartsCore.LiveCharts.Configure(config => config.HasRenderingSettings(builder => builder.UseGPU = true));

        var sut = await App.NavigateTo<Samples.General.FirstChart.View>();
        await sut.Chart.WaitUntilChartRenders();

        Assert.Contains("GPU", sut.Chart.CoreCanvas.RendererName);
        Assert.ChartIsLoaded(sut.Chart);

        // restore default settings for other tests
        LiveChartsCore.LiveCharts.Configure(config => config.HasRenderingSettings(builder => builder.UseGPU = false));
    }
#endif
}
