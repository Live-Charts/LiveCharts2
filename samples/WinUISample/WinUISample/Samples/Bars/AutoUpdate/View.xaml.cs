using LiveChartsCore.SkiaSharpView.WinUI;
using Microsoft.UI.Xaml.Controls;

namespace WinUISample.Bars.AutoUpdate;

public sealed partial class View : UserControl
{
    private bool? _isStreaming = false;

    public View()
    {
        InitializeComponent();
    }

#if UI_TESTING
    public CartesianChart Chart => chart;
#endif
}
