using Eto.Forms;
using LiveChartsCore.Geo;
using LiveChartsCore.SkiaSharpView.Eto;
using ViewModelsSamples.Maps.MarkersOnMap;

namespace EtoFormsSample.Maps.MarkersOnMap;

public class View : Panel
{
    public GeoMap Chart;

    public View()
    {
        var vm = new ViewModel();

        var chart = new GeoMap
        {
            Series = vm.Series,
            VisualElements = vm.VisualElements,
            MapProjection = MapProjection.Mercator,
        };

        Content = chart;
        Chart = chart;
    }
}
