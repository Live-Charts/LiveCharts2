<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
Samples are pulled from the repository with render_sample, the code shown always matches the samples.
-->

# Tooltips and crosshairs

A tooltip is a popup that shows the points under the pointer. The chart decides which points to show with its
`FindingStrategy`, places the popup with `TooltipPosition`, and builds its text with the tooltip formatters of each
series. A crosshair is a line that follows the pointer across the plot area, with a label on the axis; it is set on
the axes. Legends have their own
[article]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Legends).

![tooltips](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/tooltips.gif)

:::info
This article highlights the most used features. Every member is listed in the API explorer:
[TooltipPosition]({{ website_url }}/api/{{ version }}/LiveChartsCore.Measure.TooltipPosition),
[FindingStrategy]({{ website_url }}/api/{{ version }}/LiveChartsCore.Measure.FindingStrategy),
[SKDefaultTooltip]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.SKCharts.SKDefaultTooltip),
[IChartTooltip]({{ website_url }}/api/{{ version }}/LiveChartsCore.Kernel.Sketches.IChartTooltip) and
[Axis]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.Axis).
{{~ if xaml ~}}
In XAML, declare the axes as `XamlAxis`.
{{~ end ~}}
:::

## Behaviour

With a mouse, the tooltip opens when the pointer moves over a point and closes when the pointer leaves the chart.
On Android and iOS, slide your finger over the chart to open it; it closes when the finger goes up. The tooltip also
closes when the user starts panning the chart.

Every series takes part in the tooltip. Set `IsHoverable` to `false` on a series to leave it out, it is `true` by
default.

## Tooltip position

`TooltipPosition` places the tooltip. The default is `Auto`: the tooltip goes above the hovered points, or below
them when all of them are under the pivot of their series, such as negative columns. `Top`, `Bottom`, `Left`,
`Right` and `Center` place it at that side of the hovered points, or at their center. When the tooltip does not fit
in the chart at the requested side, it moves to another one. `Hidden` disables tooltips.

{{~ if xaml ~}}
```xml
<lvc:CartesianChart
    Series="{Binding Series}"
    TooltipPosition="Bottom"><!-- mark -->
</lvc:CartesianChart>
```
{{~ end ~}}

{{~ if blazor ~}}
```xml
<CartesianChart
    Series="series"
    TooltipPosition="LiveChartsCore.Measure.TooltipPosition.Bottom"><!-- mark -->
</CartesianChart>
```
{{~ end ~}}

{{~ if winforms || eto ~}}
```csharp
cartesianChart1.TooltipPosition = LiveChartsCore.Measure.TooltipPosition.Bottom; // mark
// or use Auto, Top, Left, Right, Center or Hidden
```
{{~ end ~}}

A pie chart always places the tooltip on the hovered slice, it only takes `Hidden` from this property.

## Finding strategy

Every point drawn by the library has a
[HoverArea]({{ website_url }}/api/{{ version }}/LiveChartsCore.Kernel.Drawing.HoverArea), the zone of the chart that
selects that point. `FindingStrategy` sets how the chart compares the pointer position with those areas, to pick
the points shown in the tooltip and the points passed to the pointer events, such as `DataPointerDown`.

**CompareAll**: selects the points whose hover area contains the pointer.

![tooltips](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/tsm_cac.gif)

**CompareOnlyX**: selects the points whose hover area contains the pointer on the X axis, the Y position is ignored.

![tooltips](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/tsm_cxc.gif)

**CompareOnlyY**: selects the points whose hover area contains the pointer on the Y axis, the X position is ignored.

![tooltips](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/tsm_cyc.gif)

**ExactMatch**: selects the points whose drawn shape contains the pointer.

![tooltips](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/tsm_cec.gif)

**CompareAllTakeClosest**, **CompareOnlyXTakeClosest**, **CompareOnlyYTakeClosest** and **ExactMatchTakeClosest**
compare the same way, but when several points of a series match, they keep only the closest to the pointer, one
point per series.

**Automatic** is the default. Each series prefers a strategy: line, step line, column, financial and box series
prefer `CompareOnlyXTakeClosest`, row series prefer `CompareOnlyYTakeClosest`, and the others, such as scatter and
heat series, prefer none. When all the visible series prefer the same strategy the chart uses it, otherwise it uses
`CompareAllTakeClosest`.

{{~ if xaml ~}}
```xml
<lvc:CartesianChart
    Series="{Binding Series}"
    FindingStrategy="CompareOnlyX"><!-- mark -->
</lvc:CartesianChart>
```
{{~ end ~}}

{{~ if blazor ~}}
```xml
<CartesianChart
    Series="series"
    FindingStrategy="LiveChartsCore.Measure.FindingStrategy.CompareOnlyX"><!-- mark -->
</CartesianChart>
```
{{~ end ~}}

{{~ if winforms || eto ~}}
```csharp
cartesianChart1.FindingStrategy = LiveChartsCore.Measure.FindingStrategy.CompareOnlyX; // mark
```
{{~ end ~}}

:::info
The width of the hover area of a column or a point follows the `UnitWidth` of the axis. On a date time axis, set
the unit that matches your data, see
[the unit]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Time%20and%20logarithmic%20scales#the-unit).
:::

## Custom point finding

When no strategy fits, override how a series finds its points. By default, a column series selects all the columns
that share the X coordinate of the pointer:

![custom tooltip](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/custom-ha-compare.gif)

To show only the column under the pointer instead:

![custom tooltip](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/custom-ha.gif)

inherit from the series and override `FindPointsInPosition()`. The library calls it to resolve the points of the
tooltip (`FindPointFor.HoverEvent`) and of the pointer events (`FindPointFor.PointerDownEvent`):

```csharp
{{~ render "~/../samples/ViewModelsSamples/Events/OverrideFind/ViewModel.cs" ~}}
```

:::tip
Another option is to replace the `HoverArea` of each point, the area that makes a point selectable and suggests
where the tooltip goes. See
[this example](https://github.com/beto-rodriguez/LiveCharts2/blob/master/samples/ViewModelsSamples/General/TooltipHoverArea/ViewModel.cs).
:::

## Tooltip text

The default tooltip shows a header, then one row per point with the series miniature, the series
`Name`{{ if xaml }} (`SeriesName` in XAML){{ end }} and the value. A series with no name shows no name in the
tooltip. Two properties of the series build that text, both take the point and return a string:

- `YToolTipLabelFormatter` builds the value of each row.
- `XToolTipLabelFormatter` builds the header, from the first point in the tooltip.

When they are not set, a line or column series formats the value with the `Labels` or the `Labeler` of its Y axis.
The header uses the `Labels` or the `Labeler` of its X axis, and is left out while the X axis has neither. Some
series set their own `YToolTipLabelFormatter`: a scatter series shows `X = ` and `Y = ` (and `W = ` when it is
weighted), a financial series shows the high, open, close and low values, a box series its five values, and a heat
series the Y label and the weight. Range series show both ends of the range.

Take these two series:

```csharp
public ISeries[] Series { get; set; } = [
    new LineSeries<double>
    {
        Values = [2, 1, 3, 5, 3, 4, 6],
        Fill = null,
        GeometrySize = 20
    },
    new LineSeries<int, StarGeometry>
    {
        Values = [4, 2, 5, 2, 4, 5, 3],
        Fill = null,
        GeometrySize = 20
    }
];
```

By default the tooltip is:

![tooltip](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/tooltip-format1.png)

`YToolTipLabelFormatter` changes the value:

```csharp
public ISeries[] Series { get; set; } = [
    new LineSeries<double>
    {
        Values = [2, 1, 3, 5, 3, 4, 6],
        Fill = null,
        GeometrySize = 20,
        YToolTipLabelFormatter = point => point.Model.ToString("C2") // mark
    },
    new LineSeries<int, StarGeometry>
    {
        Values = [4, 2, 5, 2, 4, 5, 3],
        Fill = null,
        GeometrySize = 20,
        YToolTipLabelFormatter = point => point.Model.ToString("C2") // mark
    }
];
```

![tooltip](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/tooltip-format2.png)

`point.Model` is the item of the `Values` collection, so with your own types it gives access to all their
properties. In the next case it is a `City`:

```csharp
public ISeries[] Series { get; set; } = [
    new LineSeries<City>
    {
        Values = [new() { Population = 4 }, new() { Population = 2 }],
        Mapping = (city, index) => new(index, city.Population),
        YToolTipLabelFormatter = point => point.Model!.Population.ToString("N2") // mark
    }
];

// ...

public class City
{
    public double Population { get; set; }
}
```

`XToolTipLabelFormatter` adds a header. `point.Index` is the position of the point in the `Values` collection:

```csharp
new LineSeries<double>
{
    Values = [2, 1, 3, 5, 3, 4, 6],
    Fill = null,
    GeometrySize = 20,
    XToolTipLabelFormatter = point => $"X = {point.Index}", // mark
    YToolTipLabelFormatter = point => point.Model.ToString("C2")
};
```

![tooltip](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/tooltip-format3.png)

Pie series have no X and Y, they use `ToolTipLabelFormatter` for the value and show no header.

## Stacked values

The points of the series that stack (stacked area, stacked step area, stacked column, stacked row and pie series)
have a `StackedValue` property with the `Start`, `End` and `Total` of the stack, and the `Share` of the point in that
total, from `0` to `1`. See the
[stacked series article]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Stacked%20series).

```csharp
public ISeries[] Series { get; set; } = [
    new StackedColumnSeries<double>
    {
        Values = [2, 1, 3, 5, 3, 4, 6],
        YToolTipLabelFormatter =
            point => $"{point.Model} / {point.StackedValue!.Total} ({point.StackedValue.Share:P2})"
    },
    new StackedColumnSeries<int>
    {
        Values = [4, 2, 5, 2, 4, 5, 3],
        YToolTipLabelFormatter =
            point => $"{point.Model} / {point.StackedValue!.Total} ({point.StackedValue.Share:P2})"
    }
];
```

![tooltip](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/tooltip-format4.png)

## Tooltip style

`TooltipTextPaint`, `TooltipTextSize` and `TooltipBackgroundPaint` style the default tooltip. When they are not
set, the default theme draws the text at size `16`, dark gray on a light gray background with a shadow (light gray
on a dark gray background in the dark theme).

{{~ if xaml ~}}
```xml
<lvc:CartesianChart
    Series="{Binding Series}"
    TooltipPosition="Left"
    TooltipBackgroundPaint="{lvc:SolidColorPaint Color='#480032'}"
    TooltipTextPaint="{lvc:SolidColorPaint Color='#F2F4FF', FontFamily='Courier New'}"
    TooltipTextSize="16">
</lvc:CartesianChart>
```
{{~ end ~}}

{{~ if blazor ~}}
```xml
<CartesianChart
    Series="series"
    TooltipPosition="LiveChartsCore.Measure.TooltipPosition.Left"
    TooltipBackgroundPaint="@tooltipBackgroundPaint"
    TooltipTextPaint="@tooltipTextPaint"
    TooltipTextSize="16">
</CartesianChart>

@code {
    private SolidColorPaint tooltipBackgroundPaint = new(new SKColor(72, 0, 50));
    private SolidColorPaint tooltipTextPaint = new(new SKColor(242, 244, 255)) { FontFamily = "Courier New" };
    // ...
}
```
{{~ end ~}}

{{~ if winforms || eto ~}}
```csharp
cartesianChart1.TooltipPosition = LiveChartsCore.Measure.TooltipPosition.Left;
cartesianChart1.TooltipBackgroundPaint = new SolidColorPaint(new SKColor(72, 0, 50)); // mark
cartesianChart1.TooltipTextPaint = new SolidColorPaint(new SKColor(242, 244, 255)) { FontFamily = "Courier New" }; // mark
cartesianChart1.TooltipTextSize = 16; // mark
```
{{~ end ~}}

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/samples/general/templatedTooltips/styling-tooltips.png)

A text wider than 170 pixels wraps to the next line; the limit is `MaxTooltipsAndLegendsLabelsWidth` in
`LiveCharts.DefaultSettings`, and legends use it too.

## Custom tooltips

Set the `Tooltip` property of the chart to replace the default tooltip. The easiest way to build your own is to
inherit from `SKDefaultTooltip`: `Initialize()` sets the background, the wedge (the triangle that points to the
hovered point) and the animations, and `GetLayout()` returns the content, built with the LiveCharts layouts
(`StackLayout`, `LabelGeometry` and the miniature of each series). `Show()` and `Hide()` can be overridden too. In this
sample, each data point has an SVG path, and `CustomTooltip` draws that path next to the value.
{{~ if xaml2006 ~}}
The XAML parser of this platform does not take generic types, so the sample declares a non-generic series in
`CustomGeometryPointColumnSeries.cs`.
{{~ end ~}}

{{ render_sample "general/templatedTooltips" }}

## Tooltip from scratch

A tooltip can be any class that implements `IChartTooltip`: the chart calls `Show()` with the points to show, and
`Hide()` to close it. Use the
[SKDefaultTooltip source code](https://github.com/beto-rodriguez/LiveCharts2/blob/master/src/skiasharp/LiveChartsCore.SkiaSharp/SKCharts/SKDefaultTooltip.cs)
as a guide, it is the tooltip the library uses by default.

A tooltip drawn with the LiveCharts API can only draw inside the bounds of the chart control, which can cut it in
small charts ([#912](https://github.com/beto-rodriguez/LiveCharts2/issues/912)). You can also build the tooltip with
your UI framework instead, [#1558](https://github.com/beto-rodriguez/LiveCharts2/issues/1558) shows a WPF control
that implements `IChartTooltip`.

## Crosshairs

Crosshairs are set on each axis. `CrosshairPaint` draws a line that follows the pointer while it is over the plot
area: a vertical line for an X axis, a horizontal one for a Y axis. An axis with no `CrosshairPaint` draws no
crosshair. The line is removed when the tooltip closes.

- `CrosshairLabelsPaint` draws a label on the axis with the value under the pointer, formatted with the `Labels` or
  `Labeler` of the axis and at the axis `TextSize`.
- `CrosshairLabelsBackground` is the background color of that label, and `CrosshairPadding` its padding; when the
  padding is not set, the axis `Padding` is used.
- `CrosshairSnapEnabled` moves the line to the point closest to the pointer, among the visible series that use that
  axis, and the label shows the value of that point. It is `false` by default.

In this sample both axes draw a crosshair, and only the Y crosshair snaps to the points:

{{ render_sample "axes/crosshairs" }}

When axes are shared between charts with `SharedAxes.Set()`, the crosshair moves in all of them, see the
[shared axes]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Axes#shared-axes).
