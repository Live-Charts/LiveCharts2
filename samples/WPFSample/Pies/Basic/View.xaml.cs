using System.Windows.Controls;
using LiveChartsCore.SkiaSharpView.WPF;

namespace WPFSample.Pies.Basic;

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
    public PieChart Chart => (PieChart)Content!;
#endif
}
