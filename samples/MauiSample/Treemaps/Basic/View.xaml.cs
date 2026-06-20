using LiveChartsCore.SkiaSharpView.Maui;

namespace MauiSample.Treemaps.Basic;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class View : ContentPage
{
    public View()
    {
        InitializeComponent();
    }

#if UI_TESTING
    public TreemapChart Chart => (TreemapChart)Content!;
#endif
}
