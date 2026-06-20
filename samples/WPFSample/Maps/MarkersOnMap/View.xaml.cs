using System.Windows.Controls;
using LiveChartsCore.SkiaSharpView.WPF;
using ViewModelsSamples.Maps.MarkersOnMap;

namespace WPFSample.Maps.MarkersOnMap;

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();

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
