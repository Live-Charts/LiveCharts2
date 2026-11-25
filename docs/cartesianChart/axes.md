<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
Samples are pulled from the repository with render_sample, the code shown always matches the samples.
-->

# Axes

A `CartesianChart` has two collections of axes, `XAxes` and `YAxes`. When a collection is not set, or is empty, the
chart uses one default `Axis` for it. An axis sets the range of the data that is visible, and draws the name, the
labels, the separators and the ticks:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/1.7.diagram.png)

:::info
This article highlights the most used features. Every property is listed in the API explorer:
[Axis]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.Axis).
{{~ if xaml ~}}
In XAML, declare an axis as `XamlAxis`, and set its name with `AxisName` (Name is taken by the framework element).
{{~ end ~}}
:::

## Style

Every part of the axis has its own paint:

- `NamePaint` draws the `Name`{{ if xaml }} (`AxisName` in XAML){{ end }}, with `NameTextSize` (`20` by default)
  and `NamePadding`.
- `LabelsPaint` draws the labels, with `TextSize` (`16` by default).
- `SeparatorsPaint` draws a line across the chart at each label, and `SubseparatorsPaint` draws
  `SubseparatorsCount` (`3` by default) thinner lines between them.
- `TicksPaint` draws a short tick next to each label, and `SubticksPaint` the ticks between them. When
  `DrawTicksPath` is `true`, `TicksPaint` also draws the axis line.
- `ZeroPaint` draws a line at the value `0`.

A paint that is `null` is not drawn. The default theme sets `NamePaint` and `LabelsPaint` on every axis, and
`SeparatorsPaint` only on the Y axes, so a default chart has horizontal separators and no vertical ones.
`ShowSeparatorLines` hides the separators and subseparators without removing their paints.

{{ render_sample "axes/style" }}

The same kind of paints set in code:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/axes-paints.png)

```csharp
XAxes = new Axis[]
{
    new Axis
    {
        Name = "X Axis",
        NamePaint = new SolidColorPaint(SKColors.Black), // mark
        LabelsPaint = new SolidColorPaint(SKColors.Blue), // mark
        TextSize = 10,
        SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) { StrokeThickness = 2 } // mark
    }
};

YAxes = new Axis[]
{
    new Axis
    {
        Name = "Y Axis",
        NamePaint = new SolidColorPaint(SKColors.Red), // mark
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

:::info
Paints can also draw gradients and dashed lines, see the
[Paints article]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Paints).
:::

## Position

`Position` places the axis at the `Start` or at the `End` of the chart. `Start`, the default, is the bottom for an
X axis and the left for a Y axis; `End` is the top for an X axis and the right for a Y axis.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/1.7.position.png)

```csharp
YAxes = new Axis[]
{
    new Axis
    {
        Position = LiveChartsCore.Measure.AxisPosition.End // mark
    }
};
```

## Labels and labelers

There are two ways to set the text of the labels:

- `Labels` is a list of strings. The label at a value is the item at that index, so it fits series where each point
  is a category: the first point is at `0`, the second at `1` and so on. Values outside the list get an empty label.
  While `Labels` is set, the step between labels is never less than `1`.
- `Labeler` is a function that takes the value and returns the label. The `Labelers` class has some ready to use,
  such as `Labelers.Currency`, which also shortens millions, billions and trillions (`M`, `B`, `T`).

When `Labels` is set, `Labeler` is ignored. This sample names the categories of the X axis with `Labels`, formats the
Y axis as currency with a `Labeler`, and changes the font of the Y labels in its `LabelsPaint`.

{{ render_sample "axes/labelsFormat" }}

The same in code, with the currency labeler of the library:

```csharp
XAxes = new Axis[]
{
    new Axis
    {
        Labels = new string[] { "Anne", "Johnny", "Zac", "Rosa" } // mark
    }
};

YAxes = new Axis[]
{
    new Axis
    {
        Labeler = Labelers.Currency // mark
        // or your own format, for example:
        // Labeler = value => value.ToString("C")
    }
};
```

When `Labeler` is not set, the axis uses `Labelers.Default`. To change it for every axis in your app, call
`Labelers.SetDefaultLabeler(...)` before the axes are created.

## Non-Latin labels

When the font of a paint has no glyph for a character, LiveCharts asks the system for a font that has it, so labels
in Chinese, Arabic or other scripts are drawn without extra setup. To pick the font yourself, set the `SKTypeface`
of the paint, for example `SKTypeface.FromFamilyName("Noto Sans SC")`.

{{ render_sample "axes/labelsFormat2" }}

## Labels rotation

`LabelsRotation` rotates the labels, in degrees. This chart rotates the Y labels 45 degrees:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/1.7.rotation.png)

```csharp
YAxes = new Axis[]
{
    new Axis
    {
        LabelsRotation = 45 // mark
    }
};
```

## Labels density

The axis calculates how many labels fit, based on the data, the size of the chart and the size of the labels.
`LabelsDensity` changes the space it leaves between them: `0` is the most dense, larger values separate the labels
more, and values below `0` let them overlap. The default is `0.85`. It is not used while the labels are rotated
between 20 and 70 degrees.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/labelsdensity.gif)

## Step

The step is the distance between two labels, in data units. The axis calculates it and rounds it to 1, 2 or 5 times a
power of ten. Three properties control it:

- `MinStep` is the smallest step allowed, `0` by default. Set it to `1` to never show decimal labels.
- `ForceStepToMin` uses `MinStep` as the step, always. Be careful with a small step on a large range: the axis draws
  every separator.
- `MinSeparators` is the minimum number of separators the calculated step must produce, `3` by default. When the
  rounding leaves fewer, the step is divided further. Set it to `0` to disable it. It is ignored when
  `ForceStepToMin` is `true`.

```csharp
YAxes = new Axis[]
{
    new Axis
    {
        MinStep = 1 // mark
    }
};
```

## Custom separators

`CustomSeparators` sets the exact values where the separators and labels go, instead of placing them every step. This
sample places them at 0, 10, 25, 50 and 100, and sets the limits of the axis to 0 and 100.

{{ render_sample "axes/customSeparatorsInterval" }}

## Limits

`MinLimit` and `MaxLimit` set the visible range of the axis, points outside it are not visible. Both are `null` by
default, and `null` (or `double.NaN`) means the axis takes the limit from the data. For example, to show only the
first 10 points of a series (paging):

```csharp
var xAxis = new Axis
{
    MinLimit = 0, // mark
    MaxLimit = 10 // mark
};

XAxes = new Axis[] { xAxis };
```

Zooming and panning change both properties, see
[zooming and panning]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Zooming%20and%20panning),
which also shows how to clear the zoom. `Axis` implements `INotifyPropertyChanged`, so you can listen to every change
of the visible range, made by code or by the user:

```csharp
xAxis.PropertyChanged += (sender, e) =>
{
    if (e.PropertyName is nameof(Axis.MinLimit) or nameof(Axis.MaxLimit))
    {
        var min = xAxis.MinLimit;
        var max = xAxis.MaxLimit;
    }
};
```

While the limits are `null`, read `DataBounds` (the range of the data) and `VisibleDataBounds` (the range of the
visible data) instead. They are set when the chart is measured, so read them in the `UpdateStarted` event of the
chart, or later.

## Inverted axes

An X axis grows to the right and a Y axis grows up. Set `IsInverted` to `true` to reverse the direction of an axis.

## Hidden axes

When `IsVisible` is `false` the axis is not drawn and takes no space in the chart. The series keep using it to scale.

## Unit width

`UnitWidth` is the width of one data point in data units, `1` by default. Bar series (columns and rows) and financial
series use it to size each point. Change it when the distance between two points is not `1`, which is normally the
case in date time scales, see
[time and logarithmic scales]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Time%20and%20logarithmic%20scales).

```csharp
XAxes = new Axis[]
{
    new Axis
    {
        // the X values are DateTime ticks and there is one point per day
        UnitWidth = TimeSpan.FromDays(1).Ticks // mark
    }
};
```

## Multiple axes

Add more than one axis to `XAxes` or `YAxes`, and choose the axis of each series with `ScalesXAt` and `ScalesYAt`,
the index of the axis in the collection (`0` by default). Axes on the same side are placed side by side; the first
one in the collection is the farthest from the series. In this sample the two "tens" series scale at the first Y axis,
and the other two at their own axes on the right, which hide their separators with `ShowSeparatorLines`.

{{ render_sample "axes/multiple" }}

## Shared axes

Shared axes keep two or more charts aligned: the range of each axis includes the data of the others, and zooming,
panning and crosshairs in one chart are applied to the others. Create one axis per chart and share them with
`SharedAxes.Set`{{ if xaml }}, or in XAML with a `SharedAxesPair` and the `FromSharedAxes` extension{{ end }}. The
labels can have different widths in each chart, so set the same left `DrawMargin` in both to align them.

```csharp
var xAxis1 = new Axis();
var xAxis2 = new Axis();

SharedAxes.Set(xAxis1, xAxis2); // mark

chart1.XAxes = new Axis[] { xAxis1 };
chart2.XAxes = new Axis[] { xAxis2 };
```

{{ render_sample "axes/shared" }}

## Match the axes scale

By default each axis fits its own range, so one unit on the X axis and one unit on the Y axis can take a different
number of pixels. Set `MatchAxesScreenDataRatio` to `true` on the chart and both take the same number of pixels per
unit, at any size, zoom or pan. This sample also forces the same step on both axes, so the separators draw squares.
It only works with one X axis and one Y axis.

{{ render_sample "axes/matchScale" }}

The chart does this in its `DrawMarginDefined` event, which runs once the chart knows the area of the series and
before the axes are drawn; there, the limits of the axes can be changed with `SetLimits(..., notify: false)`. The
sample includes two other scales built the same way: `CustomScaleExtensions` gives the Y axis twice the pixels per
unit of the X axis, and `InchScaleExtensions` makes the separators one inch apart (it assumes 96 pixels per inch,
read the real value of the screen in your app). To use one, remove `MatchAxesScreenDataRatio` and call it with the
chart control:

```csharp
CustomScaleExtensions.DoubleY(myChart); // mark
```

## Related features

- Zooming and panning, and how to clear them, are covered in
  [zooming and panning]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Zooming%20and%20panning).
- Date time, time span and logarithmic axes are covered in
  [time and logarithmic scales]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Time%20and%20logarithmic%20scales).
- Crosshairs are set on the axes, see
  [tooltips and crosshairs]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Tooltips%20and%20crosshairs).
