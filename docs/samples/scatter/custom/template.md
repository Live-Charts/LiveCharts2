<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
content is normally pulled from the examples in the repository.
-->

{{ render "~/shared/genericSampleSimpleHeader.md" }}

{{~ if mvvm ~}}
## View model

```csharp
{{ render_current_directory_view_model }}
```
{{~ end ~}}

## MyGeometry.cs

```csharp
{{~ render "~/../samples/ViewModelsSamples/Scatter/Custom/MyGeometry.cs" ~}}
```

## {{~ view_title ~}}

```
{{ render_current_directory_view }}
```

{{~ if xaml2006 ~}}
## CustomSeries.cs

```csharp
{{~ render $"~/../samples/{platform_samples_folder}/Scatter/Custom/CustomSeries.cs" ~}}
```
{{~ end ~}}

{{ render "~/shared/relatedTo.md" }}
