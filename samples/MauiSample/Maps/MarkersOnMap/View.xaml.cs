using LiveChartsCore.SkiaSharpView.Maui;
using ViewModelsSamples.Maps.MarkersOnMap;

namespace MauiSample.Maps.MarkersOnMap;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class View : ContentPage
{
    public View()
    {
        InitializeComponent();

        var vm = (ViewModel)BindingContext;
        geoMap.VisualElements = vm.VisualElements;
    }

#if UI_TESTING
    public GeoMap Chart => geoMap;
#endif
}
