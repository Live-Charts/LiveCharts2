using Eto.Forms;
using LiveChartsCore.Geo;
using LiveChartsCore.SkiaSharpView.Eto;
using ViewModelsSamples.Maps.OrthographicWorld;

namespace EtoFormsSample.Maps.OrthographicWorld;

public class View : Panel
{
    public GeoMap Chart;

    public View()
    {
        var vm = new ViewModel();

        var chart = new GeoMap
        {
            Series = vm.Series,
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
