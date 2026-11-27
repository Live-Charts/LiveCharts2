<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
Samples are pulled from the repository with render_sample, the code shown always matches the samples.
-->

# Gauges

A gauge is a `PieChart` that shows one or more values against a fixed range, set by the `MinValue` and `MaxValue`
of the chart. A solid gauge fills an arc up to each value. An angular gauge draws colored sections, ticks and a needle
that points at the value. Gauge series are pie series, so they take every property described in the
[Pie charts article]({{ website_url }}/docs/{{ platform }}/{{ version }}/PieChart.Pie%20charts); this article covers
only what is specific to gauges.

:::info
This article highlights the most used features. Every property is listed in the API explorer:
[GaugeGenerator]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.Extensions.GaugeGenerator),
[GaugeItem]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.Extensions.GaugeItem),
[PieSeries]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.PieSeries-3),
[NeedleVisual]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.VisualElements.NeedleVisual) and
[AngularTicksVisual]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.VisualElements.AngularTicksVisual).
{{~ if xaml ~}}
In XAML, declare them as `XamlGaugeSeries`, `XamlGaugeBackgroundSeries`, `XamlAngularGaugeSeries`, `XamlNeedle` and
`XamlAngularTicks`.
{{~ end ~}}
:::

## Basic gauge

Set the `MinValue` and `MaxValue` of the chart to the range of the gauge.{{ if xaml }} Then add a `XamlGaugeSeries`
and set its `GaugeValue`.{{ else }} Then build the series with `GaugeGenerator.BuildSolidGauge()`: it takes one
`GaugeItem` per value, and each item takes the value and an optional delegate that styles the series of that value.{{ end }}
When `MaxValue` is not set, the chart uses the sum of the values, as a regular pie chart does, and the gauge is always
full.

The default theme places the value at the center of the chart (`DataLabelsPosition` is `ChartCenter`) and rounds the
ends of the arc with a `CornerRadius` of `8`. `InitialRotation` is the angle where the arc starts, in degrees: `0` is
the right side of the chart and the angles grow clockwise, so `-90` starts at the top. `MaxRadialColumnWidth` limits
the width of the arc in pixels; the arc stays at the outer edge of the chart.

{{ render_sample "pies/gauge1" }}

## Angles

`MaxAngle` is the angle the whole range covers, `360` by default. This sample draws a 270 degree gauge: it starts at
`-225`, the bottom left, and ends at the bottom right, leaving the gap at the bottom. Set `IsClockwise` to `false` to
fill the arc in the other direction.

{{ render_sample "pies/gauge2" }}

## Background

The background series draws the whole arc, from `MinValue` to `MaxValue`, behind the values. It is not shown in the
legend, and the default theme fills it with a light gray (a light transparent white in the dark theme).
{{~ if xaml ~}}
Add a `XamlGaugeBackgroundSeries` to the chart to draw it, and give it the same `InnerRadius` as the gauge series so
the rings match:

```xml
<lvc:XamlGaugeBackgroundSeries
    InnerRadius="75"
    Fill="{lvc:SolidColorPaint Color='#5A64B5F6'}"/>
```

Leave it out, or set its `Fill` to `{x:Null}`, to draw the gauge without a background.
{{~ else ~}}
`BuildSolidGauge()` always adds it. To style it, pass a `GaugeItem` with `GaugeItem.Background` as the value, and
give it the same `InnerRadius` as the gauge series so the rings match:

```csharp
Series = GaugeGenerator.BuildSolidGauge(
    new GaugeItem(30, series =>
    {
        series.Fill = new SolidColorPaint(SKColors.YellowGreen);
        series.InnerRadius = 75;
    }),
    new GaugeItem(GaugeItem.Background, series => // mark
    {
        series.InnerRadius = 75;
        series.Fill = new SolidColorPaint(new SKColor(100, 181, 246, 90));
    }));
```

Set its `Fill` to `null` to draw the gauge without a background.
{{~ end ~}}

## Multiple values

{{ if xaml }}Add one `XamlGaugeSeries` per value{{ else }}Pass one `GaugeItem` per value{{ end }}. Each value gets
its own ring: the first one is the inner ring. `InnerRadius` leaves a hole in the center, and `RelativeInnerRadius`
and `RelativeOuterRadius` leave space between the rings. The background draws behind every ring.

Here the labels go at the `Start` of each arc, and the `DataLabelsFormatter` adds the series name to the value.

{{ render_sample "pies/gauge3" }}

## Slim gauges

A small `MaxRadialColumnWidth` draws each value as a thin line. This sample sets it to `5`, places the labels at the
`End` of each arc, and hides the background by setting its `Fill` to `null`.

{{ render_sample "pies/gauge4" }}

## Updating the value

{{ if xaml }}Bind `GaugeValue` to a property that notifies its changes{{ else }}Pass an `ObservableValue` to the `GaugeItem`,
keep a reference to it and change its `Value`{{ end }}, and the gauge animates to the new value.

{{ render_sample "pies/gauge5" }}

See [automatic updates]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Automatic%20updates) for more.

## Angular gauge

An angular gauge has three parts:

- **Sections**: {{ if xaml }}one `XamlAngularGaugeSeries` per section{{ else }}build them with
  `GaugeGenerator.BuildAngularGaugeSections()`, one `GaugeItem` per section{{ end }}. All the sections share one ring
  and are placed one after another, in order, starting at `MinValue`: each value is the length of its section, not
  its end. In this sample the sections `60`, `30` and `10` cover 0 to 60, 60 to 90 and 90 to 100. Sections have no
  data labels, do not react to the pointer and do not animate. `OuterRadiusOffset` makes the ring smaller to leave
  room for the ticks.
- **Ticks**: {{ if xaml }}a `XamlAngularTicks`{{ else }}an `AngularTicksVisual`{{ end }} draws an arc with ticks and
  labels along the range. The step between ticks is about a tenth of the range, rounded up to 1, 2 or 5 times a power
  of ten, with four smaller ticks between two labels. `OuterOffset` moves the arc and the ticks in from the edge of
  the chart, `LabelsOuterOffset` does the same for the labels, `TicksLength` is the length of the ticks,
  `LabelsSize` the size of the labels (`12` by default) and `Labeler` formats them.
- **Needle**: {{ if xaml }}a `XamlNeedle`{{ else }}a `NeedleVisual`{{ end }} points at its `Value`. `Width` is `20`
  in the default theme, and `Fill` paints it. Change `Value` to move it.

The ticks and the needle are visual elements, so they go in the `VisualElements` of the chart. Both use the
`MinValue`, `MaxValue`, `InitialRotation` and `MaxAngle` of the chart, so set them, and both only work in a
`PieChart`.

{{ render_sample "pies/angularGauge" }}

{{ if xaml }}Add a `XamlGaugeBackgroundSeries` to draw a background behind the sections.{{ else }}`BuildAngularGaugeSections()`
adds a background series too, `GaugeItem.Background` styles it.{{ end }}
The default theme rounds gauge slices with a `CornerRadius` of `8`; this sample sets it to `0` so the sections join
with square ends.

## Custom needle

The needle is drawn by a `NeedleGeometry`. Inherit from it to change the needle, then use your class as the type
argument of the visual: `NeedleVisual<SmallNeedle>`.{{ if xaml }} `XamlNeedle` always draws the default needle, so
create the visual in your view model and bind the `VisualElements` of the chart to a collection that contains it.{{ end }}

This needle sets a `ScaleTransform` of `0.6`, so it is 40% smaller than the default one:

```csharp
public class SmallNeedle : NeedleGeometry
{
    public SmallNeedle()
    {
        ScaleTransform = new(0.6f, 0.6f);
    }
}
```

```csharp
var needle = new NeedleVisual<SmallNeedle> { Value = 45 };
```

Override `Draw()` to draw the needle yourself with SkiaSharp. `X` and `Y` are the center of the chart, `Radius` the
length of the needle and `Width` its width; the visual rotates the geometry to the value. This one draws a
rectangle:

```csharp
public class CustomNeedle : NeedleGeometry
{
    public override void Draw(SkiaSharpDrawingContext context)
    {
        var paint = context.ActiveSkiaPaint;

        context.Canvas.DrawRect(X - Width * 0.5f, Y, Width, Radius, paint);
    }
}
```

![custom needle](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/samples/pies/angularGauge/needle-rect.png)

The default [NeedleGeometry source](https://github.com/beto-rodriguez/LiveCharts2/blob/master/src/skiasharp/LiveChartsCore.SkiaSharp/Drawing/Geometries/NeedleGeometry.cs)
is a good starting point.
