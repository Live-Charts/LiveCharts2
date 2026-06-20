using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LiveChartsCore.SkiaSharpView.Avalonia;

namespace AvaloniaSample.Treemaps.Basic;

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

#if UI_TESTING
    public TreemapChart Chart => (TreemapChart)Content!;
#endif
}
