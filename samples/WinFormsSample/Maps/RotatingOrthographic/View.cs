using System.Windows.Forms;
using LiveChartsCore.Geo;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.WinForms;

namespace WinFormsSample.Maps.RotatingOrthographic;

public partial class View : UserControl
{
    public GeoMap Chart;

    // the globe turns to the next point every 5 seconds
    private readonly (double Lon, double Lat)[] _path =
    [
        (-95, 35),   // North America
        (100, 35)    // Asia
    ];
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 5000 };
    private int _pathIndex;

    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

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
            MapProjection = MapProjection.Orthographic,
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(50, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(chart);
        Chart = chart;

        _timer.Tick += (_, _) => RotateToNext();
        Load += (_, _) =>
        {
            RotateToNext();
            _timer.Start();
        };
        HandleDestroyed += (_, _) => _timer.Stop();
    }

    private void RotateToNext()
    {
        var (lon, lat) = _path[_pathIndex];
        Chart.CoreChart.RotateTo(lon, lat);
        _pathIndex = (_pathIndex + 1) % _path.Length;
    }
}
