using Factos;
using LiveChartsCore.Kernel.Sketches;
using SharedUITests.Helpers;
using Xunit;

// to run these tests, see the UITests project, specially the program.cs file.
// to enable IDE intellisense for these tests, go to Directory.Build.props and set UITesting to true.

namespace SharedUITests;

public class OtherTests
{
    public AppController App => AppController.Current;

#if XAML_UI_TESTING
    [AppTestMethod]
    public async Task ShouldHandleNullContext()
    {
        var sut = await App.NavigateTo<Samples.Test.NullContext.View>();

        // to make it simple, wait for some time for the template to load
        await Task.Delay(2000);

        sut.SetNullContext();
        await Task.Delay(1000);
        // if we got here without exceptions it means the chart handled the null context without issues.

        Assert.True(sut.CartesianChart.CoreChart.IsLoaded);
        Assert.True(sut.PieChart.CoreChart.IsLoaded);
        Assert.True(sut.PolarChart.CoreChart.IsLoaded);
    }
#endif
}
