<!--
To get help on editing this file, see https://github.com/Live-Charts/LiveCharts2/blob/release/v2.2.x/docs/readme.md
-->

# The GeoMap Chart

The `GeoMap` control is useful to create geographical maps, it uses files in [geojson](https://en.wikipedia.org/wiki/GeoJSON) format to render
vectorized maps.

![image](https://raw.githubusercontent.com/Live-Charts/LiveCharts2/release/v2.2.x/docs/_assets/geomaphs.png)

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

## Series

There are ~multiple~ series available in the library, you can add one or mix them all in the same chart, every series has unique properties,
any image below is a link to an article explaining more about them.

<a href="{{ website_url }}/docs/{{ platform }}/{{ version }}/GeoMap.Heat%20land%20series">
<div class="series-miniature">
<img src="https://raw.githubusercontent.com/Live-Charts/LiveCharts2/release/v2.2.x/docs/samples/polarLines/basic/geomaphs.png" alt="series"/>
<div class="text-center"><b>Heat Land series</b></div>
</div>
</a>

## Stroke property

Determines the default stroke of every land, if the stroke property is not set, then LiveCharts will create it based on the series position in your series collection
and the current theme.

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

![image](https://raw.githubusercontent.com/Live-Charts/LiveCharts2/release/v2.2.x/docs/_assets/geomap-stroke.png)

:::info
Paints can create gradients, dashed lines and more, if you need help using the `Paint` instances take 
a look at the [Paints article]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Paints).
:::

## Fill property

Determines the default fill of every land, if the stroke property is not set, then LiveCharts will create it based on the series position in your series collection
and the current theme.

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

![image](https://raw.githubusercontent.com/Live-Charts/LiveCharts2/release/v2.2.x/docs/_assets/geomap-fill.png)

:::info
Paints can create gradients, dashed lines and more, if you need help using the `Paint` instances take 
a look at the [Paints article]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Paints).
:::

## MapProjection property

Defines the [projection](https://en.wikipedia.org/wiki/Map_projection) of the map coordinates in the control coordinates,
currently it only support the `Default` (none) and `Mercator` projections.

{{~ if xaml ~}}
<pre><code>&lt;lvc:GeoMap
    Series="{Binding Series}"
    MapProjection="Mercator">&lt;!-- mark -->
&lt;/lvc:GeoMap></code></pre>
{{~ end ~}}

{{~ if blazor ~}}
<pre><code>&lt;GeoMap
    Series="series"
    MapProjection="LiveChartsCore.Geo.MapProjection.Mercator">&lt;!-- mark --> 
&lt;/GeoMap></code></pre>
{{~ end ~}}

{{~ if winforms ~}}
<pre><code>geoMap1.MapProjection = LiveChartsCore.Geo.MapProjection.Mercator;</code></pre>
{{~ end ~}}

![image](https://raw.githubusercontent.com/Live-Charts/LiveCharts2/release/v2.2.x/docs/_assets/geomap-mercator.png)
