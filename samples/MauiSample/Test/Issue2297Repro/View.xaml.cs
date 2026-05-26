using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Maui;

namespace MauiSample.Test.Issue2297Repro;

// Host page for the Factos regression test:
// SharedUITests/DisposeTests.ChartHandlerSurvivesTabSwitch_Issue2297.
//
// The bug only manifests inside a real iOS UITabBarController, so the test
// pushes a TabbedPage modally on top of this ContentPage. Building the
// TabbedPage in code (rather than in XAML) keeps the chart instances directly
// accessible — the test reads them back through GetChart to assert that their
// MAUI Handler survives a tab switch's Unloaded event (the regression #2297).
[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class View : ContentPage
{
    private TabbedPage? _tabbed;
    private CartesianChart[]? _charts;

    public View() => InitializeComponent();

    public async Task PushTabbedPageAsync()
    {
        _charts =
        [
            new CartesianChart { Series = [new LineSeries<double> { Values = [2d, 5, 4, 6, 8, 3, 7] }] },
            new CartesianChart { Series = [new ColumnSeries<double> { Values = [4d, 2, 6, 5, 8] }] },
        ];

        _tabbed = new TabbedPage
        {
            Children =
            {
                new ContentPage { Title = "A", Content = _charts[0] },
                new ContentPage { Title = "B", Content = _charts[1] },
            }
        };

        await Navigation.PushModalAsync(_tabbed);
    }

    public void SwitchToTab(int index) =>
        _tabbed!.CurrentPage = _tabbed.Children.ElementAt(index);

    public CartesianChart GetChart(int index) => _charts![index];

    public Task PopTabbedPageAsync() => Navigation.PopModalAsync();
}
