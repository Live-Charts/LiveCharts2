<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
content is normally pulled from the examples in the repository.
-->

## Plotting custom types

You can plot any type of data, please see the [mappers article]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Mappers) for more information.

## Custom geometries

You can also customize the geometry for each point in a series, you can use the geometries defined on LiveCharts, SVG geometries
or draw your own using the SkiaSharp API, if you want to learn more please take a look at
[this article]({{ website_url }}/docs/{{ platform }}/{{ version }}/samples.lines.custom).

## ZIndex property

Indicates an order in the Z axis, this order controls which series is above or behind.

## IsVisible property

Indicates if the series is visible in the user interface.

## DataPadding

The data padding is the minimum distance from the edges of the series to the axis limits. It is of type `System.Drawing.PointF`. 
Both coordinates (X and Y) go from 0 to 1, where 0 is nothing and 1 is the axis tick. An axis tick is the separation between
every label or separator (even if they are not visible).

If this property is not set, the library will set it according to the series type, take a look at the following samples:

```csharp
new LineSeries<double>
{
    DataPadding = new LvcPoint(0, 0),
    Values = new ObservableCollection<double> { 2, 1, 3, 5, 3, 4, 6 },
    GeometryStroke = null,
    GeometryFill = null,
    Fill = null
}
```

Produces the following result:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/1.8.padding00.png)

But you can remove the padding only from an axis, for example:

```csharp
new LineSeries<double>
{
    DataPadding = new LvcPoint(0.5f, 0),
    Values = new ObservableCollection<double> { 2, 1, 3, 5, 3, 4, 6 },
    GeometryStroke = null,
    GeometryFill = null,
    Fill = null
}
```

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/1.8.padding50.png)

Or you can increase the distance:

```csharp
new LineSeries<double>
{
    DataPadding = new LvcPoint(2, 2),
    Values = new ObservableCollection<double> { 2, 1, 3, 5, 3, 4, 6 },
    GeometryStroke = null,
    GeometryFill = null,
    Fill = null
}
```

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/1.8.padding22.png)
