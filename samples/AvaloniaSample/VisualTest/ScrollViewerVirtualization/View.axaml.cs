using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LiveChartsCore.SkiaSharpView.Avalonia;

namespace AvaloniaSample.VisualTest.ScrollViewerVirtualization;

public partial class View : UserControl
{
    private readonly ViewModel _vm;

    public View()
    {
        _vm = new ViewModel();
        DataContext = _vm;
        InitializeComponent();
        _ = _vm.LoadDataAsync(1000);
    }

    public CartesianChart Chart1 => this.Find<CartesianChart>("chart1")!;
    public CartesianChart Chart2 => this.Find<CartesianChart>("chart2")!;
    public void OpenTab1() => this.Find<TabControl>("tabs")!.SelectedIndex = 0;
    public void OpenTab2() => this.Find<TabControl>("tabs")!.SelectedIndex = 1;
    public void ScrollToChart() => ((ScrollViewer)this.Find<TabControl>("tabs")!.SelectedContent!).ScrollToEnd();

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
