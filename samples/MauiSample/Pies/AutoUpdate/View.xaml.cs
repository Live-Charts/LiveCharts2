using LiveChartsCore.SkiaSharpView.Maui;
using ViewModelsSamples.Pies.AutoUpdate;

namespace MauiSample.Pies.AutoUpdate;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class View : ContentPage
{
    public View()
    {
        InitializeComponent();
    }

#if UI_TESTING
    public PieChart Chart => chart;
    public void UnloadChart() => grid.Children.Remove(chart);
    public void ReloadChart() => grid.Children.Add(chart);
#endif
}
