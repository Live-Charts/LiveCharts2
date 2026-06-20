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

## {{~ view_title ~}}

```csharp
{{ render_current_directory_view }}
```

{{~ if !blazor ~}}
## CustomPieSeries.cs

```csharp
{{~ render $"~/../samples/{platform_samples_folder}/Pies/Icons/CustomPieSeries.cs" ~}}
```

## SvgLabel.cs

```csharp
{{~ render $"~/../samples/{platform_samples_folder}/Pies/Icons/SvgLabel.cs" ~}}
```
{{~ end ~}}

{{ render "~/shared/relatedTo.md" }}
