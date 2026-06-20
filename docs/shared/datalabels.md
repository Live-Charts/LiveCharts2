<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
content is normally pulled from the examples in the repository.
-->

## Data labels 

Data labels are labels for every point in a series, there are multiple properties to customize them, take a look at the 
following sample:

```csharp
new {{ name  | to_title_case_no_spaces }}<double>
{
    DataLabelsSize = 20,
    DataLabelsPaint = new SolidColorPaint(SKColors.Blue),
    // all the available positions at:
    // {{ website_url }}/api/{{ version }}/LiveChartsCore.Measure.DataLabelsPosition
    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
    // The DataLabelsFormatter is a function 
    // that takes the current point as parameter
    // and returns a string.
    // in this case we returned the PrimaryValue property as currency
    DataLabelsFormatter = (point) => point.Coordinate.PrimaryValue.ToString("C2"),
    Values = new ObservableCollection<double> { 2, 1, 3, 5, 3, 4, 6 },
    Fill = null
}
```

The previous series will result in the following chart:

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/1.8.datalabels.png)
