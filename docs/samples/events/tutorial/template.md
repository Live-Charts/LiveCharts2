<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
content is normally pulled from the examples in the repository.
-->

# Pressed and hover events

You can request a chart to find the elements in a given position using the `Chart.GetPointsAt()` or
`Chart.GetVisualsAt()` methods.

## Finding hovered or pressed points

When a series point is drawn, it also defines a virtual area called `HoverArea`, this area is what determines
when a point is pressed/hovered; For example in the next gif, the tooltip opens for both Mary and Ana even when
the pointer is not in the drawn shape, this is because the `HoverArea` is not the same as the drawn column.

<div class="text-center sample-img">
    <img src="https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/{{ unique_name }}/hover-area-col.gif" alt="sample image" />
</div>

#### FindingStrategy 

The `Chart.FindingStrategy` property, defines the method to use to find points in the chart, this strategy is used for 
tooltips, hover events and pressed events. By default it is `FindingStrategy.Automatic`, and it means that the chart will decide
the best strategy based on the defaults on each series type, in the next example we use the default automatic strategy in a couple
of column series, you can learn more about the [supported strategies here](https://livecharts.dev/docs/{{ platform }}/{{ version }}/CartesianChart.Tooltips#findingstrategy-property).

## Chart events

This is the recommended way to detect when a `ChartPoint` is pressed or hovered by the user, you can use the `Pressed` or `HoveredPointsChanged`
commands/events to run actions as the user interacts with the chart.

```
{{ render_current_directory_view_model }}
```

```
{{ render_current_directory_view }}
```

In that example, we created the `OnHoveredPointsChanged` method, this method is called every time the "hovered" points change, `hover`
occurs when the pointer is over a `ChartPoint`, we toggle the `Stroke` from `null` to black when the pointer is over a column.

Also in the `OnPressed` method, we toggle the `Fill` from yellow to the default colors of the series, the `Pressed` command/event
is a standard in the library for all the supported UI frameworks, but you can also use the events/commands provided in your
UI framework.

:::tip
Both `OnHoveredPointsChanged` and `OnPressed` are marked with the `RelayCommand` attribute, this generates the `PressedCommand` and 
`HoveredPointsChangedCommand` properties.
:::

When running that example on the `FindingStrategy.Automatic` we get:

<div class="text-center sample-img">
    <img src="https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/{{ unique_name }}/events-auto.gif" alt="sample image" />
</div>

But changing the strategy to `FindingStrategy.ExactMatch` will only trigger the points whose drawn column contains the pointer:

<div class="text-center sample-img">
    <img src="https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/{{ unique_name }}/events-ex.gif" alt="sample image" />
</div>

## Override the find logic

You can also build your own logic by overriding the `Series.FindPointsInPosition` method, for example in the next case,
when the find request is made by a hover action, we return only the points whose hover area contains the pointer in the X axis,
but when the request is made by any other source, we evaluate whether the pointer is inside the Y axis.

```csharp
{{~ render "~/../samples/ViewModelsSamples/Events/OverrideFind/ViewModel.cs" ~}}
```

{{ render "~/shared/relatedTo.md" }}
