using LiveChartsCore.SkiaSharpView.Maui;

namespace MauiSample.Maps.OrthographicWorld;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class View : ContentPage
{
    public View()
    {
        InitializeComponent();
    }

    private void AmericasBtn_Clicked(object sender, EventArgs e) =>
        geoMap.CoreChart.RotateTo(-95, 35);

    private void EuropeBtn_Clicked(object sender, EventArgs e) =>
        geoMap.CoreChart.RotateTo(15, 50);

    private void AsiaBtn_Clicked(object sender, EventArgs e) =>
        geoMap.CoreChart.RotateTo(100, 35);

    private void AfricaBtn_Clicked(object sender, EventArgs e) =>
        geoMap.CoreChart.RotateTo(20, 5);

    private void OceaniaBtn_Clicked(object sender, EventArgs e) =>
        geoMap.CoreChart.RotateTo(135, -25);

#if UI_TESTING
    public GeoMap Chart => geoMap;
#endif
}
