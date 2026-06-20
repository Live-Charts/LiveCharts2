<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
content is normally pulled from the examples in the repository.
-->

:::info
This article does not include all the properties of the {{ name | to_title_case }} class, it only highlights some features, to
explore the full object checkout the [API explorer]({{ website_url }}/api/{{ version }}/LiveChartsCore.SkiaSharpView.{{ name  | to_title_case_no_spaces }}-3)
:::

## Name property

The name property is a string identifier that is normally used in tooltips and legends to display the data name. 
If this property is not set, then by default the library will generate a name for the series called 
"Series 1" when it is the first series in the series collection, "Series 2" when it is the second series in the 
series collection, "Series 3" when it is the third series in the series collection, and so on—a series *n* will be 
named "Series *n*".

```csharp
SeriesCollection = new ISeries[]
{
    new {{ name  | to_title_case_no_spaces }}<int>
    {
        Values = new []{ 2, 5, 4, 2, 6 },
        Name = "Income", // mark
        Stroke = null
    },
    new {{ name | to_title_case_no_spaces }}<int>
    {
        Values = new []{ 3, 7, 2, 9, 4 },
        Name = "Outcome", // mark
        Stroke = null
    }
};
```

## Values property

The `Values` property is of type `IEnumerable<T>`. This means that you can use any object that implements the `IEnumerable<T>` interface, 
such as `Array`, `List<T>` or `ObservableCollection<T>`. This property contains the data to plot. You can use any type as the
generic argument (`<T>`) as long as you let the library know how to handle it. The library already knows how to handle multiple types, 
but you can register any type and teach the library how to handle any object in a chart. For more information please see the 
[mappers article]({{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Mappers).

```csharp
var series1 = new {{ name  | to_title_case_no_spaces }}<int>
{
    Values = new List<int> { 2, 1, 3 }
};

// == Update the chart when a value is added, removed or replaced  == // mark
// using ObservableCollections allows the chart to update
// every time you add a new element to the values collection
// (not needed in Blazor, it just... updates)
var series2 = new {{ name  | to_title_case_no_spaces }}<double>
{
    Values = new ObservableCollection<double> { 2, 1, 3 }
}
((ObservableCollection<double>)series2.Values).Add(4); // and the chart will animate the change!

// == Update the chart when a property in our collection changes  == // mark
// if the object implements INotifyPropertyChanged, then the chart will
// update automatically when a property changes, the library already provides
// many 'ready to go' objects such as the ObservableValue class.
var observableValue =  new ObservableValue(5);
var series3 = new {{ name  | to_title_case_no_spaces }}<ObservableValue>
{
    Values = new ObservableCollection<ObservableValue> { observableValue },
}
observableValue.Value = 9; // the chart will animate the change from 5 to 9!

// == Passing X and Y coordinates // mark 
// you can indicate both, X and Y using the Observable point class.
// or you could define your own object using mappers.
var series4 = new {{ name  | to_title_case_no_spaces }}<ObservablePoint>
{
    Values = new ObservableCollection<ObservablePoint> { new ObservablePoint(2, 6)}
}
```

```csharp
// == Custom types and mappers == // mark
// finally you can also use your own object, take a look at the City class.
public class City 
{
    public string Name { get; set; }
    public double Population { get; set; }
}
```

```csharp
// we must let the series know how to handle the city class.
// use the Mapping property to build a point from the city class
// you could also register the map globally.
// for more about global mappers info see:
// {{ website_url }}/docs/{{ platform }}/{{ version }}/Overview.Mappers
var citiesSeries = new {{ name  | to_title_case_no_spaces }}<City>
{
    Values = new City[]
    { 
        new City { Name = "Tokio", Population = 9 },
        new City { Name = "New York", Population = 11 },
        new City { Name = "Mexico City", Population = 10 },
    },
    Mapping = (city, index) =>
    {
        // this function will be called for each instance in our values array,
        // the function takes each instance (city) and the index of the item in the array as parameters
        // and must return the coordinate of the city in the plot.
        
        // in this case, we return the index as the X coordinate
        // and the Population as the Y value.
        
        return new Coordinate(index, city.Population);
    }
};
```

:::info
Automatic updates do not have a significant performance impact in most of the cases!
:::
