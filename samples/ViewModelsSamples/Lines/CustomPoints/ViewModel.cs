using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using SkiaSharp;

namespace ViewModelsSamples.Lines.CustomPoints;

public partial class ViewModel : ObservableObject
{
    public ViewModel()
    {
        LiveCharts.Configure(config =>
        {
            // The map is used to map the data points to the series
            // to learn more about mappers visit:
            // https://livecharts.dev/docs/{{ platform }}/{{ version }}/Overview.Mappers

            config.HasMap<DataPoint>(
                (dataPoint, index) => new(index, dataPoint.Value));
        });
    }

    public DataPoint[] Values { get; set; } =
    [
        new DataPoint { Value = 4, Rotation = 0 },
        new DataPoint { Value = 6, Rotation = 20 },
        new DataPoint { Value = 8, Rotation = 90 },
        new DataPoint { Value = 2, Rotation = 176 },
        new DataPoint { Value = 7, Rotation = 55 },
        new DataPoint { Value = 9, Rotation = 226 },
        new DataPoint { Value = 3, Rotation = 320 }
    ];

    // The pnt measured command/event is triggered when a point size
    // and position is calculated, for this example we use a command,
    // but you could also subscribe to the series PointMeasured event.

    [RelayCommand]
    public void OnPointMeasured(ChartPoint point)
    {
        var dataPoint = (DataPoint)point.Context.DataSource!;
        point.Context.Visual!.RotateTransform = dataPoint.Rotation;
    }

    public DataPoint[] Fetch()
    {
        return [
            new DataPoint { Value = 4, Rotation = 0 },
            new DataPoint { Value = 6, Rotation = 20 },
            new DataPoint { Value = 8, Rotation = 90 },
            new DataPoint { Value = 2, Rotation = 176 },
            new DataPoint { Value = 7, Rotation = 55 },
            new DataPoint { Value = 9, Rotation = 226 },
            new DataPoint { Value = 3, Rotation = 320 }
        ];
    }
}

public class DataPoint
{
    public int Value { get; set; }
    public float Rotation { get; set; }
}


public class ArrowGeometry : BaseSVGPathGeometry
{
    // a circle with an arrow in it, pointing up; the series fills and strokes it like the
    // points of any line, so it covers the line, and OnPointMeasured turns it to the wind

    private static SKPath _path = SKPath.ParseSvgPathData(
        "M1 16 A15 15 0 1 0 31 16 A15 15 0 1 0 1 16 Z " + // the circle
        "M16 25 L16 7 M9.5 13.5 L16 7 L22.5 13.5");        // the arrow, pointing up

    public ArrowGeometry()
        : base(_path)
    { }
}
