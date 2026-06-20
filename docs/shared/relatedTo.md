<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
content is normally pulled from the examples in the repository.
-->

{{ if related_to != null }}
<span style="opacity: 0.7"> Related articles: </span>

{{ for r in related_to }}
<div>
    <a href="{{ compile this r.url }}">
        <b>- {{ r.name }}</b>
    </a>
</div>
{{ end }}

{{~ end ~}}
