<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
content is normally pulled from the examples in the repository.
-->

:::tip
This article adds geometries directly to the canvas, this is intended to explain how geometries and animations
are handled in the library, but in general the recommended way to draw a custom element in the chart is to use the 
`Visual` class, for more info please see the [visual elements article](https://livecharts.dev/docs/{{ platform }}/{{ version }}/samples.general.visualElements).
:::

{{ render "~/shared/genericSampleJustGifHeader.md" }}

## Pre-requisites

This example uses `MotionProperties`, this is a special type of property in the library that allows properties to animate
when the value changes. `MotionProperties` require a lot of metadata to work (similar to dependency or bindable properties)
but instead of manually writing all that boring code, install the [LiveChartsGenerators](https://www.nuget.org/packages/LiveChartsGenerators) NuGet package:

{{ "LiveChartsGenerators@1.0.1" | from_nuget_sg }}

## Intro

We can directly draw on the canvas to create custom shapes or effects, by default LiveCharts uses SkiaSharp
to render the controls, this means that you can use all the SkiaSharp API to draw on the canvas, you can find
more information about SkiaSharp [here](https://learn.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/graphics/skiasharp/basics/).

In the next example we use the `UpdateStarted` command/event in the `CartesianChart`, this command/event is raised every time
the control is measured, a LiveCharts control is measured when the data or the control size change.

{{~ if mvvm ~}}
## View model

```csharp
{{ render_current_directory_view_model }}
```
{{~ end ~}}

## {{~ view_title ~}}

```
{{ render_current_directory_view }}
```

In the previous case we inherited from `Geometry`, this class already contains some useful properties that we
can use to set the location, rotation, opacity or transform of the geometry, you can find more information about
the `Geometry` class [here](https://livecharts.dev/api/{{ version }}/LiveChartsCore.SkiaSharpView.Drawing.Geometries.Geometry).

We override the `OnDraw` method to define the drawing logic of our custom geometry, in this case we are only drawing a circle
based on the `X`, `Y` and `Diameter` properties.

These properties are not regular properties, they are a special type defined by LiveCharts, the
[MotionProperty<T>](https://livecharts.dev/api/{{ version }}/LiveChartsCore.Motion.MotionProperty-1) type is what handles animations
in LiveCharts. Every time we access a motion property we get the value of the property at the current time based on the
[IAnimatable](https://livecharts.dev/api/{{ version }}/LiveChartsCore.Drawing.IAnimatable) animation.

We also created an instance of the `SolidColorPaint` class, this class defines how the geometry will be rendered on the canvas,
in this case a blue color with a stroke width of 2, then we added our geometry to the paint (you can add multiple geometries),
and we also added the paint to the canvas.

The user interface update cycle is the following:

1. The chart control is invalidated (data changed or size changed), so the control is measured and with it the `UpdateStarted`
command/event is raised.

2. When the control is measured, we add Paints to the canvas, and geometries to the paints, this is only scheduling the drawing,
nothing is rendered yet at this point.

3. Now the control starts drawing all the paints and geometries in the canvas, this is when the `OnDraw` method is called.

4. LiveCharts defines the `MotionCanvas` class, it redraws the user interface as all the animations complete. This means that the
previous step is repeated multiple times per second (~60), so you must be careful when overriding the `OnDraw` method, you should only
perform drawing operations there. LiveCharts will keep drawing until all animations are finished.

{{ render "~/shared/relatedTo.md" }}
