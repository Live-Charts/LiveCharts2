using System.Windows.Forms;
using LiveChartsCore.Geo;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.WinForms;
using ViewModelsSamples.Maps.MarkersOnMap;

namespace WinFormsSample.Maps.MarkersOnMap;

public partial class View : UserControl
{
    public GeoMap Chart;

    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

        // GeoVisualElement re-projects the (Longitude, Latitude) of the visual
        // on every measure, so the markers follow pan / zoom / rotation.
        static GeoVisualElement CityMarker(double longitude, double latitude) =>
            new(new CityMarkerVisual()) { Longitude = longitude, Latitude = latitude };

        var chart = new GeoMap
        {
            Series =
            [
                new HeatLandSeries
                {
                    Lands =
                    [
                        new HeatLand { Name = "usa", Value = 15 },
                        new HeatLand { Name = "jpn", Value = 14 },
                        new HeatLand { Name = "esp", Value = 10 },
                        new HeatLand { Name = "bra", Value = 12 },
                        new HeatLand { Name = "aus", Value = 11 },
                    ],
                },
            ],
            VisualElements =
            [
                CityMarker(longitude: -74.00, latitude:  40.71), // New York
                CityMarker(longitude: 139.69, latitude:  35.69), // Tokyo
                CityMarker(longitude:  -3.70, latitude:  40.42), // Madrid
                CityMarker(longitude: -46.63, latitude: -23.55), // São Paulo
                CityMarker(longitude: 151.21, latitude: -33.87), // Sydney
            ],
            MapProjection = MapProjection.Mercator,
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(50, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(chart);
        Chart = chart;
    }
}
