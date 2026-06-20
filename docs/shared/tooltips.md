<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
content is normally pulled from the examples in the repository.
-->

## Tooltips

{{~ if name != "Cartesian chart control" ~}}
:::info
This section uses the `CartesianChart` control, but it works the same in the `{{ name  | to_title_case_no_spaces }}` control.
:::
{{~ end ~}}

Tooltips are popups that help the user to read a chart as the pointer moves.

This is a brief sample about how to use the main features of the `IChartTooltip<T>` interface, you can find a more detailed article at the button below or at the 
[API explorer]({{ website_url }}/api/{{ version }}/LiveChartsCore.Kernel.Sketches.IChartTooltip-1).

<a href="{{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Tooltips" class="btn btn-outline-primary mb-3">
<b>Go to the full tooltips article</b>
</a>

![tooltips](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/tooltips.gif)

You can place a tooltip at `Top`, `Bottom`, `Left`, `Right`, `Center` or `Hidden` positions, for now 
tooltips for the `PieChart` class only support the `Center` position, default value is `Top`.

:::info
Notice the `Hidden` position will disable tooltips in a chart.
:::

{{~ if xaml ~}}
```xml
<lvc:CartesianChart
    Series="{Binding Series}"
    TooltipPosition="Top"><!-- mark -->
</lvc:CartesianChart>
<lvc:CartesianChart
    Series="{Binding Series}"
    TooltipPosition="Bottom"><!-- mark -->
</lvc:CartesianChart>
<lvc:CartesianChart
    Series="{Binding Series}"
    TooltipPosition="Left"><!-- mark -->
</lvc:CartesianChart>
<lvc:CartesianChart
    Series="{Binding Series}"
    TooltipPosition="Right"><!-- mark -->
</lvc:CartesianChart>
<lvc:CartesianChart
    Series="{Binding Series}"
    TooltipPosition="Center"><!-- mark -->
</lvc:CartesianChart>
<lvc:CartesianChart
    Series="{Binding Series}"
    TooltipPosition="Hidden"><!-- mark -->
</lvc:CartesianChart>
```
{{~ end ~}}

{{~ if blazor ~}}
```xml
<CartesianChart
    Series="series"
    TooltipPosition="LiveChartsCore.Measure.TooltipPosition.Top"><!-- mark -->
</CartesianChart>
<CartesianChart
    Series="series"
    TooltipPosition="LiveChartsCore.Measure.TooltipPosition.Bottom"><!-- mark -->
</CartesianChart>
<CartesianChart
    Series="series"
    TooltipPosition="LiveChartsCore.Measure.TooltipPosition.Left"><!-- mark -->
</CartesianChart>
<CartesianChart
    Series="series"
    TooltipPosition="LiveChartsCore.Measure.TooltipPosition.Right"><!-- mark -->
</CartesianChart>
<CartesianChart
    Series="series"
    TooltipPosition="LiveChartsCore.Measure.TooltipPosition.Center"><!-- mark -->
</CartesianChart>
<CartesianChart
    Series="series"
    TooltipPosition="LiveChartsCore.Measure.TooltipPosition.Hidden"><!-- mark -->
</CartesianChart>
```
{{~ end ~}}

{{~ if winforms ~}}
```csharp
cartesianChart1.Series = new ISeries[] { new LineSeries<int> { Values = new[] { 2, 5, 4 } } };
cartesianChart1.TooltipPosition = LiveChartsCore.Measure.TooltipPosition.Bottom;
```
{{~ end ~}}
