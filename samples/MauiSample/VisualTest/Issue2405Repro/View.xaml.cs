using System.Collections.ObjectModel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Maui;

namespace MauiSample.VisualTest.Issue2405Repro;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class View : ContentPage
{
    private readonly ObservableCollection<double> _backValues = [1, 3, 2];

    public View()
    {
        InitializeComponent();
        BackChart.Series = [new LineSeries<double> { Values = _backValues }];
        FrontChart.Series = [new LineSeries<double> { Values = new ObservableCollection<double> { 2, 1, 3 } }];
    }

    public CartesianChart BackChart => backChart;
    public CartesianChart FrontChart => frontChart;
    public void HideBackChart() => backHost.IsVisible = false;
    public void ShowBackChart() => backHost.IsVisible = true;
    public void AddBackValue() => _backValues.Add(_backValues.Count + 1);
}
