using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LiveChartsCore.SkiaSharpView.Avalonia;

namespace AvaloniaSample.Maps.OrthographicWorld;

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

        this.Find<Button>("AmericasBtn")!.Click += (_, _) =>
            geoMap.CoreChart.RotateTo(-95, 35);

        this.Find<Button>("EuropeBtn")!.Click += (_, _) =>
            geoMap.CoreChart.RotateTo(15, 50);

        this.Find<Button>("AsiaBtn")!.Click += (_, _) =>
            geoMap.CoreChart.RotateTo(100, 35);

        this.Find<Button>("AfricaBtn")!.Click += (_, _) =>
            geoMap.CoreChart.RotateTo(20, 5);

        this.Find<Button>("OceaniaBtn")!.Click += (_, _) =>
            geoMap.CoreChart.RotateTo(135, -25);
    }
}
