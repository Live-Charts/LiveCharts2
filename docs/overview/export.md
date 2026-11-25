<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
Samples are pulled from the repository with render_sample, the code shown always matches the samples.
-->

# Charts as images

LiveCharts can render a chart to an image without any UI framework: in a console app, on a server, or from a chart
control that is already on screen. The image charts are `SKCartesianChart`, `SKPieChart`, `SKPolarChart` and
`SKGeoMap`; they take the same properties as the `CartesianChart`, `PieChart`, `PolarChart` and `GeoMap` controls, and
save the result as PNG, JPEG or WEBP, or draw it on any SkiaSharp canvas, for example to build an SVG file.

:::info
Every member is listed in the API explorer:
[SKCartesianChart]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.SKCharts.SKCartesianChart),
[SKPieChart]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.SKCharts.SKPieChart),
[SKPolarChart]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.SKCharts.SKPolarChart),
[SKGeoMap]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.SKCharts.SKGeoMap) and their base class,
[InMemorySkiaSharpChart]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.SKCharts.InMemorySkiaSharpChart).
:::

## Image from a control

Pass a chart control to the constructor of the image chart of the same kind, and save it. This sample places a
`CartesianChart` and a `PieChart` in the view and saves both as PNG files once the view is loaded.

{{ render_sample "general/chartToImage" }}

{{~ if xaml ~}}
The images are built in the code behind of the view:

```csharp
{{~ render $"~/../samples/{platform_samples_folder}/General/ChartToImage{view_code}" ~}}
```
{{~ end ~}}

The image is what the control draws at that moment, with the size of the control: `Width` and `Height` are ignored.
The control must be loaded and have a size, otherwise `GetImage` and `SaveImage` throw.

## Charts without a UI

The image charts are in the `LiveChartsCore.SkiaSharpView` package. Every LiveCharts view package (WPF, WinForms,
Avalonia, MAUI and the others) depends on it, so install it only in a project that does not use one, such as a console
app or a web server:

{{ $"LiveChartsCore.SkiaSharpView@{version}" | from_nuget }}

Create the chart, set its properties as you would on a control, and save it:

```csharp
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using SkiaSharp;

var cartesianChart = new SKCartesianChart
{
    Width = 900,
    Height = 600,
    Series = [
        new LineSeries<int> { Values = [1, 5, 4, 6] },
        new ColumnSeries<int> { Values = [4, 8, 2, 4] }
    ],
    LegendPosition = LiveChartsCore.Measure.LegendPosition.Right,
    Background = SKColors.White
};

cartesianChart.SaveImage("cartesianChart.png"); // mark
```

`SKPieChart`, `SKPolarChart` and `SKGeoMap` work the same way:

```csharp
var pieChart = new SKPieChart
{
    Width = 900,
    Height = 600,
    Series = [
        new PieSeries<int> { Values = [10] },
        new PieSeries<int> { Values = [6] },
        new PieSeries<int> { Values = [4] }
    ]
};

pieChart.SaveImage("pieChart.png");

var geoHeatMap = new SKGeoMap
{
    Width = 900,
    Height = 600,
    Series = [
        new HeatLandSeries
        {
            Lands = [
                new() { Name = "mex", Value = 10 },
                new() { Name = "usa", Value = 15 },
                new() { Name = "can", Value = 8 }
            ]
        }
    ]
};

geoHeatMap.SaveImage("geoHeatMap.png");
```

An image chart has no animations: every time it builds an image, it measures the chart and draws it at its final state.
The complete console app is in the repository, in
[samples/ConsoleSample](https://github.com/beto-rodriguez/LiveCharts2/tree/master/samples/ConsoleSample).

## Size and background

`Width` and `Height` set the size of the image in pixels, `900` by `600` by default.

`Background` is the color the image is filled with before the chart is drawn. When it is not set, the image uses the
background color of the theme, white with a light theme and black with a dark one. Set it to `SKColors.Transparent`
for an image with a transparent background:

```csharp
var chart = new SKCartesianChart
{
    Width = 400, // mark
    Height = 300, // mark
    Background = SKColors.Transparent, // mark
    Series = [new LineSeries<int> { Values = [1, 5, 4, 6] }]
};
```

## Formats and streams

`SaveImage` takes a file path or a `Stream`. The second argument is the format, an `SKEncodedImageFormat` that is
`Png` by default, and the third the quality, from `0` to `100`, `80` by default:

```csharp
chart.SaveImage("chart.jpg", SKEncodedImageFormat.Jpeg, 90);

using var stream = new MemoryStream();
chart.SaveImage(stream, SKEncodedImageFormat.Webp);
```

`GetImage` returns the image as an `SKImage`, for any other operation, such as sending it from a web API as base64.
Dispose it when you are done:

```csharp
using var image = chart.GetImage();
using var data = image.Encode();
var base64 = Convert.ToBase64String(data.AsSpan());
```

## SVG

`DrawOnCanvas` draws the chart on any SkiaSharp `SKCanvas`. To build an SVG file, draw on an `SKSvgCanvas`, and dispose
the canvas before reading the stream: the SVG document is completed when the canvas is disposed.

```csharp
using var stream = new MemoryStream();

var svgCanvas = SKSvgCanvas.Create(SKRect.Create(chart.Width, chart.Height), stream);
chart.DrawOnCanvas(svgCanvas); // mark
svgCanvas.Dispose(); // mark

stream.Position = 0;
using var file = File.Create("chart.svg");
stream.CopyTo(file);
```
