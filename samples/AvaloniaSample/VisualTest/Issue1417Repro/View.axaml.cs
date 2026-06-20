using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LiveChartsCore.SkiaSharpView.Avalonia;

namespace AvaloniaSample.VisualTest.Issue1417Repro;

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
    }

    public GeoMap Chart => this.Find<GeoMap>("geoMap")!;
    public void OpenTab1() => this.Find<TabControl>("tabs")!.SelectedIndex = 0;
    public void OpenTab2() => this.Find<TabControl>("tabs")!.SelectedIndex = 1;

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
