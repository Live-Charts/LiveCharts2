using LiveChartsCore.SkiaSharpView.WinUI;
using Microsoft.UI.Xaml.Controls;

namespace WinUISample.Polar.Basic;

public sealed partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
    }

#if UI_TESTING
    public PolarChart Chart => (PolarChart)Content!;
#endif
}
