using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using LiveChartsCore.SkiaSharpView.WinForms;
using LiveChartsCore.Geo;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;

namespace WinFormsSample.Maps.World;

public partial class View : UserControl
{
    public GeoMap Chart;

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
            new() { Name= "jpn", Value = 15 },
            new() { Name = "chn", Value = 14 },
            new() { Name = "rus", Value = 11 },
            new() { Name = "fra", Value = 8 },
            new() { Name = "esp", Value = 7 },
            new() { Name = "kor", Value = 10 },
            new() { Name = "zaf", Value = 12 },
            new() { Name = "are", Value = 13 }
        };

        var series = new HeatLandSeries[]
        {
            new() { Lands = lands }
        };

        var chart = new GeoMap
        {
            Series = series,
            MapProjection = MapProjection.Mercator,
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(50, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(chart);
        Chart = chart;

        _ = DoRandomChanges(series[0]);
    }

    private async Task DoRandomChanges(HeatLandSeries series)
    {
        var r = new Random();
        await Task.Delay(1000);
        while (true)
        {
            foreach (var shape in series.Lands ?? Enumerable.Empty<IWeigthedMapLand>())
            {
                shape.Value = r.Next(0, 20);
            }
            await Task.Delay(500);
        }
    }
}
