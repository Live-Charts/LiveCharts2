<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
-->

# {{ name | to_title_case }}

The `RangeColumnSeries<TModel>` draws each point as a vertical rectangle that
spans `[Low, High]` on the value axis — instead of `[Pivot, Value]` like a
regular `ColumnSeries`. This is the natural shape for **waterfall charts**
(running-total visualizations), **error-range columns**, or any "this metric
lives somewhere in this range" presentation.

The simplest way to feed a range series is the `RangeValue` helper in
`LiveChartsCore.Defaults`, which carries `Low` and `High` and maps to the
coordinate slots the series expects (`PrimaryValue = High`, `TertiaryValue = Low`).

```csharp
Series = new ISeries[]
{
    new RangeColumnSeries<RangeValue>
    {
        Values = new []
        {
            new RangeValue(0,  100),   // Start balance — anchored at 0
            new RangeValue(100, 150),  // Sales            (+50)
            new RangeValue(150, 180),  // Other income     (+30)
            new RangeValue(180, 130),  // Costs            (−50)
            new RangeValue(130, 110),  // Tax              (−20)
            new RangeValue(0,  110),   // End balance      — anchored at 0
        }
    }
};
```

{{ render "~/shared/series.md" }}

## Custom models

When you don't want to use `RangeValue`, register a `Mapping` that fills the
coordinate slots from your own type:

```csharp
new RangeColumnSeries<MyCashFlowStep>
{
    Values = mySteps,
    Mapping = (step, index) =>
        new Coordinate(step.High, index, step.Low, 0, 0, 0, Error.Empty)
}
```

`PrimaryValue` carries the high endpoint, `TertiaryValue` carries the low
endpoint, and `SecondaryValue` is the category index — this mirrors the
contract the series engine reads.

## Tooltip

By default each bar's tooltip shows `{low} → {high}` formatted through the
**value axis** labeler, so a numeric axis renders numbers and a `DateTimeAxis`
renders dates without any extra wiring. The category label (the X axis
`Labels[index]`) appears as the tooltip header, matching the convention used by
plain `ColumnSeries`.

To customize the body text — for example to show the delta on a waterfall —
set `YToolTipLabelFormatter`:

```csharp
new RangeColumnSeries<RangeValue>
{
    Values = steps,
    YToolTipLabelFormatter = point =>
    {
        var from = point.Coordinate.TertiaryValue;
        var to   = point.Coordinate.PrimaryValue;
        var delta = to - from;
        var sign  = delta >= 0 ? "+" : "−";
        return $"{from:N0} → {to:N0} ({sign}{Math.Abs(delta):N0})";
    }
}
```

## Animation

A new bar enters at the **midpoint of its [Low, High] range** with zero height
and expands symmetrically outward. The collapse animation (when a point is
removed) mirrors the same midpoint baseline. This differs from regular
`ColumnSeries`, which grows from the pivot — pivot 0 is meaningless for ranges
that don't include zero, so the midpoint is the natural baseline.

## Swapped endpoints

`Low > High` is a user error but does not crash — `MeasureBarLayout` normalizes
the rectangle via `Math.Min` / `Math.Abs`. The rendered span is `|High − Low|`
regardless of input order.

## Stacking

Range column series are **not stackable** — ranges have no natural baseline to
accumulate from. Use plain `StackedColumnSeries` for stacked column charts.

## Bar styling

The `Rx`, `Ry`, `MaxBarWidth`, `Padding`, `Stroke`, `Fill`, and
`IgnoresBarPosition` properties behave identically to the equivalents on
`ColumnSeries` — see the [Column Series article]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.ColumnSeries)
for examples.

{{ render "~/shared/series2.md" }}
