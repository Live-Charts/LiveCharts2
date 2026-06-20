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

    private static object[] GetCharts(NewPage1 page)
    {
        if (page.Content is not Grid grid) return [];
        var charts = new List<object>(grid.Children.Count);
        foreach (var child in grid.Children) charts.Add(child);
        return [.. charts];
    }
}
