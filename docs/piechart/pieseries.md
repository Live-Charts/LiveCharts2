<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
-->

# {{ name | to_title_case }}

{{ render "~/shared/series.md" }}

{{ render "~/shared/polarlabels.md" }}

## Stroke property

If the stroke property is not set, then LiveCharts will create it based on the series position in your series collection
and the current theme.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/piestroke.png)

{{~ if xaml ~}}
```csharp
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.Generic;

namespace ViewModelsSamples.Pies.Basic
{
    public class ViewModel
    {
        public IEnumerable<ISeries> Series { get; set; } = new List<ISeries>
        {
            new PieSeries<double>
            {
                Values = new List<double> { 4 },
                Pushout = 8,
                Stroke = new SolidColorPaint(SKColors.Red) { StrokeThickness = 3 } // mark
            },
            new PieSeries<double> { Values = new List<double> { 2 } },
            new PieSeries<double> { Values = new List<double> { 1 } },
            new PieSeries<double> { Values = new List<double> { 4 } },
            new PieSeries<double> { Values = new List<double> { 3 } }
        };
    }
}
```

```xml
<lvc:PieChart Series="{Binding Series}"></lvc:PieChart>
```
{{~ end ~}}

{{~ if blazor ~}}
```csharp
@using LiveChartsCore;
@using LiveChartsCore.SkiaSharpView;
@using LiveChartsCore.SkiaSharpView.Painting;
@using SkiaSharp;
@using System.Collections.Generic;

<PieChart Series="series"></PieChart>

@code {
    private ISeries[] series 
        = new List<ISeries>
        {
            new PieSeries<double>
            {
                Values = new List<double> { 4 },
                Pushout = 8,
                Stroke = new SolidColorPaint(SKColors.Red) { StrokeThickness = 3 } // mark
            },
            new PieSeries<double> { Values = new List<double> { 2 } },
            new PieSeries<double> { Values = new List<double> { 1 } },
            new PieSeries<double> { Values = new List<double> { 4 } },
            new PieSeries<double> { Values = new List<double> { 3 } }
        };
}
```
{{~ end ~}}

{{~ if winforms ~}}
```csharp
pieChart1.Series = new List<ISeries>
{
    new PieSeries<double>
    {
        Values = new List<double> { 4 },
        Pushout = 8,
        Stroke = new SolidColorPaint(SKColors.Red) { StrokeThickness = 3 } // mark
    },
    new PieSeries<double> { Values = new List<double> { 2 } },
    new PieSeries<double> { Values = new List<double> { 1 } },
    new PieSeries<double> { Values = new List<double> { 4 } },
    new PieSeries<double> { Values = new List<double> { 3 } }
};
```
{{~ end ~}}

:::info
Paints can create gradients, dashed lines and more, if you need help using the `Paint` instances take 
a look at the [Paints article]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Paints).
:::

## Fill property

If the fill property is not set, then LiveCharts will create it based on the series position in your series collection
and the current theme.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/piefill.png)

{{~ if xaml ~}}
```csharp
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.Generic;

namespace ViewModelsSamples.Pies.Basic
{
    public class ViewModel
    {
        public IEnumerable<ISeries> Series { get; set; } = new List<ISeries>
        {
            new PieSeries<double>
            {
                Values = new List<double> { 4 },
                Pushout = 8,
                Fill = new SolidColorPaint(SKColors.Yellow) // mark
            },
            new PieSeries<double> { Values = new List<double> { 2 } },
            new PieSeries<double> { Values = new List<double> { 1 } },
            new PieSeries<double> { Values = new List<double> { 4 } },
            new PieSeries<double> { Values = new List<double> { 3 } }
        };
    }
}
```

```xml
<lvc:PieChart Series="{Binding Series}"></lvc:PieChart>
```
{{~ end ~}}

{{~ if blazor ~}}
```csharp
@using LiveChartsCore;
@using LiveChartsCore.SkiaSharpView;
@using LiveChartsCore.SkiaSharpView.Painting;
@using SkiaSharp;
@using System.Collections.Generic;

<PieChart Series="series"></PieChart>

@code {
    private ISeries[] series 
        = new List<ISeries>
        {
            new PieSeries<double>
            {
                Values = new List<double> { 4 },
                Pushout = 8,
                Fill = new SolidColorPaint(SKColors.Yellow) // mark
            },
            new PieSeries<double> { Values = new List<double> { 2 } },
            new PieSeries<double> { Values = new List<double> { 1 } },
            new PieSeries<double> { Values = new List<double> { 4 } },
            new PieSeries<double> { Values = new List<double> { 3 } }
        };
}
```
{{~ end ~}}

{{~ if winforms ~}}
```csharp
pieChart1.Series = new List<ISeries>
{
    new PieSeries<double>
    {
        Values = new List<double> { 4 },
        Pushout = 8,
        Fill = new SolidColorPaint(SKColors.Yellow) // mark
    },
    new PieSeries<double> { Values = new List<double> { 2 } },
    new PieSeries<double> { Values = new List<double> { 1 } },
    new PieSeries<double> { Values = new List<double> { 4 } },
    new PieSeries<double> { Values = new List<double> { 3 } }
};
```
{{~ end ~}}

:::info
Paints can create gradients, dashed lines and more, if you need help using the `Paint` instances take 
a look at the [Paints article]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Paints).
:::

## Pushout property

It is the distance in pixels between the center of the control and the pie slice, notice the 
`HoverPushout` property defines the push-out when the pointer is above the pie slice shape.

```csharp
var pieSeries = new PieSeries<int>
{
    Values = new [] { ... },
    Pushout = 40 // mark
};
```

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/piepushout.png)

## MaxRadialColumnWidth

Sets the maximum value a radial column can take in pixels.

```csharp
var pieSeries = new PieSeries<int>
{
    Values = new [] { ... },
    MaxRadialColumnWidth = 50 // mark
};
```

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/pieMaxRadialCW.png)

## InnerRadius property

The inner radius of the slice in pixels, it is similar to the `MaxRadialColumnWidth` property,
both are useful to create doughnut charts, the difference is that `MaxRadialColumnWidth` is more flexible
on different screen sizes.

```csharp
var pieSeries = new PieSeries<int>
{
    Values = new [] { ... },
    InnerRadius = 50 // mark
};
```

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/pieInnerRadius.png)

## OuterRadiusOffset property

It is the distance from the maximum radius available to the end of the slice in pixels.

```csharp
var pieSeries = new PieSeries<int>
{
    Values = new [] { ... },
    OuterRadiusOffset = 20 // mark
};
```

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/pieOuterRadiusOffset.png)

{{ render "~/shared/series2.md" }}
