<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
Samples are pulled from the repository with render_sample, the code shown always matches the samples.
-->

# Cartesian chart control

The `CartesianChart` control plots data in the
[Cartesian coordinate system](https://en.wikipedia.org/wiki/Cartesian_coordinate_system): every point has an X and a
Y coordinate. This article shows how to add the control and the properties of the chart itself; each part of the
chart (series, axes, tooltips, legends, zooming) has its own article, linked from the sections below.

:::info
Every property of the control is listed in the API explorer:
[CartesianChart]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.{{ platform }}.CartesianChart).
{{~ if xaml ~}}
In XAML, declare sections as `XamlRectangularSection` and the title as `XamlDrawnLabelVisual`.
{{~ end ~}}
:::

## Add the control

To get started, assign the `Series` property with a collection of
[`ISeries`]({{ website_url }}/api/{{ version }}/LiveChartsCore.ISeries):

{{~ if xaml ~}}
```csharp
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace ViewModelsSamples.Bars.Basic
{
    public class ViewModel
    {
        public ISeries[] Series { get; set; }
            = new ISeries[]
            {
                new LineSeries<int>
                {
                    Values = new int[] { 4, 6, 5, 3, -3, -1, 2 }
                },
                new ColumnSeries<double>
                {
                    Values = new double[] { 2, 5, 4, -2, 4, -3, 5 }
                }
            };
    }
}
```

```xml
<lvc:CartesianChart
    Series="{Binding Series}">
</lvc:CartesianChart>
```

The series can also be declared inside the chart in XAML, as `XamlLineSeries`, `XamlColumnSeries` and so on, the
series articles show both ways.
{{~ end ~}}

{{~ if blazor ~}}
```xml
<CartesianChart
    Series="series">
</CartesianChart>
```

```csharp
private ISeries[] series = new ISeries[]
{
    new LineSeries<int>
    {
        Values = new int[] { 4, 6, 5, 3, -3, -1, 2 }
    },
    new ColumnSeries<double>
    {
        Values = new double[] { 2, 5, 4, -2, 4, -3, 5 }
    }
};
```
{{~ end ~}}

{{~ if winforms ~}}
Drag a new `CartesianChart` control from your toolbox, then in the code behind assign the `Series` property:

```csharp
cartesianChart1.Series = new ISeries[]
{
    new LineSeries<int>
    {
        Values = new int[] { 4, 6, 5, 3, -3, -1, 2 }
    },
    new ColumnSeries<double>
    {
        Values = new double[] { 2, 5, 4, -2, 4, -3, 5 }
    }
};
```
{{~ end ~}}

{{~ if eto ~}}
```csharp
Content = new CartesianChart
{
    Series = new ISeries[]
    {
        new LineSeries<int>
        {
            Values = new int[] { 4, 6, 5, 3, -3, -1, 2 }
        },
        new ColumnSeries<double>
        {
            Values = new double[] { 2, 5, 4, -2, 4, -3, 5 }
        }
    }
};
```
{{~ end ~}}

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/cc-mvp.png)

The main components of the chart are the series, the
[axes]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Axes), the
[tooltip]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Tooltips%20and%20crosshairs) and the
[legend]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Legends):

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/chart-overview.png)

The previous snippet does not set the axes, the tooltip, the legend or the colors of the series: LiveCharts picks them
from the current theme. Set them only when you need something different.


## Series

There are multiple series available in the library, you can add one or mix them all in the same chart, every series has unique properties,
any image below is a link to an article explaining more about them.

<a href="{{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Line%20and%20step%20line%20series">
<div class="series-miniature">
<img src="https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/samples/lines/basic/result.png" alt="series"/>
<div class="text-center"><b>Line series</b></div>
</div>
</a>

<a href="{{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Column%20and%20row%20series">
<div class="series-miniature">
<img src="https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/samples/bars/basic/result.png"  alt="series"/>
<div class="text-center"><b>Column series</b></div>
</div>
</a>

<a href="{{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Scatter%20and%20bubble%20series">
<div class="series-miniature">
<img src="https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/samples/scatter/basic/result.png"  alt="series"/>
<div class="text-center"><b>Scatter series</b></div>
</div>
</a>

<a href="{{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Line%20and%20step%20line%20series#step-lines">
<div class="series-miniature">
<img src="https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/samples/steplines/basic/result.png" alt="series"/>
<div class="text-center"><b>Step line series</b></div>
</div>
</a>

<a href="{{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Heat%20series">
<div class="series-miniature">
<img src="https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/samples/heat/basic/result.png" alt="series"/>
<div class="text-center"><b>Heat series</b></div>
</div>
</a>

<a href="{{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Financial%20series">
<div class="series-miniature">
<img src="https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/samples/financial/basicCandlesticks/result.png" alt="series"/>
<div class="text-center"><b>Candle sticks series</b></div>
</div>
</a>

<a href="{{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Stacked%20series#stacked-area">
<div class="series-miniature">
<img src="https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/samples/stackedArea/basic/result.png" alt="series"/>
<div class="text-center"><b>Stacked line series</b></div>
</div>
</a>

<a href="{{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Stacked%20series#stacked-columns">
<div class="series-miniature">
<img src="https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/samples/stackedBars/basic/result.png" alt="series"/>
<div class="text-center"><b>Stacked column series</b></div>
</div>
</a>

<a href="{{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Stacked%20series#stacked-step-area">
<div class="series-miniature">
<img src="https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/samples/stackedArea/stepArea/result.png" alt="series"/>
<div class="text-center"><b>Stacked step line series</b></div>
</div>
</a>

<a href="{{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Box%20series%20and%20error%20bars#box-series">
<div class="series-miniature">
<img src="https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/samples/box/basic/result.png" alt="series"/>
<div class="text-center"><b>Box series</b></div>
</div>
</a>

<a href="{{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Box%20series%20and%20error%20bars#error-bars">
<div class="series-miniature">
<img src="https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/samples/error/basic/result.png" alt="series"/>
<div class="text-center"><b>Error series</b></div>
</div>
</a>

<a href="{{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Scatter%20and%20bubble%20series#bubbles">
<div class="series-miniature">
<img src="https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/samples/scatter/bubbles/result.png" alt="series"/>
<div class="text-center"><b>Bubble series</b></div>
</div>
</a>

## Axes

The chart has the `XAxes` and `YAxes` properties, each a collection of axes. When a collection is not set or is empty,
the chart uses one default axis. Set them to name the labels, format them, change the paints or the limits of an
axis. In this sample `Labels` names each category of the X axis and `Labeler` formats the values of the Y axis as
currency:

```csharp
Series = new ISeries[]
{
    new ColumnSeries<int> { Values = new[] { 200, 558, 458, 249 } }
};

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
        // LiveCharts provides some common formatters in the Labelers class,
        // you can also use your own function: value => value.ToString("C")
        Labeler = Labelers.Currency // mark
    }
};
```

{{~ if xaml ~}}
```xml
<lvc:CartesianChart
    Series="{Binding Series}"
    XAxes="{Binding XAxes}"
    YAxes="{Binding YAxes}">
</lvc:CartesianChart>
```
{{~ end ~}}

{{~ if blazor ~}}
```xml
<CartesianChart
    Series="series"
    XAxes="xAxes"
    YAxes="yAxes">
</CartesianChart>
```
{{~ end ~}}

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/1.7.labels.png)

The [axes article]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Axes) covers every axis
feature: names, labels, separators, paints, limits, positions and multiple axes. Date, time and logarithmic axes are
in the [time and logarithmic scales]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Time%20and%20logarithmic%20scales)
article.

## Title

`Title` draws a visual at the top of the chart, centered, and the chart leaves room for it above the plot. A
`DrawnLabelVisual` draws a text; when its paint is not set, the theme picks it.

```csharp
Title = new DrawnLabelVisual(
    new LabelGeometry
    {
        Text = "My chart title",
        TextSize = 25,
        Padding = new LiveChartsCore.Drawing.Padding(15)
    });
```

{{~ if xaml ~}}
Bind it with `Title="{Binding Title}"`, or declare it inside the chart with `XamlDrawnLabelVisual`, as the
[basic line sample]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Line%20and%20step%20line%20series#basic-line)
does.
{{~ end ~}}

## Tooltips and legends

The tooltip shows the hovered points. `TooltipPosition` places it: `Auto` by default, which picks a position based on
the space available, or `Top`, `Bottom`, `Left`, `Right` and `Center`. `Hidden` disables tooltips.

The legend lists the series. It is hidden by default: set `LegendPosition` to `Top`, `Bottom`, `Left` or `Right`
to show it.

{{~ if xaml ~}}
```xml
<lvc:CartesianChart
    Series="{Binding Series}"
    TooltipPosition="Bottom"
    LegendPosition="Right">
</lvc:CartesianChart>
```
{{~ end ~}}

{{~ if blazor ~}}
```xml
<CartesianChart
    Series="series"
    TooltipPosition="LiveChartsCore.Measure.TooltipPosition.Bottom"
    LegendPosition="LiveChartsCore.Measure.LegendPosition.Right">
</CartesianChart>
```
{{~ end ~}}

{{~ if winforms || eto ~}}
```csharp
cartesianChart1.TooltipPosition = LiveChartsCore.Measure.TooltipPosition.Bottom;
cartesianChart1.LegendPosition = LiveChartsCore.Measure.LegendPosition.Right;
```
{{~ end ~}}

The [tooltips and crosshairs]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Tooltips%20and%20crosshairs)
and [legends]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Legends) articles cover their
text, paints and custom controls.

## Zooming and panning

Zooming and panning are disabled by default. Set `ZoomMode` to `X`, `Y` or `Both` to enable them on those axes, see
the [zooming and panning article]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Zooming%20and%20panning)
for every mode and option.

## Draw margin

The draw margin is the area where the series are drawn, inside the axes. `DrawMargin` sets the space between the
edges of the control and that area. When it is `null`, the default, the chart calculates it so the axes, the title and
the legend fit. `Margin.Auto` lets the chart calculate only some of the sides.

```csharp
// 100 pixels on every side
DrawMargin = new Margin(100);

// 70 pixels on the left, the chart calculates the rest
DrawMargin = new Margin(70, Margin.Auto, Margin.Auto, Margin.Auto);
```

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/drawmargin.png)

When you set a side, the axis labels, the title or the legend on that side must fit in that space.

## Draw margin frame

`DrawMarginFrame` paints the draw margin area: `Fill` paints the background and `Stroke` the border.

{{~ if xaml ~}}
```csharp
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace ViewModelsSamples.Lines.Area
{
    public class ViewModel
    {
        public DrawMarginFrame DrawMarginFrame => new DrawMarginFrame
        {
            Fill = new SolidColorPaint(SKColors.AliceBlue),
            Stroke = new SolidColorPaint(SKColors.Black, 3)
        };

        public ISeries[] Series { get; set; } = new ISeries[] { /* ... */ };
    }
}
```

```xml
<lvc:CartesianChart
    Series="{Binding Series}"
    DrawMarginFrame="{Binding DrawMarginFrame}"><!-- mark -->
</lvc:CartesianChart>
```
{{~ end ~}}

{{~ if blazor ~}}
```xml
<CartesianChart
    Series="series"
    DrawMarginFrame="drawMarginFrame"><!-- mark -->
</CartesianChart>
```

```csharp
private DrawMarginFrame drawMarginFrame { get; set; }
    = new DrawMarginFrame
    {
        Fill = new SolidColorPaint(SKColors.AliceBlue),
        Stroke = new SolidColorPaint(SKColors.Black, 3)
    };
```
{{~ end ~}}

{{~ if winforms ~}}
```csharp
cartesianChart1.DrawMarginFrame = new DrawMarginFrame
{
    Fill = new SolidColorPaint(SKColors.AliceBlue),
    Stroke = new SolidColorPaint(SKColors.Black, 3)
};
```
{{~ end ~}}

{{~ if eto ~}}
```csharp
cartesianChart.DrawMarginFrame = new DrawMarginFrame
{
    Fill = new SolidColorPaint(SKColors.AliceBlue),
    Stroke = new SolidColorPaint(SKColors.Black, 3)
};
```
{{~ end ~}}

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/drawmarginframe.png)

## Sections

A section highlights a range of values in the chart. `Sections` takes a collection of `RectangularSection`, and each
section sets its limits with four properties, in the scale of the axes:

- `Xi` and `Xj`: where the section starts and ends on the X axis.
- `Yi` and `Yj`: where the section starts and ends on the Y axis.

A limit that is `null` or `double.NaN`, the default, leaves that side open: with no `Xi` the section starts at the
beginning of the X axis, and with no `Xj` it goes to the end. When both limits of an axis are not set, the section
covers the whole axis. `Fill` paints the section and `Stroke` its border. `Label` draws a text in the section; it is
drawn only when `LabelPaint` is set, and `LabelSize` sets its size, `12` by default.

This sample creates a section from 3 to 4 on the X axis, a section from 5 to 6 on the X axis and from 2 to 8 on the Y
axis, and a labeled section from 8 to the end of the X axis.

{{ render_sample "general/sections" }}

When both limits on an axis are the same, the section is a line. Here `Yi` and `Yj` are both `8` and the X limits are
not set, so the `Stroke` draws a dashed line across the chart at Y = 8:

```csharp
Sections = new RectangularSection[]
{
    new RectangularSection
    {
        Yi = 8, // mark
        Yj = 8, // mark
        Stroke = new SolidColorPaint(SKColors.Red)
        {
            StrokeThickness = 3,
            PathEffect = new DashEffect(new float[] { 6, 6 })
        }
    },
    new RectangularSection
    {
        Xi = 4,
        Xj = 6,
        Fill = new SolidColorPaint(new SKColor(0, 0, 255, 48))
    }
};
```

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/samples/general/sections/result2.png)

With more than one axis, `ScalesXAt` and `ScalesYAt` set the index of the axes the section uses, `0` by default.

## Animations

The chart animates every change. `AnimationsSpeed` sets how long an animation takes, 800 milliseconds by default, and
`EasingFunction` sets its curve, `EasingFunctions.ExponentialOut` by default. Setting `EasingFunction` to `null`
disables animations. Series and axes follow the chart unless they set their own `AnimationsSpeed` or
`EasingFunction`.

{{~ if xaml ~}}
```xml
<lvc:CartesianChart
    Series="{Binding Series}"
    AnimationsSpeed="00:00:00.300"><!-- mark -->
</lvc:CartesianChart>
```
{{~ end ~}}

{{~ if blazor ~}}
```xml
<CartesianChart
    Series="series"
    AnimationsSpeed="TimeSpan.FromMilliseconds(300)"><!-- mark -->
</CartesianChart>
```
{{~ end ~}}

{{~ if winforms || eto ~}}
```csharp
cartesianChart1.AnimationsSpeed = TimeSpan.FromMilliseconds(300);
```
{{~ end ~}}

See the [animations article]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Animations) for the easing
curves and how to build your own.

## Updates

- The chart updates by itself when its properties change, and when the series or their values change if they
  notify it, see [automatic updates]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Automatic%20updates).
- `UpdaterThrottler` is the time the chart waits after a change before it updates, 50 milliseconds by default.
  Changes that arrive while it waits are drawn in the same update. See the
  [animations article]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Animations#updaterthrottler-property).
- `AutoUpdateEnabled` is `true` by default. When it is `false`, the chart ignores the changes above until you update
  it yourself with `CoreChart.Update(new ChartUpdateParams { IsAutomaticUpdate = false })`.
