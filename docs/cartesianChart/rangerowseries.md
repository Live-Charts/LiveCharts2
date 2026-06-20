<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
-->

# {{ name | to_title_case }}

The `RangeRowSeries<TModel>` is the horizontal mirror of
[RangeColumnSeries]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.RangeColumnSeries):
each point renders as a horizontal rectangle spanning `[Low, High]` on the X
(value) axis. The natural use case is **Gantt charts**, where each task is one
bar that spans the time interval from start to end.

```csharp
var projectStart = new DateTime(2026, 6, 1);

Series = new ISeries[]
{
    new RangeRowSeries<RangeValue>
    {
        Name = "Project",
        Values = new []
        {
            new RangeValue(projectStart.AddDays(0).Ticks,  projectStart.AddDays(5).Ticks),
            new RangeValue(projectStart.AddDays(3).Ticks,  projectStart.AddDays(8).Ticks),
            new RangeValue(projectStart.AddDays(5).Ticks,  projectStart.AddDays(12).Ticks),
            new RangeValue(projectStart.AddDays(7).Ticks,  projectStart.AddDays(14).Ticks),
            new RangeValue(projectStart.AddDays(10).Ticks, projectStart.AddDays(18).Ticks),
            new RangeValue(projectStart.AddDays(14).Ticks, projectStart.AddDays(20).Ticks),
        }
    }
};

XAxes = new[]
{
    new DateTimeAxis(TimeSpan.FromDays(2), d => d.ToString("MMM dd")) { Name = "Date" }
};

YAxes = new[]
{
    new Axis
    {
        Labels = new[] { "Design", "Backend API", "Frontend", "Integration", "Testing", "Deploy" }
    }
};
```

{{ render "~/shared/series.md" }}

## Custom models

You can also map your own type. The slot mapping is the same as
`RangeColumnSeries`: `PrimaryValue = High`, `TertiaryValue = Low`, and
`SecondaryValue` is the category index.

```csharp
new RangeRowSeries<MyTask>
{
    Values = myTasks,
    Mapping = (task, index) =>
        new Coordinate(task.End.Ticks, index, task.Start.Ticks, 0, 0, 0, Error.Empty)
}
```

## Tooltip

The tooltip header shows the **Y axis category label** (the task name for a
Gantt chart). The body shows `{low} → {high}` formatted through the X-axis
labeler — so a `DateTimeAxis` renders dates, a numeric axis renders numbers.

To customize the body — for example the date-range prose typical of Gantt
charts — set `YToolTipLabelFormatter`. The "Y" in the name reflects the
tooltip slot (the body / primary value), not the chart axis:

```csharp
new RangeRowSeries<RangeValue>
{
    Values = tasks,
    YToolTipLabelFormatter = point =>
    {
        var start = new DateTime((long)point.Coordinate.TertiaryValue);
        var end   = new DateTime((long)point.Coordinate.PrimaryValue);
        return $"from {start:MMM dd} to {end:MMM dd}";
    }
}
```

Set `XToolTipLabelFormatter` to override the header instead.

## Animation

New bars enter at the **midpoint of their [Low, High] range** with zero width,
then expand symmetrically outward to the full span. The midpoint baseline
preserves the visual intent of the range — there's no meaningful "pivot" for
a task that starts on day 7 and ends on day 14.

## Swapped endpoints

`Low > High` is treated as `[min(low,high), max(low,high)]` — the rectangle is
normalized through `Math.Min` / `Math.Abs` in `MeasureBarLayout`. No crash, no
inverted bar.

## Stacking

Range row series are **not stackable** — ranges have no natural baseline to
accumulate from.

## Bar styling

The `Rx`, `Ry`, `MaxBarWidth`, `Padding`, `Stroke`, `Fill`, and
`IgnoresBarPosition` properties behave identically to the equivalents on
`RowSeries` / `ColumnSeries`.

{{ render "~/shared/series2.md" }}
