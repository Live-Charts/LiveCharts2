using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LiveChartsCore.SkiaSharpView.Avalonia;

namespace AvaloniaSample.Pies.Basic;

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
    public PieChart Chart => (PieChart)Content!;
#endif
}
