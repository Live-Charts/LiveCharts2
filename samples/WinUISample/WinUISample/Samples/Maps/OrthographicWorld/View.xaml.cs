using LiveChartsCore.SkiaSharpView.WinUI;
using Microsoft.UI.Xaml.Controls;

namespace WinUISample.Maps.OrthographicWorld;

public sealed partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
    }

    private void AmericasBtn_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) =>
        geoMap.CoreChart.RotateTo(-95, 35);

    private void EuropeBtn_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) =>
        geoMap.CoreChart.RotateTo(15, 50);

    private void AsiaBtn_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) =>
        geoMap.CoreChart.RotateTo(100, 35);

    private void AfricaBtn_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) =>
        geoMap.CoreChart.RotateTo(20, 5);

    private void OceaniaBtn_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) =>
        geoMap.CoreChart.RotateTo(135, -25);

#if UI_TESTING
    public GeoMap Chart => geoMap;
#endif
}
