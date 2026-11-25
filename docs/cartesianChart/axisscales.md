<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
Samples are pulled from the repository with render_sample, the code shown always matches the samples.
-->

# Time and logarithmic scales

An axis in LiveCharts always works with `double` values. To plot dates, durations or a logarithmic scale, the data is
turned into a number when it is mapped, and the axis turns that number back into a label. LiveCharts includes an axis
for each case: `DateTimeAxis` and `TimeSpanAxis` read the values as ticks, and `LogarithmicAxis` reads them as the
exponent of a base. This article covers only what these axes change; for the rest of the axis properties see the
[axes article]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Axes).

:::info
This article highlights the most used features. Every property is listed in the API explorer:
[DateTimeAxis]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.DateTimeAxis),
[TimeSpanAxis]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.TimeSpanAxis),
[LogarithmicAxis]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.LogarithmicAxis),
[DateTimePoint]({{ website_url }}/api/{{ version }}/LiveChartsCore.Defaults.DateTimePoint) and
[TimeSpanPoint]({{ website_url }}/api/{{ version }}/LiveChartsCore.Defaults.TimeSpanPoint).
{{~ if xaml ~}}
In XAML, declare the axes as `XamlDateTimeAxis`, `XamlTimeSpanAxis` and `XamlLogarithmicAxis`.
{{~ end ~}}
:::

## Date time axis

A `DateTimePoint` has a `DateTime` and a `Value`. Its X coordinate is `DateTime.Ticks` and its Y coordinate is the
value; a `null` value leaves a gap. A `DateTimeAxis` on the X axis turns the ticks back into dates for the labels and
the tooltips.{{ if xaml }} In XAML, the `XamlDateTimeAxis` takes the unit in `Interval`, one day by default, and the
label format in `DateFormatter`, a `Func<DateTime, string>`. Without a `DateFormatter` the labels use the short date
format (`"d"`).{{ end }}

{{ render_sample "axes/dateTimeScaled" }}

In code, the constructor takes the unit and the function that formats each label:

```csharp
Series = new ISeries[]
{
    new ColumnSeries<DateTimePoint>
    {
        Values = new DateTimePoint[]
        {
            new(new DateTime(2021, 1, 1), 3),
            new(new DateTime(2021, 1, 2), 6),
            new(new DateTime(2021, 1, 3), 5)
        }
    }
};

XAxes = new[]
{
    new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("MMMM dd")) // mark
};
```

## The unit

The unit of a `DateTimeAxis` is the distance between two consecutive points, one day in the sample above. The axis
uses it twice:

- `UnitWidth` is set to the unit, in ticks. Series drawn with bars, such as columns, candlesticks and boxes, take the
  width of each bar from it, so with the default `UnitWidth` of `1` a bar would be one tick wide.
- `MinStep` is set to the unit, so the axis never draws two labels closer than one unit. The step is calculated in
  units too: 1, 2, 5, 10, 20 days and so on in the sample. Set `ForceStepToMin` to `true` to draw a label
  on every unit.

Pick the unit that matches your data: `TimeSpan.FromHours(1)` for hourly points, or an average such as
`TimeSpan.FromDays(30.4375)` for months and `TimeSpan.FromDays(365.25)` for years, since months and years do not have
a fixed length.

Because the values are ticks, the limits of the axis are ticks too, for example
`MinLimit = new DateTime(2021, 1, 1).Ticks`. The
[financial series]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Financial%20series#date-time-x-axis)
article shows the same axis with candlesticks.

## Dates in your own types

`DateTimePoint` is optional. Any type works when its mapper returns the ticks as the X coordinate. A `DateTimeAxis` is
an `Axis` with `UnitWidth`, `MinStep` and `Labeler` already set, so a plain `Axis` with the same properties works the
same way; the `AsDate()` extension in the `LiveChartsCore` namespace converts the `double` back into a `DateTime`:

```csharp
Series = new ISeries[]
{
    new LineSeries<Sale>
    {
        Values = sales,
        Mapping = (sale, index) => new(sale.Date.Ticks, sale.Total) // mark
    }
};

XAxes = new[]
{
    new Axis
    {
        UnitWidth = TimeSpan.FromDays(1).Ticks, // mark
        MinStep = TimeSpan.FromDays(1).Ticks, // mark
        Labeler = value => value.AsDate().ToString("MMM dd") // mark
    }
};
```

See [mappers]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Mappers) for more about mapping your own
types.

## Time span axis

`TimeSpanPoint` and `TimeSpanAxis` do the same for durations: the X coordinate is `TimeSpan.Ticks`, and the
constructor of the axis takes the unit and a `Func<TimeSpan, string>` that formats each label. This sample uses a unit
of one millisecond: `new TimeSpanAxis(TimeSpan.FromMilliseconds(1), value => $"{value:fff}ms")`.{{ if xaml }} In XAML,
the `XamlTimeSpanAxis` takes the unit in `Interval` and the format in `TimeFormatter`.{{ end }}

{{ render_sample "axes/timeSpanScaled" }}

The `fff` format shows only the milliseconds part of the duration; for longer spans use a format that includes the
seconds or minutes, or a property such as `TotalMilliseconds`.

## Logarithmic axis

A `LogarithmicAxis` does not transform the data. The data must be mapped to its logarithm, and the axis turns each
exponent back into a value for the labels and the tooltips: with base `10`, the value `3` on the axis is labeled
`1,000.00`. The base of the axis and the base of the mapper must be the same.

This sample registers a mapper for the `LogarithmicPoint` class that takes the base 10 logarithm of `Y`, and uses a
`LogarithmicAxis` with base 10 on the Y axis{{ if xaml }} (`XamlLogarithmicAxis` with `LogBase` in XAML){{ end }}:

{{ render_sample "axes/logarithmic" }}

`LiveCharts.Configure` registers the mapper for every series of that type in the application. To use it on one series
only, set the `Mapping` property of the series instead:

```csharp
Series = new ISeries[]
{
    new LineSeries<LogarithmicPoint>
    {
        Values = values,
        Mapping = (point, index) => new(point.X, Math.Log(point.Y, 10)) // mark
    }
};

YAxes = new[]
{
    new LogarithmicAxis(10) // mark
};
```

`Math.Log` returns `double.NaN` for negative values, which LiveCharts draws as a gap, and `double.NegativeInfinity`
for `0`, which it does not. Map zeros to `Coordinate.Empty` if your data can contain them, see
[NaN and Infinity]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Mappers#nan-and-infinity).

## Log labels and subseparators

The axis sets `MinStep` to `1`, so the separators fall on whole exponents, the powers of the base. Its `Labeler`
returns `Math.Pow(base, value)` with the `"N2"` format. To change the format, set your own `Labeler` and convert the
exponent back the same way:

```csharp
new LogarithmicAxis(10)
{
    Labeler = value => Math.Pow(10, value).ToString("N0") // mark
}
```

Subseparators are spaced on a logarithmic scale too, closer together as they approach the next power of the base.
Set `SubseparatorsCount` to the base minus one, `9` for base 10 as in the sample, so they fall on 2, 3, 4... 9 times
each power. Subseparators are only drawn when `SubseparatorsPaint` is set.
