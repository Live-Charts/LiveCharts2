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

#if XAML_UI_TESTING
    // Regression for https://github.com/Live-Charts/LiveCharts2/issues/1981.
    // Inside a DataTemplate, WPF's FrameworkElementFactory skips applying a
    // binding when the target DP already has a non-default local value. The
    // chart used to set Series/XAxes/YAxes to a fresh ObservableCollection
    // in its constructor, which silently swallowed direct bindings on those
    // properties. The fix lazy-inits those collections in the property getter
    // (via SetCurrentValue) so the constructor leaves the DP at its default,
    // letting the binding attach normally.
    //
    // The same sample runs across every XAML platform so any platform with
    // similar template-binding semantics is caught here.

    [AppTestMethod]
    public async Task ShouldBindSeriesAndAxesInsideDataTemplate()
    {
        var sut = await App.NavigateTo<Samples.VisualTest.Issue1981.View>();

        await Task.Delay(2000);

        var charts = sut.FindCharts().ToList();
        Assert.NotEmpty(charts);

        foreach (var chart in charts)
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
        var sut = await App.NavigateTo<Samples.VisualTest.Issue1986Repro.View>();

        // wait for the async data load (~1s, simulates a server fetch).
        await Task.Delay(1500);

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
