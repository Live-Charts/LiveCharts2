using LiveChartsCore.SkiaSharpView.WinUI;
using Microsoft.UI.Xaml.Controls;

namespace WinUISample.Maps.World;

public sealed partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
    }

#if UI_TESTING
    public GeoMap Chart => geoMap;
#endif
}
