<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
content is normally pulled from the examples in the repository.
-->

# Series events

{{~ if xaml ~}}
:::info
The `[ObservableObject]`, `[ObservableProperty]` and `[RelayCommand]` attributes come from the 
[CommunityToolkit.Mvvm](https://www.nuget.org/packages/CommunityToolkit.Mvvm/) package, you can read more about it 
[here]({{ website_url }}/docs/{{ platform }}/{{ version }}/About.About%20this%20samples).
:::
{{~ end ~}}


{{~ if winforms ~}}
:::info
This web site builds the control from code behind but you could also grab it from the toolbox.
:::
{{~ end ~}}

In this example a column turns yellow when the pointer is above, then it turns red when the pointer goes down and finally
restores the default paint when the pointer leaves.

<div class="text-center sample-img">
    <img src="https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/{{ unique_name }}/result.gif" alt="sample image" />
</div>

{{~ if mvvm ~}}
## View model

```csharp
{{~ render "~/../samples/ViewModelsSamples/Events/Cartesian/ViewModel.cs" ~}}
```
{{~ end ~}}

## Fruit.cs

```csharp
{{~ render "~/../samples/ViewModelsSamples/Events/Cartesian/Fruit.cs" ~}}
```

## {{~ view_title ~}}

```
{{~ render "~/../samples/{platform_samples_folder}/Events/Cartesian{view_extension}" ~}}
```

By using the `Series` events you can subscribe strongly typed method signatures, where the library knows the type of
the visual, the type of the label and the data context we are drawing.

LiveCharts allows you to use any shape to represent a chart point 
([see custom svg point example](https://lvcharts.com/docs/{{ platform }}/{{ version }}/samples.scatter.custom)), you can also
plot any type you need for example in the example above we are plotting instances of the `Fruit` class, the library is able
to keep events strongly typed, but it could be tricky to guess the signature since the it changes depending on the series type,
the visual shape and the geometry.

Please use the IDE intellisense to complete the signature:

<div class="text-center sample-img">
    <img src="https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/{{ unique_name }}/intellisense.gif" alt="sample image" />
</div>

Notice how the IDE is able to detect that the first series is of type `int` (`ScatterSeries<int>`) while the second is of 
type `double` and is drawing `RectangleGeometry` instances to represent the visual points (`ScatterSeries<double, RectangleGeometry>`).

## Using Events at the chart level

You could also detect the pointer down events/commands at the chart level but since the chart `Series` property is of type 
`ISeries` the library is not able to determine the type of the series and we lose the strongly typed chart points.

```csharp
{{~ render "~/../samples/ViewModelsSamples/Events/Polar/ViewModel.cs" ~}}
```

```
{{~ render "~/../samples/{platform_samples_folder}/Events/Polar{view_extension}" ~}}
```

{{ render "~/shared/relatedTo.md" }}
