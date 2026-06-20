using LiveChartsCore.SkiaSharpView.Maui;

namespace MauiSample.Maps.World;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class View : ContentPage
{
    public View()
    {
        InitializeComponent();
    }

#if UI_TESTING
    public GeoMap Chart => geoMap;
#endif

}
