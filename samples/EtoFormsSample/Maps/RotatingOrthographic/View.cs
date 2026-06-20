using Eto.Forms;
using LiveChartsCore.Geo;
using LiveChartsCore.SkiaSharpView.Eto;
using ViewModelsSamples.Maps.RotatingOrthographic;

namespace EtoFormsSample.Maps.RotatingOrthographic;

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
        Content = chart;

        Shown    += (_, _) => vm.Start((lon, lat) => Application.Instance.AsyncInvoke(() => chart.CoreChart.RotateTo(lon, lat)));
        UnLoad   += (_, _) => vm.Stop();
    }
}
