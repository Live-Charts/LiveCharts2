<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
This file pulls content from the ~/shared/ tempaltes
content is normally pulled from the examples in the repository.
-->

# {{ name | to_title_case }}

When the data is changing on another thread(s), there is a chance that while the chart is measured, the data changes at the same time. This 
could cause an `InvalidOperationException (Collection Was Modified)` and some other sync errors. We need to let the chart know that the data is
changing on multiple threads so it can handle it and prevent
[concurrency hazards](https://learn.microsoft.com/en-us/archive/msdn-magazine/2008/october/concurrency-hazards-solving-problems-in-your-multithreaded-code).

There are 2 alternatives you can follow to prevent this issue: 1. use the [lock](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/lock) keyword to wrap any change in your data, 2. Invoke the changes on the UI thread.

<div class="text-center sample-img">
    <img src="https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/{{ unique_name }}/result.gif" alt="sample image" />
</div>

## Locking changes (Alternative 1)

From MsDocs: 

> (the lock keyword) executes a statement block, and then releases the lock. While a lock is held, the thread that holds the lock can again acquire and release the lock. Any other thread is blocked from acquiring the lock and waits until the lock is released. The lock statement ensures that at maximum only one thread executes its body at any time moment.

Additionally from locking your data changes, you must also let LiveCharts know your lock object, then LiveCharts will grab the lock while it is 
measuring a chart.

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

Notice that we also set the chart `SyncContext` property so the chart knows our sync object.

## Invoke the changes on the UI thread (Alternative 2)

You can also force the change to happen on the same thread where the chart is measured, this will prevent concurrency hazards because
everything is happening on the same thread, but you must consider that now the UI thread is doing more operations.

{{~ if mvvm ~}}
## View model

```csharp
{{ full_name + "2" | get_vm_from_docs }}
```
{{~ end ~}}

## {{~ view_title ~}}

```
{{ full_name + "2" | get_view_from_docs }}
```

{{ render "~/shared/relatedTo.md" }}
