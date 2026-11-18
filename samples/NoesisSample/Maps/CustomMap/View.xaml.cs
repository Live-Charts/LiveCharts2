using Noesis;
using LiveChartsCore.Noesis;

namespace NoesisSample.Maps.CustomMap;

public partial class View : SampleView
{
    public View()
    {
        GUI.LoadComponent(this, "/NoesisSample;component/Maps/CustomMap/View.xaml");
    }

#if UI_TESTING
    public GeoMap Chart => (GeoMap)FindName("geoMap");
#endif
}
