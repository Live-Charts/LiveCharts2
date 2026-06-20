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
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Kernel.Observers;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.Motion;
using LiveChartsCore.Themes;
using LiveChartsCore.VisualElements;

namespace LiveChartsGeneratedCode;

// ==============================================================
// this file contains the shared code between all UI frameworks
// ==============================================================

/// <inheritdoc cref="IChartView" />
#if SKIA_IMAGE_LVC
public partial class SourceGenSKChart : IChartView
#else
public partial class SourceGenChart : IChartView
#endif
{
    private ChartObserver? _observer;

    /// <summary>
    /// Gets the core chart.
    /// </summary>
    public Chart CoreChart { get; private set; } = null!;

    /// <inheritdoc cref="IChartView.Tooltip" />
    public IChartTooltip? Tooltip { get; set; }

    /// <inheritdoc cref="IChartView.Legend" />
    public IChartLegend? Legend { get => field; set { field = value; CoreChart.Update(); } }

    /// <inheritdoc cref="IChartView.ChartTheme" />
    public Theme? ChartTheme { get; set { field = value; CoreChart.Update(); } }

    /// <inheritdoc cref="IChartView.UpdaterThrottler" />
    public TimeSpan UpdaterThrottler { get; set; } = LiveCharts.DefaultSettings.UpdateThrottlingTimeout;

    /// <inheritdoc cref="IChartView.AutoUpdateEnabled" />
    public bool AutoUpdateEnabled { get; set; } = true;

#if XAML_LVC
    private bool HasValidSource =>
        SeriesSource is not null && SeriesTemplate is not null;
#endif

    /// <inheritdoc cref="IChartView.Measuring" />
    public event ChartEventHandler? Measuring;

    /// <inheritdoc cref="IChartView.UpdateFinished" />
    public event ChartEventHandler? UpdateFinished;

    /// <inheritdoc cref="IChartView.UpdateStarted" />
    public event ChartEventHandler? UpdateStarted;

    /// <inheritdoc cref="IChartView.DataPointerDown" />
    public event ChartPointsHandler? DataPointerDown;

    /// <inheritdoc cref="IChartView.HoveredPointsChanged" />
    public event ChartPointHoverHandler? HoveredPointsChanged;

    /// <inheritdoc cref="IChartView.ChartPointPointerDown" />
    [Obsolete($"Use the {nameof(DataPointerDown)} event instead with a {nameof(FindingStrategy)} that used TakeClosest.")]
    public event ChartPointHandler? ChartPointPointerDown;

    /// <inheritdoc cref="IChartView.VisualElementsPointerDown"/>
    public event VisualElementsHandler? VisualElementsPointerDown;

    /// <summary>
    /// Creates the core chart instance for rendering and manipulation.
    /// </summary>
    /// <remarks>This method is abstract and must be implemented by derived classes to provide     a specific
    /// chart type. The returned <see cref="Chart"/> object represents the     foundational chart structure, which can
    /// be further customized or populated     with data.</remarks>
    /// <returns>A <see cref="Chart"/> object that serves as the base chart instance.</returns>
    protected abstract Chart CreateCoreChart();

    /// <inheritdoc cref="IChartView.GetPointsAt(LvcPointD, FindingStrategy, FindPointFor)"/>
    public IEnumerable<ChartPoint> GetPointsAt(
        LvcPointD point,
        FindingStrategy strategy = FindingStrategy.Automatic,
        FindPointFor findPointFor = FindPointFor.HoverEvent)
    {
        // Delegate to the core so a provider render override (e.g. transparent LOD)
        // can hit-test from its decimated data instead of walking every point.
        return CoreChart.GetPointsAt(point, strategy, findPointFor);
    }

    /// <inheritdoc cref="IChartView.GetVisualsAt(LvcPointD)"/>
    public IEnumerable<IChartElement> GetVisualsAt(LvcPointD point) =>
        CoreChart.VisualElements.SelectMany(visual =>
            ((VisualElement)visual).IsHitBy(CoreChart, new(point)));

    private void OnCoreMeasuring(IChartView chart) =>
        Measuring?.Invoke(this);

    private void OnCoreUpdateFinished(IChartView chart) =>
        UpdateFinished?.Invoke(this);

    private void StartObserving() =>
        _observer?.Initialize();

    private void StopObserving() =>
        _observer?.Dispose();

    private void InitializeChartControl()
    {
        CoreChart = CreateCoreChart();

        CoreChart.Measuring += OnCoreMeasuring;
        CoreChart.UpdateStarted += OnCoreUpdateStarted;
        CoreChart.UpdateFinished += OnCoreUpdateFinished;

#pragma warning disable CS0219 // Variable is assigned but its value is never used
        Action<object>? onAdd = null;
        Action<object>? onRemove = null;
#pragma warning restore CS0219 // Variable is assigned but its value is never used

#if XAML_LVC
        onAdd = AddUIElement;
        onRemove = RemoveUIElement;
#endif

#if !BLAZOR_LVC
        // in blazor this is initialized in the ChartControl constructor.
        _observer = new ChartObserver(
            ConfigureObserver, () => CoreChart?.Update(), onAdd, onRemove);
#endif
    }

    private void OnCoreUpdateStarted(IChartView chart)
    {
        if (UpdateStartedCommand is not null)
        {
            var args = new ChartCommandArgs(this);
            if (UpdateStartedCommand.CanExecute(args))
                UpdateStartedCommand.Execute(args);
        }

        UpdateStarted?.Invoke(this);
    }

    /// <summary>
    /// Configures the observer properties.
    /// </summary>
    /// <param name="observe">The current observer.</param>
    protected virtual ChartObserver ConfigureObserver(ChartObserver observe)
    {
        observe
            .Collection(nameof(Series), () => Series)
            .Collection(nameof(VisualElements), () => VisualElements)
            .Property(nameof(Title), () => Title);

#if XAML_LVC
        // if xaml... add the series template/source observer
        observe.AddObserver(
             new SeriesSourceObserver(this, InflateSeriesTemplate, () => HasValidSource),
             nameof(SeriesSource),
             () => SeriesSource);
#endif

        return observe;
    }

    /// <summary>
    /// Initializes the observed properties.
    /// </summary>
    protected virtual void InitializeObservedProperties() { }

    void IChartView.OnDataPointerDown(IEnumerable<ChartPoint> points, LvcPoint pointer)
    {
        DataPointerDown?.Invoke(this, points);
        if (DataPointerDownCommand is not null && DataPointerDownCommand.CanExecute(points))
            DataPointerDownCommand.Execute(points);

        ChartPointPointerDown?.Invoke(this, points.FindClosestTo(pointer));
#pragma warning disable CS0618 // Type or member is obsolete
        ChartPointPointerDownCommand?.Execute(points.FindClosestTo(pointer));
#pragma warning restore CS0618 // Type or member is obsolete
    }

    void IChartView.OnHoveredPointsChanged(IEnumerable<ChartPoint>? newPoints, IEnumerable<ChartPoint>? oldPoints)
    {
        HoveredPointsChanged?.Invoke(this, newPoints, oldPoints);

        var args = new HoverCommandArgs(this, newPoints, oldPoints);
        if (HoveredPointsChangedCommand is not null && HoveredPointsChangedCommand.CanExecute(args))
            HoveredPointsChangedCommand.Execute(args);
    }

    void IChartView.OnVisualElementPointerDown(
        IEnumerable<IInteractable> visualElements, LvcPoint pointer)
    {
        var args = new VisualElementsEventArgs(CoreChart, visualElements, pointer);

        VisualElementsPointerDown?.Invoke(this, args);
        if (VisualElementsPointerDownCommand is not null && VisualElementsPointerDownCommand.CanExecute(args))
            VisualElementsPointerDownCommand.Execute(args);
    }

    void IChartView.Invalidate() =>
        CoreCanvas.Invalidate();
}
