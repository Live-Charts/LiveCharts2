using System.Windows.Controls;
using LiveChartsCore.SkiaSharpView.WPF;

namespace WPFSample.Polar.Basic;

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
    public PolarChart Chart => (PolarChart)Content!;
#endif
}
