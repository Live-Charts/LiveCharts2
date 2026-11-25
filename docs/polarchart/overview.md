<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
Samples are pulled from the repository with render_sample, the code shown always matches the samples.
-->

# Polar charts

The `PolarChart` control plots data in the
[polar coordinate system](https://en.wikipedia.org/wiki/Polar_coordinate_system): every point has an angle and a
radius. It draws `PolarLineSeries`, a line series whose points go around a circle instead of along an X axis, and
it has two kinds of axes: the angle axes, around the circle, and the radius axes, from the center out. This article
covers the control, the series and the axes together.

:::info
This article highlights the most used features. Every property is listed in the API explorer:
[PolarChart]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.{{ platform }}.PolarChart),
[PolarLineSeries]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.PolarLineSeries-3) and
[PolarAxis]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.PolarAxis).
{{~ if xaml ~}}
In XAML, declare them as `XamlPolarLineSeries` and `XamlPolarAxis`.
{{~ end ~}}
:::

## Add the control

Assign the `Series` property with a collection of series:

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

The series can also be declared inside the chart in XAML, as `XamlPolarLineSeries`; the samples below do it that way.
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
polarChart1.Series = new[]
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

{{~ if eto ~}}
```csharp
Content = new PolarChart
{
    Series = new[]
    {
        new PolarLineSeries<double>
        {
            Values = new double[] { 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 },
            Fill = null,
            IsClosed = false
        }
    }
};
```
{{~ end ~}}

The snippet does not set the axes, the tooltip, the legend or the colors of the series: LiveCharts picks them from
the current theme. Set them only when you need something different.

## Polar line series

Each value of a `PolarLineSeries` is the radius of a point, and its position in the collection is the angle. The angle
axis goes from the first index to one past the last one, so the points are spread evenly around the circle and the
last point does not land on the first one. The first point is at the top and the next ones follow clockwise.

This sample also draws a label on each point (see [data labels](#data-labels)), sets the `MaxLimit` of the radius
axis and the angle of its labels, and rotates the labels of the angle axis (see
[labels position and rotation](#labels-position-and-rotation)).

{{ render_sample "polar/basic" }}

The second generic argument of the series changes the geometry drawn at each point, as in a line series:
`PolarLineSeries<double, StarGeometry>`.

## Closed and open lines

`IsClosed` is `true` by default: when the series has more than two points, the line goes back from the last point to
the first one. Set it to `false` to leave the line open:

```csharp
Series = new ISeries[]
{
    new PolarLineSeries<double>
    {
        Values = new double[] { 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 },
        Fill = null,
        IsClosed = false // mark
    }
};
```

## Radial area

The default theme fills the area inside the line with the color of the series, mostly transparent. Set `Fill` to use
your own paint, or to `null` to draw only the line. `LineSmoothness` goes from `0` to `1`, `0.65` by default: `0`
draws straight segments between the points and `1` the most curved line. This sample compares both, with no point
geometries (`GeometrySize` is `0`), and names each angle with the `Labels` of the angle axis.

{{ render_sample "polar/radialArea" }}

The sample also sets `InitialRotation` to `-45`, see [initial rotation](#initial-rotation).

## Polar coordinates

To set both the angle and the radius of each point, use `ObservablePolarPoint`; its `Angle` and `Radius` properties
notify the chart when they change. The angle is in the scale of the angle axis, so this sample sets the `MinLimit` of
the angle axis to `0` and its `MaxLimit` to `360` to read the angles as degrees. `MinStep` and `ForceStepToMin` draw a
label every 30 degrees, and the `Labeler` adds the degree sign.

{{ render_sample "polar/coordinates" }}

To plot your own types, see [mappers]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Mappers): in a
polar series the X of the coordinate is the angle and the Y is the radius.

## Stroke and fill paints

When `Stroke`, `Fill`, `GeometryStroke` or `GeometryFill` are not set, LiveCharts picks them from the current theme,
based on the position of the series in the chart's series collection. A polar line series takes the same paints,
point geometries and `GeometrySize` as a line series; the
[line series article]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Line%20and%20step%20line%20series#stroke-and-fill-paints)
explains them in detail.

```csharp
Series = new ISeries[]
{
    new PolarLineSeries<int>
    {
        Values = new [] { 4, 4, 7, 2, 8 },
        Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 4 }, // mark
        Fill = new SolidColorPaint(SKColors.Blue.WithAlpha(90)), // mark
        GeometryFill = new SolidColorPaint(SKColors.AliceBlue), // mark
        GeometryStroke = new SolidColorPaint(SKColors.Gray) { StrokeThickness = 4 }, // mark
        GeometrySize = 10 // mark
    },
    new PolarLineSeries<int>
    {
        Values = new [] { 7, 5, 3, 2, 6 },
        Stroke = new SolidColorPaint(SKColors.Red) { StrokeThickness = 8 },
        Fill = null,
        GeometryFill = null,
        GeometryStroke = null
    }
};
```

## Gaps

A `null` value creates a gap in the line. This is controlled by `EnableNullSplitting`, which is `true` by default.
`IsClosed` applies to each piece of the line, so this snippet sets it to `false` to keep the pieces open:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/polarlinesnullsplit.png)

```csharp
Series = new ISeries[]
{
    new PolarLineSeries<int?>
    {
        Values = new int?[] { 5, 4, 2, null, 3, 8, 6 }, // mark
        LineSmoothness = 0,
        IsClosed = false
    }
};
```

## Data labels

`ShowDataLabels` draws a label on every point, with a paint from the theme; setting `DataLabelsPaint` also turns them
on. `DataLabelsPosition` places them: `Middle` centers the label on the point, `End` moves it away from the center of
the chart and `Start` moves it toward the center.

`DataLabelsRotation` rotates the labels, in degrees. Two constants make the labels follow the circle:
`LiveCharts.TangentAngle` turns each label along the tangent of the circle at its point, and
`LiveCharts.CotangentAngle` along the line from the center to the point, as in the
[polar line series](#polar-line-series) sample. Add degrees to them to rotate further:

```csharp
new PolarLineSeries<double>
{
    Values = new double[] { 15, 14, 13, 12, 11, 10, 9, 8 },
    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
    DataLabelsSize = 10,
    // all the positions:
    // {{ website_url }}/api/{{ version }}/LiveChartsCore.Measure.PolarLabelsPosition
    DataLabelsPosition = PolarLabelsPosition.End,
    DataLabelsRotation = LiveCharts.TangentAngle + 30, // the tangent + 30 degrees
    DataLabelsFormatter = point => point.Coordinate.PrimaryValue.ToString("N1")
}
```

## Angle and radius axes

The chart has the `AngleAxes` and `RadiusAxes` properties, each a collection of `PolarAxis`. When a collection is not
set or is empty, the chart uses one default axis. The separators of an angle axis are lines from the center, the
separators of a radius axis are circles. `LabelsPaint` and `SeparatorsPaint` set their paints, and `TextSize` the size
of the labels:

```csharp
RadiusAxes = new PolarAxis[]
{
    new PolarAxis
    {
        TextSize = 10,
        LabelsPaint = new SolidColorPaint(SKColors.Blue), // mark
        SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) { StrokeThickness = 2 } // mark
    }
};

AngleAxes = new PolarAxis[]
{
    new PolarAxis
    {
        TextSize = 20,
        LabelsPaint = new SolidColorPaint(SKColors.Green), // mark
        SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) // mark
        { // mark
            StrokeThickness = 2, // mark
            PathEffect = new DashEffect(new float[] { 3, 3 }) // mark
        } // mark
    }
};
```

{{~ if xaml ~}}
```xml
<lvc:PolarChart
    Series="{Binding Series}"
    RadiusAxes="{Binding RadiusAxes}"
    AngleAxes="{Binding AngleAxes}">
</lvc:PolarChart>
```
{{~ end ~}}

{{~ if blazor ~}}
```xml
<PolarChart
    Series="series"
    RadiusAxes="radiusAxes"
    AngleAxes="angleAxes">
</PolarChart>
```
{{~ end ~}}

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/polar-axes-style.png)

A polar axis shares most of its properties with a Cartesian axis: `Labels`, `Labeler`, `MinLimit`, `MaxLimit`,
`MinStep`, `ForceStepToMin`, `CustomSeparators`, `ShowSeparatorLines`, `IsInverted` and `IsVisible` work as described
in the [axes article]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Axes). With more than one
axis, `ScalesAngleAt` and `ScalesRadiusAt` set the index of the axes a series uses, `0` by default.

## Labels and labelers

`Labels` names each position of the axis: the label at index `i` is drawn at the value `i`, and values outside the
collection get no label. With `Labels` the axis step is at least `1`; `ForceStepToMin` makes it exactly `1`, so every
label is drawn. `Labeler` formats the values instead. This radar chart names the angles and formats the radius with
two decimals:

```csharp
Series = new ISeries[]
{
    new PolarLineSeries<double>
    {
        Values = new double[] { 5, 2, 1, 4, 3, 3 },
        GeometryFill = null,
        GeometryStroke = null,
        LineSmoothness = 0.2,
        Stroke = null
    },
    new PolarLineSeries<double>
    {
        Values = new double[] { 3, 5, 2, 3, 4, 5 },
        GeometryFill = null,
        GeometryStroke = null,
        LineSmoothness = 0.2,
        Stroke = null
    }
};

RadiusAxes = new PolarAxis[]
{
    new PolarAxis
    {
        // formats the value as a number with 2 decimals.
        Labeler = value => value.ToString("N2") // mark
    }
};

AngleAxes = new PolarAxis[]
{
    new PolarAxis
    {
        Labels = new[] { "strength", "stamina", "resistance", "power", "hit points", "mana" }, // mark
        ForceStepToMin = true
    }
};
```

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/polar-star.png)

## Labels position and rotation

The labels of a radius axis are drawn along a line from the center. `LabelsAngle` sets the angle of that line in
degrees, `0` by default, which points right; the angle grows clockwise and `InitialRotation` does not change it. It
has no effect on an angle axis.

`LabelsRotation` rotates the labels, in degrees. On an angle axis, `LiveCharts.TangentAngle` and
`LiveCharts.CotangentAngle` make each label follow the circle, as they do for [data labels](#data-labels). This
snippet rotates the radius labels 45 degrees and the angle labels along the tangent:

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

With `LiveCharts.CotangentAngle` the angle labels point to the center:

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

Both constants also take extra degrees: `LiveCharts.TangentAngle + 30` is the tangent plus 30 degrees.

## Initial rotation

`InitialRotation` sets, in degrees, where the angle axis starts. It is `-90` by default, the top of the circle; `0` is
the right side, and positive values turn clockwise.

{{~ if xaml ~}}
```xml
<lvc:PolarChart
    Series="{Binding Series}"
    InitialRotation="-45"><!-- mark -->
</lvc:PolarChart>
```
{{~ end ~}}

{{~ if blazor ~}}
```xml
<PolarChart
    Series="series"
    InitialRotation="-45"><!-- mark -->
</PolarChart>
```
{{~ end ~}}

{{~ if winforms || eto ~}}
```csharp
polarChart1.InitialRotation = -45;
```
{{~ end ~}}

Changes to `InitialRotation`, `InnerRadius` and `TotalAngle` are animated like any other update:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/polar-rotation.gif)

## Inner radius

`InnerRadius` leaves an empty circle at the center of the chart, in pixels, `0` by default. The radius axis starts at
its edge.

{{~ if xaml ~}}
```xml
<lvc:PolarChart
    Series="{Binding Series}"
    InnerRadius="50"><!-- mark -->
</lvc:PolarChart>
```
{{~ end ~}}

{{~ if blazor ~}}
```xml
<PolarChart
    Series="series"
    InnerRadius="50"><!-- mark -->
</PolarChart>
```
{{~ end ~}}

{{~ if winforms || eto ~}}
```csharp
polarChart1.InnerRadius = 50;
```
{{~ end ~}}

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/polar-innerRadius.gif)

## Total angle

`TotalAngle` is the angle, in degrees, that the whole angle axis covers, `360` by default. A smaller value draws the
chart on part of the circle.

{{~ if xaml ~}}
```xml
<lvc:PolarChart
    Series="{Binding Series}"
    TotalAngle="270"><!-- mark -->
</lvc:PolarChart>
```
{{~ end ~}}

{{~ if blazor ~}}
```xml
<PolarChart
    Series="series"
    TotalAngle="270"><!-- mark -->
</PolarChart>
```
{{~ end ~}}

{{~ if winforms || eto ~}}
```csharp
polarChart1.TotalAngle = 270;
```
{{~ end ~}}

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/polar-totalangle.gif)

`FitToBounds`, `false` by default, moves the center of the chart and scales it so the series fill the control, which
helps when they use only part of the circle.

## Tooltips and legends

Tooltips and legends work as in a Cartesian chart, see the
[tooltips and crosshairs]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Tooltips%20and%20crosshairs)
and [legends]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Legends) articles. What differs:

- The tooltip shows the points under the pointer; the area of each point is at least 16 pixels wide. There is no
  finding strategy to set.
- Each row shows the radius, formatted with the `Labels` or the `Labeler` of the radius axis. The header shows the
  angle only when the angle axis has `Labels` or a `Labeler`.
- `RadiusToolTipLabelFormatter` and `AngleToolTipLabelFormatter` on the series build those two texts; they take the
  point and return a string.

`Name`{{ if xaml }} (`SeriesName` in XAML){{ end }} is shown in tooltips and legends. A polar chart has no zooming or
panning.

## Data, names and updates

- `Values` takes any `IEnumerable<T>`. Use an `ObservableCollection<T>` and the chart animates every added, removed or
  replaced value, see [automatic updates]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Automatic%20updates).
- `Title`, `AnimationsSpeed`, `EasingFunction` and `DrawMargin` work as in the
  [Cartesian chart control]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Cartesian%20chart%20control).
- `ZIndex` sets which series draws on top, and `IsVisible` shows or hides the series.
