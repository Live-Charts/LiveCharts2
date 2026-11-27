<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
Samples are pulled from the repository with render_sample, the code shown always matches the samples.
-->

# Scatter and bubble series

A `ScatterSeries` draws a geometry at the X and Y coordinates of each point, a circle by default, without joining the
points. When the points also carry a weight, the size of each geometry follows that weight and the series becomes a
bubble chart. Both cases use the same series, so this article covers them together.

:::info
This article highlights the most used features. Every property is listed in the API explorer:
[ScatterSeries]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.ScatterSeries-3).
{{~ if xaml ~}}
In XAML, declare it as `XamlScatterSeries`.
{{~ end ~}}
:::

## Basic scatter

Set the `Values` of the series and add it to the chart. An `ObservablePoint` sets both coordinates of a point, `X`
and `Y`. Plain numbers also work, then the index of each value in the collection is used as X. The tooltip shows the
X and Y of the point.

{{ render_sample "scatter/basic" }}

## Geometry size

`GeometrySize` sets the width and height of the geometry, in pixels. It is `24` by default.

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

## Bubbles

A `WeightedPoint` has `X`, `Y` and `Weight`. When the weights of a series are not all equal, the size of each
geometry is interpolated linearly between two properties: the point with the smallest weight gets
`MinGeometrySize` (`6` by default) and the point with the largest weight gets `GeometrySize` (`24` by default). A
`null` weight counts as `0`. The tooltip of a weighted series also shows the weight, as `W`.

In the next example the geometries of both series go from `15` to `40` pixels:

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

By default each series scales its weights on its own, so the largest weight of every series gets the same size. Series
that share a `StackGroup` share one scale instead. In this sample the weights of the second and third series are ten
times larger than the weights of the first one. The first series has its own scale, the second and third share
`StackGroup` `1`, so their sizes can be compared with each other.

{{ render_sample "scatter/bubbles" }}

To use your own type as a weighted point, return `new Coordinate(x, y, weight)` from the mapping, see
[mappers]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Mappers).

## Stroke and fill paints

When `Stroke` or `Fill` are not set, LiveCharts picks them from the current theme, based on the position of the
series in the chart's series collection. The default theme draws the geometries with a semi-transparent `Fill` and
no `Stroke`.

`Stroke` draws the border of each geometry:

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
            // ...
        }
    }
};
```

`Fill` paints the inside of each geometry:

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
            // ...
        }
    }
};
```

:::info
Paints can also draw gradients and dashed lines, see the
[Paints article]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Paints).
:::

## Custom geometries

The second generic argument of the series sets the geometry of each point, a `CircleGeometry` by default. This sample
uses, in order, a predefined `HeartGeometry` (the `LiveChartsCore.SkiaSharpView.Drawing.Geometries` namespace has
more, such as `StarGeometry`, `CrossGeometry`, `DiamondGeometry` and `RectangleGeometry`), an SVG path through
`VariableSVGPathGeometry` and `GeometrySvg`, and a geometry drawn with the SkiaSharp API (`MyGeometry`).
{{~ if xaml2006 ~}}
The XAML parser of this platform does not take generic types, so the sample declares a non-generic class for each
series in `CustomSeries.cs`.
{{~ end ~}}

{{ render_sample "scatter/custom" }}

## Data labels

Data labels draw a label next to every point, setting `DataLabelsPaint` turns them on. By default the label shows the
Y value of the point:

```csharp
new ScatterSeries<ObservablePoint>
{
    Values = new ObservablePoint[] { new(2.2, 5.4), new(3.6, 9.6), new(9.9, 5.2) },
    DataLabelsSize = 14,
    DataLabelsPaint = new SolidColorPaint(SKColors.Blue),
    // all the positions:
    // {{ website_url }}/api/{{ version }}/LiveChartsCore.Measure.DataLabelsPosition
    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
    // the formatter takes the point and returns the label text
    DataLabelsFormatter = point => $"{point.Coordinate.SecondaryValue}, {point.Coordinate.PrimaryValue}"
}
```

## Data, names and updates

- `Values` takes any `IEnumerable<T>`. Use an `ObservableCollection<T>` and the chart animates every added, removed or
  replaced value, see [automatic updates]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Automatic%20updates).
- To plot your own types, see [mappers]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Mappers).
- `Name`{{ if xaml }} (`SeriesName` in XAML){{ end }} is shown in tooltips and legends. When it is not set, the
  series is named "Series 1", "Series 2" and so on.
- `ZIndex` sets which series draws on top, and `IsVisible` shows or hides the series.
- `DataPadding` leaves one axis tick of space on each side of the data by default.
- A scatter series can also draw error bars, see
  [box series and error bars]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Box%20series%20and%20error%20bars).
