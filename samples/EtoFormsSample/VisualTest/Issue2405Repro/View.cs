using System.Collections.ObjectModel;
using Eto.Forms;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;

namespace EtoFormsSample.VisualTest.Issue2405Repro;

// Reproducer for https://github.com/Live-Charts/LiveCharts2/issues/2405: hide the back chart's
// host panel, add values to the back chart: the front chart should not redraw, and once the
// panel is visible again the back chart should draw all its points.
public class View : Panel
{
    private readonly ObservableCollection<double> _backValues = [1, 3, 2];
    private readonly Panel _backHost;

    public View()
    {
        BackChart = new CartesianChart { Series = [new LineSeries<double> { Values = _backValues }] };
        _backHost = new Panel { Content = BackChart };

        FrontChart = new CartesianChart
        {
            Series = [new LineSeries<double> { Values = new ObservableCollection<double> { 2, 1, 3 } }]
        };

        Content = new TableLayout(
            new TableRow(FrontChart) { ScaleHeight = true },
            new TableRow(_backHost) { ScaleHeight = true });
    }

    public CartesianChart BackChart { get; }
    public CartesianChart FrontChart { get; }
    public void HideBackChart() => _backHost.Visible = false;
    public void ShowBackChart() => _backHost.Visible = true;
    public void AddBackValue() => _backValues.Add(_backValues.Count + 1);
}
