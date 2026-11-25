<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
Samples are pulled from the repository with render_sample, the code shown always matches the samples.
-->

# Zooming and panning

Zooming and panning change the visible range of the axes: the user zooms with the mouse wheel or a pinch, and pans
by dragging the chart. Both features only move the `MinLimit` and `MaxLimit` of the axes, so the same range can also
be set from code, to show the data page by page or to drive the chart from a scroll bar. For axis limits in general,
see the [axes article]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Axes).

:::info
This article highlights the most used features. Every option is listed in the API explorer:
[ZoomAndPanMode]({{ website_url }}/api/{{ version }}/LiveChartsCore.Measure.ZoomAndPanMode),
[Axis]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.Axis) and
[CartesianChartEngine]({{ website_url }}/api/{{ version }}/LiveChartsCore.CartesianChartEngine).
{{~ if xaml ~}}
In XAML, declare the axes as `XamlAxis`.
{{~ end ~}}
:::

## Enable zooming

Zooming and panning are disabled by default. Set the `ZoomMode` property of the chart to enable them. With `X`, the
user zooms and pans only the X axis, and the Y axis fits the points that are visible in the current X range.

{{~ if xaml ~}}
```xml
<lvc:CartesianChart
    Series="{Binding Series}"
    ZoomMode="X"> <!-- mark -->
</lvc:CartesianChart>
```
{{~ end ~}}

{{~ if blazor ~}}
```xml
<CartesianChart
    Series="series"
    ZoomMode="LiveChartsCore.Measure.ZoomAndPanMode.X"> <!-- mark -->
</CartesianChart>
```
{{~ end ~}}

{{~ if winforms || eto ~}}
```csharp
cartesianChart1.ZoomMode = LiveChartsCore.Measure.ZoomAndPanMode.X; // mark
```
{{~ end ~}}

{{ render_sample "lines/zoom" }}

To change the default of every chart in the application, call
`LiveCharts.Configure(config => config.WithZoomMode(ZoomAndPanMode.X))` when the application starts, before any chart
is created.

## Zoom modes

`ZoomMode` is a [ZoomAndPanMode]({{ website_url }}/api/{{ version }}/LiveChartsCore.Measure.ZoomAndPanMode) flag
enum, so the options can be combined:

- `None`: zooming and panning are disabled, the default.
- `PanX` and `PanY`: the user can pan the X or the Y axis.
- `ZoomX` and `ZoomY`: the user can zoom the X or the Y axis, with the wheel, a pinch or a
  [section](#zoom-by-section).
- `X`: zoom and pan on the X axis, the same as `PanX | ZoomX`.
- `Y`: zoom and pan on the Y axis, the same as `PanY | ZoomY`.
- `Both`: zoom and pan on both axes, the same as `X | Y`.
- `NoFit`: lets the user move past the data, see [fit to data](#fit-to-data).
- `NoZoomBySection`: disables the [zoom by section](#zoom-by-section).
- `InvertPanningPointerTrigger`: swaps the triggers of panning and zoom by section, see
  [mouse and touch](#mouse-and-touch).

The last three flags only change the behaviour of the zoom and pan flags they are combined with.

Each gesture has its own flag, so they can be mixed: `ZoomAndPanMode.ZoomX | ZoomAndPanMode.PanY` zooms the X axis and
pans the Y axis, and `ZoomAndPanMode.ZoomX` zooms the X axis with no panning at all. Panning closes the tooltip while
the pointer drags, so a zoom-only mode keeps tooltips working when the user drags a finger over the chart on a touch
screen.

{{~ if xaml ~}}
```xml
<lvc:CartesianChart ZoomMode="Both,NoFit"> <!-- mark -->
</lvc:CartesianChart>
```
{{~ end ~}}

{{~ if !xaml ~}}
```csharp
myChart.ZoomMode = ZoomAndPanMode.Both | ZoomAndPanMode.NoFit; // mark
```
{{~ end ~}}

With `Y` the user moves only the Y axis, and the X axis fits the points visible in the current Y range. With `Both`
the user moves both axes; it is easy to get lost in the data, so consider a button that
[clears the zoom](#clearing-the-zoom).

![zoom on both axes](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/zoom-both.gif)

## Mouse and touch

| Action          | Mouse                           | Touch                                                    |
| --------------- | ------------------------------- | -------------------------------------------------------- |
| Zoom            | Mouse wheel                     | Pinch                                                    |
| Pan             | Press the left button and drag  | Press and drag                                           |
| Zoom by section | Press the right button and drag | Double tap, keep the finger down on the second tap, drag |

The wheel zooms around the pointer: the value under the pointer stays in place.

{{~ if wpf || winforms || blazor ~}}
This control does not handle the pinch gesture, use the mouse wheel to zoom.
{{~ end ~}}

{{~ if eto ~}}
This control does not handle the pinch gesture, use the mouse wheel to zoom. Any button other than the left one
starts a zoom by section.
{{~ end ~}}

{{~ if avalonia ~}}
On a desktop, a two-finger scroll on a trackpad zooms like the mouse wheel. A double tap (or a double click) followed
by a drag starts a zoom by section, as the right button does.
{{~ end ~}}

{{~ if maui ~}}
On Windows the right button starts a zoom by section. On macOS, iOS and Android, double tap the chart, keep the
finger down on the second tap and drag. On a desktop, a two-finger scroll on a trackpad zooms like the mouse wheel.
{{~ end ~}}

{{~ if winui || uno ~}}
On a desktop the right button starts a zoom by section. On a touch device, double tap the chart, keep the finger down
on the second tap and drag.
{{~ end ~}}

When `ZoomMode` has the `InvertPanningPointerTrigger` flag, the triggers swap: the right button (or a double tap) pans,
and the left button (or a single tap) starts a zoom by section.

## Zoom by section

The user can draw a rectangle over the chart and zoom to it. The feature is enabled with zooming, add the
`NoZoomBySection` flag to disable it. The section must start inside the plot area, and a drag shorter than 15 pixels
is ignored. When only one axis zooms, the rectangle covers the full height (or width) of the plot area, and only that
axis changes. The rectangle is drawn with a semi-transparent blue fill.

![zoom by section](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/samples/lines/zoom/selection-zoom.gif)

## Zooming speed

`ZoomingSpeed` sets how much each step of the mouse wheel zooms, from `0` (slowest) to `1` (fastest). It is not the
speed of the animations. Each step in makes the visible range `1 - ZoomingSpeed` times its length, each step out
divides the length by the same amount. The default is `0.2`, so each step in shows 80% of the previous range. Values
under `0.1` work as `0.1` and values over `0.95` work as `0.95`. The pinch gesture does not use this property.

{{~ if xaml ~}}
```xml
<lvc:CartesianChart
    Series="{Binding Series}"
    ZoomMode="X"
    ZoomingSpeed="0.1"> <!-- mark -->
</lvc:CartesianChart>
```
{{~ end ~}}

{{~ if blazor ~}}
```xml
<CartesianChart
    Series="series"
    ZoomMode="LiveChartsCore.Measure.ZoomAndPanMode.X"
    ZoomingSpeed="0.1"> <!-- mark -->
</CartesianChart>
```
{{~ end ~}}

{{~ if winforms || eto ~}}
```csharp
cartesianChart1.ZoomingSpeed = 0.1; // mark
```
{{~ end ~}}

`LiveCharts.Configure(config => config.WithZoomSpeed(0.1))` changes the default for every chart.

## Fit to data

By default the chart keeps the data in view:

- Zooming out stops at the bounds of the data.
- Panning can go past the first or last point by the axis `BouncingDistance`, a fraction of the plot area width (X
  axes) or height (Y axes). The default is `0.25`, a quarter of the plot area. When the pointer is released, the axis
  goes back to the data. Set `BouncingDistance` to `0` to stop panning at the data.

Add the `NoFit` flag to let the user zoom and pan freely, past the data:

![zoom on both axes with NoFit](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/samples/lines/zoom/zoom-both-nofit.gif)

## Minimum zoom

The axis `MinZoomDelta` property limits how deep the user can zoom: it is the smallest visible range the zoom can
reach, in data units (`MaxLimit - MinLimit`). Zooming in stops there, with the wheel, a pinch or a section. When it is
`null`, the default, the chart uses three times the smallest distance between two consecutive points of the data.

```csharp
XAxes = new[]
{
    new Axis
    {
        MinZoomDelta = 5 // the visible range is never shorter than 5 units // mark
    }
};
```

## Clearing the zoom

Set `MinLimit` and `MaxLimit` back to `null` (or `double.NaN`) to clear the zoom and fit the data to the chart again:

```csharp
// where myChart is a reference to the chart in the UI
foreach (var x in myChart.XAxes)
{
    x.MinLimit = null;
    x.MaxLimit = null;
}

foreach (var y in myChart.YAxes)
{
    y.MinLimit = null;
    y.MaxLimit = null;
}
```

{{~ if xaml ~}}
When the limits are bound to `double` properties of a view model, set the properties to `double.NaN`, as the
[paging](#paging) sample does.
{{~ end ~}}

## Reading the visible range

While the user zooms or pans, the chart sets the `MinLimit` and `MaxLimit` of the axes. The `Axis` class implements
`INotifyPropertyChanged`, so the `PropertyChanged` event tells when the visible range changes, by the user or by code:

```csharp
// where myChart is a reference to the chart in the UI
myChart.XAxes.First().PropertyChanged += OnAxisPropertyChanged;

// ...

private void OnAxisPropertyChanged(object? sender, PropertyChangedEventArgs e)
{
    if (e.PropertyName is not (nameof(Axis.MinLimit) or nameof(Axis.MaxLimit))) return;

    var axis = (ICartesianAxis)sender!;
    Trace.WriteLine($"{axis.Orientation} visible range: {axis.MinLimit} - {axis.MaxLimit}");
}
```

## Paging

To show the data a page at a time, set `MinLimit` and `MaxLimit` from code. In this sample, the buttons move the X
axis by pages of eleven columns, and "Clear" shows all the data again. Each column is centered on its index, so
`-0.5` to `10.5` shows the columns `0` to `10` completely. `ZoomMode` is `X`, so the user can still zoom and pan from
any page.

{{ render_sample "axes/paging" }}

## Scrollable chart

A second, smaller chart can work as a scroll bar. In this sample the main chart has `ZoomMode` set to `X`. Below it,
a chart 100 pixels tall plots the same data with hidden axes and no tooltips, and a `RectangularSection` marks the
range visible in the main chart. Both charts set the same left and right `DrawMargin`, so their plot areas line up.

The sync goes both ways:

- When the main chart updates (after a zoom or a pan), its `UpdateStarted` event copies the X axis limits to the
  `Xi` and `Xj` of the section.
- While the pointer is pressed on the small chart, each move converts the pointer position to data coordinates with
  `ScalePixelsToData`, centers the section there keeping its width, and sets the same range as the `MinLimit` and
  `MaxLimit` of the main chart's X axis.

{{ render_sample "general/scrollable" }}

## Zoom and pan from code

The chart engine has the methods that the gestures call. They take the flags to apply, so they work even when
`ZoomMode` is `None`, for example to zoom with buttons or with your own gestures. Positions and distances are in pixels.

```csharp
// where myChart is a reference to the chart in the UI
var engine = (CartesianChartEngine)myChart.CoreChart;

// zooms in on the X axis, keeping the value at the pixel (100, 100) in place
engine.Zoom(ZoomAndPanMode.X, new(100, 100), ZoomDirection.ZoomIn);

// pans both axes by 10 pixels
engine.Pan(ZoomAndPanMode.Both, new(10, 10));

// zoom by section: starts the rectangle at (100, 100),
engine.StartZoomingSection(ZoomAndPanMode.Both, new(100, 100));

// grows the drawn rectangle to (150, 150), the zoom is not applied yet,
engine.GrowZoomingSection(ZoomAndPanMode.Both, new(150, 150));

// and zooms to the rectangle that ends at (200, 200)
engine.EndZoomingSection(ZoomAndPanMode.Both, new(200, 200));
```

`Zoom` honours the `NoFit` flag it receives: without it, the zoom is fitted to the data, as the wheel does.
