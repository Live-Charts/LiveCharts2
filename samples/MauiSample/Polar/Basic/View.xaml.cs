using LiveChartsCore.SkiaSharpView.Maui;

namespace MauiSample.Polar.Basic;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class View : ContentPage
{
    public View()
    {
        InitializeComponent();
    }

#if UI_TESTING
    public PolarChart Chart => (PolarChart)Content!;
#endif
}
