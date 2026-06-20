<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
content is normally pulled from the examples in the repository.
-->

{{ render "~/shared/genericSampleJustGifHeader.md" }}

Zooming and panning is disabled by default, you can enable it by setting the `ZoomMode` property, this property is of type
[ZoomAndPanMode](https://lvcharts.com/api/{{ version }}/LiveChartsCore.Measure.ZoomAndPanMode), this type is a flag enum
it means that you can combine the options as you need, you can learn more about zooming and panning
options [here](#options).



{{~ if wpf || winforms || winui || blazor || eto ~}}
:::tip
Use the mouse wheel to zoom in/out, hold click and drag to pan.
:::
{{~ end ~}}



{{~ if maui || uno || avalonia ~}}
:::tip
On **Desktop**:

- **Zoom**: Use the scroll gesture — swipe up/down with two fingers on a trackpad or Magic Mouse, or scroll with the mouse wheel — to zoom in and out.
- **Pan**: Click and drag with the mouse, or press and hold while dragging with one finger on a trackpad or Magic Mouse.
:::

:::tip
On **Mobile** and touch screens:
- **Zoom**: Pinch the screen in or out using two fingers.
- **Pan**: Tap and hold, then drag to move the view.
:::
{{~ end ~}}


## {{~ view_title ~}}

```
{{ render_current_directory_view }}
```

# Options

You can enable zooming and panning by setting the `ZoomMode` property, this property is of type
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
so you can combine the options, for example, if you want to enable zooming on the `Both` axes and disable "Fit to Bounds"
you could set the `ZoomMode` property to:



{{~ if blazor || winforms || eto ~}}
```csharp
var flags = ZoomAndPanMode.Both | ZoomAndPanMode.NoFit;
myChart.ZoomMode = flags;
```
{{~ end ~}}



{{~ if avalonia || uno || maui || winui || wpf ~}}
```xml
<lvc:CartesianChart ZoomMode="Both,NoFit">
</lvc:CartesianChart>
```
{{~ end ~}}



<div class="text-center sample-img">
    <img src="https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/{{ unique_name }}/zoom-both-nofit.gif" alt="sample image" />
</div>

# Bouncing distance

While "Fit to bounds" is enabled, (enabled by default, to disable see `NoFit` in the [options section](#options)), 
when the user zooms or pans, the chart will allow to scroll the data 25% beyond the limits, this is controlled by the 
Axis `BouncingDistance` property, for example setting  `Axis.BouncingDistance` to `0` will disable scrolling beyond data limits.

# Min zoom level

The axis `MinZoomDelta` property is useful to control how deep the user can zoom, this property defines the minimum difference between 
the max and min visible limits of the axis, default is `null` and `null` means that the library will calculate this value based on the current data.

# Clearing zooming and panning

Setting `MinLimit` and `MaxLimit` properties to `null` or `double.NaN` will clear the current limits and fit the the data to the viewport.

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

# Zoom by Section

You can also zoom by section, this is a feature that allows you to select an area in the chart and zoom in to that area,
when zooming is enabled, this feature is also enabled by default, you can also customize the trigger of this function, you can
find more info [here](https://lvcharts.com/docs/{{ platform }}/{{ version }}/CartesianChart.Axes%20properties#zooming-and-panning).

<div class="text-center sample-img">
    <img src="https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/{{ unique_name }}/selection-zoom.gif" alt="sample image" />
</div>



{{~ if wpf || winforms || winui || blazor || eto ~}}
:::tip
Right click on the chart, hold and drag to select an area in the chart.
:::
{{~ end ~}}


{{~ if maui ~}}
:::tip
On **Windows**, right click on the chart, hold and drag to select an area on the chart.
:::

:::tip
On **MacOS**, **Android** and **iOS** double tap the chart, hold the last tap and drag to select an area.
:::
{{~ end ~}}



{{~ if avalonia ~}}
:::tip
On **Desktop**, right click on the chart, hold and drag to select an area on the chart.
:::

:::tip
On **Desktop** and **Mobile** double tap the chart, hold the last tap and drag to select an area.
:::
{{~ end ~}}



{{~ if uno ~}}
:::tip
On **Desktop**, right click on the chart, hold and drag to select an area on the chart, the right click detection
relies on the Uno implementation of `PointerPointProperties.IsRightButtonPressed`.
:::

:::tip
On **Mobile** double tap the chart, hold the last tap and drag to select an area.
:::
{{~ end ~}}

# Listen for zooming or panning user changes

You can subscribe to the axis `PropertyChanged` event and read the `MinLimit` and `MaxLimit` properties, both are updated as the
user zooms or pans.

```csharp
// where myChart is a reference to the chart in the UI
myChart.XAxes.First().PropertyChanged += OnPropertyChanged;

// ....

private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
{
    var axis = (ICartesianAxis)sender;

    if (e.PropertyName == nameof(Axis.MinLimit))
    {
        Trace.WriteLine($"{axis.Orientation} Min Limit {axis.MinLimit}");
    }

    if (e.PropertyName == nameof(Axis.MaxLimit))
    {
        Trace.WriteLine($"{axis.Orientation} Max Limit {axis.MaxLimit}");
    }
}
```

# Axis bounds control

You can listen to the axis range changes and override the limits to your needs:

```csharp
// where myChart is a reference to the chart in the UI
myChart.XAxes.First().PropertyChanged += OnPropertyChanged;

// ....

private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
{
    var axis = (ICartesianAxis)sender;

    var max = axis.MaxLimit;
    var min = axis.MinLimit;

    if (max is null || min is null)
        return;

    // forces the axis range to 0-100 always

    if (min < 0)
        min = 0;

    if (max > 100)
        max = 100;

    axis.SetLimits(min.Value, max.Value);
}
```

# Manually fire zooming or panning

You can manually trigger the functions in the chart that trigger zooming or panning, for example you could set the
`ZoomMode` property to `None`, this will prevent LiveCharts to fire zooming or panning, and then we could fire these features
when we want, for example zooming on double tap or panning on mouse move.

```csharp
// where myChart is a reference to the chart in the UI
var engine = (CartesianChartEngine)myChart.CoreChart;

// zooms in in X using the 100,100 as the center of the zoom
engine.Zoom(ZoomAndPanMode.X, new(100, 100), ZoomDirection.ZoomIn);

// pans the chart 10 pixels to the right and 10 pixels down on both axes
engine.Pan(ZoomAndPanMode.Both, new(10, 10));

// starts the zooming section feature at the point (100, 100)
// for example start the section on double tap.
engine.StartZoomingSection(ZoomAndPanMode.Both, new(100, 100));

// grows the zooming section by 10 pixels on both axes, it just grows the
// drawn rectangle in the UI, it does not apply zoom yet.
// for example when the pointer moves after the previous double tap.
engine.GrowZoomingSection(ZoomAndPanMode.Both, new(10, 10));

// ends the zooming section feature at the point (110, 110)
// for example on double tap again.
engine.EndZoomingSection(ZoomAndPanMode.Both, new(110, 110));
```

{{ render "~/shared/relatedTo.md" }}
