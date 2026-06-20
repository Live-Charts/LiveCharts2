using LiveChartsCore.SkiaSharpView.WinUI;
using Microsoft.UI.Xaml.Controls;
using ViewModelsSamples.Maps.RotatingOrthographic;

namespace WinUISample.Maps.RotatingOrthographic;

public sealed partial class View : UserControl
{
    public View()
    {
        InitializeComponent();

        var vm = (ViewModel)DataContext;
        Loaded   += (_, _) => vm.Start((lon, lat) => geoMap.CoreChart.RotateTo(lon, lat));
        Unloaded += (_, _) => vm.Stop();
    }

#if UI_TESTING
    public GeoMap Chart => geoMap;
#endif
}
