<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
-->

# {{ name | to_title_case }}

{{ render "~/shared/series.md" }}

{{ render "~/shared/datalabels.md" }}

## Stroke property

If the stroke property is not set, then LiveCharts will create it based on the series position in your series collection
and the current theme.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/scatterstroke.png)

```csharp
Series = new ISeries[]
{
    new ScatterSeries<ObservablePoint>
    {
        Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 4 }, // mark
        Fill = null,
        Values = new ObservableCollection<ObservablePoint>
        {
            new ObservablePoint(2.2, 5.4),
            new ObservablePoint(4.5, 2.5),
            new ObservablePoint(4.2, 7.4),
            ...
        }
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

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/scatterfill.png)

```csharp
Series = new ISeries[]
{
    new ScatterSeries<ObservablePoint>
    {
        Fill = new SolidColorPaint(SKColors.Blue), // mark
        Stroke = null,
        Values = new ObservableCollection<ObservablePoint>
        {
            new ObservablePoint(2.2, 5.4),
            new ObservablePoint(4.5, 2.5),
            new ObservablePoint(4.2, 7.4),
            ...
        }
    }
};
```

:::info
Paints can create gradients, dashed lines and more, if you need help using the `Paint` instances take 
a look at the [Paints article]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Paints).
:::

## GeometrySize property

Determines the size of the geometry, if this property is not set, then the library will decide it based on the theme.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/scattergeometrysize.png)

```csharp
var r = new Random();
var values1 = new ObservableCollection<ObservablePoint>();
var values2 = new ObservableCollection<ObservablePoint>();

for (var i = 0; i < 20; i++)
{
    values1.Add(new ObservablePoint(r.Next(0, 20), r.Next(0, 20)));
    values2.Add(new ObservablePoint(r.Next(0, 20), r.Next(0, 20)));
}

Series = new ISeries[]
{
    new ScatterSeries<ObservablePoint, RectangleGeometry>
    {
        Values = values1,
        GeometrySize = 10, // mark
    },
    new ScatterSeries<ObservablePoint, CircleGeometry>
    {
        Values = values2,
        GeometrySize = 30 // mark
    }
};
```

## MinGeometrySize property

This property specifies the minimum size a geometry can take when the `Weight` plane is enabled, to enable this plane
you could use the `WeightedPoint` class, the library is ready to plot this instance, alternatively you can register 
a new type using mappers, and use the `TertiaryValue` property of the `ChartPoint` instance to specify
the weight of each point.

Notice in the following image how every shape has a different size, the size of each geometry represents the `Weight` 
of each point, in this case the weight takes a random integer from 0 to 20, so when the `Weight` is `0` the 
size of the geometry will be `15` pixels as specified in the `MinGeometrySize` property, when the `Weight` is `20`
the geometry size will be `40` defined by the `GeometrySize` property, for any `Weight` between this range the library
will interpolate lineally to determine the corresponding size.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/scattermgs.png)

```csharp
var r = new Random();
var values1 = new ObservableCollection<WeightedPoint>();
var values2 = new ObservableCollection<WeightedPoint>();

for (var i = 0; i < 20; i++)
{
    values1.Add(new WeightedPoint(r.Next(0, 20), r.Next(0, 20), r.Next(0, 20)));
    values2.Add(new WeightedPoint(r.Next(0, 20), r.Next(0, 20), r.Next(0, 20)));
}

Series = new ObservableCollection<ISeries>
{
    new ScatterSeries<WeightedPoint, RoundedRectangleGeometry>
    {
        Values = values1,
        GeometrySize = 40,
        MinGeometrySize = 15 // mark
    },

    new ScatterSeries<WeightedPoint, CircleGeometry>
    {
        Values = values2,
        GeometrySize = 40,
        MinGeometrySize = 15 // mark
    }
};
```

{{ render "~/shared/series2.md" }}
