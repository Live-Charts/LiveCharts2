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

```
{{ render_current_directory_view }}
```

You can also create lines as sections, in the next example we set the same value for both
`Yi` and `Yj` and for the `Xi` and `Xj` we use the default value (`null` or `double.NaN`):

<div class="text-center sample-img">
    <img src="https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/{{ unique_name }}/result2.png" alt="sample image" />
</div>

```
{{~ render  $"~/../samples/{platform_samples_folder}/General/Sections2{view_extension}"  ~}}
```

{{ render "~/shared/relatedTo.md" }}
