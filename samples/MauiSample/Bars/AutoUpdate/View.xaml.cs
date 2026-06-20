using LiveChartsCore.SkiaSharpView.Maui;
using ViewModelsSamples.Bars.AutoUpdate;

namespace MauiSample.Bars.AutoUpdate;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class View : ContentPage
{
    public View()
    {
        InitializeComponent();
    }

#if UI_TESTING
    public CartesianChart Chart => chart;

    public void UnloadChart() => grid.Children.Remove(chart);
    public void ReloadChart() => grid.Children.Add(chart);
#endif
}
