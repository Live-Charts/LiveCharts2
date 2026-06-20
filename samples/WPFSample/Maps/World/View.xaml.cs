using System.Windows.Controls;
using LiveChartsCore.SkiaSharpView.WPF;

namespace WPFSample.Maps.World;

/// <summary>
/// Interaction logic for View.xaml
/// </summary>
public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
    }

#if UI_TESTING
    public GeoMap Chart => (GeoMap)FindName("geoMap");
#endif
}
