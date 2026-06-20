<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
-->

# {{ name | to_title_case }}

The `RangeLineSeries<TModel>` draws each point with **two** smoothed cubic-bezier
curves — one through the high endpoints and one through the low endpoints — and
fills the band between them. This is the natural shape for **min / max
envelopes**: daily temperature ranges, forecast confidence intervals, weather
bands, stock high / low channels, sensor tolerance windows, anything where the
underlying quantity lives somewhere inside a per-point range rather than a
single value.

The simplest way to feed the series is the `RangeValue` helper in
`LiveChartsCore.Defaults` (shared with `RangeColumnSeries` and `RangeRowSeries`),
which carries `Low` and `High` and maps to the coordinate slots the series
expects (`PrimaryValue = High`, `TertiaryValue = Low`).

```csharp
Series = new ISeries[]
{
    new RangeLineSeries<RangeValue>
    {
        Name = "Temperature",
        Values = new []
        {
            new RangeValue(low:  8, high: 16),  // Jan
            new RangeValue(low:  9, high: 17),  // Feb
            new RangeValue(low: 11, high: 20),  // Mar
            new RangeValue(low: 13, high: 23),  // Apr
            new RangeValue(low: 16, high: 27),  // May
            new RangeValue(low: 20, high: 31),  // Jun
            // ...
        }
    }
};

XAxes = new[]
{
    new Axis { Labels = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun" } }
};

YAxes = new[]
{
    new Axis { Labeler = v => $"{v:0}°C" }
};
```

{{ render "~/shared/series.md" }}

## Custom models

When you don't want to use `RangeValue`, register a `Mapping` that fills the
coordinate slots from your own type. The slot mapping mirrors
`RangeColumnSeries` and `RangeRowSeries`: `PrimaryValue` carries the high
endpoint, `TertiaryValue` carries the low endpoint, and `SecondaryValue` is the
category index.

```csharp
new RangeLineSeries<MyReading>
{
    Values = myReadings,
    Mapping = (reading, index) =>
        new Coordinate(reading.Max, index, reading.Min, 0, 0, 0, Error.Empty)
}
```

## Paints

The series exposes the usual `Stroke` and `Fill` properties, with a small twist:

- `Stroke` paints **both** the high curve and the low curve.
- `Fill` paints the band area between them.
- `GeometryStroke` and `GeometryFill` style the markers at each high / low endpoint.

A `Stroke = null` (or `Paint.Default`) hides both curves while keeping the band;
a `Fill = null` hides the band while keeping the two curves on their own.

## Tooltip

By default each point's tooltip shows `{low} → {high}` formatted through the Y
axis labeler — so a numeric axis renders numbers, and a `DateTimeAxis` would
render dates without any extra wiring.

To customize the body text — for example to add a unit suffix — set
`YToolTipLabelFormatter`:

```csharp
new RangeLineSeries<RangeValue>
{
    Values = temperatures,
    YToolTipLabelFormatter = point =>
        $"{point.Coordinate.TertiaryValue:0}°C → {point.Coordinate.PrimaryValue:0}°C"
}
```

## Smoothing

`LineSmoothness` controls the cubic-bezier tangent strength on both curves,
exactly like `LineSeries` — `0` = straight polyline, `1` = fully curved (default
`0.65`). The two curves always share the same smoothness; if you need
independently-shaped curves, draw two `LineSeries` instead and skip the band.

## Animation

New points enter at the **midpoint of their [Low, High] range** with zero size,
then expand symmetrically outward to the high and low endpoints. The
`SoftDeleteOrDispose` animation mirrors this: both markers collapse back to the
midpoint as the point is removed. The midpoint baseline preserves the visual
intent of the range — there's no meaningful "pivot" for a series whose points
don't include zero.

## Swapped endpoints

If a point's `Low > High`, each curve still traces whichever endpoint it was
given — so the band crosses over itself at that point. The series does **not**
silently normalize the values; if `[Low, High]` order matters to your data
model, swap them on the way in.

## Stacking

Range line series are **not stackable** — ranges have no natural baseline to
accumulate from.

## Hit-testing and tooltips

The per-point hover area covers the **full band column** at that X (from the
high pixel to the low pixel, half a unit-width on each side of the point), so
hovering anywhere inside the band picks up the point's tooltip. `ExactMatch`
strategies hit the high / low markers themselves.

{{ render "~/shared/series2.md" }}
