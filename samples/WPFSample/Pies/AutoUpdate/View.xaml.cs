using System.Threading.Tasks;
using System.Windows.Controls;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.WPF;
using ViewModelsSamples.Pies.AutoUpdate;

namespace WPFSample.Pies.AutoUpdate;

/// <summary>
/// Interaction logic for View.xaml
/// </summary>
public partial class View : UserControl
{
    private bool? isStreaming = false;

    public View()
    {
        InitializeComponent();
    }

#if UI_TESTING
    public PieChart Chart => (PieChart)FindName("chart");
#endif
}
