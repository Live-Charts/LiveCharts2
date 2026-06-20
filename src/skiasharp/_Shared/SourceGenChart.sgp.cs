// The MIT License(MIT)
//
// Copyright(c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#pragma warning disable IDE0005 // Using directive is unnecessary.

using System;
using System.Collections.Generic;
using System.Windows.Input;
using LiveChartsCore;
using LiveChartsCore.Generators;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Observers;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.Painting;

#if AVALONIA_LVC
using Avalonia;
using Avalonia.Markup.Xaml.Templates;
#endif

#if MAUI_LVC
using Microsoft.Maui.Controls;
#endif

#if WINUI_LVC
using Microsoft.UI.Xaml;
#endif

#if WPF_LVC
using System.Windows;
#endif

#if SKIA_IMAGE_LVC
using SGChart = LiveChartsGeneratedCode.SourceGenSKChart;
#else
using SGChart = LiveChartsGeneratedCode.SourceGenChart;
#endif

// ==============================================================================================================
// the static fileds in this file generate bindable/dependency/avalonia or whatever properties...
// the disabled warning make it easier to maintain the code.
//
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable IDE0052 // Remove unread private member
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable CS8618  // Non-nullable field must contain a non-null value when exiting constructor.
#pragma warning disable CS0169  // The field is never used
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0040 // Add accessibility modifiers
#pragma warning disable format
// ==============================================================================================================

namespace LiveChartsGeneratedCode;

#if SKIA_IMAGE_LVC
public partial class SourceGenSKChart
#else
public partial class SourceGenChart
#endif
{
    static LiveChartsSettings d = LiveCharts.DefaultSettings;

    /// <inheritdoc cref="IChartView.UpdateStarted"/>
    static UIProperty<ICommand>                   updateStartedCommand;
    /// <inheritdoc cref="IChartView.DataPointerDown"/>
    static UIProperty<ICommand>                   dataPointerDownCommand;
    /// <inheritdoc cref="IChartView.HoveredPointsChanged"/>
    static UIProperty<ICommand>                   hoveredPointsChangedCommand;
    /// <inheritdoc cref="IChartView.ChartPointPointerDown"/>
    static UIProperty<ICommand>                   chartPointPointerDownCommand;
    /// <inheritdoc cref="IChartView.VisualElementsPointerDown"/>
    static UIProperty<ICommand>                   visualElementsPointerDownCommand;
#if MAUI_LVC
    // in maui we removed the Pointer prefix from the commands to match the
    // maui conventions, should we instead use the other names?
    /// <summary>
    /// Ocurrs when the chart is pressed.
    /// </summary>
    static UIProperty<ICommand>                   pressedCommand;
    /// <summary>
    /// Ocurrs when the pointer is moved over the chart.
    /// </summary>
    static UIProperty<ICommand>                   movedCommand;
    /// <summary>
    /// Ocurrs when the pointer is released over the chart.
    /// </summary>
    static UIProperty<ICommand>                   releasedCommand;
#else
    /// <summary>
    /// Ocurrs when the chart is pressed.
    /// </summary>
    static UIProperty<ICommand>                   pointerPressedCommand;
    /// <summary>
    /// Ocurrs when the pointer is moved over the chart.
    /// </summary>
    static UIProperty<ICommand>                   pointerMoveCommand;
    /// <summary>
    /// Ocurrs when the pointer is released over the chart.
    /// </summary>
    static UIProperty<ICommand>                   pointerReleasedCommand;
#endif

    /// <inheritdoc cref="IChartView.AnimationsSpeed"/>
    static UIProperty<TimeSpan>                   animationsSpeed         = new(d.AnimationsSpeed);
    /// <inheritdoc cref="IChartView.EasingFunction"/>
    static UIProperty<Func<float, float>?>        easingFunction          = new(d.EasingFunction);

    /// <inheritdoc cref="IChartView.DrawMargin"/>
    static UIProperty<Margin>                     drawMargin              = new(null,                     OnChartPropertyChanged);
    /// <inheritdoc cref="IChartView.LegendPosition"/>
    static UIProperty<LegendPosition>             legendPosition          = new(d.LegendPosition,         OnChartPropertyChanged);
    /// <inheritdoc cref="IChartView.TooltipPosition"/>
    static UIProperty<TooltipPosition>            tooltipPosition         = new(d.TooltipPosition,        OnChartPropertyChanged);
    /// <inheritdoc cref="IChartView.LegendTextPaint"/>
    static UIProperty<Paint?>                     legendTextPaint         = new(d.LegendTextPaint,        OnTextPaintPropertyChanged);
    /// <inheritdoc cref="IChartView.LegendBackgroundPaint"/>
    static UIProperty<Paint?>                     legendBackgroundPaint   = new(d.LegendBackgroundPaint,  OnChartPropertyChanged);
    /// <inheritdoc cref="IChartView.LegendTextSize"/>
    static UIProperty<double>                     legendTextSize          = new(d.LegendTextSize,         OnChartPropertyChanged);
    /// <inheritdoc cref="IChartView.TooltipTextPaint"/>
    static UIProperty<Paint?>                     tooltipTextPaint        = new(d.TooltipTextPaint,       OnTextPaintPropertyChanged);
    /// <inheritdoc cref="IChartView.TooltipBackgroundPaint"/>
    static UIProperty<Paint?>                     tooltipBackgroundPaint  = new(d.TooltipBackgroundPaint, OnChartPropertyChanged);
    /// <inheritdoc cref="IChartView.TooltipTextSize"/>
    static UIProperty<double>                     tooltipTextSize         = new(d.TooltipTextSize,        OnChartPropertyChanged);

    /// <inheritdoc cref="IChartView.SyncContext"/>
    static UIProperty<object>                     syncContext             = new(onChanged: OnSyncContextChanged);

    /// <inheritdoc cref="IChartView.Title"/>
    static UIProperty<IChartElement?>             title                   = new(onChanged: OnObservedPropertyChanged(nameof(Title)));
    /// <inheritdoc cref="IChartView.VisualElements"/>
    static UIProperty<IEnumerable<IChartElement>> visualElements          = new(onChanged: OnObservedPropertyChanged(nameof(VisualElements)));
    /// <inheritdoc cref="IChartView.Series"/>
    static UIProperty<IEnumerable<ISeries>>       series                  = new(onChanged: OnObservedPropertyChanged(nameof(Series)));

#if XAML_LVC
    // templates are only supported in xaml (Avalonia, Maui, Uno, WinUI and WPF).

    /// <summary>
    /// Gets or sets the source of the series.
    /// </summary>
    static UIProperty<IEnumerable<object>>        seriesSource            = new(onChanged: OnSeriesSourceChanged);
    /// <summary>
    /// Gets or sets the template used to create series from the <see cref="SeriesSource"/>.
    /// </summary>
    static UIProperty<DataTemplate>               seriesTemplate          = new(onChanged: OnSeriesSourceChanged);

    static void OnSeriesSourceChanged(SGChart chart)
    {
        if (chart._observer is null)
            throw new InvalidOperationException("The chart observer is not initialized.");

        var seriesObserver = (SeriesSourceObserver)chart._observer[nameof(SeriesSource)].Observer;
        seriesObserver.Initialize(chart.SeriesSource);
    }
#endif

    static void OnChartPropertyChanged(SGChart chart)
    {
#if BLAZOR_LVC
        // hack for blazor, we need to wait for the OnAfterRender to have
        // a reference to the canvas in the UI, CoreChart is null until then.
        if (chart.CoreChart is null) return;
#endif
        chart.CoreChart.Update();
    }

    static void OnTextPaintPropertyChanged(SGChart chart, object oldValue, object newValue)
    {
#if BLAZOR_LVC
        // hack for blazor, we need to wait for the OnAfterRender to have
        // a reference to the canvas in the UI, CoreChart is null until then.
        if (chart.CoreChart is null) return;
#endif
        ((Paint)newValue)?.PaintStyle = PaintStyle.Text;
        chart.CoreChart.Update();
    }

    static void OnSyncContextChanged(SGChart chart, object oldValue, object newValue)
    {
#if BLAZOR_LVC
        // hack for blazor, we need to wait for the OnAfterRender to have
        // a reference to the canvas in the UI, CoreChart is null until then.
        if (chart.CoreChart is null) return;
#endif
        chart.CoreCanvas.Sync = newValue;
        chart.CoreChart.Update();
    }

#pragma warning disable IDE0060 // Remove unused parameter, hack for the source generator
    /// <summary>
    /// Called when an observed property changes.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <param name="oldValue">The old property value.</param>
    /// <param name="newValue">The new property value.</param>
    /// <returns></returns>
    protected static Action<SGChart, object, object> OnObservedPropertyChanged(
        string propertyName, object? oldValue = null, object? newValue = null) =>
            (chart, o, n) =>
            {
                if (chart._observer is null)
                    throw new InvalidOperationException("The chart observer is not initialized.");

                chart._observer[propertyName].Observer.Initialize(n);

#if BLAZOR_LVC
                // hack for blazor, we need to wait for the OnAfterRender to have
                // a reference to the canvas in the UI, CoreChart is null until then.
                if (chart.CoreChart is null) return;
#endif
                chart.CoreChart.Update();

#if !BLAZOR_LVC
                // dont reset in blazor... it seems that blazor sets
                // the series property on any seriescollection change...
                // not sure if there is a way to configure this.
                if (propertyName == nameof(Series))
                {
                    // when the series collection changes, we re-start the series count.
                    // it makes sense... and also it helps the SeriesSourceObserver
                    chart.CoreChart.ResetNextSeriesId();
                }
#endif
            };
#pragma warning restore IDE0060 // Remove unused parameter

#if AVALONIA_LVC
    // avalonia hack to mock the DependencyProperty.OnPropertyChanged delegate.

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property.Name == nameof(IsPointerOver)) return;

        OnXamlPropertyChanged(change);

        if (change.Property.Name == nameof(ActualThemeVariant))
            CoreChart.ApplyTheme();
    }
#endif
}
