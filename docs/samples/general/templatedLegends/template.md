<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
content is normally pulled from the examples in the repository.
-->

# Customize default legends

You can use the chart `LegendPosition`, `LegendTextPaint`, `LegendBackgroundPaint` and `LegendTextSize` to 
define the legend look (full example [here](https://github.com/beto-rodriguez/LiveCharts2/blob/master/samples/{{ samples_folder }}/Axes/Multiple{{ view_extension }})).

![custom](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/legend-custom-style.png)

# Override the default legend behavior

You can inherit from `SKDefaultLegend` and override the `GetLayout()` method to define your own template,
in the next example we set a larger miniature compared with the default size.

#### CustomLegend.cs

```csharp
{{~ render "~/../samples/ViewModelsSamples/General/TemplatedLegends/CustomLegend.cs" ~}}
```

#### LegendItem.cs

```csharp
{{~ render "~/../samples/ViewModelsSamples/General/TemplatedLegends/LegendItem.cs" ~}}
```

#### View

```
{{~ render $"~/../samples/{platform_samples_folder}/General/TemplatedLegends{view_extension}" ~}}
```

![custom legend](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/legend-custom-template.png)

# Legend control from scratch

You can also create your own legend, the recommended way is to use the LiveCharts API but you can
use anything as legend as soon as it implements the `IChartLegend` interface; You can use the [SKDefaultLegend source code](https://github.com/beto-rodriguez/LiveCharts2/blob/master/src/skiasharp/LiveChartsCore.SkiaSharp/SKCharts/SKDefaultLegend.cs) as a guide to build your own implementation, this class is the default legend used by the library.

Instead of using the LiveCharts API you can use your UI framework, 
see [#1558](https://github.com/beto-rodriguez/LiveCharts2/issues/1558) for more info, in that ticket, there is an example 
that implements the `IChartTooltip` interface on a WPF control, then LiveCharts uses this WPF control as the tooltip, even
that example implements `IChartTooltip`, there are no big differences from creating a control that implements `IChartLegend`.
