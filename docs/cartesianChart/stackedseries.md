<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
Samples are pulled from the repository with render_sample, the code shown always matches the samples.
-->

# Stacked series

A stacked series adds its values to the values of the series below it, so each point starts where the previous
series ended at the same X coordinate. There are four stacked series: `StackedAreaSeries` and
`StackedStepAreaSeries` are line series, `StackedColumnSeries` and `StackedRowSeries` are bar series. Each one takes
the properties of the series it is based on, so this article covers only what stacking changes; for the rest see the
[line and step line series]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Line%20and%20step%20line%20series)
and the [column and row series]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Column%20and%20row%20series)
articles.

:::info
This article highlights the most used features. Every property is listed in the API explorer:
[StackedAreaSeries]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.StackedAreaSeries-3),
[StackedStepAreaSeries]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.StackedStepAreaSeries-3),
[StackedColumnSeries]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.StackedColumnSeries-3) and
[StackedRowSeries]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.StackedRowSeries-3).
{{~ if xaml ~}}
In XAML, declare them as `XamlStackedAreaSeries`, `XamlStackedStepAreaSeries`, `XamlStackedColumnSeries` and
`XamlStackedRowSeries`.
{{~ end ~}}
:::

## Stacked area

Add two or more `StackedAreaSeries` to the chart and they stack in the order of the series collection: the first
series is at the bottom. A stacked area takes the same properties as a `LineSeries`, such as `LineSmoothness`, only
the defaults of the theme change: each area is filled with a solid color, with no line and no point geometries.

{{ render_sample "stackedArea/basic" }}

## Stacked step area

`StackedStepAreaSeries` stacks step lines the same way and takes the same properties as a `StepLineSeries`. Step
areas and smooth areas do not stack with each other: every `StackedStepAreaSeries` in the chart forms one stack, and
every `StackedAreaSeries` forms another.

{{ render_sample "stackedArea/stepArea" }}

## Stacked columns

`StackedColumnSeries` stacks bars vertically. Positive values stack up from zero and negative values stack down from
zero, each sign on its own. The default theme draws stacked bars with square corners, and places data labels in the
middle of each bar when `ShowDataLabels` is `true`.

Every point of a stacked series has a `StackedValue` property with the `Start`, `End` and `Total` of the stack, and
its `Share`: the part of the point in the total of its sign at that X coordinate, from `0` to `1`. This sample shows
the share in the tooltip with `YToolTipLabelFormatter`, see the
[tooltips article]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Tooltips) for more.

{{ render_sample "stackedBars/basic" }}

## Stacked rows

`StackedRowSeries` takes the same properties and stacks bars horizontally: the values go on the X axis and the
categories on the Y axis.

```csharp
Series = new ISeries[]
{
    new StackedRowSeries<int> { Values = new [] { 3, 5, 3, 2 } },
    new StackedRowSeries<int> { Values = new [] { 4, 2, 3, 2 } },
    new StackedRowSeries<int> { Values = new [] { 4, 6, 6, 5 } }
};
```

## Stack groups

Stacked columns and rows have a `StackGroup` property, `0` by default. Series with the same group stack together,
and each group gets its own bar in the category, placed side by side in the order the groups first appear in the
series collection. This sample stacks the first two series in group `0` and the last two in group `1`.

{{ render_sample "stackedBars/groups" }}

Stacked area and step area series have no `StackGroup`.

## Stroke and fill paints

When `Fill` is not set, LiveCharts picks it from the current theme, based on the position of the series in the
chart's series collection. The default theme draws stacked series with no `Stroke`.

`Stroke` draws the line on top of each area:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/stackedareastroke.png)

```csharp
Series = new ISeries[]
{
    new StackedAreaSeries<int>
    {
        Values = new [] { 4, 4, 7, 2, 8 },
        Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 4 }, // mark
        Fill = null,
        GeometryFill = null,
        GeometryStroke = null
    },
    new StackedAreaSeries<int>
    {
        Values = new [] { 7, 5, 3, 2, 6 },
        Stroke = new SolidColorPaint(SKColors.Red) { StrokeThickness = 8 }, // mark
        Fill = null,
        GeometryFill = null,
        GeometryStroke = null
    },
    new StackedAreaSeries<int>
    {
        Values = new [] { 4, 2, 5, 3, 9 },
        Stroke = new SolidColorPaint(SKColors.Green) { StrokeThickness = 1 }, // mark
        Fill = null,
        GeometryFill = null,
        GeometryStroke = null
    }
};
```

`Fill` paints each area. Every area is filled down to the bottom of the chart and the lower series are drawn on top
of the upper ones, so with a transparent fill the colors of the layers mix:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/stackedareafill.png)

```csharp
Series = new ISeries[]
{
    new StackedAreaSeries<int>
    {
        Values = new [] { 4, 4, 7, 2, 8 },
        Fill = new SolidColorPaint(SKColors.Blue.WithAlpha(90)), // mark
        Stroke = null,
        GeometryFill = null,
        GeometryStroke = null
    },
    new StackedAreaSeries<int>
    {
        Values = new [] { 7, 5, 3, 2, 6 },
        Fill = new SolidColorPaint(SKColors.Red.WithAlpha(90)), // mark
        Stroke = null,
        GeometryFill = null,
        GeometryStroke = null
    },
    new StackedAreaSeries<int>
    {
        Values = new [] { 4, 2, 5, 3, 9 },
        Fill = new SolidColorPaint(SKColors.Green.WithAlpha(90)), // mark
        Stroke = null,
        GeometryFill = null,
        GeometryStroke = null
    }
};
```

On stacked columns and rows, `Stroke` draws the border of each bar:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/stackedcolstroke.png)

```csharp
Series = new ISeries[]
{
    new StackedColumnSeries<int>
    {
        Values = new List<int> { 3, 5, 3, 2, 5, 4, 2 },
        Stroke = new SolidColorPaint(SKColors.Red) { StrokeThickness = 4 }, // mark
        Fill = null,
    },
    new StackedColumnSeries<int>
    {
        Values = new List<int> { 4, 2, 3, 2, 3, 4, 2 },
        Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 8 }, // mark
        Fill = null,
    },
    new StackedColumnSeries<int>
    {
        Values = new List<int> { 4, 6, 6, 5, 4, 3 , 2 },
        Stroke = new SolidColorPaint(SKColors.Green) { StrokeThickness = 12 }, // mark
        Fill = null,
    }
};
```

And `Fill` paints the inside of each bar:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/stackedcolfill.png)

```csharp
Series = new ISeries[]
{
    new StackedColumnSeries<int>
    {
        Values = new List<int> { 3, 5, 3, 2, 5, 4, 2 },
        Fill = new SolidColorPaint(SKColors.Red), // mark
        Stroke = null,
    },
    new StackedColumnSeries<int>
    {
        Values = new List<int> { 4, 2, 3, 2, 3, 4, 2 },
        Fill = new SolidColorPaint(SKColors.Blue), // mark
        Stroke = null,
    },
    new StackedColumnSeries<int>
    {
        Values = new List<int> { 4, 6, 6, 5, 4, 3 , 2 },
        Fill = new SolidColorPaint(SKColors.Green), // mark
        Stroke = null,
    }
};
```

:::info
Paints can also draw gradients and dashed lines, see the
[Paints article]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Paints).
:::

## Point geometries

Stacked area and step area series hide their point geometries: `GeometrySize` is `0` by default. Set `GeometrySize`,
`GeometryFill` and `GeometryStroke` to show them:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/stackedareageometrystrokefill.png)

```csharp
Series = new ISeries[]
{
    new StackedAreaSeries<int>
    {
        Values = new [] { 4, 4, 7, 2, 8 },
        GeometrySize = 20, // mark
        GeometryFill = new SolidColorPaint(SKColors.WhiteSmoke), // mark
        GeometryStroke = new SolidColorPaint(SKColors.Red) { StrokeThickness = 3 }, // mark
    },
    new StackedAreaSeries<int>
    {
        Values = new [] { 7, 5, 3, 2, 6 },
        GeometrySize = 20, // mark
        GeometryFill = new SolidColorPaint(SKColors.WhiteSmoke), // mark
        GeometryStroke = new SolidColorPaint(SKColors.Green) { StrokeThickness = 6 }, // mark
    },
    new StackedAreaSeries<int>
    {
        Values = new [] { 4, 2, 5, 3, 9 },
        GeometrySize = 20, // mark
        GeometryFill = new SolidColorPaint(SKColors.WhiteSmoke), // mark
        GeometryStroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 10 }, // mark
    }
};
```

## Rounded corners

`Rx` and `Ry` set the radius of the corners of each bar, in pixels. On stacked bars the default theme sets them to
`0`, a column series gets `3`.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/stackedcolcr.png)

```csharp
Series = new ISeries[]
{
    new StackedColumnSeries<int>
    {
        Values = new List<int> { 3, 5, 3, 2, 5, 4, 2 },
        Rx = 50, // mark
        Ry = 50 // mark
    },
    new StackedColumnSeries<int>
    {
        Values = new List<int> { 4, 2, 3, 2, 3, 4, 2 },
        Rx = 50, // mark
        Ry = 50 // mark
    },
    new StackedColumnSeries<int>
    {
        Values = new List<int> { 4, 6, 6, 5, 4, 3 , 2 },
        Rx = 50, // mark
        Ry = 50 // mark
    }
};
```

`MaxBarWidth` and `Padding` work as on a column series, see
[width and spacing]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Column%20and%20row%20series#width-and-spacing).
The space of a category is shared by the stack groups, not by the series.

## Data, names and updates

- `Values` takes any `IEnumerable<T>`. Use an `ObservableCollection<T>` and the chart animates every added, removed or
  replaced value, see [automatic updates]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Automatic%20updates).
- Points stack when they have the same X coordinate (Y on rows). To plot your own types, or set both X and Y, see
  [mappers]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Mappers).
- `Name`{{ if xaml }} (`SeriesName` in XAML){{ end }} is shown in tooltips and legends. When it is not set, the
  series is named "Series 1", "Series 2" and so on.
- `IsVisible` shows or hides the series. `ZIndex` sets which series draws on top; when it is not set, the series of
  a stacked area draw from the top layer to the bottom one.
