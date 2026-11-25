<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
Samples are pulled from the repository with render_sample, the code shown always matches the samples.
-->

# Events

The charts raise events when the user presses or hovers the data, and while they update. The chart events receive
the points as `ChartPoint` instances, which give access to the data source, the coordinate and the drawn shape of
each point. The series raise their own events, typed with the model and the geometry of the series. Every event that
a view model may need also has a command, so it can be bound in MVVM.

:::info
Every event is listed in the API explorer:
[IChartView]({{ website_url }}/api/{{ version }}/LiveChartsCore.Kernel.Sketches.IChartView),
[CartesianChart]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.{{ platform }}.CartesianChart),
[Series]({{ website_url }}/api/{{ version }}/LiveChartsCore.Series-3),
[PointerCommandArgs]({{ website_url }}/api/{{ version }}/LiveChartsCore.Kernel.Events.PointerCommandArgs) and
[HoverCommandArgs]({{ website_url }}/api/{{ version }}/LiveChartsCore.Kernel.Events.HoverCommandArgs).
{{~ if xaml ~}}
The samples generate the commands with the `[RelayCommand]` attribute of the
[CommunityToolkit.Mvvm](https://www.nuget.org/packages/CommunityToolkit.Mvvm/) package: a method named `OnPressed`
generates a `PressedCommand` property.
{{~ end ~}}
:::

## Chart events

Every chart control (`CartesianChart`, `PieChart`, `PolarChart` and the others) has these events:

- `DataPointerDown`: the pointer went down on the chart. It receives the points under the pointer, found with the
  `FindingStrategy` of the chart. Command: `DataPointerDownCommand`, its parameter is the
  `IEnumerable<ChartPoint>` of the points.
- `HoveredPointsChanged`: the hovered points changed, these are the points the tooltip shows. It receives the points
  that are now hovered and the points that are no longer hovered; either list can be `null`. Command:
  `HoveredPointsChangedCommand`, its parameter is a `HoverCommandArgs` with `NewPoints` and `OldPoints`.
- `VisualElementsPointerDown`: the pointer went down on one or more visual elements of the chart. Command:
  `VisualElementsPointerDownCommand`, its parameter is a `VisualElementsEventArgs`.
- `Measuring`: the chart is about to measure its elements, the first step of an update.
- `UpdateStarted`: the chart measured its elements and is about to draw them. Command: `UpdateStartedCommand`, its
  parameter is a `ChartCommandArgs`.
- `UpdateFinished`: the chart finished drawing, when no animation is still running.

`ChartPointPointerDown` is obsolete, use `DataPointerDown` with a `FindingStrategy` that takes the closest point,
such as `CompareAllTakeClosest`.

The hovered points do not depend on the tooltip being visible: `HoveredPointsChanged` also fires when
`TooltipPosition` is `Hidden`. A series with `IsHoverable` set to `false` is left out of the hovered points.

{{~ if xaml ~}}
```xml
<lvc:CartesianChart
    Series="{Binding Series}"
    DataPointerDownCommand="{Binding DataPointerDownCommand}"
    HoveredPointsChangedCommand="{Binding HoveredPointsChangedCommand}"
    UpdateStartedCommand="{Binding UpdateStartedCommand}">
</lvc:CartesianChart>
```
{{~ end ~}}

{{~ if winforms || eto || blazor ~}}
```csharp
chart.DataPointerDown += (sender, points) =>
{
    foreach (var point in points)
        Trace.WriteLine($"pressed {point.Context.DataSource}");
};

chart.HoveredPointsChanged += (sender, newPoints, oldPoints) =>
{
    // newPoints are now hovered, oldPoints are no longer hovered
};
```
{{~ end ~}}

The [geo maps article]({{ website_url }}/docs/{{ platform }}/{{ version }}/GeoMap.Geo%20maps#pointer-events)
explains how these events work on a map, where they report the lands. The map has the events but not the commands.

## Pointer events

{{~ if xaml && !maui ~}}
The pointer commands run when the pointer goes down, moves or goes up on the chart, whether or not there is a point
there: `PointerPressedCommand`, `PointerMoveCommand` and `PointerReleasedCommand`.

```xml
<lvc:CartesianChart
    Series="{Binding Series}"
    PointerPressedCommand="{Binding PressedCommand}"
    PointerMoveCommand="{Binding MoveCommand}"
    PointerReleasedCommand="{Binding ReleasedCommand}">
</lvc:CartesianChart>
```
{{~ end ~}}

{{~ if maui ~}}
The pointer commands run when the pointer goes down, moves or goes up on the chart, whether or not there is a point
there: `PressedCommand`, `MovedCommand` and `ReleasedCommand`. MAUI drops the `Pointer` prefix that the other XAML
platforms use.

```xml
<lvc:CartesianChart
    Series="{Binding Series}"
    PressedCommand="{Binding PressedCommand}"
    MovedCommand="{Binding MovedCommand}"
    ReleasedCommand="{Binding ReleasedCommand}">
</lvc:CartesianChart>
```
{{~ end ~}}

{{~ if xaml ~}}
Their parameter is a `PointerCommandArgs`: `Chart` is the chart, `PointerPosition` the position of the pointer in
pixels, relative to the chart, and `OriginalEventArgs` the event arguments of the UI framework.

```csharp
[RelayCommand]
public void OnPressed(PointerCommandArgs args)
{
    var chart = args.Chart;
    var position = args.PointerPosition; // pixels, relative to the chart
    // ...
}
```
{{~ end ~}}

{{~ if winforms || eto ~}}
Use the `MouseDown`, `MouseMove` and `MouseUp` events of the control. `e.Location` is the position of the pointer
relative to the chart, in pixels:

```csharp
chart.MouseDown += (sender, e) =>
{
    var position = new LvcPointD(e.Location.X, e.Location.Y);
    var points = chart.GetPointsAt(position);
    // ...
};
```
{{~ end ~}}

{{~ if blazor ~}}
Use the `OnPointerDownCallback`, `OnPointerMoveCallback` and `OnPointerUpCallback` parameters of the chart. They
receive the `PointerEventArgs` of Blazor, `OffsetX` and `OffsetY` are the position of the pointer relative to the
chart:

```xml
<CartesianChart
    @ref="chart"
    Series="series"
    OnPointerDownCallback="OnPointerDown">
</CartesianChart>
```

```csharp
private CartesianChart chart;

private void OnPointerDown(PointerEventArgs e)
{
    var position = new LvcPointD(e.OffsetX, e.OffsetY);
    var points = chart.GetPointsAt(position);
    // ...
}
```
{{~ end ~}}

{{~ if wpf || avalonia ~}}
:::info
The chart ignores a press while a modifier key, such as Ctrl or Shift, is down: neither `PointerPressedCommand` nor
`DataPointerDown` runs.
:::
{{~ end ~}}

{{~ if winforms ~}}
:::info
The chart ignores a press while a modifier key, such as Ctrl or Shift, is down: `DataPointerDown` does not fire. The
`MouseDown` event of the control still fires.
:::
{{~ end ~}}

## Hovered and pressed points

This sample sets the `FindingStrategy` of the chart to `ExactMatch`, so a column is hovered only while the pointer is
inside the drawn column. When the hovered points change, it sets a black `Stroke` on the columns in the new points
and clears it on the old ones. On a press, it asks the chart for the points at the pointer position with
`GetPointsAt` and toggles a yellow `Fill` on them; setting `Fill` back to `null` restores the paint of the series.

{{ render_sample "events/tutorial" }}

## Find the points under the pointer

`GetPointsAt` returns the points at a position in pixels, and `GetVisualsAt` returns the visual elements there.

```csharp
var points = chart.GetPointsAt(position);
var visuals = chart.GetVisualsAt(position);
```

`GetPointsAt` takes the finding strategy as an argument, it does not read the `FindingStrategy` of the chart. The
default, `Automatic`, uses the strategy the series prefer: for the columns of the previous sample,
`CompareOnlyXTakeClosest`, which returns a column of each series in the category. To find the same points as the
chart, pass its strategy:

```csharp
var chart = (ICartesianChartView)args.Chart;
var points = chart.GetPointsAt(args.PointerPosition, chart.FindingStrategy); // mark
```

`DataPointerDown` already uses the `FindingStrategy` of the chart. The strategies are described in the
[tooltips article]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Tooltips%20and%20crosshairs#finding-strategy).
When none of them fits, a series can override how it finds its points, see
[custom point finding]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Tooltips%20and%20crosshairs#custom-point-finding).
The last argument of `GetPointsAt`, `FindPointFor`, tells that override whether the request comes from a hover
(the default) or a press.

## Scale pixels to data

`ScalePixelsToData` converts a position in pixels to the values of the axes, and `ScaleDataToPixels` does the
opposite. This sample adds a point where the user clicks: it converts the pointer position to data and adds an
`ObservablePoint` with those coordinates to the values of the series.

{{ render_sample "events/addPointOnClick" }}

Both methods use the first X and Y axes. With more than one axis, pass the indexes of the axes to use:

```csharp
var chart = (ICartesianChartView)args.Chart;

var data = chart.ScalePixelsToData(args.PointerPosition, xAxisIndex: 1, yAxisIndex: 0);
var pixels = chart.ScaleDataToPixels(new LvcPointD(5, 5));
```

The `PolarChart` has the same two methods, they take the indexes of the angle and radius axes.

## Series events

The series events are typed: the points they receive are `ChartPoint<TModel, TVisual, TLabel>`, where `Model` is
the data item and `Visual` the geometry that draws it, so no casts are needed.

- `DataPointerDown`: the pointer went down on points of this series, it receives all of them.
- `ChartPointPointerDown`: the pointer went down on points of this series, it receives the closest one.
- `ChartPointPointerHover`: the pointer entered a point.
- `ChartPointPointerHoverLost`: the pointer left a point.
- `PointCreated`: the series created the visual of a point, the first time the point is drawn.
- `PointMeasured`: the series calculated the position and size of a point, on every update.

The type of the point depends on the series, the model, the geometry and the label, so let the IDE complete the
signature of the handler. In this sample a column turns yellow when the pointer enters it, red when the pointer goes
down on it, and gets the paint of the series back when the pointer leaves it:

![series events](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/samples/events/cartesian/result.gif)

```csharp
public class Fruit
{
    public string Name { get; set; } = string.Empty;
    public double SalesPerDay { get; set; }
}

var series = new ColumnSeries<Fruit>
{
    Values = new[]
    {
        new Fruit { Name = "Apple", SalesPerDay = 4 },
        new Fruit { Name = "Orange", SalesPerDay = 6 },
        new Fruit { Name = "Pineapple", SalesPerDay = 2 },
        new Fruit { Name = "Potato", SalesPerDay = 8 },
        new Fruit { Name = "Lettuce", SalesPerDay = 3 },
        new Fruit { Name = "Cherry", SalesPerDay = 4 }
    },
    Mapping = (fruit, index) => new(index, fruit.SalesPerDay),
    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.End,
    DataLabelsFormatter = point => $"{point.Model!.SalesPerDay} {point.Model.Name}",
    YToolTipLabelFormatter = point => $"{point.Model!.Name}, sold {point.Model.SalesPerDay} items"
};

series.ChartPointPointerHover += (chart, point) => // mark
    point.Visual!.Fill = new SolidColorPaint(SKColors.Yellow);

series.ChartPointPointerDown += (chart, point) => // mark
    point.Visual!.Fill = new SolidColorPaint(SKColors.Red);

series.ChartPointPointerHoverLost += (chart, point) => // mark
    point.Visual!.Fill = null;
```

The extension methods `OnPointMeasured`, `OnPointCreated`, `OnPointDown`, `OnPointHover` and `OnPointHoverLost`
subscribe to the same events and return the series, so they can be chained:

```csharp
var series = new ColumnSeries<Fruit> { /* ... */ }
    .OnPointHover((chart, point) => point.Visual!.Fill = new SolidColorPaint(SKColors.Yellow))
    .OnPointHoverLost((chart, point) => point.Visual!.Fill = null);
```

{{~ if xaml ~}}
In XAML, every event of a series has a command with the same name followed by `Command`, such as
`PointMeasuredCommand` or `ChartPointPointerDownCommand`. When the event has one argument, the command receives it;
when it has more, the command receives a `CommandParameters` object with `Parameter1`, `Parameter2` and so on.

```xml
<lvc:XamlLineSeries
    Values="{Binding Values}"
    PointMeasuredCommand="{Binding PointMeasuredCommand}"/>
```
{{~ end ~}}

The chart events receive the base `ChartPoint` type instead, because the `Series` property of the chart is a
collection of `ISeries` and the chart does not know their types.

The [line series article]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Line%20and%20step%20line%20series#customize-each-point)
uses `PointMeasured` to rotate each point to a value of its data.

## Update events

`Measuring`, `UpdateStarted` and `UpdateFinished` follow each update of the chart. `UpdateStarted` runs once the
axes and the series are measured, so the scale of the axes is known: the
[zooming article]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Zooming%20and%20panning) uses it
to copy the limits of an axis to another chart, and the
[custom drawing article]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Custom%20drawing) uses it to
place a geometry with `ScaleDataToPixels`.

```csharp
chart.UpdateStarted += sender =>
{
    var cartesianChart = (ICartesianChartView)sender;
    var pixels = cartesianChart.ScaleDataToPixels(new LvcPointD(5, 5));
    // ...
};
```
