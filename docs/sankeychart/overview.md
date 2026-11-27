<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
Samples are pulled from the repository with render_sample, the code shown always matches the samples.
-->

# Sankey diagrams

The `SankeyChart` control draws [sankey diagrams](https://en.wikipedia.org/wiki/Sankey_diagram): flows between
nodes, where the thickness of each flow is proportional to its value. The diagram is drawn by a `SankeySeries`: its
`Values` are the nodes, drawn as rectangles, and its `Links` are the flows between them, drawn as ribbons. The control
has no axes, the series uses the whole chart area. This article covers the control and the series together.

:::info
The SankeyChart is available since LiveCharts 2.3.

This article highlights the most used features. Every property is listed in the API explorer:
[SankeyChart]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.{{ platform }}.SankeyChart),
[SankeySeries]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.SankeySeries-3),
[SankeyNode]({{ website_url }}/api/{{ version }}/LiveChartsCore.Defaults.SankeyNode) and
[SankeyLink]({{ website_url }}/api/{{ version }}/LiveChartsCore.Defaults.SankeyLink-1).
{{~ if xaml ~}}
In XAML, declare the series as `XamlSankeySeries`, its nodes are `SankeyNode` objects.
{{~ end ~}}
:::

## Basic sankey

Create a `SankeyNode` for every node, then a `SankeyLink` for every flow, with its source node, its target node and
its weight. Set the nodes as the `Values` of a `SankeySeries` and the links as its `Links`. `Fill` paints the nodes
and `DataLabelsPaint` shows the name of each node next to it.

{{ render_sample "sankeys/basic" }}

## Nodes and links

`SankeyNode` has only a `Name`, used as its label and in the tooltip. The value of a node is calculated from its
links: it is the sum of the weights of the links that arrive at the node, or of the links that leave it, whichever is
greater. In the sample, `Fixed Expenses` receives 320 + 420 + 180 + 950 = 1870.

The `Source` and `Target` of a link are references to node objects in `Values`, and `Weight` sets the thickness of
the ribbon. The layout ignores a link when:

- its `Source` or `Target` is not in `Values`,
- its `Source` and `Target` are the same node,
- its `Weight` is `0`, negative, `NaN` or infinity.

The links must not form a cycle: when a node can be reached again by following the links from it, the layout
throws an `InvalidOperationException`.

The nodes can be any class. Pass your type as the generic argument of the series, and use `LabelMapper` to give each
node its label (without it, only `SankeyNode` labels are found):

```csharp
public class Account
{
    public string Code { get; set; }
    public string Description { get; set; }
}

var salary = new Account { Code = "4100", Description = "Salary" };
var rent = new Account { Code = "6100", Description = "Rent" };
var savings = new Account { Code = "1200", Description = "Savings" };

Series = new ISeries[]
{
    new SankeySeries<Account>
    {
        Values = new[] { salary, rent, savings },
        Links = new[]
        {
            new SankeyLink<Account>(salary, rent, 950),
            new SankeyLink<Account>(salary, savings, 300)
        },
        LabelMapper = account => account.Description, // mark
        DataLabelsPaint = new SolidColorPaint(SKColors.Black)
    }
};
```

## Layout

The nodes are placed in columns:

- Nodes with no incoming links are in the first column. Any other node is one column after the furthest node that
  flows into it, so a node that only receives links is not always in the last column.
- The columns are spaced evenly: the first one is at the left edge of the chart and the last one at the right edge,
  after the room taken by the [labels](#labels).
- The height of a node is proportional to its value, and the thickness of a ribbon to its weight, with the same
  scale for the whole chart. The scale is chosen so the column with the largest total value fills the height of the
  chart.

`NodeWidth` is the width of each node, `12` pixels by default, and `NodePadding` is the minimum vertical space
between two nodes of the same column, `8` pixels by default. `NodeCornerRadius` rounds the corners of the nodes, in
pixels, `0` by default.

The nodes of a column start stacked in the order of `Values`. Then the layout moves every node closer to the nodes it
is linked to, to reduce the ribbons that cross each other; `LayoutIterations` sets how many times it does it, `32` by
default. More iterations give a cleaner diagram but take longer. At each node, the ribbons are stacked in the order
of the nodes at their other end, so they do not cross at the node.

A ribbon goes from the right edge of its source node to the left edge of its target node, with two curves whose
control points are halfway between both nodes.

```csharp
new SankeySeries<SankeyNode>
{
    Values = nodes,
    Links = links,
    NodeWidth = 18, // mark
    NodePadding = 14, // mark
    NodeCornerRadius = 3, // mark
    LayoutIterations = 64 // mark
};
```

## Arc layout

`Layout` sets how the nodes are arranged. `Vertical`, the default, is the column layout above. `BipartiteArc` places
the source nodes on an arc on the left of a circle, the target nodes on an arc on the right, and draws the ribbons
through the center of the circle:

```csharp
new SankeySeries<SankeyNode>
{
    Values = nodes,
    Links = links,
    Layout = SankeyLayoutKind.BipartiteArc, // mark
    ArcSpanDegrees = 150 // mark
};
```

{{~ if xaml ~}}
```xml
<lvc:XamlSankeySeries
    Values="{Binding Nodes}"
    Links="{Binding Links}"
    Layout="BipartiteArc"/>
```
{{~ end ~}}

- Every node must be a source only or a target only: when a node has incoming and outgoing links, the layout throws
  an `InvalidOperationException`.
- `ArcSpanDegrees` is the angle each arc covers, `150` degrees by default, which leaves a gap at the top and at the
  bottom of the circle.
- `NodeWidth` is the thickness of the arc, and `NodePadding` the gap between two nodes, measured along the inner edge
  of the arc.
- The nodes of each arc follow the order of `Values` from top to bottom; `LayoutIterations` is not used.
- The labels are placed outside the circle, rotated to follow the radius; `NodeLabelPosition` is not used. The radius
  of the circle leaves room for the longest label.

## Paints

`Fill` paints the nodes, `Stroke` draws their border and `LinkFill` paints the ribbons. The ribbons are drawn below
the nodes, and the labels above both.

When `Fill` and `LinkFill` are not set, LiveCharts picks them from the current theme: the nodes get the color of the
series, and the ribbons the same color, translucent. The default theme draws no `Stroke`. When you set `Fill`, set
`LinkFill` too, usually to a translucent version of the same color; set `LinkFill` to `null` to paint the ribbons
with `Fill`.

```csharp
new SankeySeries<SankeyNode>
{
    Values = nodes,
    Links = links,
    Fill = new SolidColorPaint(new SKColor(96, 138, 218)), // mark
    LinkFill = new SolidColorPaint(new SKColor(96, 138, 218, 90)), // mark
    Stroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 1 } // mark
};
```

:::info
Paints can also draw gradients and dashed lines, see the
[Paints article]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Paints).
:::

## Node and link colors

The paints color every node and every ribbon the same way. `NodeColorMapper` gives each node its own color: it
receives the node and returns an `LvcColor`. The ribbons then take the color of their source node at half its
opacity.

`LinkColorMapper` gives each ribbon its own color instead: it receives the `SankeyLink` and returns the color,
including its opacity. It takes precedence over the color of the source node.

```csharp
var green = new LvcColor(64, 175, 110);
var blue = new LvcColor(96, 138, 218);

new SankeySeries<SankeyNode>
{
    Values = nodes,
    Links = links,
    NodeColorMapper = node => node == savings ? green : blue, // mark
    LinkColorMapper = link => link.Target == savings // mark
        ? new LvcColor(64, 175, 110, 120) // mark
        : new LvcColor(180, 180, 180, 90) // mark
};
```

The colors replace the color of the paints, so the nodes need a `Fill` and the ribbons a `LinkFill` or a `Fill`;
the theme sets both by default. `NodeColorMapper` needs a node geometry that supports a color per node, the default
one does.

## Labels

The node labels are hidden by default. Set `DataLabelsPaint` to show them, or set `ShowDataLabels` to `true` to use
the label paint of the theme. `DataLabelsSize` sets the text size, `16` by default. The text is the `Name` of a
`SankeyNode`, or what `LabelMapper` returns; `DataLabelsFormatter` is not used. `LabelMapper` can also add the value
of the node, or any other text:

```csharp
new SankeySeries<SankeyNode>
{
    Values = nodes,
    Links = links,
    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
    LabelMapper = node => node.Name?.ToUpper() // mark
};
```

`NodeLabelPosition` places the labels in the column layout:

- `Auto`, the default: a node with no incoming links has its label on its left, a node with no outgoing links has it
  on its right, and any other node has it centered on the node.
- `Outside`: every label is outside its node, on the left for the nodes in the left half of the chart and on the
  right for the others.
- `Inside`: every label is centered on its node.

The chart leaves room on the left and on the right for the labels placed outside the nodes, so they are not cut by
the edges of the control.

## Tooltips and legends

The tooltip shows the hovered node as `<label>: <value>`; the ribbons do not show a tooltip. `ToolTipLabelFormatter`
builds that text, `point.Model` is the node and `point.Coordinate.PrimaryValue` its value:

```csharp
new SankeySeries<SankeyNode>
{
    Values = nodes,
    Links = links,
    ToolTipLabelFormatter = point => $"{point.Model?.Name}: {point.Coordinate.PrimaryValue:C0}" // mark
};
```

`TooltipPosition` places the tooltip as in a Cartesian chart, `Auto` by default; `Hidden` disables tooltips. The
legend is hidden by default: set `LegendPosition` to `Top`, `Bottom`, `Left` or `Right` to show it. It lists one
entry per series.

{{~ if xaml ~}}
```xml
<lvc:SankeyChart
    TooltipPosition="Bottom"
    LegendPosition="Right">
    <!-- ... -->
</lvc:SankeyChart>
```
{{~ end ~}}

{{~ if blazor ~}}
```xml
<SankeyChart
    Series="series"
    TooltipPosition="LiveChartsCore.Measure.TooltipPosition.Bottom"
    LegendPosition="LiveChartsCore.Measure.LegendPosition.Right">
</SankeyChart>
```
{{~ end ~}}

{{~ if winforms || eto ~}}
```csharp
sankeyChart1.TooltipPosition = LiveChartsCore.Measure.TooltipPosition.Bottom;
sankeyChart1.LegendPosition = LiveChartsCore.Measure.LegendPosition.Right;
```
{{~ end ~}}

The [tooltips and crosshairs]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Tooltips%20and%20crosshairs)
and [legends]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Legends) articles cover their text,
paints and custom controls.

## Title and animations

Every property of the diagram is on the series. The `SankeyChart` takes the `Title`, `DrawMargin`, `AnimationsSpeed`
and `EasingFunction` properties of every chart, see the
[Cartesian chart control]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Cartesian%20chart%20control#title)
and [animations]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Animations) articles.

`SKSankeyChart`, in the `LiveChartsCore.SkiaSharpView.SKCharts` namespace, draws the same chart without a UI
control, for example to save it as an image.

## Data and updates

- Add one series to the chart. Every series is laid out in the whole chart area, so two series draw on top of each
  other.
- `Values` takes any collection of nodes. Use an `ObservableCollection<T>` and the chart updates when a node is added
  or removed, see [automatic updates]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Automatic%20updates).
- The chart reads `Links` on every update, but a change inside the links collection does not start an update by
  itself: assign a new collection to `Links` to update the diagram.
- `Name`{{ if xaml }} (`SeriesName` in XAML){{ end }} is the name of the series in the legend.
