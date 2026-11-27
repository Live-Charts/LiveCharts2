<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
Samples are pulled from the repository with render_sample, the code shown always matches the samples.
-->

# Range series

A range series draws each point from a low value to a high value, instead of from zero to a single value. A
`RangeLineSeries` draws two curves, one through the high values and one through the low values, and fills the band
between them. A `RangeColumnSeries` draws a vertical bar from the low value to the high value, and a `RangeRowSeries`
draws a horizontal one. All three take the same data, so this article covers them together.

:::info
This article highlights the most used features. Every property is listed in the API explorer:
[RangeLineSeries]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.RangeLineSeries-3),
[RangeColumnSeries]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.RangeColumnSeries-3) and
[RangeRowSeries]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.RangeRowSeries-3).
{{~ if xaml ~}}
In XAML, declare them as `XamlRangeLineSeries`, `XamlRangeColumnSeries` and `XamlRangeRowSeries`.
{{~ end ~}}
:::

## Range values

The `RangeValue` class, in the `LiveChartsCore.Defaults` namespace, holds the two ends of each point in its `Low` and
`High` properties. The X coordinate of a point is its index in `Values`, like a plain number in a line or column
series.

```csharp
Series = new ISeries[]
{
    new RangeLineSeries<RangeValue>
    {
        Values = new RangeValue[]
        {
            new(low: 8, high: 16), // mark
            new(low: 9, high: 17), // mark
            new(low: 11, high: 20) // mark
        }
    }
};
```

`RangeValue` notifies when `Low` or `High` change, and the chart updates the point. When either of them is `null`,
the point is empty: a range line breaks there, and a range bar is not drawn.

To plot your own type, set a `Mapping` that puts the high value in the `PrimaryValue` of the coordinate and the low
value in the `TertiaryValue`, which are the slots `RangeValue` uses:

```csharp
new RangeLineSeries<Reading>
{
    Values = readings,
    // PrimaryValue = high, SecondaryValue = X, TertiaryValue = low
    Mapping = (reading, index) =>
        new Coordinate(reading.Max, index, reading.Min, 0, 0, 0, Error.Empty) // mark
}
```

When the axis limits are not set, the value axis grows to fit both the low and the high values.

## Range line

`Stroke` draws both curves and `Fill` paints the band between them. `GeometryFill`, `GeometryStroke` and
`GeometrySize` style the point drawn at both ends of every value. Set `Fill` to `null` to draw only the two curves,
or set `Stroke` to `null` to draw only the band. When the paints are not set, the default theme styles the series like a
line series: a 4 pixel stroke, the same color with transparency for the band, and 12 pixel points.

This sample plots the monthly low and high temperatures. The X axis uses `Labels` for the months, and the Y axis a
`Labeler` that adds the unit.

{{ render_sample "lines/range" }}

The hover area of each value covers the whole band at that X, from the low point to the high point, so the tooltip
opens anywhere inside the band, not only over the points.

## Range columns

A `RangeColumnSeries` draws each bar from `Low` to `High` on the Y axis. `Low` does not need to be the smaller
value, the bar always covers the space between both.

A waterfall chart uses that: each bar goes from the previous total (`Low`) to the new total (`High`), so a step that
lowers the total has a `Low` greater than its `High`. The first and last bars start at zero to show the totals.

```csharp
Series = new ISeries[]
{
    new RangeColumnSeries<RangeValue> // mark
    {
        Name = "Cash flow",
        Values = new RangeValue[]
        {
            new(0, 100),   // Start
            new(100, 150), // Sales, +50
            new(150, 180), // Other income, +30
            new(180, 130), // Costs, -50
            new(130, 110), // Tax, -20
            new(0, 110)    // End
        }
    }
};

XAxes = new[]
{
    new Axis { Labels = new[] { "Start", "Sales", "Other income", "Costs", "Tax", "End" } }
};
```

The complete sample, with a tooltip that shows the change of each step, is in the
[waterfall sample]({{ website_url }}/docs/{{ platform }}/{{ version }}/samples.bars.waterfall).

## Range rows

A `RangeRowSeries` draws each bar from `Low` to `High` on the X axis. A Gantt chart uses it with dates: each task
stores the `Ticks` of its start and end dates, a `DateTimeAxis` on the X axis turns the ticks back into dates, and the
Y axis names the tasks with `Labels`.

```csharp
var start = new DateTime(2026, 6, 1);

Series = new ISeries[]
{
    new RangeRowSeries<RangeValue> // mark
    {
        Name = "Project",
        Values = new RangeValue[]
        {
            new(start.Ticks, start.AddDays(5).Ticks),             // Design
            new(start.AddDays(3).Ticks, start.AddDays(8).Ticks),  // Backend API
            new(start.AddDays(5).Ticks, start.AddDays(12).Ticks)  // Frontend
        }
    }
};

XAxes = new[]
{
    new DateTimeAxis(TimeSpan.FromDays(2), date => date.ToString("MMM dd"))
};

YAxes = new[]
{
    new Axis { Labels = new[] { "Design", "Backend API", "Frontend" } }
};
```

The complete sample is in the
[Gantt sample]({{ website_url }}/docs/{{ platform }}/{{ version }}/samples.bars.gantt).

## Tooltips

By default, the tooltip shows each point as `low → high`, both formatted by the value axis: its `Labels` when it has
them, otherwise its `Labeler`. The value axis is the Y axis for range lines and columns, and the X axis for range
rows, so in the Gantt chart above the tooltip already shows dates.

`YToolTipLabelFormatter` replaces that text. In the formatter, `point.Coordinate.TertiaryValue` is the low value and
`point.Coordinate.PrimaryValue` is the high value. The range line sample above adds the unit to both values, and the
waterfall sample shows the change of each step:

```csharp
new RangeColumnSeries<RangeValue>
{
    Values = steps,
    YToolTipLabelFormatter = point => // mark
    {
        var from = point.Coordinate.TertiaryValue; // mark
        var to = point.Coordinate.PrimaryValue; // mark
        var delta = to - from;
        var sign = delta >= 0 ? "+" : "−";
        return $"{from:N0} → {to:N0} ({sign}{Math.Abs(delta):N0})";
    }
}
```

The Gantt sample turns both values back into dates with `new DateTime((long)point.Coordinate.TertiaryValue)`.

On a range row, `YToolTipLabelFormatter` still sets this text, even though the values are on the X axis. The title of
the tooltip is the label of the Y axis for that row, and `XToolTipLabelFormatter` replaces it.

## Shared with line and column series

Range series take most properties of the series they are based on, so these articles apply to them too:

- The range line has the paints, `LineSmoothness`, `GeometrySize`, `EnableNullSplitting` and data labels of the
  [line series]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Line%20and%20step%20line%20series).
  `LineSmoothness` applies to both curves.
- Range columns and rows have the paints, `Rx`, `Ry`, `MaxBarWidth`, `Padding` and `IgnoresBarPosition` of the
  [column and row series]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Column%20and%20row%20series).
  The theme styles them like the other bars.

Range series cannot be stacked. When a point is added, it grows from the middle of its range to both ends.

## Data, names and updates

- `Values` takes any `IEnumerable<T>`. Use an `ObservableCollection<T>` and the chart animates every added, removed or
  replaced value, see [automatic updates]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Automatic%20updates).
- To plot your own types, see [mappers]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Mappers).
- `Name`{{ if xaml }} (`SeriesName` in XAML){{ end }} is shown in tooltips and legends. When it is not set, the
  series is named "Series 1", "Series 2" and so on.
- `ZIndex` sets which series draws on top, and `IsVisible` shows or hides the series.
