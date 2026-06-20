<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
-->

# {{ name | to_title_case }}

{{ render "~/shared/series.md" }}

{{ render "~/shared/datalabels.md" }}

## Stroke property

If the stroke property is not set, then LiveCharts will create it based on the series position in your series collection
and the current theme.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/box-stroke.png)

```csharp
Series = new ISeries[]
{
    new BoxSeries<BoxValue>
    {
        Values = new BoxValue[]
        {
            // max, upper quartile, lower quartile, min, median
            new(100, 80, 60, 20, 70),
            new(90, 70, 50, 30, 60),
            new(80, 60, 40, 10, 50)
        },
        Stroke = new SolidColorPaint(SKColors.Red, 3),
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

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/box-fill.png)

```csharp
Series = new ISeries[]
{
    new BoxSeries<BoxValue>
    {
        Values = new BoxValue[]
        {
            // max, upper quartile, lower quartile, min, median
            new(100, 80, 60, 20, 70),
            new(90, 70, 50, 30, 60),
            new(80, 60, 40, 10, 50)
        },
        Fill = new SolidColorPaint(SKColors.Red)
    }
};
```
:::info
Paints can create gradients, dashed lines and more, if you need help using the `Paint` instances take 
a look at the [Paints article]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Paints).
:::

## MaxBarWidth property

Specifies the maximum width a column can take, take a look at the following sample, where the max width is `10`.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/box-mw10.png)

```csharp
Series = new ISeries[]
{
    new BoxSeries<BoxValue>
    {
        Values = new BoxValue[] { ... },
        MaxBarWidth = 10 // mark
    }
};
```

But now lets use `double.MaxValue` to see the difference.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/box-mw-.png)

```csharp
Series = new ISeries[]
{
    new BoxSeries<BoxValue>
    {
        Values = new BoxValue[] { ... },
        MaxBarWidth = double.MaxValue // mark
    }
};
```

Finally we could also set the padding to `0`.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/box-mw-p0.png)

```csharp
Series = new ISeries[]
{
    new BoxSeries<BoxValue>
    {
        Values = new BoxValue[] { ... },
        MaxBarWidth = double.MaxValue,
        Padding = 0 // mark
    }
};
```

{{ render "~/shared/series2.md" }}
