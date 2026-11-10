using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;

namespace AvaloniaSample.VisualTest.Issue2405Repro;

public partial class View : UserControl
{
    private readonly ObservableCollection<double> _backValues = [1, 3, 2];

    public View()
    {
        InitializeComponent();
        BackChart.Series = [new LineSeries<double> { Values = _backValues }];
        FrontChart.Series = [new LineSeries<double> { Values = new ObservableCollection<double> { 2, 1, 3 } }];
    }

    public CartesianChart BackChart => this.Find<CartesianChart>("backChart")!;
    public CartesianChart FrontChart => this.Find<CartesianChart>("frontChart")!;
    public void HideBackChart() => this.Find<Border>("backHost")!.IsVisible = false;
    public void ShowBackChart() => this.Find<Border>("backHost")!.IsVisible = true;
    public void AddBackValue() => _backValues.Add(_backValues.Count + 1);

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
