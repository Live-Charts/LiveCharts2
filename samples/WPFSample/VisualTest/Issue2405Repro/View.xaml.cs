using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;

namespace WPFSample.VisualTest.Issue2405Repro;

public partial class View : UserControl
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
    public void HideBackChart() => backHost.Visibility = Visibility.Collapsed;
    public void ShowBackChart() => backHost.Visibility = Visibility.Visible;
    public void AddBackValue() => _backValues.Add(_backValues.Count + 1);
}
