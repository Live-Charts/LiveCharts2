using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LiveChartsCore.SkiaSharpView.Avalonia;

namespace AvaloniaSample.General.FirstChart;

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

#if UI_TESTING
    public CartesianChart Chart => (CartesianChart)Content!;
#endif
}
