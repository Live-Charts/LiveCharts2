# LiveCharts2

[![LiveCharts](https://github.com/Live-Charts/LiveCharts2/actions/workflows/livecharts.yml/badge.svg?branch=master)](https://github.com/Live-Charts/LiveCharts2/actions/workflows/livecharts.yml)
[![Line Coverage](https://raw.githubusercontent.com/Live-Charts/LiveCharts2/badges/badge_linecoverage.svg)](https://live-charts.github.io/LiveCharts2/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/LiveChartsCore)](https://www.nuget.org/packages/LiveChartsCore/)

[Watch Blazor WASM demo](https://blazor-livecharts.controli.app/) (only designed for desktop devices for now)

LiveCharts2 (v2) is the evolution of [LiveCharts](https://github.com/Live-Charts/Live-Charts) (v0). It fixes the main design issues of its predecessor, is focused on running everywhere, and improves flexibility without losing what we already had in v0.

### Extremely flexible data visualization library

The following image is a preview, `v2.0` is beta now.

Here is a preview (1.4MB gif, wait for it to load if you see a blank space below this text...):

![lv2](https://user-images.githubusercontent.com/10853349/124399763-41873900-dce3-11eb-937a-947d66d42597.gif)

### Get started

LiveCharts is a cross-platform charting library for .NET. To get started, go to https://livecharts.dev and take a look at the installation guide for your target platform.
The website contains all the samples provided in this repo, documentation, and more.

LiveCharts supports:

- Maui
- Uno Platform
- Wpf
- WinUI
- Xamarin.Forms
- WindowsForms
- BlazorWasm
- Avalonia
- Eto Forms
- Uwp

You can also use LiveCharts 2 in a console app or on the server side by installing only the core packages. Take a look at [this guide](https://lvcharts.com/docs/WPF/2.0.0-beta.330/samples.general.chartToImage#build-an-image-in-the-server-side-or-console-app).

### The Errors of v0

V0 is built on top of WPF, which has many limitations. WPF is not designed for the purposes of the library, and it is always tricky to find solutions for the library's problems.

### How Flexible is v2?

When we were on v0 and tried to bring the library to UWP, we noticed it required a huge effort with the architecture the library had in v0.
V2 is designed to work on multiple platforms and requires minimal effort to bring the library to a new platform.

### Then LiveCharts2 requires SkiaSharp?

Not necessarily. The Skia API makes it much easier to take the library everywhere, but that does not mean that LiveCharts2 requires it to work. We could easily move to any other drawing engine.

