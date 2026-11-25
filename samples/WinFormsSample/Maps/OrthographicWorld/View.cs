using System.Windows.Forms;
using LiveChartsCore.Geo;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.WinForms;

namespace WinFormsSample.Maps.OrthographicWorld;

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
            Location = new System.Drawing.Point(0, 40),
            Size = new System.Drawing.Size(50, 10),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(chart);
        Chart = chart;

        int x = 0;
        var bAmericas = new Button { Text = "Americas", Location = new System.Drawing.Point(x, 0), AutoSize = true };
        bAmericas.Click += (_, _) => Chart.CoreChart.RotateTo(-95, 35);
        Controls.Add(bAmericas);
        x += bAmericas.Width + 4;

        var bEurope = new Button { Text = "Europe", Location = new System.Drawing.Point(x, 0), AutoSize = true };
        bEurope.Click += (_, _) => Chart.CoreChart.RotateTo(15, 50);
        Controls.Add(bEurope);
        x += bEurope.Width + 4;

        var bAsia = new Button { Text = "Asia", Location = new System.Drawing.Point(x, 0), AutoSize = true };
        bAsia.Click += (_, _) => Chart.CoreChart.RotateTo(100, 35);
        Controls.Add(bAsia);
        x += bAsia.Width + 4;

        var bAfrica = new Button { Text = "Africa", Location = new System.Drawing.Point(x, 0), AutoSize = true };
        bAfrica.Click += (_, _) => Chart.CoreChart.RotateTo(20, 5);
        Controls.Add(bAfrica);
        x += bAfrica.Width + 4;

        var bOceania = new Button { Text = "Oceania", Location = new System.Drawing.Point(x, 0), AutoSize = true };
        bOceania.Click += (_, _) => Chart.CoreChart.RotateTo(135, -25);
        Controls.Add(bOceania);
    }
}
