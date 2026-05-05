using System.Collections.Generic;
using System.Windows.Controls;

namespace WPFSample.Test.Dispose;

/// <summary>
/// Interaction logic for View.xaml
/// </summary>
public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
    }

    private void Button_Click(object sender, System.Windows.RoutedEventArgs e) => _ = ChangeContent();

    public object[] ChangeContent()
    {
        var swappedOut = (UserControl1)content.Content;
        content.Content = new UserControl1();
        return GetCharts(swappedOut);
    }

    public void ReattachSameInstance()
    {
        var page = (UserControl1)content.Content;
        content.Content = null;
        content.Content = page;
    }

    private static object[] GetCharts(UserControl1 uc)
    {
        if (uc.Content is not Grid grid) return [];
        var charts = new List<object>(grid.Children.Count);
        foreach (var child in grid.Children) charts.Add(child!);
        return [.. charts];
    }
}
