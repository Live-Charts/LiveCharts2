<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
-->

# Legends

A legend is a visual element that displays a list with the name, stroke and fills of the series in a chart:

![legends](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/legend.png)

You can place a legend at `Top`, `Bottom`, `Left`, `Right` or `Hidden` positions, notice the `Hidden` position will 
disable legends in a chart, default value is `Hidden`.

{{~ if xaml ~}}
```xml
<lvc:CartesianChart
    Series="{Binding Series}"
    LegendPosition="Top"><!-- mark -->
</lvc:CartesianChart>
<lvc:CartesianChart
    Series="{Binding Series}"
    LegendPosition="Bottom"><!-- mark -->
</lvc:CartesianChart>
<lvc:CartesianChart
    Series="{Binding Series}"
    LegendPosition="Left"><!-- mark -->
</lvc:CartesianChart>
<lvc:CartesianChart
    Series="{Binding Series}"
    LegendPosition="Right"><!-- mark -->
</lvc:CartesianChart>
<lvc:CartesianChart
    Series="{Binding Series}"
    LegendPosition="Hidden"><!-- mark -->
</lvc:CartesianChart>
```
{{~ end ~}}

{{~ if blazor ~}}
```xml
<CartesianChart
    Series="series"
    LegendPosition="LiveChartsCore.Measure.LegendPosition.Top"><!-- mark -->
</CartesianChart>
<CartesianChart
    Series="series"
    LegendPosition="LiveChartsCore.Measure.LegendPosition.Bottom"><!-- mark -->
</CartesianChart>
<CartesianChart
    Series="series"
    LegendPosition="LiveChartsCore.Measure.LegendPosition.Left"><!-- mark -->
</CartesianChart>
<CartesianChart
    Series="series"
    LegendPosition="LiveChartsCore.Measure.LegendPosition.Right"><!-- mark -->
</CartesianChart>
<CartesianChart
    Series="series"
    LegendPosition="LiveChartsCore.Measure.LegendPosition.Hidden"><!-- mark -->
</CartesianChart>
```
{{~ end ~}}

{{~ if winforms ~}}
```csharp
cartesianChart1.LegendPosition = LiveChartsCore.Measure.LegendPosition.Bottom; // mark
// or use Top, Left, Right or Hidden
```
{{~ end ~}}

# Customize default legends

You can use the chart `LegendPosition`, `LegendTextPaint`, `LegendBackgroundPaint` and `LegendTextSize` to 
define the legend look (full example [here](https://github.com/beto-rodriguez/LiveCharts2/blob/master/samples/{{ samples_folder }}/Axes/Multiple{{ view_extension }})).

![custom](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/legend-custom-style.png)

## Custom legend control

You can also create your own legend. The recommended way is to use the LiveCharts API (example below), but you can
use anything as a legend as long as it implements the `IChartLegend<T>` interface. In the following example we build
a custom control to render legends in our charts using the LiveCharts API.

## CustomLegend.cs

```csharp
{{~ render "~/../samples/ViewModelsSamples/General/TemplatedLegends/CustomLegend.cs" ~}}
```

## View

```
{{~ render $"~/../samples/{platform_samples_folder}/General/TemplatedLegends{view_extension}" ~}}
```

![custom tooltip](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/legend-custom-template.png)
