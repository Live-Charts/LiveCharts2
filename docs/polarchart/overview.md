<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
-->

# The Polar Chart Control

:::info
This article is a quick guide to use the `PolarChart` control, you can explore all the properties and the source code 
at the [ApiExplorer]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.{{ platform }}.PolarChart).
:::

The `PolarChart` control is a 'ready to go' control to create plots using the 
[Polar coordinate system](https://en.wikipedia.org/wiki/Polar_coordinate_system),
To get started all you need to do is assign the `Series` property with a collection of 
[`IPolarSeries`]({{ website_url }}/api/{{ version }}/LiveChartsCore.Kernel.Sketches.IPolarSeries-1).

{{~ if xaml ~}}
```csharp
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace ViewModelsSamples.Polar.Basic
{
    public class ViewModel
    {
        public ISeries[] Series { get; set; } = new[]
        {
            new PolarLineSeries<double>
            {
                Values = new double[] { 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 },
                Fill = null,
                IsClosed = false
            }
        };
    }
}
```

```xml
<lvc:PolarChart
    Series="{Binding Series}">
</lvc:PolarChart>
```
{{~ end ~}}

{{~ if blazor ~}}
```xml
<PolarChart
    Series="series">
</PolarChart>
```

```csharp
private ISeries[] series = new[]
{
    new PolarLineSeries<double>
    {
        Values = new double[] { 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 },
        Fill = null,
        IsClosed = false
    }
};
```
{{~ end ~}}

{{~ if winforms ~}}
Drag a new `PolarChart` control from your toolbox, then in the code behind assign the `Series` property:

```csharp
PolarChart1.Series = new[]
{
    new PolarLineSeries<double>
    {
        Values = new double[] { 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 },
        Fill = null,
        IsClosed = false
    }
};
```
{{~ end ~}}

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/polar-mvp.png)

The main components of this control are:

- Series
- Axes
- Tooltip
- Legend

But notice in the previous code snippet we did not specify the Axes, Tooltip, Legend or the series colors, this is because LiveCharts will
decide them based on the current theme of the library, you can also customize any of these properties when you require it, this article will
cover the most common scenarios.

## Series

There are ~multiple~ series available in the library, you can add one or mix them all in the same chart, every series has unique properties,
any image below is a link to an article explaining more about them.

<a href="{{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Polar%20Line%20Series">
<div class="series-miniature">
<img src="https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/samples/polarLines/basic/result.png" alt="series"/>
<div class="text-center"><b>Polar Line series</b></div>
</div>
</a>

## InitialRotation property

Defines an offset to establish where the 0 angle is.

{{~ if xaml ~}}
```xml
<lvc:PolarChart
    Series="{Binding Series}"
    InitialRotation="-90">
</lvc:PolarChart>
```
{{~ end ~}}

{{~ if blazor ~}}
```xml
<PolarChart
    Series="series"
    InitialRotation="-90">
</PolarChart>
```

{{~ end ~}}

{{~ if winforms ~}}
Drag a new `PolarChart` control from your toolbox, then in the code behind assign the `Series` property:

```csharp
polarChart1.InitialRotation = -90;
```
{{~ end ~}}

Notice a change in the `InitialRotation` property is animated automatically based on the chart animations settings:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/polar-rotation.gif)

## InnerRadius property

Defines the inner radius in pixels of the chart.

{{~ if xaml ~}}
```xml
<lvc:PolarChart
    Series="{Binding Series}"
    InnerRadius="50">
</lvc:PolarChart>
```
{{~ end ~}}

{{~ if blazor ~}}
```xml
<PolarChart
    Series="Series"
    InnerRadius="50">
</PolarChart>
```

{{~ end ~}}

{{~ if winforms ~}}
Drag a new `PolarChart` control from your toolbox, then in the code behind assign the `Series` property:

```csharp
polarChart1.InnerRadius = 50;
```
{{~ end ~}}

Notice a change in the `InnerRadius` property is animated automatically based on the chart animations settings:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/polar-innerRadius.gif)

## TotalAngle property

Defines the total circumference angle in degrees, from 0 to 360, default is 360.

{{~ if xaml ~}}
```xml
<lvc:PolarChart
    Series="{Binding Series}"
    TotalAngle="270">
</lvc:PolarChart>
```
{{~ end ~}}

{{~ if blazor ~}}
```xml
<PolarChart
    Series="Series"
    TotalAngle="270">
</PolarChart>
```

{{~ end ~}}

{{~ if winforms ~}}
Drag a new `PolarChart` control from your toolbox, then in the code behind assign the `Series` property:

```csharp
polarChart1.TotalAngle = 270;
```
{{~ end ~}}

Notice a change in the `TotalAngle` property is animated automatically based on the chart animations settings:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/polar-totalangle.gif)

## PolarAxis.LabelsPaint and PolarAxis.SeparatorsPaint properties

You can set the color, use dashed lines, build gradients for the axis name, labels and separators, if you need help using `Paint` instances take 
a look at the [Paints article]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Paints).

{{~ if xaml ~}}
```csharp
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;

namespace ViewModelsSamples.Bars.Basic
{
    public class ViewModel
    {
        public ISeries[] Series { get; set; } = new ISeries[] { ... };

        public PolarAxis[] RadiusAxes { get; set; }
            = new PolarAxis[]
            {
                new PolarAxis
                {
                    TextSize = 10,
                    LabelsPaint = new SolidColorPaint(SKColors.Blue), // mark

                    SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) { StrokeThickness = 2 }  // mark
                }
            };

        public PolarAxis[] AngleAxes { get; set; }
            = new PolarAxis[]
            {
                new PolarAxis
                {
                    LabelsPaint = new SolidColorPaint(SKColors.Green), // mark
                    TextSize = 20,

                    SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) // mark
                    { // mark
                        StrokeThickness = 2, // mark
                        PathEffect = new DashEffect(new float[] { 3, 3 }) // mark
                    } // mark
                }
            };
    }
}
```

```xml
<lvc:CartesianChart
    Series="{Binding Series}"
    RadiusAxes="{Binding RadiusAxes}"
    AngleAxes="{Binding AngleAxes}">
</lvc:CartesianChart>
```
{{~ end ~}}

{{~ if blazor ~}}
```csharp
@using LiveChartsCore;
@using LiveChartsCore.SkiaSharpView;
@using LiveChartsCore.SkiaSharpView.Painting;
@using LiveChartsCore.SkiaSharpView.Painting.Effects;
@using SkiaSharp;

<CartesianChart
    Series="series"
    RadiusAxes="radiusAxes"
    AngleAxes="angleAxes">
</CartesianChart>

@code {
    private ISeries[] series { get; set; } = new ISeries[] {  };

    private PolarAxis[] radiusAxes { get; set; }
        = new PolarAxis[]
        {
            new PolarAxis
            {
                TextSize = 10,
                LabelsPaint = new SolidColorPaint(SKColors.Blue), // mark

                SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) { StrokeThickness = 2 }  // mark
            }
        };

    private PolarAxis[] angleAxes { get; set; }
        = new PolarAxis[]
        {
            new PolarAxis
            {
                LabelsPaint = new SolidColorPaint(SKColors.Green), // mark
                TextSize = 20,

                SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) // mark
                { // mark
                    StrokeThickness = 2, // mark
                    PathEffect = new DashEffect(new float[] { 3, 3 }) // mark
                } // mark
            }
        };
}
```
{{~ end ~}}

{{~ if winforms ~}}
```csharp
polarChart1.RadiusAxes = new PolarAxis[]
{
    new PolarAxis
    {
        TextSize = 10,
        LabelsPaint = new SolidColorPaint(SKColors.Blue), // mark

        SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) { StrokeThickness = 2 }  // mark
    }
};

polarChart1.AngleAxes = new PolarAxis[]
{
    new PolarAxis
    {
        LabelsPaint = new SolidColorPaint(SKColors.Green), // mark
        TextSize = 20,

        SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) // mark
        { // mark
            StrokeThickness = 2, // mark
            PathEffect = new DashEffect(new float[] { 3, 3 }) // mark
        } // mark
    }
};
```
{{~ end ~}}

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/polar-axes-style.png)

## PolarAxis.Labels vs PolarAxis.Labeler properties

There are 2 ways to format and axis labels, using the `Labels` property and using the `Labeler` property,
you must normally use the `Labels` property to indicate names, and the `Labeler` property to give format to the current label.

The labels property is a collection of string, if this property is not `null`, then the axis label will be pulled from the labels 
collection, the label is mapped to the chart based on the position of the label and the position of the point, both integers,
if the axis requires a label outside the bounds of the labels collection, then the index will be returned as the label,
default value is null.

{{~ if xaml ~}}
```csharp
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;

namespace ViewModelsSamples.Bars.Basic
{
    public class ViewModel
    {
        public ISeries[] Series { get; set; } 
            = new ObservableCollection<ISeries>
            {
                new PolarLineSeries<double>
                {
                    Values = new ObservableCollection<double> { 5, 2, 1, 4, 3, 3 },
                    GeometryFill = null,
                    GeometryStroke = null,
                    LineSmoothness = 0.2,
                    Stroke = null,
                    IsClosed = true,
                },
                new PolarLineSeries<double>
                {
                    Values = new ObservableCollection<double> { 3, 5, 2, 3, 4, 5 },
                    GeometryFill = null,
                    GeometryStroke = null,
                    LineSmoothness = 0.2,
                    Stroke = null,
                    IsClosed = true,
                }
            };

        public PolarAxis[] RadiusAxes { get; set; }
            = new PolarAxis[]
            {
                new PolarAxis
                {
                    // formats the value as a number with 2 decimals.
                    Labeler = value => value.ToString("N2")
                }
            };

        public PolarAxis[] AngleAxes { get; set; }
            = new PolarAxis[]
            {
                new PolarAxis
                {
                    Labels = new[] { "strength", "stamina", "resistance", "power", "hit points", "mana" },
                    MinStep = 1,
                    ForceStepToMin = true
                }
            };
    }
}
```

```xml
<lvc:CartesianChart
    Series="{Binding Series}"
    RadiusAxes="{Binding RadiusAxes}"
    AngleAxes="{Binding AngleAxes}">
</lvc:CartesianChart>
```
{{~ end ~}}

{{~ if blazor ~}}
```csharp
@using LiveChartsCore;
@using LiveChartsCore.SkiaSharpView;
@using LiveChartsCore.SkiaSharpView.Painting;
@using LiveChartsCore.SkiaSharpView.Painting.Effects;
@using SkiaSharp;

<CartesianChart
    Series="series"
    RadiusAxes="radiusAxes"
    AngleAxes="angleAxes">
</CartesianChart>

@code {
    private ISeries[] series { get; set; } 
        = new ObservableCollection<ISeries>
        {
            new PolarLineSeries<double>
            {
                Values = new ObservableCollection<double> { 5, 2, 1, 4, 3, 3 },
                GeometryFill = null,
                GeometryStroke = null,
                LineSmoothness = 0.2,
                Stroke = null,
                IsClosed = true,
            },
            new PolarLineSeries<double>
            {
                Values = new ObservableCollection<double> { 3, 5, 2, 3, 4, 5 },
                GeometryFill = null,
                GeometryStroke = null,
                LineSmoothness = 0.2,
                Stroke = null,
                IsClosed = true,
            }
        };

    private PolarAxis[] radiusAxes { get; set; }
        = new PolarAxis[]
        {
            new PolarAxis
            {
                // formats the value as a number with 2 decimals.
                Labeler = value => value.ToString("N2")
            }
        };

    private PolarAxis[] angleAxes { get; set; }
        = new PolarAxis[]
        {
            new PolarAxis
            {
                Labels = new[] { "strength", "stamina", "resistance", "power", "hit points", "mana" },
                MinStep = 1,
                ForceStepToMin = true
            }
        };
}
```
{{~ end ~}}

{{~ if winforms ~}}
```csharp
polarChart1.Series = new ObservableCollection<ISeries>
{
    new PolarLineSeries<double>
    {
        Values = new ObservableCollection<double> { 5, 2, 1, 4, 3, 3 },
        GeometryFill = null,
        GeometryStroke = null,
        LineSmoothness = 0.2,
        Stroke = null,
        IsClosed = true,
    },
    new PolarLineSeries<double>
    {
        Values = new ObservableCollection<double> { 3, 5, 2, 3, 4, 5 },
        GeometryFill = null,
        GeometryStroke = null,
        LineSmoothness = 0.2,
        Stroke = null,
        IsClosed = true,
    }
};
        
polarChart1.RadiusAxes = new PolarAxis[]
{
    new PolarAxis
    {
        // formats the value as a number with 2 decimals.
        Labeler = value => value.ToString("N2")
    }
};

polarChart1.AngleAxes = new PolarAxis[]
{
    new PolarAxis
    {
        Labels = new[] { "strength", "stamina", "resistance", "power", "hit points", "mana" },
        MinStep = 1,
        ForceStepToMin = true
    }
};
```
{{~ end ~}}

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/polar-star.png)

## PolarAxis.LabelsRotation property

Indicates the axis labels rotation in degrees, in the following image we have a rotation of 45 degrees in the Radius axis,
and for the angle axis the labels rotation will follow the tangent line.

```csharp
RadiusAxes = new PolarAxis[]
{
    new PolarAxis    
    {
        LabelsRotation = 45 // mark
    }
};
AngleAxes = new PolarAxis[]
{
    new PolarAxis    
    {
        LabelsRotation = LiveCharts.TangentAngle // mark
    }
};
```

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/polar-labels-rotation.png)

You can also place labels at the cotangent angle:

```csharp
AngleAxes = new PolarAxis[]
{
    new PolarAxis
    {
        LabelsRotation = LiveCharts.CotangentAngle // mark
    }
};
```

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/polar-labels-rotation2.png)

Finally notice that you can combine the `LiveCharts.CotangentAngle` and `LiveCharts.TangentAngle` with decimal degrees,
the following expressions are valid also:

```csharp
Series { get; set; } = new PolarAxis[]
{
    new PolarAxis
    {
        LabelsRotation = 30, // in degrees
    },
    new PolarAxis
    {
        LabelsRotation = LiveCharts.TangentAngle + 30, // the tangent + 30 degrees
    },
    new PolarAxis
    {
        LabelsRotation = LiveCharts.CotangentAngle + 30, // the cotangent + 30 degrees
    }
};
```

## PolarAxis.Visible property

When the `Visible` property is set to `false` the axis will not be drawn and also it will not take any space in the chart
default is `true`.

{{ render "~/shared/chart.md" }}

{{ render "~/shared/tooltips.md" }}

{{ render "~/shared/legends.md" }}
