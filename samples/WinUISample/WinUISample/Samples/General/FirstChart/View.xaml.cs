using LiveChartsCore.SkiaSharpView.WinUI;
using Microsoft.UI.Xaml.Controls;

namespace WinUISample.General.FirstChart;

public sealed partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
    }

#if UI_TESTING
    public CartesianChart Chart => (CartesianChart)Content!;
#endif
}
