using Factos;
using SharedUITests.Helpers;
using Xunit;

// to run these tests, see the UITests project, specially the program.cs file.
// to enable IDE intellisense for these tests, go to Directory.Build.props and set UITesting to true.

namespace SharedUITests;

public class MapChartTests
{
    public AppController App => AppController.Current;

    [AppTestMethod]
    public async Task ShouldLoad()
    {
        var sut = await App.NavigateTo<Samples.Maps.World.View>();
        var chart = sut.Chart;

        await chart.WaitUntilChartRenders();

        Assert.ChartIsLoaded(chart);
    }

#if AVALONIA_UI_TESTING
    // regression for https://github.com/Live-Charts/LiveCharts2/issues/1417
    // a GeoMap inside a TabControl crashed on the second tab switch (NRE in
    // GeoMapChart.Unload at _mapFactory.Dispose) and rendered blank on
    // re-attach. covers the lifecycle: detach -> attach -> detach -> attach.
    [AppTestMethod]
    public async Task GeoMap_should_survive_repeated_tab_switches()
    {
        var sut = await App.NavigateTo<Samples.VisualTest.Issue1417Repro.View>();
        await sut.Chart.WaitUntilChartRenders();
        Assert.ChartIsLoaded(sut.Chart);

        // away (Unload) -> back (Load): chart must render again, not stay blank.
        sut.OpenTab2();
        await Task.Delay(500);
        sut.OpenTab1();
        await sut.Chart.WaitUntilChartRenders();
        Assert.ChartIsLoaded(sut.Chart);

        // second cycle: pre-fix, this Unload NRE'd because it wasn't idempotent.
        sut.OpenTab2();
        await Task.Delay(500);
        sut.OpenTab1();
        await sut.Chart.WaitUntilChartRenders();
        Assert.ChartIsLoaded(sut.Chart);
    }
#endif
}
