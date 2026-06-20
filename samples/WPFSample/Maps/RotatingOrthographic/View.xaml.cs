using System.Windows.Controls;
using LiveChartsCore.SkiaSharpView.WPF;
using ViewModelsSamples.Maps.RotatingOrthographic;

namespace WPFSample.Maps.RotatingOrthographic;

public partial class View : UserControl
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
