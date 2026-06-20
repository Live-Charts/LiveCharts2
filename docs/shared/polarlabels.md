<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
content is normally pulled from the examples in the repository.
-->

## DataLabels

{{~ if name != "Pie series" ~}}
:::info
The following block uses the `PieChart` class as an example but all labels in polar series work the same way.
:::
{{~ end ~}}

Data labels are labels for every point in a series, there are multiple properties to customize them, take a look at the 
following sample:

```csharp
Series { get; set; } = new List<ISeries>
{
    new PieSeries<double>
    {
        Values = new List<double> { 8 },
        DataLabelsPaint = new SolidColorPaint(SKColors.Black),
        DataLabelsSize = 22,
        // for more information about available positions see:
        // {{ website_url }}/api/{{ version }}/LiveChartsCore.Measure.PolarLabelsPosition
        DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
        DataLabelsFormatter = point => point.Coordinate.PrimaryValue.ToString("N2") + " elements"
    },
    new PieSeries<double>
    {
        Values = new List<double> { 6 },
        DataLabelsPaint = new SolidColorPaint(SKColors.Black),
        DataLabelsSize = 22,
        DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
        DataLabelsFormatter = point => point.Coordinate.PrimaryValue.ToString("N2") + " elements"
    },
    new PieSeries<double>
    {
        Values = new List<double> { 4 },
        DataLabelsPaint = new SolidColorPaint(SKColors.Black),
        DataLabelsSize = 22,
        DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
        DataLabelsFormatter = point => point.Coordinate.PrimaryValue.ToString("N2") + " elements"
    }
};
```

The series above result in the following chart:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/pielabels.png)

You can also use the `DataLabelsRotation` property to set an angle in degrees for the labels in the chart,
notice the constants `LiveCharts.CotangentAngle` and `LiveCharts.TangentAngle` to build labels rotation.

This is the result when we set all the series to `LiveCharts.CotangentAngle`:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/pielabelscotan.png)

And this is the result when we set all the series to `LiveCharts.TangentAngle`:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/pielabelstan.png)

Finally you can also combine tangent and cotangent angles with decimal degrees:

```csharp
Series { get; set; } = new List<ISeries>
{
    new PieSeries<double>
    {
        DataLabelsRotation = 30, // in degrees
    },
    new PieSeries<double>
    {
        DataLabelsRotation = LiveCharts.TangentAngle + 30, // the tangent + 30 degrees
    },
    new PieSeries<double>
    {
        DataLabelsRotation = LiveCharts.CotangentAngle + 30, // the cotangent + 30 degrees
    }
};
