using LiveChartsCore;
using LiveChartsCore.Drawing;

namespace ViewModelsSamples.General.TemplatedTooltips;

public class GeometryPoint
{
    public double Value { get; set; }
    public string? Geometry { get; set; }
}

public class ViewModel
{
    public ViewModel()
    {
        // Register a map to convert the GeometryPoint to a coordinate in the chart
        // to learn more about the Mapping property visit:
        // https://livecharts.dev/docs/{{ platform }}/{{ version }}/Overview.Mappers
        LiveCharts.Configure(config =>
            config.HasMap<GeometryPoint>((dataPoint, index) => new(index, dataPoint.Value)));
    }

    public GeometryPoint[] Values { get; set; } = [
        new GeometryPoint { Value = 4, Geometry = SVGPoints.Star },
        new GeometryPoint { Value = 2, Geometry = SVGPoints.Square },
        new GeometryPoint { Value = 6, Geometry = SVGPoints.Circle },
        new GeometryPoint { Value = 3, Geometry = SVGPoints.Cross },
        new GeometryPoint { Value = 5, Geometry = SVGPoints.Diamond }
    ];
}
