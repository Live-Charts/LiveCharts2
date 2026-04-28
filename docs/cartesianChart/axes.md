<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/dev/docs/readme.md
-->

# Axes

The following diagram illustrates an axis and its main components:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/dev/docs/_assets/1.7.diagram.png)

The cartesian chart control has the `XAxes` and `YAxes` properties, both of type `IEnumerable<IAxis>` by default
when you do not set these properties, they will be an array containing only one element of the `Axis` class 
(`new Axis[] { new Axis() }`). 

## Zooming and Panning

Both of these features are directly related to the <a href="#MaxLimit and MinLimit properties">MaxLimit and MinLimit properties</a>,
zooming occurs when the mouse wheel moves or when the pinch gesture occurs, the panning is called when the pointer goes down,
moves and then goes up or when the pan gesture occurs.

Zooming and panning is disabled by default, you can enable it by setting the `ZoomMode` property, this property is of type
[ZoomAndPanMode](https://lvcharts.com/api/{{ version }}/LiveChartsCore.Measure.ZoomAndPanMode) and the options are:

- `None`: Disables zooming and panning.
- `PanX`: Enables panning on the X axis.
- `ZoomX`: Enables zooming on the X axis (wheel, pinch and zoom by section).
- `PanY`: Enables panning on the Y axis.
- `ZoomY`: Enables zooming on the Y axis (wheel, pinch and zoom by section).
- `X`: Enables zooming and panning on the X axis. Equivalent to `PanX | ZoomX`.
- `Y`: Enables zooming and panning on the Y axis. Equivalent to `PanY | ZoomY`.
- `Both`: Enables zooming and panning on both axes.
- `NoFit`: Disables the "Fit to Bounds" feature that forces the chart to bounce back to the data bounds when zooming and panning finishes.
- `NoZoomBySection`: Disables the "Zoom by Section" feature, this feature selects an area and zooms to this area, normally by right clicking or double tapping and then dragging to the end of the section.
- `InvertPanningPointerTrigger`: Inverts the panning and zoom by section pointer triggers, when the flag is present, panning is triggered by right clicking the chart, and zoom by section by left clicking (or inverts single/double taps on mobile).

Because each gesture has its own flag you can mix and match — for example `ZoomMode = ZoomAndPanMode.ZoomX | ZoomAndPanMode.PanY` enables zoom on the X axis and panning on the Y axis only, and `ZoomMode = ZoomAndPanMode.ZoomX` enables zoom on the X axis with no panning at all (useful on mobile where pan gestures can hide tooltips).

The [ZoomAndPanMode](https://lvcharts.com/api/{{ version }}/LiveChartsCore.Measure.ZoomAndPanMode) type is a flag enum,
so you can combine the options, for example, if you want to enable zooming on the `X` axis and disable "Fit to Bounds"
and "Zoom by Section" you can set the `ZoomMode` property to:

```csharp
var flags = ZoomAndPanMode.X | ZoomAndPanMode.NoFit | ZoomAndPanMode.NoZoomBySection;
myChart.ZoomMode = flags;
```

Here are a few examples of the different zoom modes:

**X Mode:**

When the user interacts with the chart, they are only moving the chart in the X axis direction. The Y axis range is calculated automatically
by the library to fit all the visible points in the X axis.

{{~ if xaml ~}}
```xml
<lvc:CartesianChart
    Series="{Binding Series}"
    TooltipPosition="Hidden"
    ZoomMode="X"> <!-- mark -->
</lvc:CartesianChart>
````
{{~ end ~}}

{{~ if blazor ~}}
```xml
<CartesianChart
    Series="series"
    ZoomMode="LiveChartsCore.Measure.ZoomAndPanMode.X"> <!-- mark -->
</CartesianChart>
```
{{~ end ~}}

{{~ if winforms ~}}
```csharp
cartesianChart1.ZoomMode = LiveChartsCore.Measure.ZoomAndPanMode.X;
```
{{~ end ~}}

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/dev/docs/_assets/zoom-x.gif)

**Y Mode:**

When the user interacts with the chart, they are only moving the chart in the Y axis direction. The X axis range is calculated automatically
by the library to fit all the visible points in the Y axis.

{{~ if xaml ~}}
```xml
<lvc:CartesianChart
    Series="{Binding Series}"
    TooltipPosition="Hidden"
    ZoomMode="Y"> <!-- mark -->
</lvc:CartesianChart>
```
{{~ end ~}}

{{~ if blazor ~}}
```xml
<CartesianChart
    Series="series"
    ZoomMode="LiveChartsCore.Measure.ZoomAndPanMode.Y"> <!-- mark -->
</CartesianChart>
```
{{~ end ~}}

{{~ if winforms ~}}
```csharp
cartesianChart1.ZoomMode = LiveChartsCore.Measure.ZoomAndPanMode.Y;
```
{{~ end ~}}

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/dev/docs/_assets/zoom-y.gif)

**Both Mode:**

Both axis are moved by the user, this method could lead to the user to get lost in the canvas, you probably need to implement
a button to reset the zoom to the data in the chart (settings `MinLimit` and `MaxLimit` properties to `null`).

{{~ if xaml ~}}
```xml
<lvc:CartesianChart
    Series="{Binding Series}"
    TooltipPosition="Hidden"
    ZoomMode="Both"> <!-- mark -->
</lvc:CartesianChart>
```
{{~ end ~}}

{{~ if blazor ~}}
```xml
<CartesianChart
    Series="series"
    ZoomMode="LiveChartsCore.Measure.ZoomAndPanMode.Both"> <!-- mark -->
</CartesianChart>
```
{{~ end ~}}

{{~ if winforms ~}}
```csharp
cartesianChart1.ZoomMode = LiveChartsCore.Measure.ZoomAndPanMode.Both;
```
{{~ end ~}}

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/dev/docs/_assets/zoom-both.gif)

## MaxLimit and MinLimit properties

These properties control the **zooming** and **panning** of a chart, both properties are of type of `double?`, you can
indicate the visible range of an axis setting these properties. For example, imagine you are plotting a 
`LineSeries` that goes from 0 to 100 in the `X` axis, but you only need to show to the user the first 10 points (**paging**),
in this case you could set the `MinLimit` property to 0 and the `MaxLimit` to 10.

```csharp
var xAxis = new Axis
{
    MaxLimit = 10, // mark
    MinLimit = 0 // mark
};

myChart.XAxes = new List<Axis>{ xAxis };
```

When a user uses the zooming and panning features, both properties will be calculated by the library, this means 
that you can always know where the user is at a chart when you read both of these properties, notice the `Axis` class
implements `INotifyPropertyChanged`, if you need so you could subscribe to the `PropertyChanged` event to detect every
time a user uses these features (zooming and panning).

With the code above, we could detect the chart visible range when the user is zooming or panning subscribing a handler in
the `Axis.PropertyChanged` event:

```csharp
var xAxis = new Axis
{
    MaxLimit = 10,
    MinLimit = 0
};

xAxis.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) => // mark
{ // mark
    if (e.PropertyName is (nameof(xAxis.MaxLimit)) or (nameof(xAxis.MinLimit))) // mark
    { // mark
        // at this point the axis limits changed // mark
        // the user is using the zooming or panning features // mark
        // or the range was set explicitly in code // mark

        var minXVisible = xAxis.MinLimit; // mark
        var maxXVisible = xAxis.MaxLimit; // mark
    } // mark
}; // mark

myChart.XAxes = new List<Axis>{ xAxis };
```

Finally notice that when the chart loads, by default both of these properties are `null`, and `null` means that the library will
calculate the value of both. So when you need to read the initial value of an axis, you can access the `Axis.DataBounds` or
`Axis.VisibleDataBounds` properties. These properties are loaded until the chart is measured. There are multiple options to wait
for the chart to be measured. In this example we will use the `Chart.UpdateStarted` event that happens when the chart is already measured
and the draw in the UI is about to start.

```csharp
using System.Linq; // mark

// get the control in the UI
var chartControl = (CartesianChart)FindName("cartesianChart"); // notice this code is WPF specific

chartControl.UpdateStarted +=
    (LiveChartsCore.Kernel.Sketches.IChartView<LiveChartsCore.SkiaSharpView.Drawing.SkiaSharpDrawingContext> chart) =>
    {
        var x = chartControl.XAxes.First(); 

        var minLimit = x.VisibleDataBounds.Min; // mark
        var maxLimit = x.VisibleDataBounds.Max; // mark
    };
```

## Clearing the current zooming or panning

Setting `MinLimit` and `MaxLimit` properties to `null` or `double.NaN` will clear the current limits and fit the data to the viewport.

## MinStep property

The `Step` defines the interval or distance between every separator and label in the axis. LiveCharts will calculate it automatically 
based on the chart data and the chart size, but you can configure the minimum value this property could be. For example, in the case
where you don't want decimal separations in the axis labels, you could set the `MinStep` property to `1`. This way, when the calculated 
step is less than `1`, the library will force it to be `1`.

```csharp
new Axis
{
    // The MinStep property lets you define the minimum separation (in chart values scale)
    // between every axis separator, in this case we don't want decimals,
    // so lets force it to be greater or equals than 1
    MinStep = 1
}
```

## MinSeparators property

The auto-step calculation snaps the computed step up to a "nice" number (1, 2, 5 or 10 × magnitude). On short charts with awkward 
ranges that snap can leap a whole tier and leave only one or two separators on screen, which makes the axis hard to read. The 
`MinSeparators` property is a floor on the number of separators the auto-step is allowed to produce — when the snap-up would yield 
fewer separators than this value, the library subdivides further (10 → 5 → 2 → 1 → 0.5 → ...) until the floor is met. Default is `3`.

```csharp
new Axis
{
    // Make sure the axis always renders at least 5 separators when the step is calculated automatically.
    MinSeparators = 5
}
```

Set `MinSeparators = 0` to opt out and restore the original snap-up only behavior. The floor is also ignored when `ForceStepToMin = true`,
since in that case you have explicitly pinned the step.

## Position property

The axis position property is type `AxisPosition`, it is an `enum` containing only 2 options, `Start` and `End` 
the `Start` position places the axis at the bottom for X axes and at the left for Y axes and the `End` position 
places the axis at the top for X axes and at the right for Y axes.

```csharp
YAxes = new List<Axis>
{
    new Axis    
    {
        Position = LiveChartsCore.Measure.AxisPosition.End
    }
};
```

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/dev/docs/_assets/1.7.position.png)

## LabelsPaint and SeparatorsPaint properties

You can set the color, use dashed lines, build gradients for the axis name, labels and separators.

{{~ if xaml ~}}
```csharp
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;

namespace ViewModelsSamples.Bars.Basic
{
    public class ViewModel
    {
        public ISeries[] Series { get; set; }
            = new ISeries[] { new ColumnSeries<int> { Values = new[] { 2, 5, 4, -2, 4, -3, 5 } } };

        public Axis[] XAxes { get; set; }
            = new Axis[]
            {
                new Axis
                {
                    Name = "X Axis",
                    NamePaint = new SolidColorPaint(SKColors.Black), // mark

                    LabelsPaint = new SolidColorPaint(SKColors.Blue), // mark
                    TextSize = 10,

                    SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) { StrokeThickness = 2 }  // mark
                }
            };

        public Axis[] YAxes { get; set; }
            = new Axis[]
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
    }
}
```

```xml
<lvc:CartesianChart
    Series="{Binding Series}"
    XAxes="{Binding XAxes}"
    YAxes="{Binding YAxes}">
</lvc:CartesianChart>
```
{{~ end ~}}

{{~ if blazor ~}}
```csharp
@using LiveChartsCore;
@using LiveChartsCore.SkiaSharpView;
@using LiveChartsCore.SkiaSharpView.Painting;
@using LiveChartsCore.SkiaSharpView.Painting.Effects;
@using SkiaSharp;

<CartesianChart
    Series="series"
    XAxes="xAxes"
    YAxes="yAxes">
</CartesianChart>

@code {
    private ISeries[] series { get; set; }
        = new ISeries[] { new ColumnSeries<int> { Values = new[] { 2, 5, 4, -2, 4, -3, 5 } } };

    private Axis[] xAxes { get; set; }
        = new Axis[]
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

    private Axis[] yAxes { get; set; }
        = new Axis[]
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
}
```
{{~ end ~}}

{{~ if winforms ~}}
```csharp
cartesianChart1.XAxes = new Axis[]
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

cartesianChart1.YAxes = new Axis[]
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
{{~ end ~}}

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/dev/docs/_assets/axes-paints.png)

:::info
Paints can create gradients, dashed lines and more, if you need help using the `Paint` instances take 
a look at the [Paints article]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Paints).
:::

## Labels vs Labeler properties

There are 2 ways to format and axis labels, using the `Labels` property and using the `Labeler` property,
you must normally use the `Labels` property to indicate names, and the `Labeler` property to give format to the current label.

The labels property is a collection of string, if this property is not `null`, then the axis label will be pulled from the labels 
collection, the label is mapped to the chart based on the position of the label and the position of the point, both integers,
if the axis requires a label outside the bounds of the labels collection, then the index will be returned as the label,
default value is null.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/dev/docs/_assets/1.7.labels.png)

```csharp
Series = new ObservableCollection<ISeries>
{
    new ColumnSeries<int>
    {
        Values = new ObservableCollection<int> { 200, 558, 458, 249 },
    }
};

XAxes = new List<Axis>
{
    new Axis
    {
        // Use the labels property to define named labels.
        Labels = new string[] { "Anne", "Johnny", "Zac", "Rosa" } // mark
    }
};

YAxes = new List<Axis>
{
    new Axis
    {
        // Now the Y axis we will display labels as currency
        // LiveCharts provides some common formatters
        // in this case we are using the currency formatter.
        Labeler = Labelers.Currency // mark

        // you could also build your own currency formatter
        // for example:
        // Labeler = (value) => value.ToString("C")

        // But the one that LiveCharts provides creates shorter labels when
        // the amount is in millions or trillions
    }
};
```

## LabelsRotation property

Indicates the axis labels rotation in degrees, in the following image we have a rotation of 45 degrees in the Y axis.

```csharp
YAxes = new List<Axis>
{
    new Axis    
    {
        LabelsRotation = 45
    }
};
```

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/dev/docs/_assets/1.7.rotation.png)

## Multiple axes

Both of these properties are collections because the library supports to have more than one axis, the following sample illustrates
how to create a chart that uses multiple axes:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/dev/docs/_assets/1.7.multiple.png)

```csharp
var blue = new SKColor(25, 118, 210);
var red = new SKColor(229, 57, 53);
var yellow = new SKColor(198, 167, 0);

Series = new ObservableCollection<ISeries>
{
    new LineSeries<double>
    {
        LineSmoothness = 1,
        Name = "tens",
        Values = new ObservableCollection<double> { 14, 13, 14, 15, 17 },
        Stroke = new SolidColorPaint(blue, 2),
        GeometryStroke = new SolidColorPaint(blue, 2),
        Fill = null,
        // it will be scaled at the Axis[0] instance (see YAxes property below)
        ScalesYAt = 0 // mark
    },
    new LineSeries<double>
    {
        Name = "tens 2",
        Values = new ObservableCollection<double> { 11, 12, 13, 10, 13 },
        Stroke = new SolidColorPaint(blue, 2),
        GeometryStroke = new SolidColorPaint(blue, 2),
        Fill = null,
        // it will be scaled at the Axis[0] instance (see YAxes property below)
        ScalesYAt = 0 // mark
    },
    new LineSeries<double>
    {
        Name = "hundreds",
        Values = new ObservableCollection<double> { 533, 586, 425, 579, 518 },
        Stroke = new SolidColorPaint(red, 2),
        GeometryStroke = new SolidColorPaint(red, 2),
        Fill = null,
        // it will be scaled at the YAxes[1] instance (see YAxes property below)
        ScalesYAt = 1 // mark
    },
    new LineSeries<double>
    {
        Name = "thousands",
        Values = new ObservableCollection<double> { 5493, 7843, 4368, 9018, 3902 },
        Stroke = new SolidColorPaint(yellow, 2),
        GeometryStroke = new SolidColorPaint(yellow, 2),
        Fill = null,
         // it will be scaled at the YAxes[2] instance (see YAxes property below)
        ScalesYAt = 2 // mark
    }
};

YAxes = new List<Axis>
{
    new Axis // the "units" and "tens" series will be scaled on this axis
    {
        LabelsPaint = new SolidColorPaint(blue)
    },
    new Axis // the "hundreds" series will be scaled on this axis
    {
        LabelsPaint = new SolidColorPaint(red),
        ShowSeparatorLines = false,
        Position = LiveChartsCore.Measure.AxisPosition.End
    },
    new Axis // the "thousands" series will be scaled on this axis
    {
        LabelsPaint = new SolidColorPaint(yellow),
        ShowSeparatorLines = false,
        Position = LiveChartsCore.Measure.AxisPosition.End
    }
};
```

## Custom Separators

The library calculates the separators based on the data and the size of the chart, but we can manually set our own separators,
When the `CustomSeparators` property is not null, the library will not calculate the separators, instead it will use the 
collection we passed here.

```csharp
public Axis[] YAxes { get; set; } =
    {
        new Axis
        {
            // We can specify a custom separator collection
            // the library will use this separators instead of
            // calculating them based on the data of the chart
            CustomSeparators = new double[] { 0, 10, 25, 50, 100 },
            SeparatorsPaint = new SolidColorPaint(SKColors.Black.WithAlpha(100))
        }
    };
```

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/dev/docs/samples/axes/customSeparatorsInterval/result.png)

## Labels Density

Separators are calculated automatically when the `CustomSeparators` and `Labels` properties are not set, the library decides
the number of separators and distance between each one, based on the data, the size of the chart, and also based on the 
`LabelsDensity` property, this property is a factor that determines the distance between labels; Where 0 is the most dense, 
any value greater than 0 will make the labels to be more separated, values less than 0 will make the labels to overlap 
(labels rotation could prevent overlapping). Default value is 0.85.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/dev/docs/_assets/labelsdensity.gif)

## Inverted property

Normally both `X` and `Y` axes scale according to the Cartesian coordinate system you can invert the direction of the axes
setting the `Inverted` property to `true`.

## Visible property

When the `Visible` property is set to `false` the axis will not be drawn and also it will not take any space in the chart
default is `true`.

## UnitWidth property

You normally do not need to use this property, unless you ware working in a Logarithmic or DateTime scale. 
The unit width property affects a few series, `Column`, `Row` and `Financial` series, the unit width is the width of a bar in 
a chart, by default this property is 1, but if you are using a custom scale, you must let the chart know about it.

```csharp
// The UnitWidth is only required for column or financial series
// because the library needs to know the width of each column, by default the
// width is 1, but when you are using a different scale, you must let the library know it.

XAxes = new List<Axis>
{
    new Axis
    {
        // in this case we want our columns with a width of 1 day, we can get that number
        // using the following syntax
        UnitWidth = TimeSpan.FromDays(1).Ticks

        // if the difference between our points is in hours then we would:
        // UnitWidth = TimeSpan.FromHours(1).Ticks,

        // since all the months and years have a different number of days
        // we can use the average, it would not cause any visible error in the user interface
        // Months: TimeSpan.FromDays(30.4375).Ticks
        // Years: TimeSpan.FromDays(365.25).Ticks
    }
};
