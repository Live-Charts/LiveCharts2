using LiveChartsCore.SkiaSharpView.Maui;
using ViewModelsSamples.Maps.RotatingOrthographic;

namespace MauiSample.Maps.RotatingOrthographic;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class View : ContentPage
{
    public View()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var vm = (ViewModel)BindingContext;
        vm.Start((lon, lat) => geoMap.CoreChart.RotateTo(lon, lat));
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        var vm = (ViewModel)BindingContext;
        vm.Stop();
    }

#if UI_TESTING
    public GeoMap Chart => geoMap;
#endif
}
