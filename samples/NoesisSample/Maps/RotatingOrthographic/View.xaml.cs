using Noesis;
using LiveChartsCore.Noesis;
using ViewModelsSamples.Maps.RotatingOrthographic;

namespace NoesisSample.Maps.RotatingOrthographic;

public partial class View : SampleView
{
    private LiveChartsCore.Noesis.GeoMap geoMap => (LiveChartsCore.Noesis.GeoMap)FindName("geoMap");

    public View()
    {
        GUI.LoadComponent(this, "/NoesisSample;component/Maps/RotatingOrthographic/View.xaml");

        var vm = (ViewModel)DataContext;
        Loaded   += (_, _) => vm.Start((lon, lat) => geoMap.CoreChart.RotateTo(lon, lat));
        Unloaded += (_, _) => vm.Stop();
    }

#if UI_TESTING
    public GeoMap Chart => geoMap;
#endif
}
