<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
content is normally pulled from the examples in the repository.
-->

In this example, both axes `X` and `Y` use the same scale, it means that the amount of space in the UI
is the same for both axes per data unit. We use the `ICartesianChartView`.`MatchAxesScreenDataRatio` property, 
this will set the scale up for us, but we can also build custom scales if necessary.

## MatchAxesScreenDataRatio property

When the `ICartesianChartView`.`MatchAxesScreenDataRatio` is `true`,  both axes will take the same number of pixels per data unit.

```
{{ render_current_directory_view }}
```

```
{{ render_current_directory_view_model }}
```

Now both axes use the same scale. We can easily notice this in the grid drawn by the axes separators. This grid is composed of perfect rectangles,
no matter if we zoom in/out (or use the panning feature).

<div class="text-center sample-img">
    <img src="https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/{{ unique_name }}/matchsdr.gif" alt="sample image" />
</div>

## Custom Axis scale

When using the `MatchAxesScreenDataRatio`, LiveCharts used the [MatchAxesScreenDataRatio](https://github.com/beto-rodriguez/LiveCharts2/blob/master/src/LiveChartsCore/SharedAxes.cs#L68), this function uses the `DrawMarginDefined` event to modify the axes range to match the same scale,
in the next example we will define our own scale, in this case we want the Y axis to take the double of pixels per unit of data.

```csharp
{{~ render "~/../samples/ViewModelsSamples/Axes/MatchScale/CustomScaleExtensions.cs" ~}}
```

Using the previous example, we must remove the `MatchAxesScreenDataRatio=True`, then we take the chart instance in the UI and 
call our function to initialize the custom scale:

```csharp
// where myChart is a reference to chart in the UI.
CustomScaleExtensions.DoubleY(myChart);
```

Once we run our app again, we can see that our scale works as expected:

<div class="text-center sample-img">
    <img src="https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/{{ unique_name }}/doubley.png" alt="sample image" />
</div>

## Scale separators to 1 inch sample

In the following example, we will force the axis separators to be 1 inch long (2.54 cm).

```csharp
{{~ render "~/../samples/ViewModelsSamples/Axes/MatchScale/InchScaleExtensions.cs" ~}}
```

:::tip
The `GetPixelsPerInch()` function, returns always `96`, this is a standard value, but varies depending on the screen,
and the OS, in a real app you must get the device DPI/PPI, this differs between UI frameworks, please check the
docs of your target UI framework.
:::

Finally we need to use our `InchSeparator()` extension in our chart:

```csharp
// where myChart is a reference to chart in the UI.
InchScaleExtensions.InchSeparator(myChart);
```

And that's it, now our separators will be one inch long always, no matter the chart size, the data on it, or the zooming
and panning level.

<div class="text-center sample-img">
    <img src="https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/{{ unique_name }}/inch-long-separator.jpg" alt="sample image" style="max-width: 650px" />
</div>

{{ render "~/shared/relatedTo.md" }}
