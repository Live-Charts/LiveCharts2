<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
-->

## Gauges

You can also create gauges with the `PieChart` control, the library provides the `GaugeGenerator` class, 
it is a helper class that builds a collection of `PieSeries<ObservableValue>` based on the properties
we specify, this documentation contains multiple gauges samples, 
[here you can find the basic gauge sample](https://livecharts.dev/docs/{{ platform }}/{{ version }}/samples.pies.gauge1).

The `GaugeGenerator.BuildSolidGauge()` function, takes one or multiple `GaugeItem` instances as parameters, 
a `GaugeItem` instance represents an element in our gauge, the constructor of the `GaugeItem` class takes 2 arguments:

- `value`: the gauge value of type `double`.
- `builder`: a delegate where we can configure the assigned series to the `value`, we can here set the color, the labels size and customize every property as a regular [pie series](https://livecharts.dev/docs/{{ platform }}/{{ version }}/PieChart.Pie%20series).

You can find an example that uses both parameters in the [slim gauge sample](https://livecharts.dev/docs/{{ platform }}/{{ version }}/samples.pies.gauge4).

Finally, there is a special value to customize the background series of a gauge, using the `GaugeItem.Background` as the `value` in the 
`GaugeItem` class, will create a series behind our gauge values that will behave as the background in our plot, 
[here is an example](https://livecharts.dev/docs/{{ platform }}/{{ version }}/samples.pies.gauge2) that uses this feature.
