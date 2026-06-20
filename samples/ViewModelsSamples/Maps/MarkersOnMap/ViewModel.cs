using LiveChartsCore.Drawing;
using LiveChartsCore.Geo;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.VisualElements;
using SkiaSharp;

namespace ViewModelsSamples.Maps.MarkersOnMap;

public class ViewModel
{
    public ViewModel()
    {
        // A handful of cities scattered across hemispheres so the markers
        // visibly track pan / zoom / orthographic rotation.
        VisualElements =
        [
            CityMarker(longitude: -74.00, latitude:  40.71), // New York
            CityMarker(longitude: 139.69, latitude:  35.69), // Tokyo
            CityMarker(longitude:  -3.70, latitude:  40.42), // Madrid
            CityMarker(longitude: -46.63, latitude: -23.55), // São Paulo
            CityMarker(longitude: 151.21, latitude: -33.87), // Sydney
        ];
    }

    public HeatLandSeries[] Series { get; } =
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
    ];

    public IChartElement[] VisualElements { get; }

    private static GeoVisualElement CityMarker(double longitude, double latitude)
    {
        // GeoVisualElement wraps any VisualElement and re-projects its
        // (Longitude, Latitude) to screen pixels each Measure pass. The
        // inner visual then draws at that pixel — pan / zoom / orthographic
        // rotation all follow automatically.
        //
        // The inner Translate centers the bbox on the projected point;
        // without it the circle's top-left corner would sit on the coord.
        const float size = 14f;
        return new GeoVisualElement(new GeometryVisual<CircleGeometry>
        {
            Width = size,
            Height = size,
            Fill = new SolidColorPaint(new SKColor(255, 87, 51)), // OrangeRed
            Stroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 2 },
            Translate = new LvcPoint(-size / 2f, -size / 2f),
        })
        {
            Longitude = longitude,
            Latitude = latitude,
        };
    }
}
