using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;

namespace WinUISample.Test.Dispose;

public sealed partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
    }

    private void Button_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) => _ = ChangeContent();

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

    private UserControl1? _removed;

    // https://github.com/Live-Charts/LiveCharts2/issues/2404: take the content out of the tree
    // for a while and put the same instance back (unlike ReattachSameInstance, which does both
    // at once, so the platform sees the charts leave the window before they come back).
    public void RemoveContent()
    {
        _removed = (UserControl1)content.Content;
        content.Content = null;
    }

    public void RestoreContent() => content.Content = _removed;

    public object[] GetCurrentCharts() => GetCharts((UserControl1)content.Content);

    private static object[] GetCharts(UserControl1 uc)
    {
        if (uc.Content is not Grid grid) return [];
        var charts = new List<object>(grid.Children.Count);
        foreach (var child in grid.Children) charts.Add(child);
        return [.. charts];
    }
}
