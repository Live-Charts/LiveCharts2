using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LiveChartsCore.SkiaSharpView.Avalonia;

namespace AvaloniaSample.Bars.AutoUpdate;

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
    public CartesianChart Chart => this.Find<CartesianChart>("chart")!;
#endif
}
