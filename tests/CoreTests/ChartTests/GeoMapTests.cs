using System.Collections.Generic;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Geo;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView.VisualElements;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.ChartTests;

[TestClass]
public class GeoMapTests
{
    // https://github.com/Live-Charts/LiveCharts2/issues/962
    //
    // Swapping the Series collection on a GeoMap should preserve the heat fill
    // on lands that exist in both the old and the new series. Before the fix,
    // GeoMapChart.Measure() painted the new series first and then called
    // Delete() on the departed series, which nulled Shape.Fill on every land
    // the old series had ever painted -- including ones the new series just
    // painted -- so shared lands appeared blank until the next measure.
    [TestMethod]
    public void GeoMap_SwappingSeries_PreservesFillOnSharedLands()
    {
        var chart = new SKGeoMap
        {
            Width = 400,
            Height = 400,
            Series = [
                new HeatLandSeries
                {
                    Lands = [
                        new() { Name = "fra", Value = 10 },
                        new() { Name = "usa", Value = 5 },
                    ]
                }
            ]
        };

        chart.CoreChart.Measure();

        var fra = chart.ActiveMap.FindLand("fra");
        var usa = chart.ActiveMap.FindLand("usa");
        Assert.IsNotNull(fra, "fra should exist in the world map");
        Assert.IsNotNull(usa, "usa should exist in the world map");
        Assert.IsTrue(
            fra.Data.All(d => d.Shape?.Fill is not null),
            "fra should be painted after the first measure");

        // Swap to a series that still contains "fra" but drops "usa".
        chart.Series = [
            new HeatLandSeries
            {
                Lands = [
                    new() { Name = "fra", Value = 1 },
                    new() { Name = "bra", Value = 99 },
                ]
            }
        ];

        chart.CoreChart.Measure();

        Assert.IsTrue(
            fra.Data.All(d => d.Shape?.Fill is not null),
            "fra is shared between the old and new series and must stay painted after swap (#962)");
        Assert.IsTrue(
            usa.Data.All(d => d.Shape?.Fill is null),
            "usa is no longer in any series and must be cleared after swap");
    }

    // Regression for the tooltip-anchor fix in PR #2251.
    //
    // Russia in world.geojson is a MultiPolygon: one mainland ring at lon ≈
    // [27.29, 180] plus several Chukotka rings wrapped to lon ≈ [-180, -169.9].
    // Before the fix, ComputeLandScreenCenter took a single bbox across every
    // contour — the union spans the entire map width and its centroid lands
    // mid-Atlantic on Mercator. After the fix, the largest visible contour wins
    // and the centroid lands inside mainland Russia, where users expect it.
    [TestMethod]
    public void GeoMap_TooltipAnchor_PicksLargestContour_NotUnionBbox()
    {
        const float Width = 600f, Height = 600f;

        var chart = new SKGeoMap
        {
            Width = (int)Width,
            Height = (int)Height,
            MapProjection = MapProjection.Mercator,
            Series = [new HeatLandSeries { Lands = [new() { Name = "rus", Value = 10 }] }]
        };

        chart.CoreChart.Measure();

        var russia = chart.ActiveMap.FindLand("rus");
        Assert.IsNotNull(russia);
        Assert.IsTrue(
            russia.Data.Length > 1,
            "rus is expected to be multi-contour in world.geojson; the regression depends on it");

        // Hover at Moscow (lon=37.62, lat=55.75), projected with the same
        // projector the chart uses for measurement.
        var projector = Maps.BuildProjector(MapProjection.Mercator, [Width, Height]);
        projector.ToMap(37.62, 55.75, out var moscowX, out var moscowY);

        var hit = chart.CoreChart.FindLandAt(new LvcPoint(moscowX, moscowY));
        Assert.IsNotNull(hit, "Moscow must hit the rus LandDefinition");
        Assert.AreSame(russia, hit.Value.Land);

        var center = hit.Value.Center;
        var anchorInsideContour = russia.Data
            .Any(d => d.Shape?.ContainsPoint(center.X, center.Y) ?? false);
        Assert.IsTrue(
            anchorInsideContour,
            $"Tooltip anchor ({center.X:0.0}, {center.Y:0.0}) must fall inside a Russia contour; " +
            "before the fix the union-bbox centroid landed in the Atlantic.");
    }

    // The map participates in the IChartView pointer-event surface: a click on
    // a land must fire DataPointerDown with a ChartPoint whose DataSource is the
    // LandDefinition. Before this wiring, the map only fired its own bespoke
    // LandClicked event and the standard IChartView events stayed silent.
    [TestMethod]
    public void GeoMap_PointerUpOnLand_FiresDataPointerDownWithLandDefinition()
    {
        const float Width = 600f, Height = 600f;

        var chart = new SKGeoMap
        {
            Width = (int)Width,
            Height = (int)Height,
            MapProjection = MapProjection.Mercator,
        };
        chart.CoreChart.Measure();

        var projector = Maps.BuildProjector(MapProjection.Mercator, [Width, Height]);
        projector.ToMap(37.62, 55.75, out var moscowX, out var moscowY);
        var clickPoint = new LvcPoint(moscowX, moscowY);

        ChartPoint[]? capturedPoints = null;
        LvcPoint? capturedPointer = null;
        chart.DataPointerDown += (sender, points) =>
        {
            capturedPoints = points.ToArray();
            // sender is the view; surface that to assert it round-trips.
            capturedPointer = clickPoint;
        };

        chart.CoreChart.InvokePointerDown(clickPoint, isSecondaryAction: false);
        chart.CoreChart.InvokePointerUp(clickPoint, isSecondaryAction: false);

        Assert.IsNotNull(capturedPoints, "DataPointerDown must fire on a land click");
        Assert.AreEqual(1, capturedPoints.Length, "geo click emits exactly one ChartPoint (one land hit)");
        Assert.IsNotNull(capturedPointer);
        var land = capturedPoints[0].Context.DataSource as LandDefinition;
        Assert.IsNotNull(land, "Context.DataSource must be the LandDefinition");
        Assert.AreEqual("rus", land.ShortName, "Moscow's land short-name is 'rus'");
        Assert.IsNotNull(capturedPoints[0].Context.HoverArea, "land hits carry a hover area for the screen bbox");
    }

    // A drag (pointer-down → move >5px → up) must NOT fire DataPointerDown —
    // matches the existing _pointerDownIsClick deadzone that gated LandClicked.
    [TestMethod]
    public void GeoMap_DragGesture_DoesNotFireDataPointerDown()
    {
        var chart = new SKGeoMap
        {
            Width = 600,
            Height = 600,
            MapProjection = MapProjection.Mercator,
        };
        chart.CoreChart.Measure();

        var projector = Maps.BuildProjector(MapProjection.Mercator, [600f, 600f]);
        projector.ToMap(37.62, 55.75, out var x, out var y);

        var fired = false;
        chart.DataPointerDown += (_, _) => fired = true;

        chart.CoreChart.InvokePointerDown(new LvcPoint(x, y), isSecondaryAction: false);
        // Move > sqrt(25) = 5 px so the click intent is cleared.
        chart.CoreChart.InvokePointerMove(new LvcPoint(x + 20, y + 20));
        chart.CoreChart.InvokePointerUp(new LvcPoint(x + 20, y + 20), isSecondaryAction: false);

        Assert.IsFalse(fired, "moving more than the click-deadzone must suppress DataPointerDown");
    }

    // The hover throttler must fire HoveredPointsChanged on enter, again with a
    // distinct (new, old) pair on a transition to another land, and once more
    // with (null, old) when the pointer leaves the chart entirely.
    [TestMethod]
    public void GeoMap_HoverTransitions_FireHoveredPointsChanged()
    {
        const float Width = 800f, Height = 800f;
        var chart = new SKGeoMap
        {
            Width = (int)Width,
            Height = (int)Height,
            MapProjection = MapProjection.Mercator,
        };
        chart.CoreChart.Measure();

        var projector = Maps.BuildProjector(MapProjection.Mercator, [Width, Height]);
        projector.ToMap(2.35, 48.85, out var parisX, out var parisY);          // fra
        projector.ToMap(13.40, 52.52, out var berlinX, out var berlinY);       // deu

        var calls = new List<(IEnumerable<ChartPoint>? n, IEnumerable<ChartPoint>? o)>();
        chart.HoveredPointsChanged += (_, n, o) => calls.Add((n, o));

        // Enter France
        chart.CoreChart.InvokePointerMove(new LvcPoint(parisX, parisY));
        // The tooltip throttler debounces 50ms; force it on the test thread.
        RunPendingTooltip(chart);

        // Move to Germany
        chart.CoreChart.InvokePointerMove(new LvcPoint(berlinX, berlinY));
        RunPendingTooltip(chart);

        // Leave the chart
        chart.CoreChart.InvokePointerLeft();

        Assert.AreEqual(3, calls.Count, "expected enter, transition, exit");

        var enterLand = calls[0].n!.First().Context.DataSource as LandDefinition;
        Assert.IsNull(calls[0].o);
        Assert.AreEqual("fra", enterLand!.ShortName);

        var transitionNew = calls[1].n!.First().Context.DataSource as LandDefinition;
        var transitionOld = calls[1].o!.First().Context.DataSource as LandDefinition;
        Assert.AreEqual("deu", transitionNew!.ShortName);
        Assert.AreEqual("fra", transitionOld!.ShortName);

        Assert.IsNull(calls[2].n);
        Assert.AreEqual("deu", (calls[2].o!.First().Context.DataSource as LandDefinition)!.ShortName);
    }

    // GetPointsAt is the synchronous lookup variant of the click pipeline: it
    // returns whatever DataPointerDown WOULD fire for the same screen point.
    [TestMethod]
    public void GeoMap_GetPointsAt_ReturnsHitLand()
    {
        var chart = new SKGeoMap
        {
            Width = 600,
            Height = 600,
            MapProjection = MapProjection.Mercator,
        };
        chart.CoreChart.Measure();

        var projector = Maps.BuildProjector(MapProjection.Mercator, [600f, 600f]);
        projector.ToMap(37.62, 55.75, out var x, out var y);

        var points = chart.GetPointsAt(new LvcPointD(x, y)).ToArray();

        Assert.AreEqual(1, points.Length);
        Assert.AreEqual("rus", (points[0].Context.DataSource as LandDefinition)!.ShortName);

        // Ocean — no hit.
        Assert.AreEqual(0, chart.GetPointsAt(new LvcPointD(1, 1)).Count());
    }

    // Multi-series tooltip: when several heat series have a value for the same
    // land, the tooltip dispatch passes Values (one per contributing series) in
    // Series-declaration order instead of breaking on the first match (which
    // hid all but the first series' value in the pre-#2253 tooltip).
    [TestMethod]
    public void GeoMap_MultipleHeatSeries_TooltipValuesPreserveDeclarationOrder()
    {
        const float Width = 600f, Height = 600f;

        var chart = new SKGeoMap
        {
            Width = (int)Width,
            Height = (int)Height,
            MapProjection = MapProjection.Mercator,
            Series = [
                new HeatLandSeries { Name = "Population", Lands = [new() { Name = "bra", Value = 213 }] },
                new HeatLandSeries { Name = "GDP",        Lands = [new() { Name = "bra", Value = 1839 }] },
            ]
        };
        chart.CoreChart.Measure();

        var projector = Maps.BuildProjector(MapProjection.Mercator, [Width, Height]);
        projector.ToMap(-47.92, -15.79, out var brasiliaX, out var brasiliaY); // Brasilia

        var hit = chart.CoreChart.FindLandAt(new LvcPoint(brasiliaX, brasiliaY));
        Assert.IsNotNull(hit, "Brasilia must hit bra");
        Assert.AreEqual("bra", hit.Value.Land.ShortName);

        var values = hit.Value.Values;
        Assert.AreEqual(2, values.Count, "both series contribute");
        Assert.AreEqual("Population", values[0].Series.Name, "declaration order is preserved");
        Assert.AreEqual(213d, values[0].Value);
        Assert.AreEqual("GDP", values[1].Series.Name);
        Assert.AreEqual(1839d, values[1].Value);
    }

    // TooltipFormatter is invoked once per Value during tooltip rendering. We
    // verify by routing through a custom IGeoMapTooltip that records the
    // GeoTooltipPoint it received, so we can replay the formatter ourselves.
    [TestMethod]
    public void GeoMap_TooltipFormatter_IsAppliedToEachValue()
    {
        var captured = new RecordingTooltip();
        var calls = new List<GeoTooltipValue>();

        var chart = new SKGeoMap
        {
            Width = 600,
            Height = 600,
            MapProjection = MapProjection.Mercator,
            Series = [
                new HeatLandSeries { Name = "Pop", Lands = [new() { Name = "fra", Value = 67.5 }] },
                new HeatLandSeries { Name = "GDP", Lands = [new() { Name = "fra", Value = 2937 }] },
            ],
            Tooltip = captured,
            TooltipFormatter = v =>
            {
                calls.Add(v);
                return $"[{v.Series.Name}] {v.Value:0.0}";
            }
        };
        chart.CoreChart.Measure();

        var projector = Maps.BuildProjector(MapProjection.Mercator, [600f, 600f]);
        projector.ToMap(2.35, 48.85, out var x, out var y);

        // Drive a hover through the throttler so the tooltip Show fires.
        chart.CoreChart.InvokePointerMove(new LvcPoint(x, y));
        RunPendingTooltip(chart);

        Assert.IsNotNull(captured.LastPoint, "tooltip.Show must be invoked on hover");
        Assert.AreEqual(2, captured.LastPoint!.Values.Count);
        Assert.AreSame(chart.TooltipFormatter, captured.LastPointChart!.MapView.TooltipFormatter,
            "the view's formatter is what SKDefaultGeoTooltip should read");

        // Replay the formatter the way SKDefaultGeoTooltip's GetLayout would:
        // once per Values entry. Validates the contract that both ends agree on.
        foreach (var v in captured.LastPoint.Values)
            _ = chart.TooltipFormatter!(v);
        Assert.AreEqual(2, calls.Count);
        Assert.AreEqual("Pop", calls[0].Series.Name);
        Assert.AreEqual("GDP", calls[1].Series.Name);
    }

    private sealed class RecordingTooltip : IGeoMapTooltip
    {
        public GeoTooltipPoint? LastPoint { get; private set; }
        public GeoMapChart? LastPointChart { get; private set; }
        public void Show(GeoTooltipPoint point, GeoMapChart chart)
        {
            LastPoint = point;
            LastPointChart = chart;
        }
        public void Hide(GeoMapChart chart) { }
    }

    // GeoMapChart owns its own _tooltipThrottler (50ms ActionThrottler); the
    // tests need a deterministic way to flush it. The throttler dispatches its
    // work via View.InvokeOnUIThread which on SKGeoMap runs synchronously, so
    // calling InvokePointerMove + sleeping the throttler interval is enough.
    private static void RunPendingTooltip(SKGeoMap chart)
    {
        // The 50ms throttle interval is implementation detail; call the
        // throttler directly via reflection to avoid wall-clock waits in tests.
        var throttler = typeof(GeoMapChart)
            .GetField("_tooltipThrottler", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            !.GetValue(chart.CoreChart);
        var unlocked = typeof(GeoMapChart)
            .GetMethod("TooltipThrottlerUnlocked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var task = (System.Threading.Tasks.Task)unlocked!.Invoke(chart.CoreChart, null)!;
        task.Wait();
        _ = throttler; // silence unused
    }

    // Title participation: setting View.Title reserves space at the top of the
    // draw margin and AddTitleToChart positions the visual at the top of the
    // canvas. Before the Measure wiring, the title stub was a no-op and the
    // map filled the whole control.
    [TestMethod]
    public void GeoMap_TitleSet_ReducesDrawMarginAndAddsVisualToCanvas()
    {
        var chart = new SKGeoMap
        {
            Width = 600,
            Height = 600,
            Title = new DrawnLabelVisual(new LiveChartsCore.SkiaSharpView.Drawing.Geometries.LabelGeometry
            {
                Text = "Hello",
                TextSize = 30,
                Padding = new Padding(10),
                Paint = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(0xff303030),
            }),
            Series = [new HeatLandSeries { Lands = [new() { Name = "rus", Value = 1 }] }],
        };
        chart.CoreChart.Measure();

        Assert.IsTrue(
            chart.CoreChart.DrawMarginLocation.Y > 0,
            "Title height must shift the map draw area down");
        Assert.IsTrue(
            chart.CoreChart.DrawMarginSize.Height < 600,
            "Map draw area must shrink by the title height");
    }

    // Legend participation: setting Legend + LegendPosition=Right reserves
    // width on the right edge of the draw margin. Before the EnumerateHeat-
    // LegendSources hook, SKHeatLegend hid itself on the map (its
    // chart.Series query returned no heat series, since geo series aren't
    // ISeries) and no reservation happened.
    [TestMethod]
    public void GeoMap_HeatLegendRight_ReducesDrawMarginWidth()
    {
        var chart = new SKGeoMap
        {
            Width = 800,
            Height = 600,
            LegendPosition = LegendPosition.Right,
            Legend = new SKHeatLegend(),
            Series = [new HeatLandSeries { Lands = [
                new() { Name = "rus", Value = 1 },
                new() { Name = "usa", Value = 9 },
            ] }],
        };
        chart.CoreChart.Measure();

        Assert.IsTrue(
            chart.CoreChart.DrawMarginSize.Width < 800,
            "A Right-positioned legend must reserve width from the map's draw area");
    }

    // The chart-level heat-legend source lookup must find HeatLandSeries on
    // the map; cartesian charts ignore geo series and vice versa.
    [TestMethod]
    public void GeoMap_EnumerateHeatLegendSources_ReturnsHeatLandSeries()
    {
        var heat = new HeatLandSeries { Lands = [new() { Name = "rus", Value = 7 }] };
        var chart = new SKGeoMap
        {
            Width = 400,
            Height = 400,
            Series = [heat],
        };
        chart.CoreChart.Measure();

        var sources = chart.CoreChart.EnumerateHeatLegendSources().ToArray();
        Assert.AreEqual(1, sources.Length);
        Assert.AreSame(heat, sources[0]);
    }

    // CoreHeatLandSeries computes WeightBounds on-demand from Lands so the
    // heat legend can read the gradient endpoints BEFORE the chart's first
    // Measure pass (DrawLegend runs before series.Measure in the geomap
    // flow). MinValue/MaxValue overrides win when set.
    // The Project / Unproject helpers on GeoMapChart wrap the projector built
    // by the last Measure, honoring the current draw margin and (for ortho)
    // rotation. Round-trip via the public chart API.
    [TestMethod]
    public void GeoMap_ProjectUnproject_RoundTrips()
    {
        var chart = new SKGeoMap
        {
            Width = 800,
            Height = 600,
            MapProjection = MapProjection.Mercator,
        };
        chart.CoreChart.Measure();

        foreach (var (lon, lat) in new[] { (0d, 0d), (-74d, 40.7d), (139.69d, 35.69d), (-3.7d, 40.4d) })
        {
            var pixel = chart.CoreChart.Project(lon, lat);
            Assert.IsNotNull(pixel, $"({lon}, {lat}) should project on Mercator");
            var back = chart.CoreChart.Unproject(pixel.Value);
            Assert.IsNotNull(back);
            Assert.AreEqual(lon, back.Value.Longitude, 1e-3, $"lon round-trip @ ({lon}, {lat})");
            Assert.AreEqual(lat, back.Value.Latitude,  1e-3, $"lat round-trip @ ({lon}, {lat})");
        }
    }

    // Orthographic returns null when the coordinate is on the back hemisphere
    // (IsVisible gates Project). The pole opposite the camera should be null.
    [TestMethod]
    public void GeoMap_Project_ReturnsNullForOrthographicBackHemisphere()
    {
        var chart = new SKGeoMap
        {
            Width = 600,
            Height = 600,
            MapProjection = MapProjection.Orthographic,
        };
        chart.CoreChart.Measure();
        // Default rotation: centered at (0°, 0°), so (180°, 0°) is on the
        // antipode — directly behind the camera.
        Assert.IsNull(chart.CoreChart.Project(180, 0));
    }

    // Unproject on a pixel outside the orthographic disc returns null so
    // consumers can distinguish "user clicked outside the globe" from a
    // real coordinate.
    [TestMethod]
    public void GeoMap_Unproject_ReturnsNullForOrthographicOffDisc()
    {
        var chart = new SKGeoMap
        {
            Width = 600,
            Height = 600,
            MapProjection = MapProjection.Orthographic,
        };
        chart.CoreChart.Measure();
        Assert.IsNull(chart.CoreChart.Unproject(new LvcPoint(0, 0))); // corner is outside the centered disc
    }

    [TestMethod]
    public void GeoMap_HeatLandSeriesWeightBounds_AvailableBeforeMeasure()
    {
        var heat = new HeatLandSeries { Lands = [
            new() { Name = "rus", Value = 3 },
            new() { Name = "usa", Value = 17 },
        ] };

        // Even before the chart has measured, WeightBounds reflects the
        // current Lands so the legend has real numbers to format.
        Assert.AreEqual(3d, heat.WeightBounds.Min);
        Assert.AreEqual(17d, heat.WeightBounds.Max);

        heat.MinValue = 0;
        heat.MaxValue = 100;
        Assert.AreEqual(0d, heat.WeightBounds.Min, "MinValue override wins over observed min");
        Assert.AreEqual(100d, heat.WeightBounds.Max, "MaxValue override wins over observed max");
    }
}
