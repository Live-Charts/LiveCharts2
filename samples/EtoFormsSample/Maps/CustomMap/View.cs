using Eto.Forms;
using LiveChartsCore.Geo;
using LiveChartsCore.SkiaSharpView.Eto;
using ViewModelsSamples.Maps.CustomMap;

namespace EtoFormsSample.Maps.CustomMap;

public class View : Panel
{
    public GeoMap Chart;

    public View()
    {
        var vm = new ViewModel();

        var chart = new GeoMap
        {
            Series = vm.Series,
            ActiveMap = vm.ActiveMap,
            MapProjection = MapProjection.Mercator,
            MinLatitude = 13,
            MaxLatitude = 33,
            MinLongitude = -120,
            MaxLongitude = -85,
        };

        Content = chart;
        Chart = chart;
    }
}
