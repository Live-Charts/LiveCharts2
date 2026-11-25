<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
Sample code is pulled from the repository, the code shown always matches the samples.
-->

# Legends

A legend lists the series of a chart, each one with a small shape drawn with the series paints (the miniature) and
the series name. Charts do not show a legend until you set `LegendPosition`. The default legend can be styled with a
few chart properties, and it can be replaced by your own legend.

![legends](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/samples/bars/basic/result.png)

:::info
This article highlights the most used features. Every member is listed in the API explorer:
[LegendPosition]({{ website_url }}/api/{{ version }}/LiveChartsCore.Measure.LegendPosition),
[SKDefaultLegend]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.SKCharts.SKDefaultLegend),
[SKHeatLegend]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.SKCharts.SKHeatLegend) and
[IChartLegend]({{ website_url }}/api/{{ version }}/LiveChartsCore.Kernel.Sketches.IChartLegend).
:::

## Legend position

`LegendPosition` places the legend at the `Top`, `Bottom`, `Left` or `Right` of the chart. The default is `Hidden`,
which draws no legend. The chart makes room for the legend, so the plot area gets smaller by the size of the legend.

{{~ if xaml ~}}
```xml
<lvc:CartesianChart
    Series="{Binding Series}"
    LegendPosition="Top"><!-- mark -->
</lvc:CartesianChart>
<lvc:CartesianChart
    Series="{Binding Series}"
    LegendPosition="Bottom"><!-- mark -->
</lvc:CartesianChart>
<lvc:CartesianChart
    Series="{Binding Series}"
    LegendPosition="Left"><!-- mark -->
</lvc:CartesianChart>
<lvc:CartesianChart
    Series="{Binding Series}"
    LegendPosition="Right"><!-- mark -->
</lvc:CartesianChart>
<lvc:CartesianChart
    Series="{Binding Series}"
    LegendPosition="Hidden"><!-- mark -->
</lvc:CartesianChart>
```
{{~ end ~}}

{{~ if blazor ~}}
```xml
<CartesianChart
    Series="series"
    LegendPosition="LiveChartsCore.Measure.LegendPosition.Top"><!-- mark -->
</CartesianChart>
<CartesianChart
    Series="series"
    LegendPosition="LiveChartsCore.Measure.LegendPosition.Bottom"><!-- mark -->
</CartesianChart>
<CartesianChart
    Series="series"
    LegendPosition="LiveChartsCore.Measure.LegendPosition.Left"><!-- mark -->
</CartesianChart>
<CartesianChart
    Series="series"
    LegendPosition="LiveChartsCore.Measure.LegendPosition.Right"><!-- mark -->
</CartesianChart>
<CartesianChart
    Series="series"
    LegendPosition="LiveChartsCore.Measure.LegendPosition.Hidden"><!-- mark -->
</CartesianChart>
```
{{~ end ~}}

{{~ if winforms || eto ~}}
```csharp
cartesianChart1.LegendPosition = LiveChartsCore.Measure.LegendPosition.Bottom; // mark
// or use Top, Left, Right or Hidden
```
{{~ end ~}}

At `Top` and `Bottom` the default legend places the series in a row, centered, and starts a new row when the row
is wider than the chart. At `Left` and `Right` it places them in a column, centered vertically, and starts a new
column when the column is taller than the chart.

## Legend text and background

`LegendTextPaint`, `LegendTextSize` and `LegendBackgroundPaint` style the default legend. When they are not set, the
default theme draws the names at size `16`, in dark gray (light gray in the dark theme), with no background.

{{~ if xaml ~}}
```xml
<lvc:CartesianChart
    Series="{Binding Series}"
    LegendPosition="Left"
    LegendTextPaint="{lvc:SolidColorPaint Color='#323232'}"
    LegendBackgroundPaint="{lvc:SolidColorPaint Color='#eeeeee'}"
    LegendTextSize="16">
</lvc:CartesianChart>
```
{{~ end ~}}

{{~ if blazor ~}}
```xml
<CartesianChart
    Series="series"
    LegendPosition="LiveChartsCore.Measure.LegendPosition.Left"
    LegendTextPaint="@legendTextPaint"
    LegendBackgroundPaint="@legendBackgroundPaint"
    LegendTextSize="16">
</CartesianChart>

@code {
    private SolidColorPaint legendTextPaint = new(new SKColor(50, 50, 50));
    private SolidColorPaint legendBackgroundPaint = new(new SKColor(238, 238, 238));
    // ...
}
```
{{~ end ~}}

{{~ if winforms || eto ~}}
```csharp
cartesianChart1.LegendPosition = LiveChartsCore.Measure.LegendPosition.Left;
cartesianChart1.LegendTextPaint = new SolidColorPaint(new SKColor(50, 50, 50)); // mark
cartesianChart1.LegendBackgroundPaint = new SolidColorPaint(new SKColor(238, 238, 238)); // mark
cartesianChart1.LegendTextSize = 16; // mark
```
{{~ end ~}}

![custom](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/legend-custom-style.png)

A name wider than 170 pixels wraps to the next line; the limit is `MaxTooltipsAndLegendsLabelsWidth` in
`LiveCharts.DefaultSettings`, and tooltips use it too. Tooltips also have their own `TooltipTextPaint`,
`TooltipTextSize` and `TooltipBackgroundPaint` properties, see the
[tooltips article]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Tooltips%20and%20crosshairs).

## Which series appear

The legend shows the `Name`{{ if xaml }} (`SeriesName` in XAML){{ end }} of each series, so set it on every
series that appears in the legend. Set `IsVisibleAtLegend` to `false` to leave a series out of the
legend, it is `true` by default:

```csharp
Series = new ISeries[]
{
    new ColumnSeries<int> { Name = "Mary", Values = new [] { 2, 5, 4 } },
    new ColumnSeries<int> { Name = "Ana", Values = new [] { 3, 1, 6 } },
    new LineSeries<int>
    {
        Name = "Goal",
        Values = new [] { 4, 4, 4 },
        IsVisibleAtLegend = false // mark
    }
};
```

`IsVisibleAtLegend` does not depend on `IsVisible`: a series hidden with `IsVisible = false` is still listed in the
default legend. The default legend does not react to the pointer, so clicking an item does not hide its series. To
let users hide series, toggle `IsVisible` from your own UI; a [custom legend](#custom-legends) can show which series
are hidden, as the `LegendItem` class of the sample below does.

## Series miniature

The shape next to each name is the miniature of the series, drawn with the series paints. A column or row series
draws a small bar. A line series draws its point geometry, or a short line when the points are hidden (when
`GeometrySize` is below `1`, or when both `GeometryFill` and `GeometryStroke` are `null`). Tooltips draw the same
miniature.

`MiniatureShapeSize` sets the size of the miniature, `12` by default, and `MiniatureStrokeThickness` the thickness of
its stroke, `2` by default:

```csharp
new LineSeries<int>
{
    Values = new [] { 4, 2, 5, 3, 9 },
    MiniatureShapeSize = 20, // mark
    MiniatureStrokeThickness = 4 // mark
}
```

## Custom legends

Set the `Legend` property of the chart to replace the default legend. The easiest way to build your own is to inherit
from `SKDefaultLegend` and override `GetLayout()`, which returns the content of the legend built with the LiveCharts
layouts (`StackLayout`, `LabelGeometry` and the miniature of each series). In this sample `CustomLegend` stacks the
items vertically, and each `LegendItem` draws a taller miniature and a larger text, and dims the series that are not
visible.

{{ render_sample "general/templatedLegends" }}

## Legend from scratch

A legend can be any class that implements `IChartLegend`. The chart calls its `Measure()` method to know how much
space to reserve, `Draw()` to place it, and `Hide()` when `LegendPosition` is `Hidden`. Use the
[SKDefaultLegend source code](https://github.com/beto-rodriguez/LiveCharts2/blob/master/src/skiasharp/LiveChartsCore.SkiaSharp/SKCharts/SKDefaultLegend.cs)
as a guide, it is the legend the library uses by default.

You can also build the legend with your UI framework instead of the LiveCharts API.
[#1558](https://github.com/beto-rodriguez/LiveCharts2/issues/1558) shows a WPF control that implements
`IChartTooltip`; a control that implements `IChartLegend` follows the same idea.

## Heat legend

A heat series has no miniature to show, so for a heat map set the `Legend` of the chart to an `SKHeatLegend`: it
draws the colors of the `HeatMap` as a gradient, with the lowest and highest weights at its ends. It follows the same
`LegendPosition`, `LegendTextSize` and `LegendBackgroundPaint` properties. See the
[basic heat map]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Heat%20series#basic-heat-map)
sample.
