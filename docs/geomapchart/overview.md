<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
-->

# The GeoMap Chart

The `GeoMap` control is useful to create geographical maps, it uses files in [geojson](https://en.wikipedia.org/wiki/GeoJSON) format to render
vectorized maps.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/geomaphs.png)

:::info
**A note on disputed boundaries.** Maps reflect the borders encoded in
the underlying GeoJSON file — there are real disagreements about
Crimea, Taiwan, Western Sahara, Kashmir, and other regions, and no
single GeoJSON satisfies every audience. LiveCharts2 doesn't take a
position; it just renders the GeoJSON you give it. If the borders
shown don't match what you need, load a different GeoJSON — see the
[CustomMap sample]({{ website_url }}/docs/{{ platform }}/{{ version }}/samples.maps.customMap)
for the loader recipe.
:::

{{~ if xaml ~}}
<pre><code>using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;

namespace ViewModelsSamples.Maps.World
{
    public class ViewModel
    {
        public HeatLandSeries[] Series { get; set; }
            = new HeatLandSeries[]
            {
                new HeatLandSeries
                {
                    // every country has a unique identifier
                    // check the "shortName" property in the following
                    // json file to assign a value to a country in the heat map
                    // https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/_assets/word-map-index.json
                    Lands = new HeatLand[]
                    {
                        new HeatLand { Name = "bra", Value = 13 },
                        new HeatLand { Name = "mex", Value = 10 },
                        new HeatLand { Name = "usa", Value = 15 },
                        new HeatLand { Name = "deu", Value = 13 },
                        new HeatLand { Name = "fra", Value = 8 },
                        new HeatLand { Name = "kor", Value = 10 },
                        new HeatLand { Name = "zaf", Value = 12 },
                        new HeatLand { Name = "are", Value = 13 }
                    }
                }
            };
    }
}</code></pre>

<pre><code>&lt;lvc:GeoMap Series="{Binding Series}">&lt;/lvc:GeoMap></code></pre>
{{~ end ~}}

{{~ if blazor ~}}
<pre><code>@using LiveChartsCore.SkiaSharpView;
@using LiveChartsCore.SkiaSharpView.Drawing.Geometries;

&lt;GeoMap Series="series">&lt;/GeoMap>

@code {
    private HeatLandSeries[] series = new HeatLandSeries[]
    {
        new HeatLandSeries
        {
            // every country has a unique identifier
            // check the "shortName" property in the following
            // json file to assign a value to a country in the heat map
            // https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/_assets/word-map-index.json
            Lands = new HeatLand[]
            {
                new HeatLand { Name = "bra", Value = 13 },
                new HeatLand { Name = "mex", Value = 10 },
                new HeatLand { Name = "usa", Value = 15 },
                new HeatLand { Name = "deu", Value = 13 },
                new HeatLand { Name = "fra", Value = 8 },
                new HeatLand { Name = "kor", Value = 10 },
                new HeatLand { Name = "zaf", Value = 12 },
                new HeatLand { Name = "are", Value = 13 }
            }
        }
    };
}</code></pre>
{{~ end ~}}

{{~ if winforms ~}}
<pre><code>geoMap1.Series = new HeatLandSeries[]
{
    new HeatLandSeries
    {
        // every country has a unique identifier
        // check the "shortName" property in the following
        // json file to assign a value to a country in the heat map
        // https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/_assets/word-map-index.json
        Lands = new HeatLand[]
        {
            new HeatLand { Name = "bra", Value = 13 },
            new HeatLand { Name = "mex", Value = 10 },
            new HeatLand { Name = "usa", Value = 15 },
            new HeatLand { Name = "deu", Value = 13 },
            new HeatLand { Name = "fra", Value = 8 },
            new HeatLand { Name = "kor", Value = 10 },
            new HeatLand { Name = "zaf", Value = 12 },
            new HeatLand { Name = "are", Value = 13 }
        }
    }
};</code></pre>
{{~ end ~}}

## Stroke property

Paints the outline of every land. When `null` (the default) no outline is
drawn — the heat fill alone defines each land's silhouette.

{{~ if xaml ~}}
<pre><code>using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;

namespace ViewModelsSamples.Maps.World
{
    public class ViewModel
    {
        public HeatLandSeries[] Series { get; set; }
            = new HeatLandSeries[]
            {
                new HeatLandSeries { Lands = new HeatLand[] { ... } }
            };

        public SolidColorPaint Stroke { get; set; } 
            = new SolidColorPaint(SKColors.Red) { StrokeThickness = 2 }; // mark
    }
}</code></pre>

<pre><code>&lt;lvc:GeoMap
    Series="{Binding Series}"
    Stroke="{Binding Stroke}">&lt;!-- mark -->
&lt;/lvc:GeoMap></code></pre>
{{~ end ~}}

{{~ if blazor ~}}
<pre><code>@using LiveChartsCore.SkiaSharpView;
@using LiveChartsCore.SkiaSharpView.Drawing.Geometries;

&lt;GeoMap
    Series="series"
    Stroke="stroke">&lt;!-- mark -->
&lt;/GeoMap>

@code {
    private HeatLandSeries[] series = new HeatLandSeries[]
    {
        new HeatLandSeries { Lands = new HeatLand[] { ... } }
    };

    private SolidColorPaint stroke = new SolidColorPaint(SKColors.Red) { StrokeThickness = 2 }; // mark
}</code></pre>
{{~ end ~}}

{{~ if winforms ~}}
<pre><code>geoMap1.Series = new HeatLandSeries[]
{
    new HeatLandSeries
    {
        Lands = new HeatLand[] { ... }
    }
};
geoMap1.Stroke = new SolidColorPaint(SKColors.Red) { StrokeThickness = 2 }; // mark</code></pre>
{{~ end ~}}

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/geomap-stroke.png)

:::info
Paints can create gradients, dashed lines and more, if you need help using the `Paint` instances take 
a look at the [Paints article]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Paints).
:::

## Fill property

Paints lands that have **no value** in any series — the "background" lands
on the map. Lands that participate in a heat series keep their interpolated
heat color regardless of `Fill`. When `null` (the default) unmapped lands
stay transparent.

{{~ if xaml ~}}
<pre><code>using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;

namespace ViewModelsSamples.Maps.World
{
    public class ViewModel
    {
        public HeatLandSeries[] Series { get; set; }
            = new HeatLandSeries[]
            {
                new HeatLandSeries { Lands = new HeatLand[] { ... } }
            };

        public SolidColorPaint Fill { get; set; } = new SolidColorPaint(SKColors.LightPink); // mark
    }
}</code></pre>

<pre><code>&lt;lvc:GeoMap
    Series="{Binding Series}"
    Fill="{Binding Fill}">&lt;!-- mark -->
&lt;/lvc:GeoMap></code></pre>
{{~ end ~}}

{{~ if blazor ~}}
<pre><code>@using LiveChartsCore.SkiaSharpView;
@using LiveChartsCore.SkiaSharpView.Drawing.Geometries;

&lt;GeoMap
    Series="series"
    Fill="fill">&lt;!-- mark -->
&lt;/GeoMap>

@code {
    private HeatLandSeries[] series = new HeatLandSeries[]
    {
        new HeatLandSeries { Lands = new HeatLand[] { ... } }
    };

    private SolidColorPaint fill = new SolidColorPaint(SKColors.LightPink); // mark
}</code></pre>
{{~ end ~}}

{{~ if winforms ~}}
<pre><code>geoMap1.Series = new HeatLandSeries[]
{
    new HeatLandSeries
    {
        Lands = new HeatLand[] { ... }
    }
};
geoMap1.Fill = new SolidColorPaint(SKColors.LightPink); // mark</code></pre>
{{~ end ~}}

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/geomap-fill.png)

:::info
Paints can create gradients, dashed lines and more, if you need help using the `Paint` instances take 
a look at the [Paints article]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Paints).
:::

## Title property

A `Title` is a `VisualElement` rendered above the map. The same
`DrawnLabelVisual` used by cartesian and pie charts works here — set
`Text`, `TextSize`, `Padding`, and the `Paint` that draws it. The map
shrinks vertically to make room for the title.

{{~ if xaml ~}}
<pre><code>&lt;lvc:GeoMap Series="{Binding Series}"&gt;
    &lt;lvc:GeoMap.Title&gt;&lt;!-- mark -->
        &lt;lvc:XamlDrawnLabelVisual
            Text="World population by country"
            Paint="{lvc:SolidColorPaint Color='#303030'}"
            TextSize="20"
            Padding="{lvc:Padding '12'}"/&gt;
    &lt;/lvc:GeoMap.Title&gt;
&lt;/lvc:GeoMap></code></pre>
{{~ end ~}}

{{~ if blazor ~}}
<pre><code>&lt;GeoMap Series="series" Title="title"&gt;&lt;/GeoMap&gt;

@code {
    private DrawnLabelVisual title = new DrawnLabelVisual(
        new LabelGeometry
        {
            Text = "World population by country",
            TextSize = 20,
            Padding = new Padding(12),
            Paint = new SolidColorPaint(SKColors.Black)
        });
}</code></pre>
{{~ end ~}}

{{~ if winforms ~}}
<pre><code>geoMap1.Title = new DrawnLabelVisual(
    new LabelGeometry
    {
        Text = "World population by country",
        TextSize = 20,
        Padding = new Padding(12),
        Paint = new SolidColorPaint(SKColors.Black)
    });</code></pre>
{{~ end ~}}

## Legend property

Heat maps benefit from a gradient legend so the reader can map colors
back to values. Set `LegendPosition` and assign an `SKHeatLegend` —
the legend reads `HeatMap`, `ColorStops`, and the per-series
`WeightBounds` (min/max value across the data) to render the gradient
bar and its end labels.

| LegendPosition | Effect                                           |
| -------------- | ------------------------------------------------ |
| `Hidden`       | Default — no legend.                             |
| `Left`         | Vertical gradient bar pinned to the left.        |
| `Right`        | Vertical gradient bar pinned to the right.       |
| `Top`          | Horizontal gradient bar pinned to the top.      |
| `Bottom`       | Horizontal gradient bar pinned to the bottom.    |

{{~ if xaml ~}}
<pre><code>&lt;lvc:GeoMap
    Series="{Binding Series}"
    LegendPosition="Right"&gt;&lt;!-- mark -->
    &lt;lvc:GeoMap.Legend&gt;
        &lt;draw:SKHeatLegend BadgePadding="{lvc:Padding '20, 16, 8, 16'}"/&gt;&lt;!-- mark -->
    &lt;/lvc:GeoMap.Legend&gt;
&lt;/lvc:GeoMap></code></pre>

Make sure to declare the `draw` namespace on the root element:

<pre><code>xmlns:draw="using:LiveChartsCore.SkiaSharpView.SKCharts"</code></pre>
{{~ end ~}}

{{~ if blazor ~}}
<pre><code>&lt;GeoMap
    Series="series"
    LegendPosition="LiveChartsCore.Measure.LegendPosition.Right"
    Legend="legend"&gt;&lt;!-- mark -->
&lt;/GeoMap>

@code {
    private SKHeatLegend legend = new();
}</code></pre>
{{~ end ~}}

{{~ if winforms ~}}
<pre><code>geoMap1.LegendPosition = LiveChartsCore.Measure.LegendPosition.Right;
geoMap1.Legend = new SKHeatLegend(); // mark</code></pre>
{{~ end ~}}

:::info
Override the gradient endpoints (e.g. to pin the legend to 0–100 even
when the data only spans 5–87) by setting `MinValue` and `MaxValue`
on the `HeatLandSeries`. The map's heat ramp uses the same bounds, so
the rendered colors and the legend stay in sync.
:::

## MapProjection property

Defines the [projection](https://en.wikipedia.org/wiki/Map_projection) of the
map coordinates in the control coordinates. Three projections are available:

| Value          | Use case                                                            |
| -------------- | ------------------------------------------------------------------- |
| `Default`      | No projection — raw control-coordinate plot. Useful for non-geographic maps. |
| `Mercator`     | Flat world map; preserves angles, exaggerates polar areas.          |
| `Orthographic` | 3D globe view — only one hemisphere visible at a time, rotate to look at the other side. |

### Mercator

{{~ if xaml ~}}
<pre><code>&lt;lvc:GeoMap
    Series="{Binding Series}"
    MapProjection="Mercator"&gt;&lt;!-- mark -->
&lt;/lvc:GeoMap></code></pre>
{{~ end ~}}

{{~ if blazor ~}}
<pre><code>&lt;GeoMap
    Series="series"
    MapProjection="LiveChartsCore.Geo.MapProjection.Mercator"&gt;&lt;!-- mark -->
&lt;/GeoMap></code></pre>
{{~ end ~}}

{{~ if winforms ~}}
<pre><code>geoMap1.MapProjection = LiveChartsCore.Geo.MapProjection.Mercator;</code></pre>
{{~ end ~}}

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/geomap-mercator.png)

By default the Mercator projection is clipped to latitudes -65° (south)
to +85° (north) — drops the sub-Antarctic empty band while keeping
Greenland fully visible. Each edge is configurable via `MinLatitude`,
`MaxLatitude`, `MinLongitude`, and `MaxLongitude` on the chart — leave a
value as `double.NaN` (the default) to keep the projection's natural
default.

To render the classic full-earth Mercator including Antarctica, extend
the bottom edge with `MinLatitude = -85` (the top is already at +85):

{{~ if xaml ~}}
<pre><code>&lt;lvc:GeoMap
    Series="{Binding Series}"
    MapProjection="Mercator"
    MinLatitude="-85"
    MaxLatitude="85"/&gt;&lt;!-- mark, full earth --></code></pre>
{{~ end ~}}

{{~ if blazor ~}}
<pre><code>&lt;GeoMap
    Series="series"
    MapProjection="LiveChartsCore.Geo.MapProjection.Mercator"
    MinLatitude="-85"
    MaxLatitude="85"&gt;&lt;!-- mark, full earth -->
&lt;/GeoMap></code></pre>
{{~ end ~}}

{{~ if winforms ~}}
<pre><code>geoMap1.MinLatitude = -85; // mark
geoMap1.MaxLatitude = 85;  // mark, full earth</code></pre>
{{~ end ~}}

Combine all four bounds to focus the map on a region — Iceland to the
Caucasus, North Africa coast to North Cape gives a tight Europe frame:

{{~ if xaml ~}}
<pre><code>&lt;lvc:GeoMap
    Series="{Binding Series}"
    MapProjection="Mercator"
    MinLatitude="35"&lt;!-- mark -->
    MaxLatitude="72"&lt;!-- mark -->
    MinLongitude="-25"&lt;!-- mark -->
    MaxLongitude="45"/&gt;&lt;!-- mark --></code></pre>
{{~ end ~}}

{{~ if blazor ~}}
<pre><code>&lt;GeoMap
    Series="series"
    MapProjection="LiveChartsCore.Geo.MapProjection.Mercator"
    MinLatitude="35"
    MaxLatitude="72"
    MinLongitude="-25"
    MaxLongitude="45"&gt;&lt;/GeoMap></code></pre>
{{~ end ~}}

{{~ if winforms ~}}
<pre><code>geoMap1.MinLatitude  =  35; // mark
geoMap1.MaxLatitude  =  72; // mark
geoMap1.MinLongitude = -25; // mark
geoMap1.MaxLongitude =  45; // mark</code></pre>
{{~ end ~}}

![image](https://raw.githubusercontent.com/Live-Charts/LiveCharts2/refs/heads/master/tests/SnapshotTests/Snapshots/MapsTests_MercatorEuropeView.png)

The same bounds work with `MapProjection.Default` (equirectangular),
just without Mercator's poleward stretching:

![image](https://raw.githubusercontent.com/Live-Charts/LiveCharts2/refs/heads/master/tests/SnapshotTests/Snapshots/MapsTests_DefaultProjectionEuropeView.png)

`Orthographic` ignores these bounds — on a globe, viewpoint is set via
`CoreChart.CenterLongitude` / `CenterLatitude` / `ZoomLevel` instead
(see [Rotating the globe](#rotating-the-globe) below).

### Orthographic

`Orthographic` renders the map as a 3D globe — only the hemisphere facing
the camera is drawn, lands that cross the horizon are clipped along the
disc rim. Out of the box the globe is centered at 0° longitude / 0°
latitude (the Gulf of Guinea):

{{~ if xaml ~}}
<pre><code>&lt;lvc:GeoMap
    Series="{Binding Series}"
    MapProjection="Orthographic"/&gt;&lt;!-- mark --></code></pre>
{{~ end ~}}

{{~ if blazor ~}}
<pre><code>&lt;GeoMap
    Series="series"
    MapProjection="LiveChartsCore.Geo.MapProjection.Orthographic"&gt;&lt;/GeoMap></code></pre>
{{~ end ~}}

{{~ if winforms ~}}
<pre><code>geoMap1.MapProjection = LiveChartsCore.Geo.MapProjection.Orthographic;</code></pre>
{{~ end ~}}

![image](https://raw.githubusercontent.com/Live-Charts/LiveCharts2/refs/heads/master/tests/SnapshotTests/Snapshots/MapsTests_OrthographicDefault.png)

#### Rotating the globe

`CoreChart.CenterLongitude` and `CoreChart.CenterLatitude` control the
center of view; setting them directly snaps, while
`CoreChart.RotateTo(longitude, latitude, durationMs)` animates the
transition. The example below snaps to `CenterLongitude = 15` /
`CenterLatitude = 20` to center on Europe and Africa:

{{~ if xaml ~}}
<pre><code>&lt;lvc:GeoMap
    x:Name="geoMap"
    Series="{Binding Series}"
    MapProjection="Orthographic"/&gt;</code></pre>

<pre><code>// Code-behind / ViewModel: center the globe on Europe + Africa.
geoMap.CoreChart.CenterLongitude = 15; // mark
geoMap.CoreChart.CenterLatitude  = 20; // mark</code></pre>
{{~ end ~}}

{{~ if blazor ~}}
<pre><code>&lt;GeoMap
    @ref="geoMap"
    Series="series"
    MapProjection="LiveChartsCore.Geo.MapProjection.Orthographic"&gt;&lt;/GeoMap>

@code {
    private GeoMap geoMap = null!;

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender) return;
        geoMap.CoreChart.CenterLongitude = 15; // mark
        geoMap.CoreChart.CenterLatitude  = 20; // mark
    }
}</code></pre>
{{~ end ~}}

{{~ if winforms ~}}
<pre><code>geoMap1.MapProjection = LiveChartsCore.Geo.MapProjection.Orthographic;
geoMap1.CoreChart.CenterLongitude = 15; // mark
geoMap1.CoreChart.CenterLatitude  = 20; // mark</code></pre>
{{~ end ~}}

![image](https://raw.githubusercontent.com/Live-Charts/LiveCharts2/refs/heads/master/tests/SnapshotTests/Snapshots/MapsTests_OrthographicRotated.png)

For an animated transition between viewpoints, use `RotateTo` instead
of setting the properties directly:

<pre><code>// Animate to Tokyo over 800 ms (the default).
geoMap.CoreChart.RotateTo(longitude: 139.69, latitude: 35.69);</code></pre>

Mouse-wheel zoom is supported the same way as on the flat projections;
pan is disabled by default — set `InteractionMode="Both"` to enable
click-drag pan.

## InteractionMode property

Controls which user interactions the map responds to. Defaults to
`MapInteractionMode.None` — geo maps are most often embedded as static
dashboard tiles, so the default is no interaction. Set it to `Zoom`
for wheel-zoom only, `Pan` for click-drag pan only, or `Both` for both.

| Value  | Wheel zoom | Click-drag pan |
| ------ | ---------- | -------------- |
| `None` | ✗          | ✗ *(default)*  |
| `Pan`  | ✗          | ✓              |
| `Zoom` | ✓          | ✗              |
| `Both` | ✓          | ✓              |

{{~ if xaml ~}}
<pre><code>&lt;lvc:GeoMap
    Series="{Binding Series}"
    InteractionMode="Both">&lt;!-- mark -->
&lt;/lvc:GeoMap></code></pre>
{{~ end ~}}

{{~ if blazor ~}}
<pre><code>&lt;GeoMap
    Series="series"
    InteractionMode="LiveChartsCore.Geo.MapInteractionMode.Both">&lt;!-- mark -->
&lt;/GeoMap></code></pre>
{{~ end ~}}

{{~ if winforms ~}}
<pre><code>geoMap1.InteractionMode = LiveChartsCore.Geo.MapInteractionMode.Both;</code></pre>
{{~ end ~}}

## Tooltip placement and styling

The `TooltipPosition` property controls where the popup anchors relative to the
hovered land's centroid. The map auto-flips between top and bottom when
`Auto` is set and the popup would clip the chart edge.

| Value    | Behavior                                            |
| -------- | --------------------------------------------------- |
| `Auto`   | Default — places above the land, flips below near the top edge. |
| `Top`    | Always above the land (wedge points down).          |
| `Bottom` | Always below the land (wedge points up).            |
| `Hidden` | Disables the tooltip entirely.                      |

{{~ if xaml ~}}
<pre><code>&lt;lvc:GeoMap
    Series="{Binding Series}"
    TooltipPosition="Top"&gt;&lt;!-- mark -->
&lt;/lvc:GeoMap></code></pre>
{{~ end ~}}

{{~ if blazor ~}}
<pre><code>&lt;GeoMap
    Series="series"
    TooltipPosition="LiveChartsCore.Measure.TooltipPosition.Top"&gt;&lt;!-- mark -->
&lt;/GeoMap></code></pre>
{{~ end ~}}

{{~ if winforms ~}}
<pre><code>geoMap1.TooltipPosition = LiveChartsCore.Measure.TooltipPosition.Top;</code></pre>
{{~ end ~}}

`TooltipTextSize`, `TooltipTextPaint`, and `TooltipBackgroundPaint` style the
default tooltip without replacing it. `TooltipTextSize` defaults to the active
theme; `TooltipTextPaint` and `TooltipBackgroundPaint` fall back to theme
paints when null.

{{~ if xaml ~}}
<pre><code>&lt;lvc:GeoMap
    Series="{Binding Series}"
    TooltipTextSize="16"&gt;&lt;!-- mark -->
&lt;/lvc:GeoMap></code></pre>
{{~ end ~}}

{{~ if blazor ~}}
<pre><code>&lt;GeoMap
    Series="series"
    TooltipTextSize="16"&gt;&lt;!-- mark -->
&lt;/GeoMap></code></pre>
{{~ end ~}}

{{~ if winforms ~}}
<pre><code>geoMap1.TooltipTextSize = 16;
geoMap1.TooltipTextPaint = new SolidColorPaint(SKColors.White);
geoMap1.TooltipBackgroundPaint = new SolidColorPaint(SKColors.Black);</code></pre>
{{~ end ~}}

## Programmatic viewport control

There are **two parallel ways** to control where the map is looking:

1. **Declarative bounds** on the view — `MinLatitude`, `MaxLatitude`,
   `MinLongitude`, `MaxLongitude`. XAML-bindable, ideal for the
   "frame on this region by default" use case. Honored by the
   `Default` and `Mercator` projections.
2. **Imperative center + zoom** on `CoreChart` — `CenterLongitude`,
   `CenterLatitude`, `ZoomLevel`. Settable from code at any time
   (responds to interactions, animations, business logic). Works for
   every projection — for `Orthographic` it rotates the globe; for
   the flat projections it pans within the bounds.

Setting `CenterLongitude` / `CenterLatitude` / `ZoomLevel` snaps; for
animated transitions on the globe use
`CoreChart.RotateTo(longitude, latitude, durationMs)`.

Other viewport methods:

- `CoreChart.ResetViewport()` — snap back to zoom 1.0 / no pan.
- `CoreChart.Pan(LvcPoint delta)` — pan by a screen-space offset
  (in pixels, not lat/lon).
- `CoreChart.Zoom(LvcPoint pivot, ZoomDirection direction)` — zoom in
  / out around a screen point (the gesture form;
  `CoreChart.ZoomLevel = …` is the value form).

<pre><code>// Reset zoom and pan to defaults.
geoMap.CoreChart.ResetViewport();

// Snap the orthographic globe to look at Tokyo.
geoMap.CoreChart.CenterLongitude = 139.69;
geoMap.CoreChart.CenterLatitude  =  35.69;

// Or animate it over 800 ms.
geoMap.CoreChart.RotateTo(longitude: 139.69, latitude: 35.69);</code></pre>

## Finding lands on click or hover

The map participates in the same `IChartView` pointer-event surface as the
other charts. `DataPointerDown` fires once per land click, and
`HoveredPointsChanged` fires when the pointer enters, transitions between, or
leaves a land. Each `ChartPoint` carries the `LandDefinition` as its data
source — unwrap it to read the land's name / short name and look up the
per-series values yourself.

<pre><code>using LiveChartsCore.Geo;
using LiveChartsCore.Kernel;

geoMap.DataPointerDown += (sender, points) =>
{
    if (points.FirstOrDefault()?.Context.DataSource is not LandDefinition land) return;

    // Look up each series' value for this land.
    foreach (var series in geoMap.Series ?? [])
        if (series.TryGetValue(land.ShortName, out var value))
            Console.WriteLine($"{series.Name}: {value}");
};</code></pre>

## Custom overlays at lat/lon (markers, callouts, etc.)

The map's `VisualElements` collection accepts any `VisualElement` — the
same primitives the cartesian charts use (`GeometryVisual`,
`DrawnLabelVisual`, `LineVisual`, ...). Wrap one in a
`GeoVisualElement(visual) { Longitude, Latitude }` to anchor it at a
geographic coordinate; the wrapper re-projects on every measure so the
overlay follows zoom, pan, and orthographic rotation.

{{~ if xaml ~}}
<pre><code>// In your ViewModel:
public IChartElement[] CityMarkers { get; } = [
    Marker(longitude: -74.00, latitude:  40.71), // New York
    Marker(longitude: 139.69, latitude:  35.69), // Tokyo
    Marker(longitude:  -3.70, latitude:  40.42), // Madrid
];

static GeoVisualElement Marker(double longitude, double latitude) =>
    new(new GeometryVisual&lt;CircleGeometry&gt;
    {
        Width = 14, Height = 14,
        Fill = new SolidColorPaint(new SKColor(255, 87, 51)),
        Stroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 2 },
        // Inner X/Y is the bbox top-left; center the dot on the point.
        Translate = new LvcPoint(-7, -7),
    })
    {
        Longitude = longitude,
        Latitude = latitude,
    };</code></pre>

<pre><code>&lt;lvc:GeoMap
    Series="{Binding Series}"
    VisualElements="{Binding CityMarkers}"/&gt;&lt;!-- mark --></code></pre>
{{~ end ~}}

{{~ if blazor ~}}
<pre><code>&lt;GeoMap
    Series="series"
    VisualElements="markers"/&gt;

@code {
    private IChartElement[] markers = [
        Marker(-74.00,  40.71),
        Marker(139.69,  35.69),
        Marker( -3.70,  40.42),
    ];

    static GeoVisualElement Marker(double lon, double lat) =>
        new(new GeometryVisual&lt;CircleGeometry&gt;
        {
            Width = 14, Height = 14,
            Fill = new SolidColorPaint(new SKColor(255, 87, 51)),
            Stroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 2 },
            Translate = new LvcPoint(-7, -7),
        })
        {
            Longitude = lon,
            Latitude = lat,
        };
}</code></pre>
{{~ end ~}}

{{~ if winforms ~}}
<pre><code>geoMap1.VisualElements = new IChartElement[]
{
    Marker(longitude: -74.00, latitude:  40.71),
    Marker(longitude: 139.69, latitude:  35.69),
    Marker(longitude:  -3.70, latitude:  40.42),
};

static GeoVisualElement Marker(double longitude, double latitude) =>
    new(new GeometryVisual&lt;CircleGeometry&gt;
    {
        Width = 14, Height = 14,
        Fill = new SolidColorPaint(new SKColor(255, 87, 51)),
        Stroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 2 },
        Translate = new LvcPoint(-7, -7),
    })
    {
        Longitude = longitude,
        Latitude = latitude,
    };</code></pre>
{{~ end ~}}

![image](https://raw.githubusercontent.com/Live-Charts/LiveCharts2/refs/heads/master/tests/SnapshotTests/Snapshots/MapsTests_MarkersOnMap.png)

If you need the raw projector — to position something yourself, hit-test
a click, etc. — use `GeoMapChart.Project(lon, lat)` and the inverse
`GeoMapChart.Unproject(LvcPoint)`. Both return `null` when the
coordinate / pixel is outside the visible region (e.g. the back
hemisphere of the orthographic globe), so you can distinguish "clicked
the void" from a real coordinate:

<pre><code>// Pixel → coordinate (returns null off-disc on Orthographic).
var coord = geoMap.CoreChart.Unproject(new LvcPoint(clickX, clickY));
if (coord is null) return; // click was outside the projection's visible area

Console.WriteLine($"clicked at {coord.Value.Longitude:0.00}°, {coord.Value.Latitude:0.00}°");

// Coordinate → pixel (returns null if the coordinate isn't currently visible).
var pixel = geoMap.CoreChart.Project(longitude: -74, latitude: 40.7);</code></pre>

If you only need a synchronous lookup (e.g. on a custom gesture), call
`geoMap.GetPointsAt(new LvcPointD(x, y))` — same `ChartPoint` shape, no event
subscription needed.

## Customizing the tooltip

The default tooltip (`SKDefaultGeoTooltip`) renders the land name followed by
one labeled line per heat series that has a value for it. For most cases the
quickest knob is `TooltipFormatter` — a `Func<GeoTooltipValue, string>` that
takes over the per-value line text:

{{~ if xaml ~}}
<pre><code>// In your ViewModel:
public Func&lt;GeoTooltipValue, string> TooltipFormatter { get; }
    = v => $"{v.Series.Name}: {v.Value:C0}"; // mark</code></pre>

<pre><code>&lt;lvc:GeoMap
    Series="{Binding Series}"
    TooltipFormatter="{Binding TooltipFormatter}">&lt;!-- mark -->
&lt;/lvc:GeoMap></code></pre>
{{~ end ~}}

{{~ if blazor ~}}
<pre><code>&lt;GeoMap
    Series="series"
    TooltipFormatter="@(v => $"{v.Series.Name}: {v.Value:C0}")">&lt;!-- mark -->
&lt;/GeoMap></code></pre>
{{~ end ~}}

{{~ if winforms ~}}
<pre><code>geoMap1.TooltipFormatter = v => $"{v.Series.Name}: {v.Value:C0}"; // mark</code></pre>
{{~ end ~}}

The default format is `"{Series.Name}: {Value:N2}"` (or just `"{Value:N2}"`
when the series has no `Name`). When several series cover the same land, you
get one line per series in the order they appear in `Series`.

For deeper customization (layout, multiple paints, icons, etc.), subclass
`SKDefaultGeoTooltip` or implement `IGeoMapTooltip` from scratch and assign
it to the `Tooltip` property:

<pre><code>public class MyTooltip : SKDefaultGeoTooltip
{
    protected override Layout&lt;SkiaSharpDrawingContext> GetLayout(
        GeoTooltipPoint point, GeoMapChart chart, Theme theme, PopUpPlacement placement)
    {
        // build and return your own layout
    }
}

geoMap.Tooltip = new MyTooltip();</code></pre>
