using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;
using ViewModelsSamples.General.TemplatedTooltips;

namespace EtoFormsSample.General.TemplatedTooltips;

public class View : Panel
{
    private readonly CartesianChart cartesianChart;

    public View()
    {
        // Register a map to convert the GeometryPoint to a coordinate in the chart
        LiveCharts.Configure(config =>
            config.HasMap<GeometryPoint>((dataPoint, index) => new(index, dataPoint.Value)));

        var values = new GeometryPoint[]
        {
            new() { Value = 4, Geometry = SVGPoints.Star },
            new() { Value = 2, Geometry = SVGPoints.Square },
            new() { Value = 6, Geometry = SVGPoints.Circle },
            new() { Value = 3, Geometry = SVGPoints.Cross },
            new() { Value = 5, Geometry = SVGPoints.Diamond }
        };

        var series = new ISeries[]
        {
            new ColumnSeries<GeometryPoint> { Values = values }
        };

        cartesianChart = new CartesianChart
        {
            Series = series,
            Tooltip = new CustomTooltip()
        };

        Content = cartesianChart;
    }
}
