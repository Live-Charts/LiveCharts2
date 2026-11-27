using Eto.Forms;
using LiveChartsCore.Geo;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Eto;

namespace EtoFormsSample.Maps.RotatingOrthographic;

public class View : Panel
{
    public GeoMap Chart;

    // the globe turns to the next point every 5 seconds
    private readonly (double Lon, double Lat)[] _path =
    [
        (-95, 35),   // North America
        (100, 35)    // Asia
    ];
    private readonly UITimer _timer = new() { Interval = 5 };
    private int _pathIndex;

    public View()
    {
        var lands = new HeatLand[]
        {
            new() { Name = "bra", Value = 13 },
            new() { Name = "mex", Value = 10 },
            new() { Name = "usa", Value = 15 },
            new() { Name = "can", Value = 8 },
            new() { Name = "ind", Value = 12 },
            new() { Name = "deu", Value = 13 },
            new() { Name = "jpn", Value = 15 },
            new() { Name = "chn", Value = 14 },
            new() { Name = "rus", Value = 11 },
            new() { Name = "fra", Value = 8 },
            new() { Name = "esp", Value = 7 },
            new() { Name = "kor", Value = 10 },
            new() { Name = "zaf", Value = 12 },
            new() { Name = "are", Value = 13 },
            new() { Name = "aus", Value = 9 },
            new() { Name = "arg", Value = 6 },
            new() { Name = "egy", Value = 7 },
            new() { Name = "nga", Value = 11 },
            new() { Name = "gbr", Value = 9 }
        };

        var chart = new GeoMap
        {
            Series = [new HeatLandSeries { Lands = lands }],
            MapProjection = MapProjection.Orthographic
        };

        Chart = chart;
        Content = chart;

        _timer.Elapsed += (_, _) => RotateToNext();
        Shown += (_, _) =>
        {
            RotateToNext();
            _timer.Start();
        };
        UnLoad += (_, _) => _timer.Stop();
    }

    private void RotateToNext()
    {
        var (lon, lat) = _path[_pathIndex];
        Chart.CoreChart.RotateTo(lon, lat);
        _pathIndex = (_pathIndex + 1) % _path.Length;
    }
}
