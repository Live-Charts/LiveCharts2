<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
content is normally pulled from the examples in the repository.
-->

# Customize default tooltips

:::tip
The next article is a quick guide on how to customize the default tooltip. If you want to learn more, you can read the full
article:

<a href="{{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Tooltips" class="btn btn-outline-primary mb-3">
<b>Go to the full tooltips article</b>
</a>
:::

You can quickly change the position, the font, the text size or the background color:

#### View

{{~ render $"~/../samples/{platform_samples_folder}/Axes/NamedLabels{view_extension}" ~}}

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/samples/general/customTooltips/styling-tooltips.png)

# Customize tooltip format

You can define the text the tooltip will display for a given point, using the 
`YToolTipLabelFormatter`, `XToolTipLabelFormatter` or `ToolTipLabelFormatter` properties, these 
properties are of type `Func<ChartPoint, string>` it means that both are a function, that takes a point as parameter
and return a string, the point will be injected by LiveCharts in this function to get a string out of it when it
requires to build the text for a point in a tooltip, the injected point will be different as the user moves the pointer over the
user interface.

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

We can also show a label for the `X` coordinate, the default tooltip uses the X label as the header.

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

# Override the default tooltip behavior

You can also inherit from `SKDefaultTooltip` and override the parts you need to make the tooltip behave as your app needs,
in the next example, we draw a geometry in the tooltip based on the point that is shown in the tooltip.

![custom tooltip](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/tooltip-custom-template.gif)

#### CustomTooltip.cs

```csharp
{{~ render $"~/../samples/ViewModelsSamples/General/TemplatedTooltips/CustomTooltip.cs" ~}}
```

#### View

```csharp
{{~ render $"~/../samples/{platform_samples_folder}/General/TemplatedTooltips{view_extension}" ~}}
```

{{~ if xaml2006 ~}}
## CustomGeometryPointColumnSeries.cs
```csharp
{{~ render $"~/../samples/{platform_samples_folder}/General/TemplatedTooltips/CustomGeometryPointColumnSeries.cs" ~}}
```
{{~ end ~}}

# Tooltip control from scratch

You can also create your own tooltip, the recommended way is to use the LiveCharts API but you can
use anything as tooltip as soon as it implements the `IChartTooltip` interface. 

The LiveCharts API can only draw inside the control bounds, in some cases it could 
cause issues like [#912](https://github.com/beto-rodriguez/LiveCharts2/issues/912), you can use the [SKDefaultTooltip source code](https://github.com/beto-rodriguez/LiveCharts2/blob/master/src/skiasharp/LiveChartsCore.SkiaSharp/SKCharts/SKDefaultTooltip.cs) as a guide to build your own implementation, this class is the default tooltip used by the library.

Alternatively, you can build your own Tooltips and use the power of your UI framework, 
see [#1558](https://github.com/beto-rodriguez/LiveCharts2/issues/1558) for more info.

# Override Series.FindPointsInPosition 

Depending on the series type and [FindingStrategy](https://livecharts.dev/docs/{{ platform }}/{{ version }}/CartesianChart.Tooltips#findingstrategy-property), LiveCharts decides the logic to show points on tooltips and also the points passed
to any pointer event in the library (like `Hover`, `HoverLeft` or `PointerDown`), lets take as an example the default behavior of the
`ColumnSeries<T>`, it selects all the points that share the same `X` coordinate:

![custom tooltip](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/custom-ha-compare.gif)

But for this example, we want to override this behavior, instead we only need the tooltip to display the exact column where the pointer is in:

![custom tooltip](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/custom-ha.gif)

When the [FindingStrategy](https://livecharts.dev/docs/{{ platform }}/{{ version }}/CartesianChart.Tooltips#findingstrategy-property), 
is not enough, we can override the logic to determine whether a given point is inside a drawn `ChartPoint`. This method
will be used by the library to resolve the points to show in a tooltip, or the points passed in any pointer event:

```csharp
{{~ render "~/../samples/ViewModelsSamples/Events/OverrideFind/ViewModel.cs" ~}}
```
