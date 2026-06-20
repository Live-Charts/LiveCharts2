<!--
To get help on editing this file, see https://github.com/beto-rodriguez/LiveCharts2/blob/master/docs/readme.md
content is normally pulled from the examples in the repository.
-->

## AnimationsSpeed property

{{~ if name != "Cartesian chart control" ~}}
:::info
This section uses the `CartesianChart` control, but it works the same in the `{{ name  | to_title_case_no_spaces }}` control.
:::
{{~ end ~}}


Defines the animations speed of all the [chart elements]({{ website_url }}/api/{{ version }}/LiveChartsCore.Kernel.IChartElement-1) (axes, series, sections).

{{~ if xaml ~}}
<pre><code>&lt;lvc:CartesianChart
    Series="{Binding Series}"
    AnimationsSpeed="00:00:00.500"> &lt;!-- 500 milliseconds --> &lt;!-- mark -->
&lt;/lvc:CartesianChart></code></pre>
{{~ end ~}}

{{~ if blazor ~}}
<pre><code>&lt;lvc:CartesianChart
    Series="{Binding Series}"
    AnimationsSpeed="speed"> &lt;!-- mark -->
&lt;/lvc:CartesianChart></code></pre>

<pre><code>private TimeSpan speed = TimeSpan.FromMilliseconds(500);</code></pre>
{{~ end ~}}

{{~ if winforms ~}}
<pre><code>cartesianChart1.AnimationsSpeed = TimeSpan.FromMilliseconds(500);</code></pre>
{{~ end ~}}

## EasingFunction property

{{~ if name != "Cartesian chart control" ~}}
:::info
This section uses the `CartesianChart` control, but it works the same in the `{{ name  | to_title_case_no_spaces }}` control.
:::
{{~ end ~}}

This property defines the way the shapes in the chart animate, in other words it controls the way the 
[IMotionProperties]({{ website_url }}/api/{{ version }}/LiveChartsCore.Motion.IMotionProperty) of all the 
[chart elements]({{ website_url }}/api/{{ version }}/LiveChartsCore.Kernel.IChartElement-1) (axes, series, sections) in the chart
move from a state 'A' to state 'B'.

The property is of type `Func<float, float>`, it means that it is a function that takes a `float` argument (the time elapsed from 0 to 1), 
and  returns `float` value as the result (the progress of the animation from 0 to 1), you can learn more about easing curves at 
[this article](https://medium.com/@ryan_brownhill/crafting-easing-curves-for-user-interfaces-34f39e1b4a43).

{{~ if xaml ~}}
<pre><code>&lt;Container
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:lvcore="clr-namespace:LiveChartsCore;assembly=LiveChartsCore"> &lt;!-- import the core ns --> &lt;!-- mark -->

    &lt;lvc:CartesianChart
        Series="{Binding Series}"
        AnimationsSpeed="00:00:00.500"
        EasingFunction="{Binding Source={x:Static lvcore:EasingFunctions.BounceOut}}"> &lt;!-- mark -->
    &lt;/lvc:CartesianChart>
    
&lt;/Container></code></pre>
{{~ end ~}}

{{~ if blazor ~}}
<pre><code>&lt;lvc:CartesianChart
    Series="{Binding Series}"
    AnimationsSpeed="speed"
    EasingFunction=""> &lt;!-- mark -->
&lt;/lvc:CartesianChart></code></pre>

<pre><code>private Func&lt;float, float> easing = LiveChartsCore.EasingFunctions.BounceOut; // mark</code></pre>
{{~ end ~}}

{{~ if winforms ~}}
<pre><code>cartesianChart1.EasingFunction = LiveChartsCore.EasingFunctions.BounceOut;</code></pre>
{{~ end ~}}

Now the chart will animate following the **BounceOut** curve.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/bounceout-anim.gif)

Now try the `LiveChartsCore.EasingFunctions.Lineal` function, it will animate things lineally as the time elapses.

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/lineal-anim.gif)

Finally you can also build your own function:

{{~ if xaml ~}}
<pre><code>public Func&lt;float, float> Easing { get; set; } = time => time * time; // mark</code></pre>

<pre><code>&lt;Control
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    &lt;lvc:CartesianChart
        Series="{Binding Series}"
        AnimationsSpeed="00:00:00.500"
        EasingFunction="{Binding Easing}"> &lt;!-- mark -->
    &lt;/lvc:CartesianChart></code></pre>
{{~ end ~}}

{{~ if blazor ~}}
<pre><code>&lt;lvc:CartesianChart
    Series="{Binding Series}"
    AnimationsSpeed="speed"
    EasingFunction="easing"> &lt;!-- mark -->
&lt;/lvc:CartesianChart></code></pre>

<pre><code>private Func&lt;float, float> easing = time => time * time; // mark</code></pre>
{{~ end ~}}

{{~ if winforms ~}}
<pre><code>cartesianChart1.EasingFunction = time => time * time;</code></pre>
{{~ end ~}}

The library also provides some builders based on [d3-ease](https://github.com/d3/d3-ease) easing curves.

<pre><code>Func<float, float> easingCurve = LiveChartsCore.EasingFunctions.BuildCustomBackOut(0.8f);
Func<float, float> easingCurve = LiveChartsCore.EasingFunctions.BuildCustomElasticOut(0.8f, 1.1f);
// there are more builders, check them out all, they start with Build{ function }({ args })</code></pre>

## Disable animations

{{~ if name != "Cartesian chart control" ~}}
:::info
This section uses the `CartesianChart` control, but it works the same in the `{{ name  | to_title_case_no_spaces }}` control.
:::
{{~ end ~}}

Setting the `EasingFunction` to `null` disables animations.

{{~ if xaml ~}}
<pre><code>&lt;lvc:CartesianChart
    EasingFunction="{x:Null}"> &lt;!-- mark -->
&lt;/lvc:CartesianChart></code></pre>
{{~ end ~}}

{{~ if blazor ~}}
<pre><code>&lt;lvc:CartesianChart
    EasingFunction="null"> &lt;!-- mark -->
&lt;/lvc:CartesianChart></code></pre>
{{~ end ~}}

{{~ if winforms ~}}
<pre><code>cartesianChart1.EasingFunction = null; // mark</code></pre>
{{~ end ~}}

:::info
**Disabling animations will not improve performance drastically**: if you come from `LiveCharts 0.x` version then
maybe you are thinking that disabling animations will improve the performance of the library, in most of the cases
that is not true, animations are not the bottleneck in performance in `LiveCharts 2.x`, normally you need to 
clean your code somewhere else, not here, plus we put a lot of effort building the animations of the library, please
just do not disable them 😭, instead try to make them run faster, animating data visualization normally brings
an excellent user experience.
:::

## DrawMargin property

{{~ if name != "Cartesian chart control" ~}}
:::info
This section uses the `CartesianChart` control, but it works the same in the `{{ name  | to_title_case_no_spaces }}` control.
:::
{{~ end ~}}

Defines the distance from the axes (or edge of the chart if there is no axis) to the draw margin area.

{{~ if xaml ~}}
<pre><code>// Notice the constructor of the Margin class has multiple overloads
// {{ website_url }}/api/{{ version }}/LiveChartsCore.Measure.Margin
public LiveChartsCore.Measure.Margin Margin { get; set; } = new LiveChartsCore.Measure.Margin(100); // mark</code></pre>

<pre><code>&lt;lvc:CartesianChart
    Series="{Binding Series}"
    DrawMargin="{Binding Margin}"> &lt;!-- mark -->
&lt;/lvc:CartesianChart></code></pre>
{{~ end ~}}

{{~ if blazor ~}}
<pre><code>&lt;lvc:CartesianChart
    Series="{Binding Series}"
    AnimationsSpeed="speed"
    DrawMargin="margin"> &lt;!-- mark -->
&lt;/lvc:CartesianChart></code></pre>

<pre><code>// Notice the constructor of the Margin class has multiple overloads
// {{ website_url }}/api/{{ version }}/LiveChartsCore.Measure.Margin
private LiveChartsCore.Measure.Margin margin = new LiveChartsCore.Measure.Margin(100); // mark</code></pre>
{{~ end ~}}

{{~ if winforms ~}}
<pre><code>// Notice the constructor of the Margin class has multiple overloads
// {{ website_url }}/api/{{ version }}/LiveChartsCore.Measure.Margin
cartesianChart1.DrawMargin = new LiveChartsCore.Measure.Margin(100);</code></pre>
{{~ end ~}}

![image](https://raw.githubusercontent.com/beto-rodriguez/LiveCharts2/master/docs/_assets/drawmargin.png)
