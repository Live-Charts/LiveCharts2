using LiveChartsCore.SkiaSharpView.Maui;

namespace MauiSample.Sankeys.Basic;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class View : ContentPage
{
    public View()
    {
        InitializeComponent();
    }

#if UI_TESTING
    public SankeyChart Chart => (SankeyChart)Content!;
#endif
}
