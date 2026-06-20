<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
-->

# Tooltips

Tooltips are popups that help the user to read a chart as the pointer moves.

![tooltips](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/tooltips.gif)

## Behaviour

On **Windows** or **MacOS** you can move the pointer over the chart to display the tooltip, tooltips will be closed when the
pointer leaves the chart area.

On **Android** or **iOS** slide your finger over the chart to display the tooltip, the tooltip will be closed when the finger
goes up.

## TooltipPosition property

You can place a tooltip at `Top`, `Bottom`, `Left`, `Right`, `Center` or `Hidden` positions, for now 
tooltips for the `PieChart` class only support the `Center` position, default value is `Top`.

Notice the `Hidden` position will disable tooltips in a chart.

{{~ if xaml ~}}
<pre><code>&lt;lvc:CartesianChart
    Series="{Binding Series}"
    TooltipPosition="Top">&lt;!-- mark -->
&lt;/lvc:CartesianChart>
&lt;lvc:CartesianChart
    Series="{Binding Series}"
    TooltipPosition="Bottom">&lt;!-- mark -->
&lt;/lvc:CartesianChart>
&lt;lvc:CartesianChart
    Series="{Binding Series}"
    TooltipPosition="Left">&lt;!-- mark -->
&lt;/lvc:CartesianChart>
&lt;lvc:CartesianChart
    Series="{Binding Series}"
    TooltipPosition="Right">&lt;!-- mark -->
&lt;/lvc:CartesianChart>
&lt;lvc:CartesianChart
    Series="{Binding Series}"
    TooltipPosition="Center">&lt;!-- mark -->
&lt;/lvc:CartesianChart>
&lt;lvc:CartesianChart
    Series="{Binding Series}"
    TooltipPosition="Hidden">&lt;!-- mark -->
&lt;/lvc:CartesianChart></code></pre>
{{~ end ~}}

{{~ if blazor ~}}
<pre><code>&lt;CartesianChart
    Series="series"
    TooltipPosition="LiveChartsCore.Measure.TooltipPosition.Top">&lt;!-- mark -->
&lt;/CartesianChart>
&lt;CartesianChart
    Series="series"
    TooltipPosition="LiveChartsCore.Measure.TooltipPosition.Bottom">&lt;!-- mark -->
&lt;/CartesianChart>
&lt;CartesianChart
    Series="series"
    TooltipPosition="LiveChartsCore.Measure.TooltipPosition.Left">&lt;!-- mark -->
&lt;/CartesianChart>
&lt;CartesianChart
    Series="series"
    TooltipPosition="LiveChartsCore.Measure.TooltipPosition.Right">&lt;!-- mark -->
&lt;/CartesianChart>
&lt;CartesianChart
    Series="series"
    TooltipPosition="LiveChartsCore.Measure.TooltipPosition.Center">&lt;!-- mark -->
&lt;/CartesianChart>
&lt;CartesianChart
    Series="series"
    TooltipPosition="LiveChartsCore.Measure.TooltipPosition.Hidden">&lt;!-- mark -->
&lt;/CartesianChart></code></pre>
{{~ end ~}}

{{~ if winforms ~}}
<pre><code>cartesianChart1.Series = new ISeries[] { new LineSeries&lt;int> { Values = new[] { 2, 5, 4 } } };
cartesianChart1.TooltipPosition = LiveChartsCore.Measure.TooltipPosition.Bottom;</code></pre>
{{~ end ~}}

## FindingStrategy property

Every point drawn by the library defines a [HoverArea]({{ website_url }}/api/{{ version }}/LiveChartsCore.Kernel.Drawing.HoverArea),
it defines an area in the chart that "triggers" the point, it is specially important to fire tooltips, a point will be included in a
tooltip when the hover area was triggered by the pointer position.

The `FindingStrategy` property determines the hover area planes (X or Y) that a chart will use to trigger the `HoverArea` instances.
In a chart, the following options are available:

**CompareAll**: Selects all the points whose hover area contain the pointer position.

![tooltips](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/tsm_cac.gif)

**CompareOnlyX**: Selects all the points whose hover area contain the pointer position, but it ignores the Y plane.

![tooltips](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/tsm_cxc.gif)

**CompareOnlyY**: Selects all the points whose hover area contain the pointer position, but it ignores the X plane.

![tooltips](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/tsm_cyc.gif)

**ExactMatch**: Selects all the points whose drawn shape contain the pointer position.

![tooltips](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/tsm_cec.gif)

**CompareAllTakeClosest**: Selects the points whose hover area contain the pointer position, 
if overlapped then takes the closest to the pointer in each series.

**CompareOnlyXTakeClosest**: Selects the points whose hover area contain the pointer position, but it ignores the Y plane,
if overlapped then takes the closest to the pointer in each series.

**CompareOnlyYTakeClosest**: Selects the points whose hover area contain the pointer position, but it ignores the X plane,
if overlapped then takes the closest to the pointer in each series.

**ExactMatchTakeClosest**: Selects all the points whose drawn shape contain the pointer position,
if overlapped then takes the closest to the pointer in each series.

**Automatic** *(default)*: Based on the series in the chart, LiveCharts will determine a finding strategy (one of the previous mentioned),
all the series have a preferred finding strategy, normally vertical series prefer the `CompareOnlyXTakeClosest` strategy, 
horizontal series prefer `CompareOnlyYTakeClosest`, and scatter series prefers `CompareAllTakeClosest`, if all the series prefer the same 
strategy, then that strategy will be selected for the chart, if any series differs then the `CompareAllTakeClosest` strategy will be used.

:::info
Notice that the `Axis.UnitWidth` property might affect the tooltips in `DateTime` scaled charts, ensure your chart axis is using
the properly [unit width]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Axes%20properties#unitwidth-property).
:::

{{~ if xaml ~}}
<pre><code>&lt;lvc:CartesianChart
    Series="{Binding Series}"
    FindingStrategy="CompareOnlyX">&lt;!-- mark -->
&lt;/lvc:CartesianChart></code></pre>
{{~ end ~}}

{{~ if blazor ~}}
<pre><code>&lt;CartesianChart
    Series="series"
    FindingStrategy="LiveChartsCore.Measure.FindingStrategy.CompareOnlyX">&lt;!-- mark -->
&lt;/CartesianChart></code></pre>
{{~ end ~}}

{{~ if winforms ~}}
<pre><code>cartesianChart1.Series = new ISeries[] { new LineSeries&lt;int> { Values = new[] { 2, 5, 4 } } };
cartesianChart1.FindingStrategy = LiveChartsCore.Measure.FindingStrategy.CompareOnlyX;</code></pre>
{{~ end ~}}

## Customize FindingStrategy

Depending on the series type and [FindingStrategy](https://livecharts.dev/docs/{{ platform }}/{{ version }}/CartesianChart.Tooltips#findingstrategy-property), LiveCharts decides the logic to show points on tooltips and also the points passed
to any pointer event in the library (like `Hover`, `HoverLeft` or `PointerDown`), lets take as an example the default behavior of 
the `ColumnSeries<T>`, it selects all the points that share the same `X` coordinate:

![custom tooltip](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/custom-ha-compare.gif)

But for this example, we want to override this behavior, instead we only need the tooltip to display the exact column where the pointer is in:

![custom tooltip](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/custom-ha.gif)

When the [FindingStrategy](https://livecharts.dev/docs/{{ platform }}/{{ version }}/CartesianChart.Tooltips#findingstrategy-property), 
is not enough, we can override the logic to determine whether a given point is inside a drawn `ChartPoint`. This method
will be used by the library to resolve the points to show in a tooltip, or the points passed in any pointer event:

```csharp
{{~ render "~/../samples/ViewModelsSamples/Events/OverrideFind/ViewModel.cs" ~}}
```

:::tip
Another alternative is to override the `HoverArea`, this area defines a section in the UI where the point is "selectable", you can find an example [here](https://github.com/beto-rodriguez/LiveCharts2/blob/master/samples/ViewModelsSamples/General/TooltipHoverArea/ViewModel.cs).
:::

## Tooltip text

You can define the text the tooltip will display for a given point, using the 
`YToolTipLabelFormatter`, `XToolTipLabelFormatter` or `ToolTipLabelFormatter` properties, these 
properties are of type `Func<ChartPoint, string>` it means that both are a function, that takes a point as parameter
and return a string, the point will be injected by LiveCharts in this function to get a string out of it when it
requires to build the text for a point in a tooltip, the injected point will be different as the user moves the pointer 
over the user interface.

By default the library already defines a default formatter  for every series, all the series have a different
formatters, but generally the default value uses the `Series.Name` and the `ChartPoint.Coordinate.PrimaryValue` properties, the following
code snippet illustrates how to build a custom tooltip formatter.

Lets take the example of the next series:"

<pre><code>public ISeries[] Series { get; set; } = [
    new LineSeries&lt;double>
    {
        Values = [2, 1, 3, 5, 3, 4, 6],
        Fill = null,
        GeometrySize = 20,
    },
    new LineSeries&lt;int, StarGeometry>
    {
        Values = [4, 2, 5, 2, 4, 5, 3],
        Fill = null,
        GeometrySize = 20
    }
];</code></pre>

By default the tooltip will be:

![tooltip](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/tooltip-format1.png)

We can add format to the tooltip:

<pre><code>public ISeries[] Series { get; set; } = [
    new LineSeries&lt;double>
    {
        Values = [2, 1, 3, 5, 3, 4, 6],
        Fill = null,
        GeometrySize = 20,
        YToolTipLabelFormatter = point => point.Model.ToString("N2") // mark
    },
    new LineSeries&lt;int, StarGeometry>
    {
        Values = [4, 2, 5, 2, 4, 5, 3],
        Fill = null,
        GeometrySize = 20,
        YToolTipLabelFormatter = point => point.Model.ToString("N2") // mark
    }
];</code></pre>

![tooltip](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/tooltip-format2.png)

We used the Model property of the point, the Model property is just the item in the Values
collection, for example in the next case, the Model property is of type `City`.

<pre><code>public ISeries[] Series { get; set; } = [
    new LineSeries&lt;City>
    {
        Values = [new() { Population = 4 }, new() { Population = 2}],
        YToolTipLabelFormatter = point => point.Model.Population.ToString("N2") // mark
    }
];

// ...

public class City
{
    public double Population { get; set; }
}</code></pre>

We can also show a label for the `X` coordinate, the default tooltip uses the X label as the header in the tooltip.

<pre><code>new LineSeries&lt;double>
{
    Values = [2, 1, 3, 5, 3, 4, 6],
    Fill = null,
    GeometrySize = 20,
    XToolTipLabelFormatter = point => point.Index.ToString(), // mark
    YToolTipLabelFormatter = point => point.Model.ToString("C2")
};</code></pre>

![tooltip](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/tooltip-format3.png)

When the series is "Stacked" (`PieSeries`, `StackedColumn` or `StackedRow`) we can find information about the stacked data
in the `StackedValue` property, for example:


<pre><code>public ISeries[] Series { get; set; } = [
    new StackedColumnSeries&lt;double>
    {
        Values = [2, 1, 3, 5, 3, 4, 6],
        YToolTipLabelFormatter =
            point => $"{point.Model} / {point.StackedValue!.Total} ({point.StackedValue.Share:P2})"
    },
    new StackedColumnSeries&lt;int>
    {
        Values = [4, 2, 5, 2, 4, 5, 3],
        YToolTipLabelFormatter =
            point => $"{point.Model} / {point.StackedValue!.Total} ({point.StackedValue.Share:P2})"
    }
];</code></pre>

Will result in:

![tooltip](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/tooltip-format4.png)

:::tip
The PieSeries class uses the `ToolTipLabelFormatter` property to configure the text inside the tooltip.
:::

# Customize default tooltips

You can quickly change the position, the font, the text size or the background color:

#### {{ view_title }}

```
{{~ render $"~/../samples/{platform_samples_folder}/Axes/NamedLabels{view_extension}" ~}}
```

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/samples/general/customTooltips/styling-tooltips.png)

## Custom tooltip control

You can also create your own tooltip, the recommended way is to use the LiveCharts API (example below) but you can
use anything as tooltip as soon as it implements the `IChartTooltip<T>` interface. In the following example we build
a custom control to render tooltips in our charts using the LiveCharts API.

:::tip
The next tooltip is drawn by the library, LiveCharts can only draw inside the control bounds, in some cases it could 
cause issues like [#912](https://github.com/beto-rodriguez/LiveCharts2/issues/912).

Alternatively, you can build your own Tooltips and use the power of your UI framework, 
see [#1558](https://github.com/beto-rodriguez/LiveCharts2/issues/1558) for more info.
:::

#### CustomTooltip.cs

```csharp
{{~ render "~/../samples/ViewModelsSamples/General/TemplatedTooltips/CustomTooltip.cs" ~}}
```

#### View

```
{{~ render $"~/../samples/{platform_samples_folder}/General/TemplatedTooltips{view_extension}" ~}}
```

![custom tooltip](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/tooltip-custom-template.gif)
