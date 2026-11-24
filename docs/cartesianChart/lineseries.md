<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
Samples are pulled from the repository with render_sample, the code shown always matches the samples.
-->

# Line and step line series

A `LineSeries` joins its points with a line, straight or curved, and can fill the area below it. A `StepLineSeries`
joins them with horizontal and vertical segments instead. Both series share almost every property, so this article covers
them together: everything below applies to both, except `LineSmoothness`, which only the line series has.

:::info
This article highlights the most used features. Every property is listed in the API explorer:
[LineSeries]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.LineSeries-3) and
[StepLineSeries]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.StepLineSeries-3).
{{~ if xaml ~}}
In XAML, declare them as `XamlLineSeries` and `XamlStepLineSeries`.
{{~ end ~}}
:::

## Basic line

Set the `Values` of the series and add it to the chart. Every point is drawn with a geometry, a circle by default.
The second generic argument changes the geometry, the second series in this sample uses a `StarGeometry`.

{{ render_sample "lines/basic" }}

## Straight or smooth lines

`LineSmoothness` goes from `0` to `1`: `0` draws straight segments between the points and `1` draws the most curved
line. Values outside that range are ignored. Setting `GeometrySize` to `0` hides the points.

{{ render_sample "lines/straight" }}

## Area

The `Fill` paint fills the area under the line. Set `Fill` to `null` to draw only the line, or set `Stroke` and the
geometry paints to `null` to draw only the area.

{{ render_sample "lines/area" }}

## Stroke and fill paints

When `Stroke`, `Fill`, `GeometryStroke` or `GeometryFill` are not set, LiveCharts picks them from the current theme,
based on the position of the series in the chart's series collection. Set them to use your own colors.

`Stroke` draws the line:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/linestroke.png)

```csharp
Series = new ISeries[]
{
    new LineSeries<int>
    {
        Values = new [] { 4, 4, 7, 2, 8 },
        Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 4 }, // mark
        Fill = null,
        GeometryFill = null,
        GeometryStroke = null
    },
    new LineSeries<int>
    {
        Values = new [] { 7, 5, 3, 2, 6 },
        Stroke = new SolidColorPaint(SKColors.Red) { StrokeThickness = 8 }, // mark
        Fill = null,
        GeometryFill = null,
        GeometryStroke = null
    },
    new LineSeries<int>
    {
        Values = new [] { 4, 2, 5, 3, 9 },
        Stroke = new SolidColorPaint(SKColors.Green) { StrokeThickness = 1 }, // mark
        Fill = null,
        GeometryFill = null,
        GeometryStroke = null
    }
};
```

`Fill` draws the area. The alpha channel goes from `0` (transparent) to `255` (opaque):

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/linefill.png)

```csharp
Series = new ISeries[]
{
    new LineSeries<int>
    {
        Values = new [] { 4, 4, 7, 2, 8 },
        Fill = new SolidColorPaint(SKColors.Blue.WithAlpha(90)), // mark
        Stroke = null,
        GeometryFill = null,
        GeometryStroke = null
    },
    new LineSeries<int>
    {
        Values = new [] { 7, 5, 3, 2, 6 },
        Fill = new SolidColorPaint(SKColors.Red.WithAlpha(90)), // mark
        Stroke = null,
        GeometryFill = null,
        GeometryStroke = null
    },
    new LineSeries<int>
    {
        Values = new [] { 4, 2, 5, 3, 9 },
        Fill = new SolidColorPaint(SKColors.Green.WithAlpha(90)), // mark
        Stroke = null,
        GeometryFill = null,
        GeometryStroke = null
    }
};
```

:::info
Paints can also draw gradients and dashed lines, see the
[Paints article]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Paints).
:::

## Point geometries

`GeometryFill` and `GeometryStroke` paint the shape drawn at each point, and `GeometrySize` sets its size. While
`GeometryStroke` is not set, the theme derives it from the series' `Stroke` (when `Stroke` is a `SolidColorPaint`),
so the points match the line.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/geometrystrokefill.png)

```csharp
Series = new ISeries[]
{
    new LineSeries<int>
    {
        Values = new [] { 4, 4, 7, 2, 8 },
        Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 4 },
        Fill = null,
        GeometryFill = new SolidColorPaint(SKColors.AliceBlue), // mark
        GeometryStroke = new SolidColorPaint(SKColors.Gray) { StrokeThickness = 4 } // mark
    },
    new LineSeries<int>
    {
        Values = new [] { 7, 5, 3, 2, 6 },
        Stroke = new SolidColorPaint(SKColors.Red) { StrokeThickness = 8 },
        Fill = null,
        GeometryFill = new SolidColorPaint(SKColors.IndianRed), // mark
        GeometryStroke = new SolidColorPaint(SKColors.DarkSalmon) { StrokeThickness = 8 } // mark
    }
};
```

The shape itself can be any geometry. The next sample uses, in order, the default circle, a predefined
`StarGeometry` (the `LiveChartsCore.SkiaSharpView.Drawing.Geometries` namespace has more, such as `CrossGeometry`,
`DiamondGeometry` and `RectangleGeometry`), an SVG path through `VariableSVGPathGeometry` and `GeometrySvg`, and a
geometry drawn with the SkiaSharp API (`MyGeometry`).

{{ render_sample "lines/custom" }}

## Customize each point

The `PointMeasured` event (a command in the XAML samples) runs every time a point gets its size and position, and
gives access to both the data and the drawn shape. This sample stores a wind direction in each data point and rotates
an arrow geometry to match it.

{{ render_sample "lines/custompoints" }}

## Gaps

A `null` value creates a gap in the line. This is controlled by `EnableNullSplitting`, which is `true` by default;
disabling it saves some work when your data never contains nulls.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/linesnullsplit.png)

```csharp
Series = new ISeries[]
{
    new LineSeries<int?>
    {
        Values = new int?[] { 5, 4, 2, null, 3, 8, 6 }, // mark
        LineSmoothness = 0
    }
};
```

## Step lines

A `StepLineSeries` takes the same properties as the line series, except `LineSmoothness`. All the paints, geometries,
gaps and data labels in this article work the same way on it.

{{ render_sample "steplines/basic" }}

## Data labels

Data labels draw a label next to every point:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/1.8.datalabels.png)

```csharp
new LineSeries<double>
{
    Values = new double[] { 2, 1, 3, 5, 3, 4, 6 },
    Fill = null,
    DataLabelsSize = 20,
    DataLabelsPaint = new SolidColorPaint(SKColors.Blue),
    // all the positions:
    // {{ website_url }}/api/{{ version }}/LiveChartsCore.Measure.DataLabelsPosition
    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
    // the formatter takes the point and returns the label text
    DataLabelsFormatter = point => point.Coordinate.PrimaryValue.ToString("C2")
}
```

## Data padding

`DataPadding` is the space between the first and last points and the edges of the chart, measured in axis ticks
(the distance between two separators). It is a `LvcPoint`, `X` for the horizontal edges and `Y` for the vertical ones.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/1.8.padding00.png)

```csharp
new LineSeries<double>
{
    Values = new double[] { 2, 1, 3, 5, 3, 4, 6 },
    DataPadding = new LvcPoint(0, 0), // mark
    GeometryStroke = null,
    GeometryFill = null,
    Fill = null
}
```

## Data, names and updates

- `Values` takes any `IEnumerable<T>`. Use an `ObservableCollection<T>` and the chart animates every added, removed or
  replaced value, see [automatic updates]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Automatic%20updates).
- To plot your own types, or set both X and Y, see [mappers]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Mappers).
- `Name`{{ if xaml }} (`SeriesName` in XAML){{ end }} is shown in tooltips and legends. When it is not set, the
  series is named "Series 1", "Series 2" and so on.
- `ZIndex` sets which series draws on top, and `IsVisible` shows or hides the series.
