using Microsoft.UI.Xaml.Controls;

namespace WinUISample.Test.NullContext;

public sealed partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
    }

    public void SetNullContext() =>
        DataContext = null;
}
