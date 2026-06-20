using LiveChartsCore.Drawing;
using LiveChartsCore.Geo;
using LiveChartsCore.Kernel;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.VisualElements;
using SkiaSharp;

namespace SnapshotTests;

[TestClass]
public sealed class MapsTests
{
    private static HeatLandSeries[] CreateHeatSeries() =>
    [
        new()
        {
            Lands =
            [
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
                new() { Name = "are", Value = 13 }
            ]
        }
    ];

    [TestMethod]
    public void Basic()
    {
        var chart = new SKGeoMap
        {
            Series = CreateHeatSeries(),
            MapProjection = MapProjection.Mercator,
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(MapsTests)}_{nameof(Basic)}");
    }

    [TestMethod]
    public void OrthographicDefault()
    {
        var chart = new SKGeoMap
        {
            Series = CreateHeatSeries(),
            MapProjection = MapProjection.Orthographic,
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(MapsTests)}_{nameof(OrthographicDefault)}");
    }

    [TestMethod]
    public void OrthographicRotated()
    {
        var chart = new SKGeoMap
        {
            Series = CreateHeatSeries(),
            MapProjection = MapProjection.Orthographic,
            Width = 600,
            Height = 600
        };

        // Center the globe on Europe / Africa
        chart.CoreChart.CenterLongitude = 15;
        chart.CoreChart.CenterLatitude = 20;

        chart.AssertSnapshotMatches($"{nameof(MapsTests)}_{nameof(OrthographicRotated)}");
    }

    // Regression for the orthographic horizon-arc fix in PR #2251.
    //
    // Centered over Central Asia, Russia/China/India straddle the horizon and
    // exercise BuildOrthographicPath's exit/entry handling. Before the fix,
    // path.Close() drew a chord between the last horizon-exit and the first
    // horizon-entry, slicing through the visible disc and producing visible
    // bulges past the rim. After the fix, EmitHorizonArc walks the disc
    // boundary in 3° steps and the silhouette stays clean.
    [TestMethod]
    public void OrthographicHorizonClipsAlongDiscRim()
    {
        var chart = new SKGeoMap
        {
            Series = CreateHeatSeries(),
            MapProjection = MapProjection.Orthographic,
            Width = 600,
            Height = 600
        };

        chart.CoreChart.CenterLongitude = 80;
        chart.CoreChart.CenterLatitude = 20;

        chart.AssertSnapshotMatches($"{nameof(MapsTests)}_{nameof(OrthographicHorizonClipsAlongDiscRim)}");
    }

    // Mirrors the Avalonia World sample: Title above the map + Right-anchored
    // SKHeatLegend. Locks both the title position and the legend gradient
    // rendering against the layout reservation done by GeoMapChart.Measure.
    [TestMethod]
    public void BasicWithTitleAndLegend()
    {
        var chart = new SKGeoMap
        {
            Series = CreateHeatSeries(),
            MapProjection = MapProjection.Mercator,
            Width = 800,
            Height = 600,
            LegendPosition = LegendPosition.Right,
            Legend = new SKHeatLegend(),
            Title = new DrawnLabelVisual(new LabelGeometry
            {
                Text = "World population by country",
                TextSize = 20,
                Padding = new Padding(12),
                Paint = new SolidColorPaint(SKColors.Black),
            }),
        };

        chart.AssertSnapshotMatches($"{nameof(MapsTests)}_{nameof(BasicWithTitleAndLegend)}");
    }

    // The Default projection treats geo coordinates as raw equirectangular
    // (lon -180..180 → 0..w, lat 90..-90 → 0..h) with no Mercator stretching
    // and no latitude clip. Continents render in their unprojected aspect.
    [TestMethod]
    public void DefaultProjection()
    {
        var chart = new SKGeoMap
        {
            Series = CreateHeatSeries(),
            MapProjection = MapProjection.Default,
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(MapsTests)}_{nameof(DefaultProjection)}");
    }

    // Frames the map on Europe via the lat/lon bounds API. Locks that the
    // bounds drive both the projection's scale (Iceland..Caucasus fills the
    // width) and the canvas-clip rect (lands outside the bounds are pixel-
    // clipped instead of bleeding into the canvas padding).
    [TestMethod]
    public void MercatorEuropeView()
    {
        var chart = new SKGeoMap
        {
            Series = CreateHeatSeries(),
            MapProjection = MapProjection.Mercator,
            Width = 600,
            Height = 600,
            // Europe: ~Iceland (lon -25°) east to the Caucasus (+45°),
            // ~North Africa coast (lat 35°) north to North Cape (72°).
            MinLatitude = 35,
            MaxLatitude = 72,
            MinLongitude = -25,
            MaxLongitude = 45,
        };

        chart.AssertSnapshotMatches($"{nameof(MapsTests)}_{nameof(MercatorEuropeView)}");
    }

    // Same Europe bounds applied to Default (equirectangular) projection —
    // proves the lat/lon-bounds API is projection-agnostic on flat
    // projections. Visual differs from MercatorEuropeView because
    // equirectangular doesn't stretch poleward.
    [TestMethod]
    public void DefaultProjectionEuropeView()
    {
        var chart = new SKGeoMap
        {
            Series = CreateHeatSeries(),
            MapProjection = MapProjection.Default,
            Width = 600,
            Height = 600,
            MinLatitude = 35,
            MaxLatitude = 72,
            MinLongitude = -25,
            MaxLongitude = 45,
        };

        chart.AssertSnapshotMatches($"{nameof(MapsTests)}_{nameof(DefaultProjectionEuropeView)}");
    }

    // Five city markers placed via GeoVisualElement wrappers in
    // GeoMap.VisualElements. The wrapper projects (Longitude, Latitude) to
    // pixels each Measure, the inner GeometryVisual draws an orange-red
    // circle centered on the projected point via Translate(-half, -half).
    [TestMethod]
    public void MarkersOnMap()
    {
        var chart = new SKGeoMap
        {
            Series = CreateHeatSeries(),
            MapProjection = MapProjection.Mercator,
            Width = 800,
            Height = 600,
            VisualElements = new IChartElement[]
            {
                CityMarker(-74.00,  40.71),  // New York
                CityMarker(139.69,  35.69),  // Tokyo
                CityMarker( -3.70,  40.42),  // Madrid
                CityMarker(-46.63, -23.55),  // São Paulo
                CityMarker(151.21, -33.87),  // Sydney
            },
        };

        chart.AssertSnapshotMatches($"{nameof(MapsTests)}_{nameof(MarkersOnMap)}");
    }

    private static GeoVisualElement CityMarker(double longitude, double latitude)
    {
        const float size = 14f;
        return new GeoVisualElement(new GeometryVisual<CircleGeometry>
        {
            Width = size,
            Height = size,
            Fill = new SolidColorPaint(new SKColor(255, 87, 51)), // OrangeRed
            Stroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 2 },
            // Inner X/Y from the projector is the top-left of the bbox; shift
            // by -size/2 so the circle is visually centered on the lat/lon.
            Translate = new LvcPoint(-size / 2f, -size / 2f),
        })
        {
            Longitude = longitude,
            Latitude = latitude,
        };
    }

    // Two-frame snapshot pinning the VisualElements lifecycle. Frame 0
    // shows two markers; frame 1 (after setting VisualElements = empty)
    // must NOT show them — this exercises GeoMapChart.Measure's
    // InitializeVisualsCollector → CollectVisuals path that drops elements
    // removed from the collection between frames.
    // Loads the Mexico-states GeoJSON shipped with the CustomMap sample
    // (Natural Earth derivative, CC0) and renders it with a HeatLandSeries
    // keyed by the ISO 3166-2 region code. Pins the full chain:
    // Maps.GetMapFromStreamReader → IGeoMapView.ActiveMap → HeatLandSeries
    // matching by shortName → render.
    [TestMethod]
    public void CustomMap_LoadsAndRendersMexicoStates()
    {
        // Use the same sample asset the runnable sample uses so any
        // regeneration of mexico-states.geojson keeps the snapshot in sync.
        var geojsonPath = Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "samples", "ViewModelsSamples", "Maps", "CustomMap", "mexico-states.geojson");
        geojsonPath = Path.GetFullPath(geojsonPath);

        using var reader = new StreamReader(geojsonPath);
        var mexicoMap = LiveChartsCore.Geo.Maps.GetMapFromStreamReader(reader);

        var chart = new SKGeoMap
        {
            ActiveMap = mexicoMap,
            MapProjection = MapProjection.Mercator,
            Width = 600,
            Height = 600,
            // Auto-fit on Mexico's extent — the default world bounds would
            // letterbox MX into a sliver. Padding a few degrees.
            MinLatitude = 13,
            MaxLatitude = 33,
            MinLongitude = -120,
            MaxLongitude = -85,
            Series =
            [
                new HeatLandSeries
                {
                    Lands =
                    [
                        new HeatLand { Name = "cmx", Value = 92 },
                        new HeatLand { Name = "mex", Value = 70 },
                        new HeatLand { Name = "jal", Value = 84 },
                        new HeatLand { Name = "nle", Value = 58 },
                        new HeatLand { Name = "pue", Value = 65 },
                        new HeatLand { Name = "ver", Value = 80 },
                        new HeatLand { Name = "yuc", Value = 23 },
                        new HeatLand { Name = "bcn", Value = 36 },
                    ],
                },
            ],
        };

        chart.AssertSnapshotMatches($"{nameof(MapsTests)}_{nameof(CustomMap_LoadsAndRendersMexicoStates)}");
    }

    [TestMethod]
    public void MarkersOnMap_RemovedBetweenFrames()
    {
        var chart = new SKGeoMap
        {
            Series = CreateHeatSeries(),
            MapProjection = MapProjection.Mercator,
            Width = 800,
            Height = 600,
            VisualElements = new IChartElement[]
            {
                CityMarker(-74.00,  40.71),  // New York
                CityMarker(139.69,  35.69),  // Tokyo
            },
        };
        // ExplicitDisposing keeps the chart alive between AssertSnapshotMatches
        // calls — simulates a real app mutating VisualElements on the same
        // chart instance (and exercises the CollectVisuals cleanup path on
        // the second Measure).
        chart.ExplicitDisposing = true;

        // Frame 0: both markers present.
        chart.AssertSnapshotMatches($"{nameof(MapsTests)}_{nameof(MarkersOnMap_RemovedBetweenFrames)}_0");

        // Frame 1: removed from the collection — the canvas must drop them.
        chart.VisualElements = [];
        chart.AssertSnapshotMatches($"{nameof(MapsTests)}_{nameof(MarkersOnMap_RemovedBetweenFrames)}_1");
    }
}
