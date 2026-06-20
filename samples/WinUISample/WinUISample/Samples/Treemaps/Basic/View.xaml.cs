using LiveChartsCore.SkiaSharpView.WinUI;
using Microsoft.UI.Xaml.Controls;

namespace WinUISample.Treemaps.Basic;

public sealed partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
    }

#if UI_TESTING
    public TreemapChart Chart => (TreemapChart)Content!;
#endif
}
