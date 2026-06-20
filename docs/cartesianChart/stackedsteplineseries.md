<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
-->

# {{ name | to_title_case }}

{{ render "~/shared/series.md" }}

{{ render "~/shared/datalabels.md" }}

## Stroke property

If the stroke property is not set, then LiveCharts will create it based on the series position in your series collection
and the current theme.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/stackedstepstroke.png)

```csharp
Series = new ISeries[]
{
    new StackedStepAreaSeries<double>
    {
        Values = new List<double> { 3, 2, 3, 5, 3, 4, 6 },
        Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 4 }, // mark
        Fill = null
    },
    new StackedStepAreaSeries<double>
    {
        Values = new List<double> { 6, 5, 6, 3, 8, 5, 2 },
        Stroke = new SolidColorPaint(SKColors.Red) { StrokeThickness = 8 }, // mark
        Fill = null
    },
    new StackedStepAreaSeries<double>
    {
        Values = new List<double> { 4, 8, 2, 8, 9, 5, 3 },
        Stroke = new SolidColorPaint(SKColors.Green) { StrokeThickness = 12 }, // mark
        Fill = null
    }
};
```

:::info
Paints can create gradients, dashed lines and more, if you need help using the `Paint` instances take 
a look at the [Paints article]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Paints).
:::

## Fill property

If the fill property is not set, then LiveCharts will create it based on the series position in your series collection
and the current theme.

The alpha channel enables transparency, it goes from 0 to 255, 0 is transparent and 255 disables transparency completely.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/stackedstepfill.png)

```csharp
Series = new ISeries[]
{
    new StackedStepAreaSeries<double>
    {
        Values = new List<double> { 3, 2, 3, 5, 3, 4, 6 },
        Fill = new SolidColorPaint(SKColors.Blue.WithAlpha(90)), // mark
        Stroke = null
    },
    new StackedStepAreaSeries<double>
    {
        Values = new List<double> { 6, 5, 6, 3, 8, 5, 2 },
        Fill = new SolidColorPaint(SKColors.Red.WithAlpha(90)), // mark
        Stroke = null
    },
    new StackedStepAreaSeries<double>
    {
        Values = new List<double> { 4, 8, 2, 8, 9, 5, 3 },
        Fill = new SolidColorPaint(SKColors.Green.WithAlpha(90)), // mark
        Stroke = null
    }
};
```

:::info
Paints can create gradients, dashed lines and more, if you need help using the `Paint` instances take 
a look at the [Paints article]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Paints).
:::

## GeometryFill and GeometryStroke properties

The geometry is the circle shape (by default) that the line series draws for every point, you can customize
the fill and stroke of this shape, if none of these properties are set then LiveCharts will create them based on 
the series position in your series collection and the current theme.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/stackedstepgs.png)

```csharp
Series = new ISeries[]
{
    new StackedStepAreaSeries<double>
    {
        Values = new List<double> { 3, 2, 3, 5, 3, 4, 6 },
        GeometrySize = 20,
        GeometryFill = new SolidColorPaint(SKColors.WhiteSmoke), // mark
        GeometryStroke = new SolidColorPaint(SKColors.Red) { StrokeThickness = 3 }, // mark
    },
    new StackedStepAreaSeries<double>
    {
        Values = new List<double> { 6, 5, 6, 3, 8, 5, 2 },
        GeometrySize = 20,
        GeometryFill = new SolidColorPaint(SKColors.WhiteSmoke), // mark
        GeometryStroke = new SolidColorPaint(SKColors.Green) { StrokeThickness = 6 }, // mark
    },
    new StackedStepAreaSeries<double>
    {
        Values = new List<double> { 4, 8, 2, 8, 9, 5, 3 },
        GeometrySize = 20,
        GeometryFill = new SolidColorPaint(SKColors.WhiteSmoke), // mark
        GeometryStroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 10 }, // mark
    }
};
```

:::info
Paints can create gradients, dashed lines and more, if you need help using the `Paint` instances take 
a look at the [Paints article]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Paints).
:::

## GeometrySize property

Determines the size of the geometry, if this property is not set, then the library will decide it based on the theme.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/stackedstepgss.png)

```csharp
Series = new ISeries[]
{
    new StackedStepAreaSeries<double>
    {
        Values = new List<double> { 3, 2, 3, 5, 3, 4, 6 },
        GeometryFill = new SolidColorPaint(SKColors.Black),
        GeometrySize = 10, // mark
    },
    new StackedStepAreaSeries<double>
    {
        Values = new List<double> { 6, 5, 6, 3, 8, 5, 2 },
        GeometryFill = new SolidColorPaint(SKColors.Black),
        GeometrySize = 20, // mark
    }
};
```

## EnableNullSplitting property

Even the property is visible, this feature is not supported by now.

{{ render "~/shared/series2.md" }}
