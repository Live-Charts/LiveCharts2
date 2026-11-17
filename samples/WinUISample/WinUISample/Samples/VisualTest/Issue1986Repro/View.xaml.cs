using LiveChartsCore.SkiaSharpView.WinUI;
using Microsoft.UI.Xaml.Controls;

namespace WinUISample.VisualTest.Issue1986Repro;

public sealed partial class View : UserControl
{
    private readonly ViewModel _vm;

    public View()
    {
        _vm = new ViewModel();
        DataContext = _vm;
        InitializeComponent();
        _ = _vm.LoadDataAsync(1000);
    }

    public CartesianChart Chart1 => chart1;
    public CartesianChart Chart2 => chart2;
    public void OpenTab1() => tabs.SelectedIndex = 0;
    public void OpenTab2() => tabs.SelectedIndex = 1;

    public void ScrollToChart()
    {
        var scrollViewer = (ScrollViewer)((TabViewItem)tabs.SelectedItem).Content;
        _ = scrollViewer.ChangeView(null, scrollViewer.ScrollableHeight, null, true);
    }
}
