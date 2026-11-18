using Factos;
using SharedUITests.Helpers;
using Xunit;

#if WPF_UI_TESTING
using System.Windows;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
#endif

// to run these tests, see the UITests project, specially the program.cs file.
// to enable IDE intellisense for these tests, go to Directory.Build.props and set UITesting to true.

namespace SharedUITests;

public class StyleTests
{
    public AppController App => AppController.Current;

#if WPF_UI_TESTING
    // https://github.com/Live-Charts/LiveCharts2/issues/2397
    // WPF applies an implicit style while the chart is still being constructed: the base
    // constructor sets Content, which raises Initialized and applies the style's setters
    // before the chart's own constructor created CoreChart. A setter for a chart property
    // then threw a NullReferenceException (an observed one, like Series, an
    // InvalidOperationException). The styled values must also be used once the chart loads.
    [AppTestMethod]
    public async Task ImplicitStyleIsAppliedWhileTheChartIsConstructed()
    {
        var style = new Style(typeof(CartesianChart));
        style.Setters.Add(new Setter(CartesianChart.LegendPositionProperty, LegendPosition.Right));
        style.Setters.Add(new Setter(CartesianChart.TooltipPositionProperty, TooltipPosition.Bottom));
        style.Setters.Add(new Setter(CartesianChart.SeriesProperty,
            new ISeries[] { new LineSeries<double> { Values = [2, 5, 3, 6] } }));

        var resources = Application.Current.Resources;
        resources[typeof(CartesianChart)] = style;

        try
        {
            var sut = new CartesianChart();

            Assert.Equal(LegendPosition.Right, sut.LegendPosition);
            Assert.Equal(TooltipPosition.Bottom, sut.TooltipPosition);

            // the styled series reaches the chart and renders once it loads.
            await App.NavigateToView(sut);
            await sut.WaitUntilChartLoadsAndRenders();
            Assert.ChartIsLoaded(sut);
        }
        finally
        {
            resources.Remove(typeof(CartesianChart));
        }
    }
#endif
}
