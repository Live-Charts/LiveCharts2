<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
Samples are pulled from the repository with render_sample, the code shown always matches the samples.
-->

# Pie charts

The `PieChart` control draws pie and doughnut charts with `PieSeries`. Each series is a slice: the slices share the
circle in proportion to their values. This article covers the control and the pie series together. The same control
also draws gauges, see the [gauges article]({{ website_url }}/docs/{{ platform }}/{{ version }}/PieChart.Gauges).

:::info
This article highlights the most used features. Every property is listed in the API explorer:
[PieChart]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.{{ platform }}.PieChart) and
[PieSeries]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.PieSeries-3).
{{~ if xaml ~}}
In XAML, declare the series as `XamlPieSeries`.
{{~ end ~}}
:::

## Basic pie

Add one `PieSeries` with one value for every slice. The angle of a slice is its value divided by the sum of the
values of all the series. By default the first slice starts at 3 o'clock and the next ones follow clockwise.
{{~ if xaml ~}}
This sample builds one series for every item of the view model with `SeriesSource` and `SeriesTemplate`.
{{~ end ~}}

{{ render_sample "pies/basic" }}

In C#, create one series per slice:

```csharp
Series = new ISeries[]
{
    new PieSeries<double> { Values = new double[] { 2 } },
    new PieSeries<double> { Values = new double[] { 4 } },
    new PieSeries<double> { Values = new double[] { 1 } },
    new PieSeries<double> { Values = new double[] { 4 } },
    new PieSeries<double> { Values = new double[] { 3 } }
};
```

The `AsPieSeries` extension, in the `LiveChartsCore.SkiaSharpView.Extensions` namespace, builds the same collection
from any `IEnumerable<T>`: every item becomes a series with that item as its only value. Its optional argument takes
the item and its series, to set the properties of each series:

```csharp
Series = new[] { 2, 4, 1, 4, 3 }.AsPieSeries((value, series) =>
{
    series.Name = $"Slice {value}";
});
```

A series with more than one value draws one ring per value, see [nested pies](#nested-pies).

## Doughnut

`MaxRadialColumnWidth` caps the width of the slices, from the inner to the outer edge, in pixels. When the slices
would be wider, they keep their outer edge and the center of the pie is left empty, which draws a doughnut that
keeps the same ring width at any chart size. It is `double.MaxValue` by default.

{{ render_sample "pies/doughnut" }}

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/pieMaxRadialCW.png)

`RadialAlign` sets which edge the slice keeps when it is capped: `Outer` (the default), `Center` or `Inner`.

`InnerRadius` sets the radius of the hole instead, in pixels. The hole keeps its size and the ring gets thinner when
the chart gets smaller:

```csharp
var pieSeries = new PieSeries<int>
{
    Values = new[] { 4 },
    InnerRadius = 50 // mark
};
```

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/pieInnerRadius.png)

`CornerRadius` rounds the corners of every slice, in pixels, and `InvertedCornerRadius` inverts the direction of the
curve.

## Pushout

`Pushout` is the distance in pixels between the center of the chart and the slice. `HoverPushout` is the push-out
while the pointer is over the slice, `20` by default; set it to `0` to keep the slices in place on hover. The chart
leaves room for the largest `Pushout` or `HoverPushout` of its series, so a slice that moves out stays inside the
control.

{{ render_sample "pies/pushout" }}

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/piepushout.png)

```csharp
var pieSeries = new PieSeries<int>
{
    Values = new[] { 4 },
    Pushout = 40 // mark
};
```

## Rotation and direction

The `PieChart` has three properties that set where and how the slices are drawn:

- `InitialRotation` is the angle, in degrees, where the first slice starts, `0` by default, which is 3 o'clock.
  Angles grow clockwise, so `-90` starts the pie at 12 o'clock.
- `MaxAngle` is the angle the whole pie covers, in degrees, `360` by default. `270` draws three quarters of a circle.
- `IsClockwise` is `true` by default. When it is `false` the slices follow counterclockwise, and the first slice
  starts at `InitialRotation` plus `MaxAngle` (the same point when `MaxAngle` is `360`).

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/pie-rotation.png)

{{~ if xaml ~}}
```xml
<lvc:PieChart
    Series="{Binding Series}"
    InitialRotation="-90"
    MaxAngle="270"
    IsClockwise="False">
</lvc:PieChart>
```
{{~ end ~}}

{{~ if blazor ~}}
```xml
<PieChart
    Series="series"
    InitialRotation="-90"
    MaxAngle="270"
    IsClockwise="false">
</PieChart>
```
{{~ end ~}}

{{~ if winforms || eto ~}}
```csharp
pieChart1.InitialRotation = -90;
pieChart1.MaxAngle = 270;
pieChart1.IsClockwise = false;
```
{{~ end ~}}

A change in `InitialRotation` or `MaxAngle` is animated, as any other change in the chart:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/pie-inrot.gif)

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/pie-maxangle.gif)

## Max value

By default the slices fill the whole `MaxAngle`, because the total of the pie is the sum of the values. The
`MaxValue` property of the chart sets that total instead: with `MaxValue = 100`, a slice with a value of `25` takes a
quarter of the `MaxAngle`, and when the values add up to less than `100` part of the circle stays empty. Values above
`MaxValue` are cut at `MaxValue`. It is `double.NaN` by default, which means the sum of the values.

{{~ if xaml ~}}
```xml
<lvc:PieChart
    Series="{Binding Series}"
    MaxValue="100">
</lvc:PieChart>
```
{{~ end ~}}

{{~ if blazor ~}}
```xml
<PieChart
    Series="series"
    MaxValue="100">
</PieChart>
```
{{~ end ~}}

{{~ if winforms || eto ~}}
```csharp
pieChart1.MaxValue = 100;
```
{{~ end ~}}

`MinValue` and the series property `IsRelativeToMinValue` are meant for gauges, see the
[gauges article]({{ website_url }}/docs/{{ platform }}/{{ version }}/PieChart.Gauges).

## Stroke and fill paints

When `Fill` is not set, LiveCharts picks it from the current theme, based on the position of the series in the
chart's series collection. The default theme draws pie series with no `Stroke`.

`Stroke` draws the border of the slice:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/piestroke.png)

```csharp
Series = new ISeries[]
{
    new PieSeries<double>
    {
        Values = new double[] { 4 },
        Pushout = 8,
        Stroke = new SolidColorPaint(SKColors.Red) { StrokeThickness = 3 } // mark
    },
    new PieSeries<double> { Values = new double[] { 2 } },
    new PieSeries<double> { Values = new double[] { 1 } },
    new PieSeries<double> { Values = new double[] { 4 } },
    new PieSeries<double> { Values = new double[] { 3 } }
};
```

`Fill` paints the inside of the slice:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/piefill.png)

```csharp
Series = new ISeries[]
{
    new PieSeries<double>
    {
        Values = new double[] { 4 },
        Pushout = 8,
        Fill = new SolidColorPaint(SKColors.Yellow) // mark
    },
    new PieSeries<double> { Values = new double[] { 2 } },
    new PieSeries<double> { Values = new double[] { 1 } },
    new PieSeries<double> { Values = new double[] { 4 } },
    new PieSeries<double> { Values = new double[] { 3 } }
};
```

:::info
Paints can also draw gradients and dashed lines, see the
[Paints article]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Paints).
:::

## Data labels

Set `ShowDataLabels` to `true` to draw a label on every slice; setting `DataLabelsPaint` also shows them. When
`DataLabelsPaint` is not set, the theme picks it: a color that reads on the slice, or the text color of the theme for
outer labels. `DataLabelsSize` sets the text size, `16` by default, and `DataLabelsFormatter` takes the point and
returns the text, the value by default.

`DataLabelsPosition` places the label:

- `Middle`, the default, inside the slice, at the middle of its angle.
- `Outer`, outside the slice, see [outer labels](#outer-labels).
- `Start` and `End`, at the edge where the slice starts or ends, rotated along that edge.
- `ChartCenter`, at the center of the chart, used by gauges.

```csharp
Series = new ISeries[]
{
    new PieSeries<double>
    {
        Values = new double[] { 8 },
        DataLabelsPaint = new SolidColorPaint(SKColors.Black),
        DataLabelsSize = 22,
        // all the positions:
        // {{ website_url }}/api/{{ version }}/LiveChartsCore.Measure.PolarLabelsPosition
        DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
        DataLabelsFormatter = point => point.Coordinate.PrimaryValue.ToString("N2") + " elements"
    },
    new PieSeries<double>
    {
        Values = new double[] { 6 },
        DataLabelsPaint = new SolidColorPaint(SKColors.Black),
        DataLabelsSize = 22,
        DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
        DataLabelsFormatter = point => point.Coordinate.PrimaryValue.ToString("N2") + " elements"
    },
    new PieSeries<double>
    {
        Values = new double[] { 4 },
        DataLabelsPaint = new SolidColorPaint(SKColors.Black),
        DataLabelsSize = 22,
        DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
        DataLabelsFormatter = point => point.Coordinate.PrimaryValue.ToString("N2") + " elements"
    }
};
```

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/pielabels.png)

Every point of a pie series has a `StackedValue` property: its `Total` is the sum of the slices and its `Share` is
the part of the point in that total, from `0` to `1`. This sample uses them in the labels and the tooltips, and
gives every series a different `OuterRadiusOffset` (see [nightingale rose](#nightingale-rose)):

{{ render_sample "pies/custom" }}

`DataLabelsRotation` rotates the labels, in degrees. Two constants rotate each label to follow its slice.
`LiveCharts.CotangentAngle` aligns the text with the radius of the slice:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/pielabelscotan.png)

And `LiveCharts.TangentAngle` draws it across the radius:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/pielabelstan.png)

The constants can be combined with an angle in degrees:

```csharp
Series = new ISeries[]
{
    new PieSeries<double>
    {
        Values = new double[] { 8 },
        DataLabelsRotation = 30 // in degrees
    },
    new PieSeries<double>
    {
        Values = new double[] { 6 },
        DataLabelsRotation = LiveCharts.TangentAngle + 30 // the tangent + 30 degrees
    },
    new PieSeries<double>
    {
        Values = new double[] { 4 },
        DataLabelsRotation = LiveCharts.CotangentAngle + 30 // the cotangent + 30 degrees
    }
};
```

## Outer labels

With `DataLabelsPosition` set to `Outer`, the label is drawn outside the slice, at the middle of its angle. The chart
makes the pie smaller to leave room for the largest outer label. This sample also sets `IsClockwise` to `false` and
`InitialRotation` to `-90`, so the slices start at 12 o'clock and go counterclockwise.

{{ render_sample "pies/outlabels" }}

## Custom labels

The third generic argument of `PieSeries<TModel, TVisual, TLabel>` sets the class of the data labels. This sample
draws an SVG icon and the name of the series instead of a text: `SvgLabel` inherits from `LabelGeometry` and
overrides `Draw`, and the series sets the icon of every label in the `PointMeasured` event, which runs every time a
point gets its size and position.

{{ render_sample "pies/icons" }}

## Nightingale rose

`OuterRadiusOffset` makes the slices of a series shorter: it is subtracted, in pixels, from the space the pie can
take. Giving each series a different offset draws a nightingale rose, where both the angle and the length of the
slices change. This sample also sets an `InnerRadius`.

{{ render_sample "pies/nightingaleRose" }}

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/pieOuterRadiusOffset.png)

```csharp
var pieSeries = new PieSeries<int>
{
    Values = new[] { 4 },
    OuterRadiusOffset = 20 // mark
};
```

## Nested pies

When the series have more than one value, every value draws a ring: the first value of every series shares the
inner ring, the second value the next ring, and so on. The rings have the same width, and each ring is a pie of its
own: the angle of a slice is its value divided by the sum of the values of that ring. A `null` value draws no slice
but keeps its ring, so give every series the same number of values.

This sample draws countries on the outer ring, their zones in the middle and the sales people inside. Each series
has a value in only one ring, the others are `null`. The series set `HoverPushout` to `0` so the rings stay aligned,
show data labels only on the countries, and hide the other series from the legend with `IsVisibleAtLegend`.

{{ render_sample "pies/nested" }}

`RelativeInnerRadius` adds space at the inner side of every ring and `RelativeOuterRadius` removes space from its
outer side, both in pixels; use them to leave a gap between the rings. When a slice is capped by
`MaxRadialColumnWidth`, `RadialAlign` sets these values instead.

## Tooltips and legends

The tooltip shows the hovered slice with the series `Name`{{ if xaml }} (`SeriesName` in XAML){{ end }} and the value.
`ToolTipLabelFormatter` builds the value text; a pie tooltip has no header. The tooltip is placed next to the outer
edge of the slice, above it, or below it when there is no room. `TooltipPosition` is ignored, except `Hidden`, which
disables the tooltips.

The legend is hidden by default: set `LegendPosition` to `Top`, `Bottom`, `Left` or `Right` to show it. It lists one
entry per series.

The [tooltips and crosshairs]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Tooltips%20and%20crosshairs)
and [legends]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Legends) articles cover their text,
paints and custom controls.

## Title and animations

The `PieChart` also takes the `Title`, `DrawMargin`, `AnimationsSpeed` and `EasingFunction` properties of every
chart; the basic sample sets a `Title`. See the
[Cartesian chart control]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Cartesian%20chart%20control#title)
and [animations]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Animations) articles.

## Data, names and updates

- `Values` takes any `IEnumerable<T>`. Use an `ObservableCollection<T>` and the chart animates every added, removed or
  replaced value, see [automatic updates]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Automatic%20updates).
- To plot your own types, see [mappers]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Mappers).
- `Name`{{ if xaml }} (`SeriesName` in XAML){{ end }} is shown in tooltips and legends. When it is not set, the tooltip shows the
  value without a series name.
- `IsVisible` shows or hides the series, and the other slices take its space. `ZIndex` sets which series draws on
  top.
