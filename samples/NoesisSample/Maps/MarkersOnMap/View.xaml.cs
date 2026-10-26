using Noesis;
using LiveChartsCore.Noesis;
using ViewModelsSamples.Maps.MarkersOnMap;

namespace NoesisSample.Maps.MarkersOnMap;

public partial class View : SampleView
{
    private LiveChartsCore.Noesis.GeoMap geoMap => (LiveChartsCore.Noesis.GeoMap)FindName("geoMap");

    public View()
    {
        GUI.LoadComponent(this, "/NoesisSample;component/Maps/MarkersOnMap/View.xaml");

        // VisualElements is a CLR property on GeoMap today (not a DP), so
        // the XAML binding doesn't pick it up. Set it in code-behind from
        // the ViewModel.
        var vm = (ViewModel)DataContext;
        geoMap.VisualElements = vm.VisualElements;
    }

#if UI_TESTING
    public GeoMap Chart => (GeoMap)FindName("geoMap");
#endif
}
