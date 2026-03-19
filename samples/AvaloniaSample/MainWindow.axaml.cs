using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaSample;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
        // disabled due a breaking change from Avalonia 11 to 12,
        // ToDo: add it back when things stabilize to v12.
        //this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
