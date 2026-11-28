<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
Samples are pulled from the repository with render_sample, the code shown always matches the samples.
-->

# Column and row series

A `ColumnSeries` draws a vertical bar for each value, a `RowSeries` draws a horizontal one. Both are bar series and
share every property, so this article covers them together: a row series is a column series turned on its side, the
values go on the X axis and the categories on the Y axis.

:::info
This article highlights the most used features. Every property is listed in the API explorer:
[ColumnSeries]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.ColumnSeries-3) and
[RowSeries]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.RowSeries-3).
{{~ if xaml ~}}
In XAML, declare them as `XamlColumnSeries` and `XamlRowSeries`.
{{~ end ~}}
:::

## Basic columns

Set the `Values` of each series and add them to the chart. Columns that share the same X coordinate are placed side
by side, one per series. The X axis in this sample uses `Labels` to name each category, and each series has a name
for the tooltip and legend{{ if xaml }} (`SeriesName` in XAML){{ end }}.

{{ render_sample "bars/basic" }}

## Rows

A `RowSeries` takes the same properties. This sample also shows data labels: `ShowDataLabels` turns them on and
`DataLabelsPosition` places them at the `End`, `Middle` or `Start` of each bar. When `DataLabelsPaint` is not set,
the theme picks a paint that contrasts with the bar, a light one for `Middle` (inside the bar) and a dark one
otherwise. Setting `DataLabelsPaint` also turns the labels on.

{{ render_sample "bars/rowsWithLabels" }}

## Stroke and fill paints

When `Fill` is not set, LiveCharts picks it from the current theme, based on the position of the series in the
chart's series collection. The default theme draws bars with no `Stroke`.

:::: variations
::: variation
![Two column series drawn only with a blue and a red border](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/columnstroke.png)

`Stroke` draws the border of each bar.

```csharp
Series = new ISeries[]
{
    new ColumnSeries<int>
    {
        Values = new [] { 4, 4, 7, 2, 8 },
        Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 4 }, // mark
        Fill = null
    },
    new ColumnSeries<int>
    {
        Values = new [] { 7, 5, 3, 2, 6 },
        Stroke = new SolidColorPaint(SKColors.Red) { StrokeThickness = 8 }, // mark
        Fill = null
    }
};
```
:::
::: variation
![Two column series filled in blue and red, without a border](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/columnfill.png)

`Fill` paints the inside of each bar.

```csharp
Series = new ISeries[]
{
    new ColumnSeries<int>
    {
        Values = new [] { 4, 4, 7, 2, 8 },
        Fill = new SolidColorPaint(SKColors.Blue), // mark
        Stroke = null
    },
    new ColumnSeries<int>
    {
        Values = new [] { 7, 5, 3, 2, 6 },
        Fill = new SolidColorPaint(SKColors.Red), // mark
        Stroke = null
    }
};
```
:::
::::

:::info
Paints can also draw gradients and dashed lines, see the
[Paints article]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Paints).
:::

## Rounded corners

`Rx` and `Ry` set the radius of the corners, in pixels, on the X and Y directions. The default theme rounds them by
`3` pixels.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/columnr.png)

```csharp
Series = new ISeries[]
{
    new ColumnSeries<int>
    {
        Values = new [] { 4, 4, 7, 2, 8 },
        Rx = 50, // mark
        Ry = 50 // mark
    }
};
```

## Width and spacing

The space of each category is shared by the bar series in it, then two properties adjust each bar:

- `MaxBarWidth` limits the width of a bar, in pixels. It is `50` by default, so bars stop growing on wide charts.
- `Padding` is the space between the bars, in pixels. It is `2` by default.

:::: variations
::: variation
![Thin columns, ten pixels wide](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/columnmw10.png)

With `MaxBarWidth = 10` the bars are thin.

```csharp
Series = new ISeries[]
{
    new ColumnSeries<int>
    {
        Values = new [] { 4, 4, 7, 2, 8 },
        MaxBarWidth = 10 // mark
    }
};
```
:::
::: variation
![Columns that take all the width of their category](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/columnmwmax.png)

With `MaxBarWidth = double.MaxValue` the bars take all the space they get.

```csharp
Series = new ISeries[]
{
    new ColumnSeries<int>
    {
        Values = new [] { 4, 4, 7, 2, 8 },
        MaxBarWidth = double.MaxValue // mark
    }
};
```
:::
::: variation
![Columns that touch each other](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/columnmwmaxp0.png)

And with `Padding = 0` too, the bars touch each other.

```csharp
Series = new ISeries[]
{
    new ColumnSeries<int>
    {
        Values = new [] { 4, 4, 7, 2, 8 },
        MaxBarWidth = double.MaxValue,
        Padding = 0 // mark
    }
};
```
:::
::::

This sample uses both to draw a histogram-like chart:

{{ render_sample "bars/spacing" }}

## Backgrounds and layers

By default, bars that share a coordinate are placed side by side. With `IgnoresBarPosition` set to `true`, a series
takes the whole space of the category and is drawn over or under the other series. A common use is a background for
each column: the first series in this sample draws light bars with the maximum value of the axis, and sets
`IsHoverable` to `false` so they are not shown in tooltips.

{{ render_sample "bars/withBackground" }}

With a different `MaxBarWidth` for each series, the same property layers the bars, here a thin series in front of a
wide one:

{{ render_sample "bars/layered" }}

## Custom geometries

The second generic argument of the series sets the geometry of each bar, a `RoundedRectangleGeometry` by default.
This sample uses, in order, the default geometry, a predefined `DiamondGeometry` (the
`LiveChartsCore.SkiaSharpView.Drawing.Geometries` namespace has more), an SVG path through `VariableSVGPathGeometry`
and `GeometrySvg`, and a geometry drawn with the SkiaSharp API (`MyGeometry`).

{{ render_sample "bars/custom" }}

## Data, names and updates

- `Values` takes any `IEnumerable<T>`. Use an `ObservableCollection<T>` and the chart animates every added, removed or
  replaced value, see [automatic updates]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Automatic%20updates).
- To plot your own types, or set both X and Y, see [mappers]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Mappers).
- `Name`{{ if xaml }} (`SeriesName` in XAML){{ end }} is shown in tooltips and legends. When it is not set, the tooltip shows the
  values without a series name.
- `ZIndex` sets which series draws on top, and `IsVisible` shows or hides the series.
- To stack the bars of several series, see
  [stacked series]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Stacked%20series).
