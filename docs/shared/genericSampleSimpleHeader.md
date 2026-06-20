<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
content is normally pulled from the examples in the repository.
-->

# {{ name | to_title_case }}

{{~ if xaml ~}}
:::info
This sample uses C# 13 preview features such as partial properties, it also uses features from the
[CommunityToolkit.Mvvm](https://www.nuget.org/packages/CommunityToolkit.Mvvm/) package, you can learn more about it 
[here]({{ website_url }}/docs/{{ platform }}/{{ version }}/About.About%20this%20samples).
:::
{{~ end ~}}


{{~ if winforms ~}}
:::info
This web site builds the control from code behind but you could also grab it from the toolbox.
:::
{{~ end ~}}

<div class="text-center sample-img">
    <img src="https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/{{ unique_name }}/result.png" alt="sample image" />
</div>
