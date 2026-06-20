<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
-->

# {{ name | to_title_case }}

{{ render "~/shared/series.md" }}

{{ render "~/shared/datalabels.md" }}

## Stroke property

If the stroke property is not set, then LiveCharts will create it based on the series position in your series collection
and the current theme.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/stackedcolstroke.png)

```csharp
Series = new ISeries[]
{
    new StackedColumnSeries<int>
    {
        Values = new List<int> { 3, 5, 3, 2, 5, 4, 2 },
        Stroke = new SolidColorPaint(SKColors.Red) { StrokeThickness = 4 }, // mark
        Fill = null,
    },
    new StackedColumnSeries<int>
    {
        Values = new List<int> { 4, 2, 3, 2, 3, 4, 2 },
        Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 8 }, // mark
        Fill = null,
    },
    new StackedColumnSeries<int>
    {
        Values = new List<int> { 4, 6, 6, 5, 4, 3 , 2 },
        Stroke = new SolidColorPaint(SKColors.Green) { StrokeThickness = 12 }, // mark
        Fill = null,
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

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/stackedcolfill.png)

```csharp
Series = new ISeries[]
{
    new StackedColumnSeries<int>
    {
        Values = new List<int> { 3, 5, 3, 2, 5, 4, 2 },
        Fill = new SolidColorPaint(SKColors.Red), // mark
        Stroke = null,
    },
    new StackedColumnSeries<int>
    {
        Values = new List<int> { 4, 2, 3, 2, 3, 4, 2 },
        Fill = new SolidColorPaint(SKColors.Blue), // mark
        Stroke = null,
    },
    new StackedColumnSeries<int>
    {
        Values = new List<int> { 4, 6, 6, 5, 4, 3 , 2 },
        Fill = new SolidColorPaint(SKColors.Green), // mark
        Stroke = null,
    }
};
```

:::info
Paints can create gradients, dashed lines and more, if you need help using the `Paint` instances take 
a look at the [Paints article]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Paints).
:::

## Rx and Ry properties

These properties define the corners radius in the rectangle geometry.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/stackedcolcr.png)

```csharp
Series = new ISeries[]
{
    new StackedColumnSeries<int>
    {
        Values = new List<int> { 3, 5, 3, 2, 5, 4, 2 },
        Rx = 50, // mark
        Ry = 50 // mark
    },
    new StackedColumnSeries<int>
    {
        Values = new List<int> { 4, 2, 3, 2, 3, 4, 2 },
        Rx = 50, // mark
        Ry = 50 // mark
    },
    new StackedColumnSeries<int>
    {
        Values = new List<int> { 4, 6, 6, 5, 4, 3 , 2 },
        Rx = 50, // mark
        Ry = 50 // mark
    }
};
```

## MaxBarWidth property

:::info
this section uses the `ColumnSeries` class, but it works the same for the `StackedColumnSeries`.
:::

Specifies the maximum width a column can take, take a look at the following sample, where the max width is `10`.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/columnmw10.png)

```csharp
Series = new ISeries[]
{
    new ColumnSeries<int>
    {
        Values = new [] { 4, 4, 7, 2, 8 },
        MaxBarWidth = 10 // mark
    }
};
```

But now lets use `double.MaxValue` to see the difference.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/columnmwmax.png)

```csharp
Series = new ISeries[]
{
    new ColumnSeries<int>
    {
        Values = new [] { 4, 4, 7, 2, 8 },
        MaxBarWidth = double.MaxValue // mark
    }
};
```

Finally we could also set the padding to `0`.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/columnmwmaxp0.png)

```csharp
Series = new ISeries[]
{
    new ColumnSeries<int>
    {
        Values = new [] { 4, 4, 7, 2, 8 },
        MaxBarWidth = double.MaxValue,
        Padding = 0 // mark
    }
};
```

## Padding property

Gets or sets the padding for each bar in the series.

```csharp
Series = new ISeries[]
{
    new StackedColumnSeries<int>
    {
        Values = new [] { 4, 4, 7, 2, 8 },
        Padding = 5 // mark
    },
    new StackedColumnSeries<int>
    {
        Values = new [] { 2, 3,1, 4, 6 },
        Padding = 5 // mark
    }
};
```

{{ render "~/shared/series2.md" }}
