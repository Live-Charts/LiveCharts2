<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
Samples are pulled from the repository with render_sample, the code shown always matches the samples.
-->

# Box series and error bars

A `BoxSeries` draws a box and whisker plot: for each value it draws a box between the first and third quartiles, a
line at the median and whiskers that reach the minimum and the maximum. Error bars are a different feature: column,
row, line and scatter series can draw a line around each point to show the error of the value. Both show how far the
data spreads, so this article covers them together.

:::info
This article highlights the most used features. Every property is listed in the API explorer:
[BoxSeries]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.BoxSeries-3) and
[IErrorSeries]({{ website_url }}/api/{{ version }}/LiveChartsCore.Kernel.Sketches.IErrorSeries).
{{~ if xaml ~}}
In XAML, declare the box series as `XamlBoxSeries`.
{{~ end ~}}
:::

## Box series

Each value of a box series is a `BoxValue`. Its constructor takes the five values in this order: maximum, third
quartile, first quartile, minimum and median. The X coordinate is the index of the value in the collection. Box series
that share the same X coordinate are placed side by side, one per series, and the tooltip shows the values of each box.

{{ render_sample "box/basic" }}

## Stroke and fill paints

`Stroke` draws the whiskers, the median line and the border of the box. `Fill` paints the inside of the box. When
`Fill` is not set, LiveCharts picks it from the current theme, based on the position of the series in the chart's
series collection. The default theme draws the `Stroke` in dark gray, or light gray on dark themes, 2 pixels thick.

`Stroke`:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/box-stroke.png)

```csharp
Series = new ISeries[]
{
    new BoxSeries<BoxValue>
    {
        Values = new BoxValue[]
        {
            // max, third quartile, first quartile, min, median
            new(100, 80, 60, 20, 70),
            new(90, 70, 50, 30, 60),
            new(80, 60, 40, 10, 50)
        },
        Stroke = new SolidColorPaint(SKColors.Red, 3), // mark
        Fill = null
    }
};
```

`Fill`:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/box-filll.png)

```csharp
Series = new ISeries[]
{
    new BoxSeries<BoxValue>
    {
        Values = new BoxValue[]
        {
            // max, third quartile, first quartile, min, median
            new(100, 80, 60, 20, 70),
            new(90, 70, 50, 30, 60),
            new(80, 60, 40, 10, 50)
        },
        Fill = new SolidColorPaint(SKColors.Red) // mark
    }
};
```

:::info
Paints can also draw gradients and dashed lines, see the
[Paints article]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Paints).
:::

## Width and spacing

The space of each category is shared by the box series in it, then two properties adjust each box:

- `MaxBarWidth` limits the width of a box, in pixels. The default theme sets it to `60`.
- `Padding` is the space between the boxes, in pixels. It is `5` by default.

With `MaxBarWidth = 10`:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/box-mw10.png)

```csharp
Series = new ISeries[]
{
    new BoxSeries<BoxValue>
    {
        Values = new BoxValue[] { /* ... */ },
        MaxBarWidth = 10 // mark
    }
};
```

With `MaxBarWidth = double.MaxValue` the boxes take all the space they get:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/box-mw-.png)

```csharp
Series = new ISeries[]
{
    new BoxSeries<BoxValue>
    {
        Values = new BoxValue[] { /* ... */ },
        MaxBarWidth = double.MaxValue // mark
    }
};
```

And with `Padding = 0` too:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/box-mw-p0.png)

```csharp
Series = new ISeries[]
{
    new BoxSeries<BoxValue>
    {
        Values = new BoxValue[] { /* ... */ },
        MaxBarWidth = double.MaxValue,
        Padding = 0 // mark
    }
};
```

## Error bars

`ColumnSeries`, `RowSeries`, `LineSeries` and `ScatterSeries` can draw error bars. Use one of the error types below
as the values of the series and set `ShowError` to `true`; it is `false` by default. This sample draws the same kind
of data with column, line and scatter series.

{{ render_sample "error/basic" }}

## Error values

The error of a point is stored in its value. LiveCharts has three types for it:

- `ErrorValue` takes the Y value and its error. The X coordinate is the index of the value in the collection.
- `ErrorPoint` takes the X and Y values, then the error in X and the error in Y.
- `ErrorDateTimePoint` takes a `DateTime` as X and the Y value, then the error in X and the error in Y.

Errors are distances from the value, in the units of the axis: `new ErrorValue(65, 6)` draws a line from `59` to
`71`. The X coordinate of an `ErrorDateTimePoint` is the `Ticks` of the date, so its error in X is in ticks too.

Each error can take one number, used on both sides, or two numbers, one per side
(`YErrorI` and `YErrorJ`, `XErrorI` and `XErrorJ`):

```csharp
new ErrorValue(65, 6);                    // Y, error in Y // mark
new ErrorValue(70, 15, 4);                // Y, top error, bottom error // mark
new ErrorPoint(0, 50, 0.2, 8);            // X, Y, error in X, error in Y // mark
new ErrorPoint(1, 45, 0.1, 0.3, 15, 4);   // X, Y, left error, right error, top error, bottom error // mark
```

## Error paint

`ErrorPaint` draws the error lines. When `ShowError` is `true` and `ErrorPaint` is not set, the theme sets it: dark
gray on light themes, light gray on dark themes. Setting `ErrorPaint` also turns the error bars on.

```csharp
new ColumnSeries<ErrorValue>
{
    Values = new ErrorValue[] { new(65, 6), new(70, 15, 4), new(35, 4) },
    ErrorPaint = new SolidColorPaint(SKColors.Red, 2) // mark
}
```

## Data, names and updates

- `Values` takes any `IEnumerable<T>`. Use an `ObservableCollection<T>` and the chart animates every added, removed or
  replaced value, see [automatic updates]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Automatic%20updates).
  `BoxValue` and the error types notify their changes too, so changing one of their properties updates the chart.
- To plot your own types, see [mappers]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Mappers). For a
  box series, the mapping returns `new Coordinate(x, max, thirdQuartile, firstQuartile, min, median)`.
- `Name`{{ if xaml }} (`SeriesName` in XAML){{ end }} is shown in tooltips and legends. When it is not set, the
  series is named "Series 1", "Series 2" and so on.
- `ZIndex` sets which series draws on top, and `IsVisible` shows or hides the series.
- For the other properties of the series that draw error bars, see
  [column and row series]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Column%20and%20row%20series),
  [line series]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Line%20and%20step%20line%20series) and
  [scatter series]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Scatter%20and%20bubble%20series).
