using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LiveChartsCore.SkiaSharpView.Avalonia;
using ViewModelsSamples.Maps.RotatingOrthographic;

namespace AvaloniaSample.Maps.RotatingOrthographic;

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        var geoMap = this.Find<GeoMap>("geoMap")!;
        var vm = (ViewModel)DataContext!;

        AttachedToVisualTree   += (_, _) => vm.Start((lon, lat) => geoMap.CoreChart.RotateTo(lon, lat));
        DetachedFromVisualTree += (_, _) => vm.Stop();
    }
}
