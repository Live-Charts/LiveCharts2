using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LiveChartsCore.SkiaSharpView.Avalonia;
using ViewModelsSamples.Pies.AutoUpdate;

namespace AvaloniaSample.Pies.AutoUpdate;

public partial class View : UserControl
{
    private bool? isStreaming = false;

    public View()
    {
        InitializeComponent();
    }

    private async void ButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var vm = (ViewModel?)DataContext;
        if (vm is null) return;

        isStreaming = isStreaming is null ? true : !isStreaming;

        while (isStreaming.Value)
        {
            vm.RemoveSeries();
            vm.AddSeries();
            await Task.Delay(1000);
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

#if UI_TESTING
    public PieChart Chart => this.Find<PieChart>("chart")!;
#endif
}
