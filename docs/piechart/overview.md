<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
-->

# The Pie Chart Control

:::info
This article is a quick guide to use the `PieChart` control, you can explore all the properties and the source code 
at the [ApiExplorer]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.{{ platform }}.PieChart).
:::

The `PieChart` control can build Pie, Doughnut and gauges charts, this article will cover only Pie and Doughnut charts,
if you need to know more about gauges please read 
[this guide]({{ website_url }}/docs/{{ platform }}/{{ version }}/PieChart.Gauges).

{{~ if xaml ~}}
```csharp
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace ViewModelsSamples.Pies.Basic
{
    public class ViewModel
    {
        public ISeries[] Series { get; set; }
            = new ISeries[]
            {
                new PieSeries<double> { Values = new double[] { 2 } },
                new PieSeries<double> { Values = new double[] { 4 } },
                new PieSeries<double> { Values = new double[] { 1 } },
                new PieSeries<double> { Values = new double[] { 4 } },
                new PieSeries<double> { Values = new double[] { 3 } }
            };
    }
}
```

```xml
<lvc:PieChart
    Series="{Binding Series}">
</lvc:PieChart>
```
{{~ end ~}}

{{~ if blazor ~}}
```xml
<PieChart
    Series="series">
</PieChart>
```

```csharp
private ISeries[] series = new ISeries[]
{
    new PieSeries<double> { Values = new double[] { 2 } },
    new PieSeries<double> { Values = new double[] { 4 } },
    new PieSeries<double> { Values = new double[] { 1 } },
    new PieSeries<double> { Values = new double[] { 4 } },
    new PieSeries<double> { Values = new double[] { 3 } }
};
```
{{~ end ~}}

{{~ if winforms ~}}
Drag a new `PieChart` control from your toolbox, then in the code behind assign the `Series` property:

```csharp
PieChart1.Series = new ISeries[]
{
    new PieSeries<double> { Values = new double[] { 2 } },
    new PieSeries<double> { Values = new double[] { 4 } },
    new PieSeries<double> { Values = new double[] { 1 } },
    new PieSeries<double> { Values = new double[] { 4 } },
    new PieSeries<double> { Values = new double[] { 3 } }
};
```
{{~ end ~}}

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/piemvp.png)

## InitialRotation property

Controls the angle in degrees where the first slice is drawn, the `InitialRotation` property will change the start angle of
the pie, the following diagram explains where the `PieChart` rotation starts:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/pie-rotation.png)

{{~ if xaml ~}}
```xml
<lvc:PieChart
    InitialRotation="-90">
</lvc:PieChart>
```
{{~ end ~}}

{{~ if blazor ~}}
```xml
<PieChart
    InitialRotation="-90">
</PieChart>
```
{{~ end ~}}

{{~ if winforms ~}}
Drag a new `PieChart` control from your toolbox, then in the code behind assign the `Series` property:

```csharp
PieChart1.InitialRotation = -90;
```
{{~ end ~}}

Notice a change in the `InitialRotation` property is animated automatically based on the chart animations settings:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/pie-inrot.gif)

## MaxAngle property

This property determines the complete angle in degrees of the chart, the default value is 360.

{{~ if xaml ~}}
```xml
<lvc:PieChart
    MaxAngle="270">
</lvc:PieChart>
```
{{~ end ~}}

{{~ if blazor ~}}
```xml
<PieChart
    MaxAngle="270">
</PieChart>
```
{{~ end ~}}

{{~ if winforms ~}}
Drag a new `PieChart` control from your toolbox, then in the code behind assign the `Series` property:

```csharp
PieChart1.MaxAngle = 270;
```
{{~ end ~}}

Notice the `MaxAngle` property is animated automatically based on the chart animations settings:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/pie-maxangle.gif)

{{ render "~/shared/chart.md" }}

{{ render "~/shared/tooltips.md" }}

{{ render "~/shared/legends.md" }}
