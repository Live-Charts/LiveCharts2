<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
Samples are pulled from the repository with render_sample, the code shown always matches the samples.
-->

# Custom drawing

LiveCharts draws every chart with SkiaSharp, and you can add your own shapes to that drawing. There are three ways:
visual elements add shapes, labels or layouts to the chart and the chart places and animates them for you; visual
states change how a single point is drawn, for example its fill, while the app runs; and you can also add geometries
straight to the canvas of the chart. Visual elements are the recommended way to draw a custom element in a chart.

:::info
This article highlights the most used features. Every member is listed in the API explorer:
[Visual]({{ website_url }}/api/{{ version }}/LiveChartsCore.VisualElements.Visual),
[DrawnLabelVisual]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.VisualElements.DrawnLabelVisual),
[VisualStatesDictionary]({{ website_url }}/api/{{ version }}/LiveChartsCore.VisualStates.VisualStatesDictionary) and
[CoreMotionCanvas]({{ website_url }}/api/{{ version }}/LiveChartsCore.Motion.CoreMotionCanvas).
{{~ if xaml ~}}
In XAML, declare a label visual as `XamlDrawnLabelVisual`, and the states of a series with `ChartPointState` and `Set`.
{{~ end ~}}
:::

## Visual elements

The `VisualElements` property of a chart takes a collection of `IChartElement`. Inherit from the `Visual` class to
define an element: the chart adds it to the canvas, animates it, and removes it when it leaves the collection. This
sample adds ten visuals to a chart, each one is a class in the code below and is explained in the next sections.

{{ render_sample "general/visualElements" }}

## Place a visual

A `Visual` overrides two members. `DrawnElement` is the shape it draws; the property is declared as `IDrawnElement`,
and the override can return a more specific type, such as `RectangleGeometry`, thanks to
[covariant returns](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/covariant-returns).
`Measure` runs every time the chart updates, and sets the position and size of the shape, in pixels from the top-left
corner of the chart:

```csharp
public class RectangleVisual : Visual
{
    protected override RectangleGeometry DrawnElement { get; } =
        new RectangleGeometry { Fill = new SolidColorPaint(SKColors.Red) };

    protected override void Measure(Chart chart)
    {
        DrawnElement.X = 100; // mark
        DrawnElement.Y = 100; // mark
        DrawnElement.Width = 40;
        DrawnElement.Height = 40;
    }
}
```

To place the shape at a data coordinate, convert it with `ScaleDataToPixels` (the `ScaledRectangleVisual` class in the
sample):

```csharp
protected override void Measure(Chart chart)
{
    var cartesianChart = (ICartesianChartView)chart.View;

    var locationInPixels = cartesianChart.ScaleDataToPixels(new LvcPointD(5, 5)); // mark

    DrawnElement.X = (float)locationInPixels.X;
    DrawnElement.Y = (float)locationInPixels.Y;
    DrawnElement.Width = 40;
    DrawnElement.Height = 40;
}
```

When a property of the drawn element changes, it animates to the new value with the animation of the chart. Set the
`Easing` and `AnimationSpeed` of the visual to use a different animation for it. `IsVisible` shows or hides the visual.

## Labels

`DrawnLabelVisual` draws a text. Pass it a `LabelGeometry` with the text, the size and the position; when its paint is
not set, the theme picks it. It is the same visual the `Title` of a chart uses.

```csharp
VisualElements = new IChartElement[]
{
    new DrawnLabelVisual(
        new LabelGeometry
        {
            Text = "Hello",
            TextSize = 20,
            X = 100,
            Y = 50
        })
};
```

## Geometries and SVG paths

`DrawnElement` can be any geometry of the `LiveChartsCore.SkiaSharpView.Drawing.Geometries` namespace, such as
`RectangleGeometry`, `CircleGeometry`, `DiamondGeometry` or `LabelGeometry`. To draw an SVG path, use a
`VariableSVGPathGeometry` and set its `Path` (the `SvgVisual` class in the sample):

```csharp
protected override VariableSVGPathGeometry DrawnElement { get; } =
    new VariableSVGPathGeometry
    {
        Stroke = new SolidColorPaint(SKColors.Blue),
        Path = SKPath.ParseSvgPathData(SVGPoints.Star) // mark
    };
```

## Custom geometries

To draw anything else, write your own geometry with the SkiaSharp API. Inherit from `BoundedDrawnGeometry`, which
already has animated `X`, `Y`, `Width` and `Height` properties, and implement `IDrawnElement<SkiaSharpDrawingContext>`.
`Draw` runs once for each paint of the geometry, its `Fill` and then its `Stroke`; `context.Canvas` is the SkiaSharp
canvas and `context.ActiveSkiaPaint` the paint being drawn:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/samples/general/visualElements/custom.png)

```csharp
public class CustomSkiaShape : BoundedDrawnGeometry, IDrawnElement<SkiaSharpDrawingContext>
{
    public void Draw(SkiaSharpDrawingContext context)
    {
        var canvas = context.Canvas; // mark
        var paint = context.ActiveSkiaPaint; // mark

        var x = X + Width / 2f;
        var y = Y + Height / 2f;
        var r = (float)Width / 2f;

        canvas.DrawCircle(x, y, r, paint);
        canvas.DrawCircle(x, y, r * 0.75f, paint);
        canvas.DrawCircle(x, y, r * 0.5f, paint);
    }
}
```

The `CustomVisual` class in the sample returns this geometry as its `DrawnElement`. To add your own animated
properties to a geometry, see [draw on the canvas](#draw-on-the-canvas).

## Layouts

A layout is a drawn element that positions other elements; the library uses them to build the tooltips and the
legends. They are in the `LiveChartsCore.SkiaSharpView.Drawing.Layouts` namespace:

- `AbsoluteLayout` places its `Children` at their own `X` and `Y`, relative to the position of the layout
  (`AbsoluteVisual` in the sample).
- `StackLayout` places its `Children` one after the other, set `Orientation` to stack them vertically or horizontally
  (`StackedVisual`).
- `TableLayout` places its `Cells` in rows and columns, each cell takes the row, the column and the element
  (`TableVisual`).
- `Container` draws a shape behind its `Content` and takes the size of the content; set the `Fill` and `Stroke` of its
  `Geometry` to paint the background and the border (`ContainerVisual`).

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/samples/general/visualElements/table.png)

```csharp
protected override TableLayout DrawnElement { get; } =
    new TableLayout
    {
        Cells = [
            new(0, 0, GetLabel("A")),
            new(0, 1, GetLabel("-")),
            new(0, 2, GetLabel("B")),
            new(1, 0, GetLabel("-")),
            new(1, 1, GetLabel("C")),
            new(1, 2, GetLabel("-"))
        ],
    };
```

## Themes

A visual can take its paints from the theme, so it changes with the light and dark modes. The `ThemedVisual` class in
the sample registers a rule for its type with `HasRuleFor<ThemedVisual>`, and applies it with
`theme.ApplyStyleTo<ThemedVisual>(this)` when the theme of the chart changes: the fill is white on a dark theme and
black on a light one. See the [themes article]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Themes)
for more about rules.

## Pointer events

A `Visual` raises `PointerDown` when the pointer goes down over it (the `PointerDownAwareVisual` class in the sample).
The element is hit when the pointer is inside the rectangle that starts at the `X` and `Y` of the drawn element and has
its measured size.

```csharp
public PointerDownAwareVisual()
{
    PointerDown += OnPointerDown; // mark
}

private void OnPointerDown(IInteractable visual, VisualElementEventArgs args)
{
    var location = args.PointerLocation;
    Trace.WriteLine($"Pointer down at {location.X}, {location.Y}");
}
```

The chart also raises `VisualElementsPointerDown`{{ if xaml }} (`VisualElementsPointerDownCommand` in XAML){{ end }} with
every visual under the pointer, see the [events article]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Events).

## Drawing order

By default a visual is drawn on top of the series, which is what you normally want for glyphs, callouts or markers.
To draw it behind the series, for example as a background, set its `ZIndex` to a negative value. It is then drawn
before everything else on the canvas, so a filled draw margin frame covers it.

```csharp
new RectangleVisual { ZIndex = -1 };
```

A `ZIndex` of `0` (the default) or a positive value keeps the visual on top of the series. Visuals on the same side of
the series are drawn in `ZIndex` order, so a visual with a higher `ZIndex` covers one with a lower value. The `ZIndex`
can change at run time, and the visual moves to the other side of the series when its sign changes.

## Per point states

A visual state is a named set of property values for the shape of a point. Define the states of a series with
`HasState`{{ if xaml }} (or in XAML, with `ChartPointState` items in its `AdditionalVisualStates`){{ end }}, then apply one
to a point with `SetState` and remove it with `ClearState`. Removing a state restores the values the shape had before.
The `PointMeasured` event (a command in the XAML samples) runs every time a point is measured, so it can choose the
states from the data. In this sample the columns and their labels turn red when the value is greater than `5`.

{{ render_sample "general/conditionalDraw" }}

The properties a state sets belong to the drawn shape, `point.Context.Visual`, or to its data label,
`point.Context.Label`; a `Fill` set by a state replaces the paint of the series for that shape only:

```csharp
columnSeries.HasState("Danger", [
    (nameof(IDrawnElement.Fill), new SolidColorPaint(SKColors.Red)) // mark
]);

columnSeries.PointMeasured += point =>
{
    var ctx = point.Context;
    if (ctx.DataSource is not ObservableValue observable) return;

    if (observable.Value > 5)
        ctx.Series.VisualStates.SetState("Danger", ctx.Visual); // mark
    else
        ctx.Series.VisualStates.ClearState("Danger", ctx.Visual); // mark
};
```

The library applies the `Hover` state when the pointer enters a point and clears it when the pointer leaves. The
default theme defines it for every series: it lowers the opacity of the shape, except on line, step line and polar line
series, where it scales the point up instead. A `Hover` state you define replaces the one of the theme, as the sample
does to draw a shadow.

To change a point only once, when it is measured, without restoring it later, see
[customize each point]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Line%20and%20step%20line%20series#customize-each-point).

## Draw on the canvas

You can also add a geometry straight to the canvas of the chart. This is what a `Visual` does for you, so it is
normally the easier option, but drawing on the canvas shows how geometries and animations work in the library.

The sample adds a circle at the data coordinate `(5, 5)`, and animates its size when the chart is clicked.

{{ render_sample "general/drawOnCanvas" }}

The `UpdateStarted` event{{ if xaml }} (`UpdateStartedCommand` in XAML){{ end }} runs every time the chart updates, after
the axes, the series and the visual elements were measured and before the chart is drawn, so `ScaleDataToPixels` returns
the current position there. The sample adds the geometry once with `chart.CoreChart.Canvas.AddGeometry(...)`; a geometry
added to the canvas is drawn with its own `Fill` and `Stroke`, on top of the series.

The `Diameter` of `MotionGeometry` is a motion property: every time it changes, it animates from the current value to the
new one, using the animation set with `Animate`. The `[MotionProperty]` attribute generates that code, it needs the
`LiveChartsGenerators` package, and both the class and the property must be `partial` (partial properties need
C# 13 or later):

{{ "LiveChartsGenerators@1.0.1" | from_nuget_sg }}

The drawing happens in these steps:

1. The chart updates when its data, its size or one of its properties changes. It measures its elements and raises
   `UpdateStarted`.
2. Adding paints and geometries to the canvas only schedules them, nothing is drawn yet.
3. The canvas draws every paint and geometry, this is when `Draw` runs.
4. While any animation is running, the canvas draws a new frame again and again until every animation has finished,
   so `Draw` runs many times per second. Only draw in `Draw`; set the values in the update.
