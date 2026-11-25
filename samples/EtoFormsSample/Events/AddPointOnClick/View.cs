using System.Collections.ObjectModel;
using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;
using ViewModelsSamples.Events.AddPointOnClick;

namespace EtoFormsSample.Events.AddPointOnClick;

public class View : Panel
{
    private readonly ObservableCollection<ObservablePoint> _data;

    public View()
    {
        var viewModel = new ViewModel();
        _data = viewModel.Points;

        var series = new ISeries[]
        {
            new LineSeries<ObservablePoint>
            {
                Values = _data,
                Fill = null,
                DataPadding = new LiveChartsCore.Drawing.LvcPoint(5, 5)
            }
        };

        var cartesianChart = new CartesianChart
        {
            Series = series,
            TooltipPosition = LiveChartsCore.Measure.TooltipPosition.Hidden
        };

        cartesianChart.MouseDown += CartesianChart_Click;

        Content = cartesianChart;
    }

    private void CartesianChart_Click(object sender, MouseEventArgs e)
    {
        var chart = (CartesianChart)sender;
        var p = new LiveChartsCore.Drawing.LvcPointD(e.Location.X, e.Location.Y);
        var dataCoordinates = chart.ScalePixelsToData(p);
        _data.Add(new ObservablePoint(dataCoordinates.X, dataCoordinates.Y));
    }
}
