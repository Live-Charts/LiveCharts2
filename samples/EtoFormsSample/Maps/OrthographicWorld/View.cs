using Eto.Forms;
using LiveChartsCore.Geo;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Eto;

namespace EtoFormsSample.Maps.OrthographicWorld;

public class View : Panel
{
    public GeoMap Chart;

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

        var bAmericas = new Button { Text = "Americas" };
        bAmericas.Click += (_, _) => chart.CoreChart.RotateTo(-95, 35);

        var bEurope = new Button { Text = "Europe" };
        bEurope.Click += (_, _) => chart.CoreChart.RotateTo(15, 50);

        var bAsia = new Button { Text = "Asia" };
        bAsia.Click += (_, _) => chart.CoreChart.RotateTo(100, 35);

        var bAfrica = new Button { Text = "Africa" };
        bAfrica.Click += (_, _) => chart.CoreChart.RotateTo(20, 5);

        var bOceania = new Button { Text = "Oceania" };
        bOceania.Click += (_, _) => chart.CoreChart.RotateTo(135, -25);

        var buttons = new StackLayout(bAmericas, bEurope, bAsia, bAfrica, bOceania)
        {
            Orientation = Orientation.Horizontal,
            Padding = 2,
            Spacing = 4
        };

        Content = new DynamicLayout(buttons, chart);
    }
}
