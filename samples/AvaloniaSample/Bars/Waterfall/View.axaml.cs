using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaSample.Bars.Waterfall;

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
}
