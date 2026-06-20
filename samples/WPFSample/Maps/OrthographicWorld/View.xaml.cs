using System.Windows.Controls;
using LiveChartsCore.SkiaSharpView.WPF;

namespace WPFSample.Maps.OrthographicWorld;

/// <summary>
/// Interaction logic for View.xaml
/// </summary>
public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
    }

    private void AmericasBtn_Click(object sender, System.Windows.RoutedEventArgs e) =>
        geoMap.CoreChart.RotateTo(-95, 35);

    private void EuropeBtn_Click(object sender, System.Windows.RoutedEventArgs e) =>
        geoMap.CoreChart.RotateTo(15, 50);

    private void AsiaBtn_Click(object sender, System.Windows.RoutedEventArgs e) =>
        geoMap.CoreChart.RotateTo(100, 35);

    private void AfricaBtn_Click(object sender, System.Windows.RoutedEventArgs e) =>
        geoMap.CoreChart.RotateTo(20, 5);

    private void OceaniaBtn_Click(object sender, System.Windows.RoutedEventArgs e) =>
        geoMap.CoreChart.RotateTo(135, -25);

#if UI_TESTING
    public GeoMap Chart => geoMap;
#endif
}
