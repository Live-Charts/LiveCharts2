<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
-->

# {{ name | to_title_case }}

{{ render "~/shared/series.md" }}

{{ render "~/shared/datalabels.md" }}

## HeatMap property

This property defines the gradient colors, it is an array of [`LvcColor`]({{ website_url }}/api/{{ version }}/LiveChartsCore.Drawing.LvcColor) 
where the first element in the array is the the smallest or coldest and the last item in the array  is the greatest or hottest, 
any value between the chart limits will be interpolated lineally to create a new color, you can add as many colors as you need 
to define the gradient.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/heathm.png)

```csharp
using SkiaSharp;
using LiveChartsCore.SkiaSharpView;

Series = new ISeries[]
{
    new HeatSeries<WeightedPoint>
    {
        HeatMap = new[] // mark
        { // mark
            SKColors.Yellow.AsLvcColor(), // the first element is the "coldest" // mark
            SKColors.Black.AsLvcColor(), // mark
            SKColors.Blue.AsLvcColor() // the last element is the "hottest" // mark
        }, // mark
        Values = new ObservableCollection<WeightedPoint> { ... }
    }
};
```

## ColorStops property

By default all the colors in the `HeatMap` property are separated equidistantly, you can define the distance 
between each color using the `ColorStops` property, it is an array of double, every item in the array must
go from 0 to 1, where 0 is the "coldest" and 1 the "hottest", notice in the following sample how the 
black to blue gradient is only used in the last 10 percent of the gradient, while the yellow to black is
used in the remaining 90% of the gradient.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/heatcs.png)

```csharp
using SkiaSharp;
using LiveChartsCore.SkiaSharpView;

Series = new ISeries[]
{
    new HeatSeries<WeightedPoint>
    {
        HeatMap = new[]
        {
            SKColors.Yellow.AsLvcColor(), // the first element is the "coldest" // mark
            SKColors.Black.AsLvcColor(), // mark
            SKColors.Blue.AsLvcColor() // the last element is the "hottest" // mark
        },
        ColorStops = new[] // mark
        { // mark
            0, // mark
            0.9, // mark
            1 // mark
        }, // mark
        Values = new ObservableCollection<WeightedPoint> { ... }
    }
};
```

## PointPadding property

Defines the padding for every point in the series.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/heatp.png)

```csharp
Series = new ISeries[]
{
    new HeatSeries<WeightedPoint>
    {
        PointPadding = new LiveChartsCore.Drawing.Common.Padding(20), // mark
        HeatMap = new[]
        {
            Color.FromArgb(255, 255, 241, 118), // the first element is the "coldest"
            Color.DarkSlateGray,
            Color.Blue // the last element is the "hottest"
        },
        Values = new ObservableCollection<WeightedPoint>
        {
            // Charles
            new WeightedPoint(0, 0, 150), // Jan
            new WeightedPoint(0, 1, 123), // Feb
            new WeightedPoint(0, 2, 310), // Mar
            new WeightedPoint(0, 3, 225), // Apr
            new WeightedPoint(0, 4, 473), // May
            new WeightedPoint(0, 5, 373), // Jun

            // Richard
            new WeightedPoint(1, 0, 432), // Jan
            new WeightedPoint(1, 1, 312), // Feb
            new WeightedPoint(1, 2, 135), // Mar
            new WeightedPoint(1, 3, 78), // Apr
            new WeightedPoint(1, 4, 124), // May
            new WeightedPoint(1, 5, 423), // Jun

            // Ana
            new WeightedPoint(2, 0, 543), // Jan
            new WeightedPoint(2, 1, 134), // Feb
            new WeightedPoint(2, 2, 524), // Mar
            new WeightedPoint(2, 3, 315), // Apr
            new WeightedPoint(2, 4, 145), // May
            new WeightedPoint(2, 5, 80), // Jun

            // Mari
            new WeightedPoint(3, 0, 90), // Jan
            new WeightedPoint(3, 1, 123), // Feb
            new WeightedPoint(3, 2, 70), // Mar
            new WeightedPoint(3, 3, 123), // Apr
            new WeightedPoint(3, 4, 432), // May
            new WeightedPoint(3, 5, 142), // Jun
        }
    }
};

XAxes = new ObservableCollection<Axis>
{
    new Axis
    {
        Labels = new [] { "Charles", "Richard", "Ana", "Mari" }
    }
};

YAxes = new ObservableCollection<Axis>
{
    new Axis
    {
        Labels = new [] { "Jan", "Feb", "Mar", "Apr", "May", "Jun" }
    }
};
```

{{ render "~/shared/series2.md" }}
