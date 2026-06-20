<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
content is normally pulled from the examples in the repository.
-->

{{ render "~/shared/genericSampleHeader.md" }}

{{~ if mvvm ~}}
## View model

```csharp
{{ render_current_directory_view_model }}
```
{{~ end ~}}

## {{~ view_title ~}}

```
{{ render_current_directory_view }}
```

{{~ if xaml2006 ~}}
## CustomGeometryPointColumnSeries.cs

```csharp
{{~ render $"~/../samples/{platform_samples_folder}/Lines/Basic/CustomStarLineSeries.cs" ~}}
```
{{~ end ~}}

{{ render "~/shared/relatedTo.md" }}
