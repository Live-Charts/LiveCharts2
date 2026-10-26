using Noesis;
using LiveChartsCore.Noesis;

namespace NoesisSample.Maps.OrthographicWorld;
public partial class View : SampleView
{
    private LiveChartsCore.Noesis.GeoMap geoMap => (LiveChartsCore.Noesis.GeoMap)FindName("geoMap");

    public View()
    {
        GUI.LoadComponent(this, "/NoesisSample;component/Maps/OrthographicWorld/View.xaml");
    }

    private void AmericasBtn_Click(object sender, Noesis.RoutedEventArgs e) =>
        geoMap.CoreChart.RotateTo(-95, 35);

    private void EuropeBtn_Click(object sender, Noesis.RoutedEventArgs e) =>
        geoMap.CoreChart.RotateTo(15, 50);

    private void AsiaBtn_Click(object sender, Noesis.RoutedEventArgs e) =>
        geoMap.CoreChart.RotateTo(100, 35);

    private void AfricaBtn_Click(object sender, Noesis.RoutedEventArgs e) =>
        geoMap.CoreChart.RotateTo(20, 5);

    private void OceaniaBtn_Click(object sender, Noesis.RoutedEventArgs e) =>
        geoMap.CoreChart.RotateTo(135, -25);

#if UI_TESTING
    public GeoMap Chart => geoMap;
#endif
}
