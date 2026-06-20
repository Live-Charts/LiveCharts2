using LiveChartsCore.SkiaSharpView.Maui;

namespace MauiSample.Pies.Basic;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class View : ContentPage
{
    public View()
    {
        InitializeComponent();
    }

#if UI_TESTING
    public PieChart Chart => (PieChart)Content!;
#endif
}
