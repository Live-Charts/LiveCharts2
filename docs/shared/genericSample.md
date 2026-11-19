<!--
To get help on editing this file, see https://github.com/Live-Charts/LiveCharts2/blob/release/v2.2.x/docs/readme.md
content is normally pulled from the examples in the repository.
-->

{{ render "~/shared/genericSampleHeader.md" }}

{{~ if mvvm ~}}
## View Model

```csharp
{{ render_current_directory_view_model }}
```
{{~ end ~}}

## {{~ view_title ~}}

```
{{ render_current_directory_view }}
```

{{ render "~/shared/relatedTo.md" }}
