using System.Windows.Controls;

namespace WPFSample.Test.NullContext;

/// <summary>
/// Interaction logic for View.xaml
/// </summary>
public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
    }

    public void SetNullContext() =>
        DataContext = null;
}
