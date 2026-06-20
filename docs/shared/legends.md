<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
content is normally pulled from the examples in the repository.
-->

## Legends

{{~ if name != "Cartesian chart control" ~}}
:::info
This section uses the `CartesianChart` control, but it works the same in the `{{ name  | to_title_case_no_spaces }}` control.
:::
{{~ end ~}}

A legend is a visual element that displays a list with the name, stroke and fills of the series in a chart:

This is a brief sample about how to use the main features of the `IChartLegend<T>` interface, you can find a more detailed article at the button below or at the 
[API explorer]({{ website_url }}/api/{{ version }}/LiveChartsCore.Kernel.Sketches.IChartLegend-1).

<a href="{{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Legends" class="btn btn-outline-primary mb-3">
<b>Go to the full legends article</b>
</a>

![legends](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/legend.png)

You can place a legend at `Top`, `Bottom`, `Left`, `Right` or `Hidden` positions, notice the `Hidden` position will 
disable legends in a chart, default value is `Hidden`.

{{~ if xaml ~}}
<pre><code>&lt;lvc:CartesianChart
    Series="{Binding Series}"
    LegendPosition="Top">&lt;!-- mark -->
&lt;/lvc:CartesianChart>
&lt;lvc:CartesianChart
    Series="{Binding Series}"
    LegendPosition="Bottom">&lt;!-- mark -->
&lt;/lvc:CartesianChart>
&lt;lvc:CartesianChart
    Series="{Binding Series}"
    LegendPosition="Left">&lt;!-- mark -->
&lt;/lvc:CartesianChart>
&lt;lvc:CartesianChart
    Series="{Binding Series}"
    LegendPosition="Right">&lt;!-- mark -->
&lt;/lvc:CartesianChart>
&lt;lvc:CartesianChart
    Series="{Binding Series}"
    LegendPosition="Hidden">&lt;!-- mark -->
&lt;/lvc:CartesianChart></code></pre>
{{~ end ~}}

{{~ if blazor ~}}
<pre><code>&lt;CartesianChart
    Series="series"
    LegendPosition="LiveChartsCore.Measure.LegendPosition.Top">&lt;!-- mark -->
&lt;/CartesianChart>
&lt;CartesianChart
    Series="series"
    LegendPosition="LiveChartsCore.Measure.LegendPosition.Bottom">&lt;!-- mark -->
&lt;/CartesianChart>
&lt;CartesianChart
    Series="series"
    LegendPosition="LiveChartsCore.Measure.LegendPosition.Left">&lt;!-- mark -->
&lt;/CartesianChart>
&lt;CartesianChart
    Series="series"
    LegendPosition="LiveChartsCore.Measure.LegendPosition.Right">&lt;!-- mark -->
&lt;/CartesianChart>
&lt;CartesianChart
    Series="series"
    LegendPosition="LiveChartsCore.Measure.LegendPosition.Hidden">&lt;!-- mark -->
&lt;/CartesianChart></code></pre>
{{~ end ~}}

{{~ if winforms ~}}
<pre><code>cartesianChart1.TooltipPosition = LiveChartsCore.Measure.LegendPosition.Bottom; // mark
// or use Top, Left, Right or Hidden
</code></pre>
{{~ end ~}}
