<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
content is normally pulled from the examples in the repository.
-->

{{ render "~/shared/genericSampleJustGifHeader.md" }}

:::tip
This example uses the `LineSeries` class but it works the same for any series in the library
:::

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

{{ render "~/shared/relatedTo.md" }}
