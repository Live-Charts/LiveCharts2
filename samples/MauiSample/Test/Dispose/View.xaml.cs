namespace MauiSample.Test.Dispose;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class View : ContentPage
{
    private NewPage1 _currentView;

    public View()
    {
        InitializeComponent();
        _currentView = new NewPage1();
        Grid.SetRow(_currentView, 1);
        container.Add(_currentView);
    }

    private void Button_Clicked(object sender, EventArgs e) => _ = ChangeContent();

    public object[] ChangeContent()
    {
        var swappedOut = _currentView;
        _ = container.Remove(_currentView);
        _currentView = new NewPage1();
        Grid.SetRow(_currentView, 1);
        container.Add(_currentView);
        return GetCharts(swappedOut);
    }

    public void ReattachSameInstance()
    {
        _ = container.Remove(_currentView);
        Grid.SetRow(_currentView, 1);
        container.Add(_currentView);
    }

    // https://github.com/Live-Charts/LiveCharts2/issues/2404: take the content out of the tree
    // for a while and put the same instance back (unlike ReattachSameInstance, which does both
    // at once, so the platform sees the charts leave the window before they come back).
    public void RemoveContent() => _ = container.Remove(_currentView);

    public void RestoreContent()
    {
        Grid.SetRow(_currentView, 1);
        container.Add(_currentView);
    }

    public object[] GetCurrentCharts() => GetCharts(_currentView);

    private static object[] GetCharts(NewPage1 page)
    {
        if (page.Content is not Grid grid) return [];
        var charts = new List<object>(grid.Children.Count);
        foreach (var child in grid.Children) charts.Add(child);
        return [.. charts];
    }
}
