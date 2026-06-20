using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LiveChartsCore.SkiaSharpView.Avalonia;

namespace AvaloniaSample.Maps.CustomMap;

public partial class View : UserControl
{
    public View()
    {
        AvaloniaXamlLoader.Load(this);
    }

#if UI_TESTING
    public GeoMap Chart => this.Find<GeoMap>("geoMap")!;
#endif
}
