using Factos;
using SharedUITests.Helpers;
using Xunit;

// to run these tests, see the UITests project, specially the program.cs file.
// to enable IDE intellisense for these tests, go to Directory.Build.props and set UITesting to true.

namespace SharedUITests;

public class TreemapChartTests
{
    public AppController App => AppController.Current;

    [AppTestMethod]
    public async Task ShouldLoad()
    {
        var sut = await App.NavigateTo<Samples.Treemaps.Basic.View>();
        await sut.Chart.WaitUntilChartRenders();

        Assert.ChartIsLoaded(sut.Chart);
    }
}
