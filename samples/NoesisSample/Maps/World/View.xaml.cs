using Noesis;
using LiveChartsCore.Noesis;

namespace NoesisSample.Maps.World;
public partial class View : SampleView
{
    public View()
    {
        GUI.LoadComponent(this, "/NoesisSample;component/Maps/World/View.xaml");
    }

#if UI_TESTING
    public GeoMap Chart => (GeoMap)FindName("geoMap");
#endif
}
