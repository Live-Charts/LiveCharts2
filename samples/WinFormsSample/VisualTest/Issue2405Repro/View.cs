using System.Collections.ObjectModel;
using System.Windows.Forms;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;

namespace WinFormsSample.VisualTest.Issue2405Repro;

// Reproducer for https://github.com/Live-Charts/LiveCharts2/issues/2405: hide the back chart's
// host panel, add values to the back chart: the front chart should not redraw, and once the
// panel is visible again the back chart should draw all its points.
public class View : UserControl
{
    private readonly ObservableCollection<double> _backValues = [1, 3, 2];
    private readonly Panel _backHost;

    public View()
    {
        Size = new System.Drawing.Size(400, 300);

        BackChart = new CartesianChart { Dock = DockStyle.Fill };
        BackChart.Series = [new LineSeries<double> { Values = _backValues }];
        _backHost = new Panel { Dock = DockStyle.Fill };
        _backHost.Controls.Add(BackChart);

        FrontChart = new CartesianChart { Dock = DockStyle.Top, Height = 150 };
        FrontChart.Series = [new LineSeries<double> { Values = new ObservableCollection<double> { 2, 1, 3 } }];

        Controls.Add(_backHost);
        Controls.Add(FrontChart);
    }

    public CartesianChart BackChart { get; }
    public CartesianChart FrontChart { get; }
    public void HideBackChart() => _backHost.Visible = false;
    public void ShowBackChart() => _backHost.Visible = true;
    public void AddBackValue() => _backValues.Add(_backValues.Count + 1);
}
