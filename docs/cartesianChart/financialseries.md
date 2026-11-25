<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
Samples are pulled from the repository with render_sample, the code shown always matches the samples.
-->

# Financial series

Financial series plot the high, open, close and low prices of each period. A `CandlesticksSeries` draws every point
as a candle: a box from the open to the close price, with lines up to the high and down to the low. An `OhlcSeries`
draws the same data as OHLC bars. Both series share every property, so this article covers them together.

:::info
This article highlights the most used features. Every property is listed in the API explorer:
[CandlesticksSeries]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.CandlesticksSeries-3) and
[OhlcSeries]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.OhlcSeries-3).
{{~ if xaml ~}}
In XAML, declare them as `XamlCandlesticksSeries` and `XamlOhlcSeries`.
{{~ end ~}}
:::

## Candlesticks

The values of a financial series are usually
[FinancialPoint]({{ website_url }}/api/{{ version }}/LiveChartsCore.Defaults.FinancialPoint) instances, with a
`Date`, `High`, `Open`, `Close` and `Low`. The date is the X coordinate, stored as `DateTime.Ticks`, so the X axis of
this sample is a date time axis (see below). Notice the order of the arguments when you use the constructor:
`new FinancialPoint(date, high, open, close, low)`.

{{ render_sample "financial/basicCandlesticks" }}

## OHLC bars

An `OhlcSeries` takes the same data and properties. Each point is drawn with lines: a vertical line from the high to
the low, a tick to the left at the open price and a tick to the right at the close price.

{{ render_sample "financial/basicOhlc" }}

## Date time X axis

Each candle is as wide as the `UnitWidth` of the X axis, a distance in the units of the axis, up to `MaxBarWidth`
pixels. With dates on the X axis, the unit is a number of ticks, so it must match the distance between your points,
one day in the samples above. See
[the unit]({{ website_url }}/docs/{{ platform }}/{{ version }}/CartesianChart.Time%20and%20logarithmic%20scales#the-unit)
for more. A `DateTimeAxis`
does that for you: `new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("yyyy MMM dd"))` sets the
`UnitWidth` and the `MinStep` of the axis to one day, and formats the labels with the given function.{{ if xaml }}
In XAML, the `XamlDateTimeAxis` takes the unit in its `Interval` property (one day by default) and the format
function in `DateFormatter`, as the samples above do.{{ end }}

When the dates do not matter, use
[FinancialPointI]({{ website_url }}/api/{{ version }}/LiveChartsCore.Defaults.FinancialPointI) instead: it has no date
and uses the index of each point as its X coordinate.

## Up and down paints

A point is drawn with `UpFill` and `UpStroke` when its close price is greater than its open price, and with `DownFill`
and `DownStroke` otherwise. When they are not set, the default theme draws up points in green and down points in red,
with a stroke of `3` pixels.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/financialpaints.png)

```csharp
XAxes = new[]
{
    new Axis
    {
        LabelsRotation = 15,
        Labeler = value => new DateTime((long)value).ToString("yyyy MMM dd"),
        // set the unit width of the axis to "days"
        // since our X axis is of type date time and 
        // the interval between our points is in days
        UnitWidth = TimeSpan.FromDays(1).Ticks
    }
};

Series = new ISeries[]
{
    new CandlesticksSeries<FinancialPoint>
    {
        UpFill = new SolidColorPaint(SKColors.Blue), // mark
        UpStroke = new SolidColorPaint(SKColors.CornflowerBlue) { StrokeThickness = 5 }, // mark
        DownFill = new SolidColorPaint(SKColors.Red), // mark
        DownStroke = new SolidColorPaint(SKColors.Orange) { StrokeThickness = 5 }, // mark
        Values = new ObservableCollection<FinancialPoint>
        {
            //                             date,        high, open, close, low
            new FinancialPoint(new DateTime(2021, 1, 1), 523, 500, 450, 400),
            new FinancialPoint(new DateTime(2021, 1, 2), 500, 450, 425, 400),
            new FinancialPoint(new DateTime(2021, 1, 3), 490, 425, 400, 380),
            new FinancialPoint(new DateTime(2021, 1, 4), 420, 400, 420, 380),
            new FinancialPoint(new DateTime(2021, 1, 5), 520, 420, 490, 400),
            new FinancialPoint(new DateTime(2021, 1, 6), 580, 490, 560, 440),
            new FinancialPoint(new DateTime(2021, 1, 7), 570, 560, 350, 340),
            new FinancialPoint(new DateTime(2021, 1, 8), 380, 350, 380, 330),
            new FinancialPoint(new DateTime(2021, 1, 9), 440, 380, 420, 350),
            new FinancialPoint(new DateTime(2021, 1, 10), 490, 420, 460, 400),
            new FinancialPoint(new DateTime(2021, 1, 11), 520, 460, 510, 460),
            new FinancialPoint(new DateTime(2021, 1, 12), 580, 510, 560, 500),
            new FinancialPoint(new DateTime(2021, 1, 13), 600, 560, 540, 510),
            new FinancialPoint(new DateTime(2021, 1, 14), 580, 540, 520, 500),
            new FinancialPoint(new DateTime(2021, 1, 15), 580, 520, 560, 520),
            new FinancialPoint(new DateTime(2021, 1, 16), 590, 560, 580, 520),
            new FinancialPoint(new DateTime(2021, 1, 17), 650, 580, 630, 550),
            new FinancialPoint(new DateTime(2021, 1, 18), 680, 630, 650, 600),
            new FinancialPoint(new DateTime(2021, 1, 19), 670, 650, 600, 570),
            new FinancialPoint(new DateTime(2021, 1, 20), 640, 600, 610, 560),
            new FinancialPoint(new DateTime(2021, 1, 21), 630, 610, 630, 590),
        }
    }
};
```

:::info
Paints can also draw gradients and dashed lines, see the
[Paints article]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Paints).
:::

## Width

`MaxBarWidth` limits the width of each candle, in pixels. It is `25` by default, so candles stop growing on wide
charts. The point can still be hovered across the whole unit of the axis. With `MaxBarWidth = 10`:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/financialw10.png)

```csharp
XAxes = new[]
{
    new Axis
    {
        LabelsRotation = 15,
        Labeler = value => new DateTime((long)value).ToString("yyyy MMM dd"),
        // set the unit width of the axis to "days"
        // since our X axis is of type date time and 
        // the interval between our points is in days
        UnitWidth = TimeSpan.FromDays(1).Ticks
    }
};

Series = new ISeries[]
{
    new CandlesticksSeries<FinancialPoint>
    {
        MaxBarWidth = 10, // mark
        Values = new ObservableCollection<FinancialPoint>
        {
            new FinancialPoint(new DateTime(2021, 1, 1), 523, 500, 450, 400),
            new FinancialPoint(new DateTime(2021, 1, 2), 500, 450, 425, 400),
            new FinancialPoint(new DateTime(2021, 1, 3), 490, 425, 400, 380),
            new FinancialPoint(new DateTime(2021, 1, 4), 420, 400, 420, 380),
            new FinancialPoint(new DateTime(2021, 1, 5), 520, 420, 490, 400),
            new FinancialPoint(new DateTime(2021, 1, 6), 580, 490, 560, 440),
            new FinancialPoint(new DateTime(2021, 1, 7), 570, 560, 350, 340),
            new FinancialPoint(new DateTime(2021, 1, 8), 380, 350, 380, 330),
            new FinancialPoint(new DateTime(2021, 1, 9), 440, 380, 420, 350),
            new FinancialPoint(new DateTime(2021, 1, 10), 490, 420, 460, 400)
        }
    }
};
```

## Tooltips and data labels

The tooltip shows the four prices of the point (`H`, `O`, `C` and `L`) formatted as currency. Set
`YToolTipLabelFormatter` to change that text. Data labels are off by default, setting `DataLabelsPaint` turns them on,
and by default they show the high and low prices of the point, also as currency. `DataLabelsFormatter` changes them:

```csharp
new CandlesticksSeries<FinancialPoint>
{
    Values = new ObservableCollection<FinancialPoint> { /* ... */ },
    YToolTipLabelFormatter = point => // mark
        $"Open {point.Model?.Open:N2}, Close {point.Model?.Close:N2}", // mark
    DataLabelsPaint = new SolidColorPaint(SKColors.Gray), // mark
    DataLabelsFormatter = point => $"{point.Model?.Close:N2}" // mark
}
```

## Data, names and updates

- `Values` takes any `IEnumerable<T>`. Use an `ObservableCollection<T>` and the chart animates every added, removed or
  replaced value. `FinancialPoint` also updates the chart when one of its properties changes, see
  [automatic updates]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Automatic%20updates).
- To plot your own types, see [mappers]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Mappers).
- `Name`{{ if xaml }} (`SeriesName` in XAML){{ end }} is shown in tooltips and legends. When it is not set, the tooltip shows the
  values without a series name.
- `ZIndex` sets which series draws on top, and `IsVisible` shows or hides the series.
