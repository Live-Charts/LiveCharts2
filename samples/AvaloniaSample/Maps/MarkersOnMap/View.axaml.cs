using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LiveChartsCore.SkiaSharpView.Avalonia;
using ViewModelsSamples.Maps.MarkersOnMap;

namespace AvaloniaSample.Maps.MarkersOnMap;

public partial class View : UserControl
{
    public View()
    {
        AvaloniaXamlLoader.Load(this);

        // VisualElements is a CLR property on GeoMap today (not a DP), so
        // the XAML binding doesn't pick it up. Set it in code-behind from
        // the ViewModel.
        var vm = (ViewModel)DataContext!;
        var geoMap = this.Find<GeoMap>("geoMap")!;
        geoMap.VisualElements = vm.VisualElements;
    }

#if UI_TESTING
    public GeoMap Chart => this.Find<GeoMap>("geoMap")!;
#endif
}
