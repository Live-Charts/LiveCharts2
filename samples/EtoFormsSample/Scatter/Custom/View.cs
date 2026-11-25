using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Eto;
using SkiaSharp;
using LiveChartsCore.Drawing;
using ViewModelsSamples.Scatter.Custom;

namespace EtoFormsSample.Scatter.Custom;

public class View : Panel
{
    public View()
    {
        var viewModel = new ViewModel();
        var pinPath = SVGPoints.Pin;

        var series = new ISeries[]
        {
            new ScatterSeries<ObservablePoint, HeartGeometry>
            {
                Values = viewModel.Values1,
                Stroke = null,
                GeometrySize = 40
            },
            new ScatterSeries<ObservablePoint, VariableSVGPathGeometry>
            {
                Values = viewModel.Values2,
                GeometrySvg = pinPath,
                GeometrySize = 40
            },
            new ScatterSeries<ObservablePoint, MyGeometry>
            {
                Values = viewModel.Values3,
                GeometrySize = 40
            }
        };

        var cartesianChart = new CartesianChart
        {
            Series = series
        };

        Content = cartesianChart;
    }
}
