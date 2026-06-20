using LiveChartsCore.SkiaSharpView.Maui;

namespace MauiSample.General.FirstChart;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class View : ContentPage
{
    public View()
    {
        InitializeComponent();
    }

#if UI_TESTING
    public CartesianChart Chart => (CartesianChart)Content!;
#endif
}
