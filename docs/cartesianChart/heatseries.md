<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
Samples are pulled from the repository with render_sample, the code shown always matches the samples.
-->

# Heat series

A `HeatSeries` draws a grid of cells. Each point has an X and a Y coordinate, which place the cell, and a weight,
which sets its color: the weight is mapped to a gradient that goes from the lowest weight in the series to the
highest.

:::info
This article highlights the most used features. Every property is listed in the API explorer:
[HeatSeries]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.HeatSeries-3).
{{~ if xaml ~}}
In XAML, declare it as `XamlHeatSeries`.
{{~ end ~}}
:::

## Basic heat map

The values of a heat series are usually
[WeightedPoint]({{ website_url }}/api/{{ version }}/LiveChartsCore.Defaults.WeightedPoint) instances, created with
`new WeightedPoint(x, y, weight)`. In this sample X is the person and Y the month, and both axes use `Labels` to name
them. A point with a `null` X or Y is not drawn, and a `null` weight counts as `0`.

Each cell is centered at its X and Y. Its size is the smallest distance between two X values, and between two Y
values, of the series, so the points do not need to be one unit apart. The tooltip shows the label of the Y axis
and the weight of the cell.

The chart in this sample also replaces the default legend with an `SKHeatLegend`, which draws the colors of the
`HeatMap` as a gradient, with the lowest and highest weights at its ends.

{{ render_sample "heat/basic" }}

## Heat map colors

`HeatMap` is an array of [LvcColor]({{ website_url }}/api/{{ version }}/LiveChartsCore.Drawing.LvcColor), it needs
at least two colors. The first color is used for the lowest weight (the "coldest") and the last one for the highest
(the "hottest"), any weight in between gets a color interpolated linearly between them. You can add as many colors as
you need.{{ if xaml }} In XAML, set it as a list of hex colors separated by commas, as the sample above does:
`HeatMap="#FFF176, #2F4F4F, #0000FF"`.{{ end }}

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/heathm.png)

```csharp
using SkiaSharp;
using LiveChartsCore.SkiaSharpView;

Series = new ISeries[]
{
    new HeatSeries<WeightedPoint>
    {
        HeatMap = new[] // mark
        { // mark
            SKColors.Yellow.AsLvcColor(), // the first element is the "coldest" // mark
            SKColors.Black.AsLvcColor(), // mark
            SKColors.Blue.AsLvcColor() // the last element is the "hottest" // mark
        }, // mark
        Values = new ObservableCollection<WeightedPoint> { /* ... */ }
    }
};
```

By default the gradient goes from the lowest to the highest weight in the series. Set `MinValue` and `MaxValue` to
fix that range, for example to keep the same colors while the data changes. Weights outside the range take the first
or the last color.

## Color stops

By default the colors in the `HeatMap` are spread evenly along the gradient. `ColorStops` sets where each color is
placed, from `0` (the coldest) to `1` (the hottest), and must have one item for each color in the `HeatMap`. In the next
sample the black to blue part of the gradient only takes the last 10% of the range, and the yellow to black part
takes the other 90%.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/heatcs.png)

```csharp
using SkiaSharp;
using LiveChartsCore.SkiaSharpView;

Series = new ISeries[]
{
    new HeatSeries<WeightedPoint>
    {
        HeatMap = new[]
        {
            SKColors.Yellow.AsLvcColor(), // the first element is the "coldest" // mark
            SKColors.Black.AsLvcColor(), // mark
            SKColors.Blue.AsLvcColor() // the last element is the "hottest" // mark
        },
        ColorStops = new[] // mark
        { // mark
            0, // mark
            0.9, // mark
            1 // mark
        }, // mark
        Values = new ObservableCollection<WeightedPoint> { /* ... */ }
    }
};
```

## Point padding

`PointPadding` is the space, in pixels, between the edges of the cell and the drawn rectangle. It is `4` on every
side by default. This sample uses `20`, with the data and axes of the basic sample:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/heatp.png)

```csharp
Series = new ISeries[]
{
    new HeatSeries<WeightedPoint>
    {
        PointPadding = new LiveChartsCore.Drawing.Padding(20), // mark
        HeatMap = new[]
        {
            SKColor.Parse("#FFF176").AsLvcColor(), // the first element is the "coldest"
            SKColor.Parse("#2F4F4F").AsLvcColor(),
            SKColor.Parse("#0000FF").AsLvcColor() // the last element is the "hottest"
        },
        Values = new ObservableCollection<WeightedPoint> { /* ... */ }
    }
};
```

## Data labels

Setting `DataLabelsPaint` draws a label on every cell, in the middle of the cell by default. The default label is
the Y coordinate of the point, use `DataLabelsFormatter` to show the weight instead, it is the `TertiaryValue` of the
coordinate:

```csharp
new HeatSeries<WeightedPoint>
{
    Values = new ObservableCollection<WeightedPoint> { /* ... */ },
    DataLabelsPaint = new SolidColorPaint(SKColors.White), // mark
    DataLabelsSize = 12, // mark
    DataLabelsFormatter = point => point.Coordinate.TertiaryValue.ToString("N0") // mark
}
```

## Data, names and updates

- `Values` takes any `IEnumerable<T>`. Use an `ObservableCollection<T>` and the chart animates every added, removed or
  replaced value. `WeightedPoint` also updates the chart when its `X`, `Y` or `Weight` change, see
  [automatic updates]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Automatic%20updates).
- To plot your own types, see [mappers]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Mappers).
- `Name`{{ if xaml }} (`SeriesName` in XAML){{ end }} is shown in tooltips. When it is not set, the tooltip shows the
  values without a series name.
- `ZIndex` sets which series draws on top, and `IsVisible` shows or hides the series.
