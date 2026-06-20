using System.Threading.Tasks;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.WinUI;
using Microsoft.UI.Xaml.Controls;
using ViewModelsSamples.Pies.AutoUpdate;

namespace WinUISample.Pies.AutoUpdate;

public sealed partial class View : UserControl
{
    private bool? _isStreaming = false;

    public View()
    {
        InitializeComponent();
    }

#if UI_TESTING
    public PieChart Chart => chart;
#endif
}
