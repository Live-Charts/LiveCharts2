using LiveChartsCore.SkiaSharpView.WinUI;
using Microsoft.UI.Xaml.Controls;
using ViewModelsSamples.Maps.MarkersOnMap;

namespace WinUISample.Maps.MarkersOnMap;

public sealed partial class View : UserControl
{
    public View()
    {
        InitializeComponent();

        var vm = (ViewModel)DataContext;
        geoMap.VisualElements = vm.VisualElements;
    }

#if UI_TESTING
    public GeoMap Chart => geoMap;
#endif
}
