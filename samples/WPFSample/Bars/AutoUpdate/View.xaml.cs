using System.Threading.Tasks;
using System.Windows.Controls;
using LiveChartsCore.SkiaSharpView.WPF;
using ViewModelsSamples.Bars.AutoUpdate;

namespace WPFSample.Bars.AutoUpdate;

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
    public CartesianChart Chart => chart;
#endif
}
