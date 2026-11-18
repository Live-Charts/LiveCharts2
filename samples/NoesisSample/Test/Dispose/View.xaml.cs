using System.Collections.Generic;
using Noesis;

namespace NoesisSample.Test.Dispose;
public partial class View : SampleView
{
    private ContentControl content => (ContentControl)FindName("content");

    public View()
    {
        GUI.LoadComponent(this, "/NoesisSample;component/Test/Dispose/View.xaml");
    }

    private void Button_Click(object sender, Noesis.RoutedEventArgs e) => _ = ChangeContent();

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
